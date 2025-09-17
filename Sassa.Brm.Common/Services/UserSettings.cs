namespace Sassa.Brm.Common.Services
{
    public class UserSettings
    {
        public UserSettings(string storedSettings)
        {
            var settingsArray = storedSettings.Split(";".ToCharArray());
            IsSupervisor = settingsArray[0] == "Y" ? true : false;
            ReportFormat = settingsArray[1];
        }
        public bool IsSupervisor { get; set; }
        public string ReportFormat { get; set; } = "csv";

        public override string ToString()
        {
            return $"{(IsSupervisor ? "Y" : "N")};{ReportFormat}";
        }
    }
}
