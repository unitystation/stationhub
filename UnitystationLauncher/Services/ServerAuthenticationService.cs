using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.OpenSsl;
using UnitystationLauncher.Models;
using UnitystationLauncher.Models.Api;

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
        try
        {
            string Port = "7778";
            string url = $"http://{IP}:{Port}/";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            string content = await response.Content.ReadAsStringAsync();
            var serverInfo = JsonConvert.DeserializeObject<Server>(content);

            return serverInfo;
        }
        catch (Exception ex)
        {
            return null;
        }
    }


    static AsymmetricKeyParameter ImportKeyFromPem(string pem)
    {
        using (var sr = new StringReader(pem))
        {
            var pr = new PemReader(sr);
            return (AsymmetricKeyParameter)pr.ReadObject();
        }
    }


    public async Task<Dictionary<string, string>> AuthenticateWithServer(string IP, Installation Installation)
    {
        var Info = await GetServerInfoByIP(IP);
        var RSAEncrypt = new OaepEncoding(
            new RsaEngine(),
            new Sha256Digest()
        );

        RSAEncrypt.Init(true, ImportKeyFromPem(Info.ServerPublicKey)); // false = decrypt mode

        byte[] sharedSecret = new byte[32]; // 256-bit key
        RandomNumberGenerator.Fill(sharedSecret);

        // Optional: convert to Base64 if you want to transmit/store it
        string base64Secret = Convert.ToBase64String(sharedSecret);

        var SHA512Check = Convert.ToBase64String(
            SHA512.ComputeHash(
                Encoding.UTF8.GetBytes(
                    base64Secret
                    + Info.ServerPublicKey
                    + Installation.BuildVersion.ToString()
                    + Installation.ForkName.ToString()
                    + Installation.GoodFileVersion.ToString()
                    + Info.ServerConnectionPublicKey)));
        _AuthService.RegisterJoiningServerWithSecret(SHA512Check);


        var ToSend = new ServerConnectionAuthenticationRequest
        {
            ConnectionPublicServerKey = Info.ServerConnectionPublicKey,
            ClientVersion = Installation.BuildVersion.ToString(),
            GoodFileVersion = Installation.GoodFileVersion.ToString(),
            ClientFork = Installation.ForkName.ToString(),
            EncryptedSharedSecret = EncryptString(RSAEncrypt, base64Secret),
            EncryptedAccountID = EncryptString(RSAEncrypt, _AuthService.AccountLoginResponse.Account.UniqueIdentifier)
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

        return new Dictionary<string, string>
        {
            {"-SharedSecret", base64Secret},
            {"-ServerPublicConnectionKey", Info.ServerConnectionPublicKey},
        };
    }

    private string EncryptString(OaepEncoding rsa, string ToEncrypt)
    {
        var Bytes = Encoding.UTF8.GetBytes(ToEncrypt);
        return Convert.ToBase64String(rsa.ProcessBlock(Bytes, 0, Bytes.Length));
    }
}