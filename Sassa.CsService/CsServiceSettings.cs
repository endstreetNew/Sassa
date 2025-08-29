namespace Sassa.Services
{
    public class CsServiceSettings
    {
        public string CsServiceUser { get; set; } = string.Empty;
        public string CsServicePass { get; set; } = string.Empty;
        public string CsWSEndpoint { get; set; } = string.Empty;
        public string CsConnection { get; set; } = string.Empty;
        public string CsDocFolder { get; set; } = string.Empty;
        public string CsBeneficiaryRoot { get; set; } = string.Empty;
        public int CsMaxRetries { get; set; } = 3;
        public CsServiceSettings() { }
        public CsServiceSettings(string user, string pass, string endpoint, string connection, string csdocfolder, string csBenificaryRoot, int maxRetries )      
        {
            CsServiceUser = user;
            CsServicePass = pass;
            CsWSEndpoint = endpoint;
            CsConnection = connection;
            CsDocFolder = csdocfolder;
            CsBeneficiaryRoot = csBenificaryRoot;
            CsMaxRetries = maxRetries;
        }
    }
}
