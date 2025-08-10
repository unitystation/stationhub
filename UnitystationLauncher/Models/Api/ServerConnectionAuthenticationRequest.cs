namespace UnitystationLauncher.Models.Api;

public class ServerConnectionAuthenticationRequest
{
    public string? ClientFork { get; set; }
    
    public string? ClientVersion { get; set; }
    
    public string? GoodFileVersion { get; set; }
    public string? EncryptedSharedSecret{ get; set; }
    public string? EncryptedAccountID { get; set; }
    
    public string? ConnectionPublicServerKey{ get; set; }
}