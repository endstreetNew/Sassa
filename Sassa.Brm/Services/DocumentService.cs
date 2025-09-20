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

        public void RejectDocument(string reference)
        {
            string filePath = GetFirstDocument(reference);
            string fileName = Path.GetFileName(filePath);
            System.IO.File.Move(filePath, Path.Combine(_rejectedDirectory, fileName), true);
        }
        public void RepairDocument(string reference)
        {
            string filePath = GetFirstDocument(reference);
            string fileName = Path.GetFileName(filePath);
            System.IO.File.Move(filePath, Path.Combine(_rootDirectory, fileName), true);
        }
    }
}
