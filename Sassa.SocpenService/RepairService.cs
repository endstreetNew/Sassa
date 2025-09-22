namespace Sassa.Services
{
    public class RepairService
    {
        private string _rootDirectory;
        private string _repairDirectory;
        public RepairService(string rootfolder)
        {
            _rootDirectory = rootfolder;
            _repairDirectory = Path.Combine(_rootDirectory, "RepairQueue");
        }

        public void RetryAllRepairs()
        {

            foreach (string filePath in Directory.GetFiles(_repairDirectory, "*", SearchOption.TopDirectoryOnly))
            {
                string fileName = Path.GetFileName(filePath);
                string destPath = Path.Combine(_rootDirectory, fileName);

                // Overwrite if file exists
                File.Move(filePath, destPath, overwrite: true);
            }

            //Console.WriteLine("All files moved successfully.");

        }
    }
}
