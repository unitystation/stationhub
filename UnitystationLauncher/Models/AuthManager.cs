using System;
using System.IO;
using System.Threading.Tasks;
using Firebase.Auth;
using System.Net.Http;
using System.Threading;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace UnitystationLauncher.Models
{
    public class AuthManager
    {
        readonly FirebaseAuthProvider _authProvider;
        private readonly HttpClient _http;
        public LoginMsg? LoginMsg { get; set; }
        public bool AttemptingAutoLogin { get; set; }

        public AuthManager(HttpClient http, FirebaseAuthProvider authProvider)
        {
            _authProvider = authProvider;
            _http = http;
            if (File.Exists(Path.Combine(Config.RootFolder, "settings.json")))
            {
                var json = File.ReadAllText(Path.Combine(Config.RootFolder, "settings.json"));
                var authLink = JsonSerializer.Deserialize<FirebaseAuthLink>(json);
                AuthLink = authLink;
            }
        }

        public FirebaseAuthLink? AuthLink { get; set; }
        public string? CurrentRefreshToken => AuthLink?.RefreshToken;

        public string? Uid => AuthLink?.User.LocalId;

        public void Store()
        {
            var json = JsonSerializer.Serialize(AuthLink);

            using (StreamWriter writer = File.CreateText(Path.Combine(Config.RootFolder, "settings.json")))
            {
                writer.WriteLine(json);
            }
        }

        public void ResendVerificationEmail()
        {
            _authProvider.SendEmailVerificationAsync(AuthLink);
        }

        public void SendForgotPasswordEmail(string email)
        {
            _authProvider.SendPasswordResetEmailAsync(email);
        }

        internal Task<FirebaseAuthLink> SignInWithEmailAndPasswordAsync(string email, string password) =>
            _authProvider.SignInWithEmailAndPasswordAsync(email, password);

        internal Task<FirebaseAuthLink> SignInWithCustomToken(string token) =>
            _authProvider.SignInWithCustomTokenAsync(token);

        /// <summary>
        /// Asks firebase to create the user's account.
        /// The provided email's domain is checked against a list of disposable email addresses.
        /// If the domain is not in the list (or if GitHub is down) then account creation continues.
        /// Otherwise an exception is thrown.
        /// </summary>
        /// <returns></returns>
        internal async Task<FirebaseAuthLink> CreateAccount(string username, string email, string password)
        {
            // Client-side check for disposable email address.
            const string url =
                "https://raw.githubusercontent.com/martenson/disposable-email-domains/master/disposable_email_blocklist.conf";
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);

            var cancellationToken = new CancellationTokenSource(60000).Token;
            var isDomainBlacklisted = false;
            try
            {
                var response = await _http.SendAsync(requestMessage, cancellationToken);
                var msg = await response.Content.ReadAsStringAsync();

                // Turn msg into a hashset of all domains
                using var stringReader = new StringReader(msg);
                var lines = new List<string>();
                {
                    string line;

                    while ((line = stringReader.ReadLine()!) != null)
                    {
                        if (!string.IsNullOrWhiteSpace(line) && !line.TrimStart().StartsWith("//"))
                        {
                            lines.Add(line);
                        }
                    }
                }
                var blacklist = new HashSet<string>(lines, StringComparer.OrdinalIgnoreCase);

                var address = new System.Net.Mail.MailAddress(email);
                if (blacklist.Contains(address.Host))
                {
                    // Randomly wait before failing. Might frustrate users who try different disposable emails.
                    await Task.Delay(new Random().Next(3000, 12000), cancellationToken);
                    isDomainBlacklisted = true;
                }
            }
            catch (Exception e)
            {
                Console.Write("Error or timeout in check for email domain blacklist. Check has been skipped." +
                              e.Message);
            }

            if (isDomainBlacklisted)
            {
                throw new Exception("The email domain provided by the user is on our blacklist.");
            }

            return await _authProvider.CreateUserWithEmailAndPasswordAsync(email, password, username, true);
        }

        internal Task<User> GetUpdatedUser() => _authProvider.GetUserAsync(AuthLink);

        public async Task<string> GetCustomToken(RefreshToken refreshToken, string email)
        {
            var url = "https://api.unitystation.org/validatetoken?data=";

            HttpRequestMessage r = new HttpRequestMessage(HttpMethod.Get,
                url + Uri.EscapeUriString(JsonSerializer.Serialize(refreshToken)));

            CancellationToken cancellationToken = new CancellationTokenSource(120000).Token;

            HttpResponseMessage res;
            try
            {
                res = await _http.SendAsync(r, cancellationToken);
            }
            catch (Exception e)
            {
                Console.Write("Error: " + e.Message);
                return "";
            }

            string msg = await res.Content.ReadAsStringAsync();
            var response = JsonSerializer.Deserialize<ApiResponse>(msg,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (response.ErrorCode != 0)
            {
                Console.WriteLine("Error: " + response.ErrorMsg);
                return "";
            }

            return response.Message ?? "";
        }

        public async void SignOutUser()
        {
            if (AuthLink == null) return;

            if (Uid == null || CurrentRefreshToken == null) return;

            var token = new RefreshToken
            {
                UserId = Uid,
                Token = CurrentRefreshToken
            };

            var url = "https://api.unitystation.org/signout?data=";

            HttpRequestMessage r = new HttpRequestMessage(HttpMethod.Get,
                url + Uri.EscapeUriString(JsonSerializer.Serialize(token)));

            CancellationToken cancellationToken = new CancellationTokenSource(120000).Token;

            HttpResponseMessage res;
            try
            {
                res = await _http.SendAsync(r, cancellationToken);
            }
            catch (Exception e)
            {
                Console.Write("Error: " + e.Message);
                return;
            }

            string msg = await res.Content.ReadAsStringAsync();

            Console.WriteLine(msg);
            AuthLink = null;
        }
    }

    public class LoginMsg
    {
        public string Email { get; set; } = "";
        public string Pass { get; set; } = "";
    }

    [Serializable]
    public class RefreshToken
    {
        [JsonPropertyName("RefreshToken")] public string? Token { get; set; }
        public string? UserId { get; set; }
    }

    [Serializable]
    public class ApiResponse
    {
        /// <summary>
        /// 0 = all good, read the message variable now, otherwise read errorMsg
        /// </summary>
        public int ErrorCode { get; set; }

        public string? ErrorMsg { get; set; }
        public string? Message { get; set; }
    }
}