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
using Org.BouncyCastle.Math.EC;
using Org.BouncyCastle.OpenSsl;
using UnitystationLauncher.Models;
using UnitystationLauncher.Models.Api;

namespace UnitystationLauncher.Services;

public interface IServerAuthenticationService
{
    public Task<Server> QueryServerInfo(string IP);

    public Task<Dictionary<string, string>> PrenegotiateWithServer(string IP, Installation Installation);
}

public class ServerAuthenticationService : IServerAuthenticationService
{
    public ServerAuthenticationService(AuthService AuthService)
    {
        _AuthService = AuthService;
    }


    private readonly AuthService _AuthService;
    private readonly HttpClient _httpClient = new HttpClient();

    public async Task<Server> QueryServerInfo(string IP)
    {
        try
        {
            // check if IP is in the format "IP:Port"
            // if not, assume default port 7778
            string[] parts = IP.Split(':');
            string Port = parts.Length == 2 ? parts[1] : "7778";

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


    public async Task<Dictionary<string, string>> PrenegotiateWithServer(string IP, Installation Installation)
    {
        var Info = await QueryServerInfo(IP);
        var CryptoEncoding = new OaepEncoding(
            new ElGamalEngine(),
            new Sha256Digest()
        );

        // wrap the base64 encoded public key in PEM format
        var ServerPublicKey = ImportKeyFromPem("-----BEGIN PUBLIC KEY-----\n" +
            Info.ServerPublicKey + "\n" +
            "-----END PUBLIC KEY-----\n");

        CryptoEncoding.Init(true, ServerPublicKey); // false = decrypt mode

        byte[] sharedSecret = new byte[32]; // 256-bit key
        RandomNumberGenerator.Fill(sharedSecret);

        var hashInput = new byte[64];
        Array.Copy(sharedSecret, 0, hashInput, 0, 32);
        Array.Copy(Convert.FromBase64String(Info.ServerPublicKey), 0, hashInput, 32, 32);

        var ConnectionChallenge = Convert.ToHexString(SHA512.HashData(hashInput));

        var ScopeToken = await _AuthService.RegisterConnectionChallenge(ConnectionChallenge, Installation.ForkName);


        var ToSend = new ServerConnectionAuthenticationRequest
        {
            SharedSecret = Convert.ToBase64String(sharedSecret),
            UniqueIdentifier = _AuthService.AccountLoginResponse.Account.UniqueIdentifier,
            AuthRealm = _AuthService
        };


        // Serialize the object to JSON
        string json = JsonConvert.SerializeObject(ToSend);

        // Wrap it in a StringContent with JSON media type
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        //TODO make this into a function like QueryServerIP

        string Port = "7778";
        string url = $"http://{IP}:{Port}/";

        var response = await _httpClient.PostAsync(url, content);
        response.EnsureSuccessStatusCode();

        string contentBack = await response.Content.ReadAsStringAsync();
        if (contentBack != "OK")
        {
            throw new AuthenticationException(contentBack + $" When trying to authenticate with {url}");
        }
        //end of todo

        return new Dictionary<string, string>
        {
            {"-SharedSecret", base64Secret},
            {"-ServerPublicConnectionKey", Info.DEPRECATEME_ServerConnectionPublicKey},
            {"-ScopedToken", ScopeToken.ScopeToken}
        };
    }

    private string EncryptString(OaepEncoding encoding, string ToEncrypt)
    {
        var Bytes = Encoding.UTF8.GetBytes(ToEncrypt);
        return Convert.ToBase64String(encoding.ProcessBlock(Bytes, 0, Bytes.Length));
    }
}