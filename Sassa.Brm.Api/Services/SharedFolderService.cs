using SimpleImpersonation;
using System.IO;

/// <summary>
/// Redundant  can be removed once Fasttrack is complete.
/// </summary>
public class SharedFolderService
{
    public string ReadFile(string filePath)
    {
        var credentials = new UserCredentials("DOMAIN", "username", "password");

        return Impersonation.RunAsUser(credentials, LogonType.Interactive, () =>
        {
            if (!File.Exists(filePath))
                return "File not found.";

            return File.ReadAllText(filePath);
        });
    }
}