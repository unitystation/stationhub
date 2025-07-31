namespace UnitystationLauncher.Models.Api;

public class ServerConnectionAuthenticationRequest
{
    public string EncryptedClientFork { get; set; }
    public string EncryptedClientVersion { get; set; }
    public string EncryptedGoodFileVersion { get; set; }
    public string EncryptedSharedSecret { get; set; }
    public string EncryptedAccountID { get; set; }
    public string EncryptedConnectionPublicServerKey { get; set; }
}