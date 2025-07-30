namespace UnitystationLauncher.Models.Api;

public class ServerConnectionAuthenticationRequest
{
    public string EncryptedClientFork;
    public string EncryptedClientVersion;
    public string EncryptedGoodFileVersion;
    public string EncryptedSharedSecret;
    public string EncryptedAccountID;
    public string EncryptedConnectionPublicServerKey;
}