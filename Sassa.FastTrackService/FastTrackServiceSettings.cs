public class FastTrackServiceSettings
{
    public LoggingSettings Logging { get; set; } = new LoggingSettings();
    public SerilogSettings Serilog { get; set; } = new SerilogSettings();
    public string BrmUser { get; set; } = "SVC_BRM_LO";
    public string AllowedHosts { get; set; } = "*";
    public ConnectionStringsSettings ConnectionStrings { get; set; } = new ConnectionStringsSettings();
    public ContentServerSettings ContentServer { get; set; } = new ContentServerSettings();
    public FunctionsSettings Functions { get; set; } = new FunctionsSettings();
    public UrlsSettings Urls { get; set; } = new UrlsSettings();
}

public class LoggingSettings
{
    public LogLevelSettings LogLevel { get; set; } = new LogLevelSettings();
}

public class LogLevelSettings
{
    public string Default { get; set; } = "Information";
    public string MicrosoftAspNetCore { get; set; } = "Warning";
}

public class SerilogSettings
{
    public MinimumLevelSettings MinimumLevel { get; set; } = new MinimumLevelSettings();
}

public class MinimumLevelSettings
{
    public string Default { get; set; } = "Information";
    public Dictionary<string, string> Override { get; set; } = new Dictionary<string, string>
    {
        { "Microsoft", "Warning" },
        { "Microsoft.Hosting.Lifetime", "Information" },
        { "System", "Warning" }
    };
}

public class ConnectionStringsSettings
{
    public string BrmConnection { get; set; } = "DATA SOURCE=10.117.123.20:1525/brmtrn;PERSIST SECURITY INFO=True;USER ID=CONTENTSERVER;Password=Password123;";
    public string LoConnection { get; set; } = "Data Source=10.117.123.51:1525/ecsqa;Persist Security Info=True;User ID=lo_admin;Password=sassa123";
    public string CsConnection { get; set; } = "Data Source=10.117.123.51:1525/=ecsqa; user id=contentserver; password=Password123;";
}

public class ContentServerSettings
{
    public string CSServiceUser { get; set; } = "lo-upload";
    public string CSServicePass { get; set; } = "!0-Up10@d";
    public string CSWSEndpoint { get; set; } = "http://ssvsqadsshc01.sassa.local:8080/cws/services/";
    public string CSFILEURL { get; set; } = "http://edrms.sassa.gov.za/otcs/llisapi.dll?fetch/2000/47634/";
    public string CSBeneficiaryRoot { get; set; } = "Default";
    public int CSMaxRetries { get; set; } = 3;
}

public class FunctionsSettings
{
    public bool AddCover { get; set; } = false;
    public bool KofaxFileWatcher { get; set; } = true;
    public bool CsFileWatcher { get; set; } = true;
    public int KofaxPollIntervalSeconds { get; set; } = 120;
    public int CsPollIntervalSeconds { get; set; } = 120;
}

public class UrlsSettings
{
    public int AppPort { get; set; } = 8088;
    public string ScanFolderRoot { get; set; } = "\\\\SSVSDVKFSHC03\\FastTrack_Output";
}