#if WINDOWS
using Meziantou.Framework.Win32;
using System.Runtime.Versioning;

namespace Helper;

public static class CredentialHelper
{

    [SupportedOSPlatform("windows5.1.2600")]
    public static void SaveCredential(
        string applicationName,
        string userName,
        string secret,
        string comment = "",
        CredentialPersistence credentialPersistence = CredentialPersistence.LocalMachine
    )
    {
        CredentialManager.WriteCredential(
            applicationName: applicationName,
            userName: userName,
            secret: secret,
            comment: comment,
            persistence: credentialPersistence
        );
    }

    [SupportedOSPlatform("windows5.1.2600")]
    public static (string UserName, string Secret)? LoadCredential(string applicationName)
    {
        var credential = CredentialManager.ReadCredential(applicationName);
        if (credential == null)
        {
            return null;
        }
        return (credential.UserName ?? "", credential.Password ?? "");
    }

    [SupportedOSPlatform("windows5.1.2600")]
    public static void DeleteCredential(string applicationName)
    {
        CredentialManager.DeleteCredential(applicationName);
    }


}
#endif
