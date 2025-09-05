namespace Sassa.LO.RepairQueue.Services
{
    public class DocumentService
    {
        private string _rootFolder;
        public DocumentService(string rootfolder) 
        {
            _rootFolder = rootfolder;
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
    }
}
