using System;
using System.IO;
using System.Threading.Tasks;
using Firebase.Auth;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading;

namespace UnitystationLauncher.Models
{
    public class AuthManager
    {
        readonly FirebaseAuthProvider authProvider;
        private readonly HttpClient http;
        public LoginMsg LoginMsg { get; set; }
        public bool AttemptingAutoLogin { get; set; }

        public AuthManager(HttpClient http, FirebaseAuthProvider authProvider)
        {
            this.authProvider = authProvider;
            this.http = http;
            if (File.Exists("settings.json"))
            {
                var json = File.ReadAllText("settings.json");
                var authLink = JsonConvert.DeserializeObject<FirebaseAuthLink>(json);
                AuthLink = authLink;
            }
        }

        public FirebaseAuthLink? AuthLink { get; set; }
        public string CurrentRefreshToken
        {
            get { return AuthLink.RefreshToken; }
        }

        public string UID
        {
            get { return AuthLink.User.LocalId; }
        }

        public void Store()
        {
            var json = JsonConvert.SerializeObject(AuthLink);

            using (StreamWriter writer = System.IO.File.CreateText("settings.json"))
            {
                writer.WriteLine(json);
            }
        }

        public void ResendVerificationEmail()
        {
            authProvider.SendEmailVerificationAsync(AuthLink);
        }

        public void SendForgotPasswordEmail(string email)
        {
            authProvider.SendPasswordResetEmailAsync(email);
        }

        internal Task<FirebaseAuthLink> SignInWithEmailAndPasswordAsync(string email, string password) =>
            authProvider.SignInWithEmailAndPasswordAsync(email, password);

        internal Task<FirebaseAuthLink> SignInWithCustomToken(string token) =>
            authProvider.SignInWithCustomTokenAsync(token);

        /// <summary>
        /// Asks firebase to create the user's account.
        /// TODO The provided email's domain is checked against a list of disposable email addresses.
        /// TODO If the domain is not in the list (or if GitHub is down) then account creation continues.
        /// TODO Otherwise an exception is thrown.
        /// </summary>
        /// <returns></returns>
        internal async Task<FirebaseAuthLink> CreateAccount(string username, string email, string password)
        {
            // Client-side check for disposable email address.
            var url = "https://raw.githubusercontent.com/martenson/disposable-email-domains/master/disposable_email_blocklist.conf";
            HttpRequestMessage r = new HttpRequestMessage(HttpMethod.Get, url);

            CancellationToken cancellationToken = new CancellationTokenSource(60000).Token;

            HttpResponseMessage res;
            try
            {
                res = await http.SendAsync(r, cancellationToken);
            }
            catch (Exception e)
            {
                Console.Write("List of disposable emails: Timeout. " + e.Message);
            }

            //TODO The list of disposable addresses is successfully retrieved, but 

            return await authProvider.CreateUserWithEmailAndPasswordAsync(email, password, username, true);
        }

        internal Task<User> GetUpdatedUser() => authProvider.GetUserAsync(AuthLink);

        public async Task<string> GetCustomToken(RefreshToken refreshToken, string email)
        {
            var url = "https://api.unitystation.org/validatetoken?data=";

            HttpRequestMessage r = new HttpRequestMessage(HttpMethod.Get, url + Uri.EscapeUriString(JsonConvert.SerializeObject(refreshToken)));

            CancellationToken cancellationToken = new CancellationTokenSource(120000).Token;

            HttpResponseMessage res;
            try
            {
                res = await http.SendAsync(r, cancellationToken);
            }
            catch (Exception e)
            {
                Console.Write("Error: " + e.Message);
                return "";
            }

            string msg = await res.Content.ReadAsStringAsync();
            var response = JsonConvert.DeserializeObject<ApiResponse>(msg);

            if (response.errorCode != 0)
            {
                Console.WriteLine("Error: " + response.errorMsg);
                return "";
            }

            return response.message;
        }

        public async void SignOutUser()
        {
            if (AuthLink == null) return;

            var token = new RefreshToken
            {
                userID = UID,
                refreshToken = CurrentRefreshToken
            };

            var url = "https://api.unitystation.org/signout?data=";

            HttpRequestMessage r = new HttpRequestMessage(HttpMethod.Get, url + Uri.EscapeUriString(JsonConvert.SerializeObject(token)));

            CancellationToken cancellationToken = new CancellationTokenSource(120000).Token;

            HttpResponseMessage res;
            try
            {
                res = await http.SendAsync(r, cancellationToken);
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
        public string Email { get; set; }
        public string Pass { get; set; }
    }

    [Serializable]
    public class RefreshToken
    {
        public string refreshToken;
        public string userID;
    }

    [Serializable]
    public class ApiResponse
    {
        public int errorCode = 0; //0 = all good, read the message variable now, otherwise read errorMsg
        public string errorMsg;
        public string message;
    }
}