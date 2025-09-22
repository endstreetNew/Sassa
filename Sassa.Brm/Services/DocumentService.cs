namespace Sassa.Services
{
    public class DocumentService
    {
        private string _rootDirectory;
        private string _repairDirectory;
        private string _rejectedDirectory;
        public DocumentService(string rootfolder)
        {
            _rootDirectory = rootfolder;
            _rejectedDirectory = Path.Combine(_rootDirectory, "Rejected");
            _repairDirectory = Path.Combine(_rootDirectory, "RepairQueue");
            if (!Directory.Exists(_rejectedDirectory))
                Directory.CreateDirectory(_rejectedDirectory);

        }

        public string GetFirstDocument(string reference)
        {
            if (!Directory.Exists(_repairDirectory))
                return string.Empty;
            var files = Directory.GetFiles(_repairDirectory, $"{reference}**", SearchOption.TopDirectoryOnly)
                                 .OrderBy(f => f)
                                 .ToList();
            return files.FirstOrDefault() ?? string.Empty;
        }

        //This file should just be deleted
        public void RejectDocument(string reference)
        {
            string filePath = GetFirstDocument(reference);
            string fileName = Path.GetFileName(filePath);
            System.IO.File.Move(filePath, Path.Combine(_rejectedDirectory, "delete." + fileName), true);
        }
        public void RepairDocument(string reference)
        {
            string filePath = GetFirstDocument(reference);
            if (string.IsNullOrEmpty(filePath))
            {
                throw new Exception("This file needs to be re-scanned due to a scanning error.");
            }
            string fileName = Path.GetFileName(filePath);

            int maxRetries = 5;
            int delayMs = 1000;
            int attempt = 0;
            bool fileReady = false;

            while (attempt < maxRetries && !fileReady)
            {
                try
                {
                    using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                    {
                        fileReady = true;
                    }
                }
                catch (IOException)
                {
                    attempt++;
                    if (attempt < maxRetries)
                    {
                        Thread.Sleep(delayMs);
                    }
                    else
                    {
                        throw new IOException($"File {filePath} is currently in use and cannot be moved after {maxRetries} attempts.");
                    }
                }
            }

            System.IO.File.Move(filePath, Path.Combine(_rootDirectory, fileName), true);


        }
    }
}
