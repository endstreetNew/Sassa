namespace Sassa.Services
{
    public class DocumentService
    {
        private string _rootFolder;
        private string _rejectedDirectory;
        public DocumentService(string rootfolder)
        {
            _rootFolder = rootfolder;
            _rejectedDirectory = Path.Combine(_rootFolder, "Rejected");
            if (!Directory.Exists(_rejectedDirectory))
                Directory.CreateDirectory(_rejectedDirectory);

        }

        public string GetFirstDocument(string reference)
        {
            if (!Directory.Exists(_rootFolder))
                return string.Empty;
            var files = Directory.GetFiles(_rootFolder, $"{reference}**", SearchOption.TopDirectoryOnly)
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
    }
}
