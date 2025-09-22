using Sassa.Scanning.Settings;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Sassa.Scanning.Services
{
    public class ProgressWindow :IDisposable
    {
        // Preview window plumbing
        private Thread? _previewThread;
        private Form? _previewForm;
        private PictureBox? _previewPictureBox;
        private ManualResetEventSlim? _previewReady;
        private CheckBox? _previewToggle;
        private volatile bool _previewEnabled;

        ScanningSettings _settings;
        public ProgressWindow(ScanningSettings settings)
        {
            _settings = settings;
            _previewEnabled = settings.PreviewWindow.Enabled;
        }


        public void Initialize()
        {
            if (_settings.PreviewWindow.Enabled)
            {
                StartPreviewWindow(_settings.PreviewWindow.Width, _settings.PreviewWindow.Height, "Preview");
            }
        }


        private void StartPreviewWindow(int width, int height, string title = "Preview")
        {
            _previewReady = new ManualResetEventSlim(false);
            _previewThread = new Thread(() =>
            {
                Application.SetHighDpiMode(HighDpiMode.SystemAware);
                _previewForm = new Form
                {
                    Text = title,
                    Width = (int)(width * 1.5),
                    Height = (int)(height * 1.5)
                };

                // Header panel with logo
                var headerPanel = new Panel
                {
                    Dock = DockStyle.Top,
                    Height = 80,
                    BackColor = Color.White
                };
                var logoBox = new PictureBox
                {
                    SizeMode = PictureBoxSizeMode.Zoom, // Ensures the image fits the box
                    Dock = DockStyle.Fill,
                    BackColor = Color.Transparent
                };
                try
                {
                    var logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images", "sassa_logoSmall.jpg");
                    if (File.Exists(logoPath))
                    {
                        logoBox.Image = Image.FromFile(logoPath);
                    }
                }
                catch { /* ignore image load errors */ }
                headerPanel.Controls.Add(logoBox);

                _previewToggle = new CheckBox
                {
                    Dock = DockStyle.Top,
                    AutoSize = true,
                    Checked = _previewEnabled,
                    Text = _previewEnabled ? "Preview On" : "Preview Off",
                    Padding = new Padding(8)
                };
                _previewToggle.CheckedChanged += (s, e) =>
                {
                    _previewEnabled = _previewToggle.Checked;
                    _previewToggle.Text = _previewEnabled ? "Preview On" : "Preview Off";
                };
                _previewPictureBox = new PictureBox
                {
                    Dock = DockStyle.Fill,
                    SizeMode = PictureBoxSizeMode.Zoom
                };
                _previewForm.Controls.Add(_previewPictureBox);
                _previewForm.Controls.Add(_previewToggle);
                _previewForm.Controls.Add(headerPanel); // Add header at the top
                _previewReady!.Set();
                Application.Run(_previewForm);
            });
            _previewThread.SetApartmentState(ApartmentState.STA);
            _previewThread.IsBackground = true;
            _previewThread.Start();
            _previewReady.Wait();
        }

        public void UpdatePreviewImage(Bitmap clone)
        {
            if (_previewPictureBox == null || !_previewEnabled) return;

            void SetImage()
            {
                if (!_previewEnabled) return;
                // Clone to avoid disposing in caller affecting UI
                //var clone = (Bitmap)bmp.Clone();
                var old = _previewPictureBox!.Image;
                _previewPictureBox.Image = clone;
                old?.Dispose();
            }

            if (_previewPictureBox.InvokeRequired)
                _previewPictureBox.BeginInvoke(new Action(SetImage));
            else
                SetImage();
        }

        public void StopPreviewWindow()
        {
            if (_previewForm == null) return;
            try
            {
                if (_previewForm.InvokeRequired)
                    _previewForm.BeginInvoke(new Action(() => _previewForm.Close()));
                else
                    _previewForm.Close();
                _previewThread?.Join(2000);
            }
            catch { /* ignore */ }
            finally
            {
                _previewThread = null;
                _previewForm = null;
                _previewPictureBox = null;
                _previewToggle = null;
                _previewReady?.Dispose();
                _previewReady = null;
            }
        }        // Start preview window (non-blocking) if enabled

        public void Dispose()
        {
            if (_settings.PreviewWindow.Enabled)
                StopPreviewWindow();

        }
    }
}
