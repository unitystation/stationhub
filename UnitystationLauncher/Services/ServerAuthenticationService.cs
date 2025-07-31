using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Serilog;
using UnitystationLauncher.Models;
using UnitystationLauncher.Models.Api;
using System.Security.Cryptography;

namespace UnitystationLauncher.Services;

public interface IServerAuthenticationService
{
    public Task<Server> GetServerInfoByIP(string IP);

    public Task<Dictionary<string, string>> AuthenticateWithServer(string IP, Installation Installation);

}

public class ServerAuthenticationService : IServerAuthenticationService
{

    public ServerAuthenticationService(AuthService AuthService)
    {
        _AuthService = AuthService;
    }


    private readonly AuthService _AuthService;
    private readonly HttpClient _httpClient = new HttpClient();

    private readonly SHA512 SHA512 = SHA512.Create();

    public async Task<Server> GetServerInfoByIP(string IP)
    {
        string Port = "7778";
        string url = $"http://{IP}:{Port}/";

        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        string content = await response.Content.ReadAsStringAsync();
        var serverInfo = JsonConvert.DeserializeObject<Server>(content);

        return serverInfo;
    }

    public async Task<Dictionary<string, string>> AuthenticateWithServer(string IP, Installation Installation)
    {

        var Info = await GetServerInfoByIP(IP);
        string base64PublicKey = Info.ServerPublicKey;
        byte[] publicKeyBytes = Convert.FromBase64String(base64PublicKey);

        using RSA rsa = RSA.Create();
        rsa.ImportRSAPublicKey(publicKeyBytes, out _);

        byte[] sharedSecret = new byte[32]; // 256-bit key
        RandomNumberGenerator.Fill(sharedSecret);

        // Optional: convert to Base64 if you want to transmit/store it
        string base64Secret = Convert.ToBase64String(sharedSecret);

        var ToSend = new ServerConnectionAuthenticationRequest
        {
            EncryptedConnectionPublicServerKey = EncryptString(rsa, Info.ServerConnectionPublicKey),
            EncryptedClientVersion = EncryptString(rsa, Installation.BuildVersion.ToString()),
            EncryptedGoodFileVersion = EncryptString(rsa, Installation.GoodFileVersion.ToString()),
            EncryptedClientFork = EncryptString(rsa, Installation.ForkName.ToString()),
            EncryptedSharedSecret = EncryptString(rsa, base64Secret),
            EncryptedAccountID = EncryptString(rsa, _AuthService.AccountLoginResponse.Account.UniqueIdentifier)
        };


        // Serialize the object to JSON
        string json = JsonConvert.SerializeObject(ToSend);

        // Wrap it in a StringContent with JSON media type
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        string Port = "7778";
        string url = $"http://{IP}:{Port}/";

        var response = await _httpClient.PostAsync(url, content);
        response.EnsureSuccessStatusCode();

        string contentBack = await response.Content.ReadAsStringAsync();
        if (contentBack != "OK")
        {
            throw new AuthenticationException(contentBack + $" When trying to authenticate with {url}");
        }

        var SHA512Check = Convert.ToBase64String(SHA512.ComputeHash(Encoding.UTF8.GetBytes(base64Secret + Info.ServerPublicKey)));
        _AuthService.RegisterJoiningServerWithSecret(SHA512Check);

        var CharacterToken = await _AuthService.GenerateCharacterSheetTokenForFork(Installation.ForkName);

        return new Dictionary<string, string>
        {
            { "-CharacterToken", CharacterToken.CharacterToken},
            { "-SharedSecret", base64Secret},
            { "-ServerPublicConnectionKey", Info.ServerConnectionPublicKey},
        };
    }

    private string EncryptString(RSA rsa, string ToEncrypt)
    {
        return Convert.ToBase64String(rsa.Encrypt(Encoding.UTF8.GetBytes(ToEncrypt), RSAEncryptionPadding.OaepSHA256));
    }
}