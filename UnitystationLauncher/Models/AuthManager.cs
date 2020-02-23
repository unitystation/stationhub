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
            Console.WriteLine("Expires in: " + AuthLink.ExpiresIn);
            Console.WriteLine("Refresh token: " + AuthLink.RefreshToken);

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

        internal Task<FirebaseAuthLink> CreateAccount(string username, string email, string password) =>
            authProvider.CreateUserWithEmailAndPasswordAsync(email, password, username, true);

        internal Task<User> GetUpdatedUser() => authProvider.GetUserAsync(AuthLink);

        public async Task<bool> GetCustomToken(RefreshToken refreshToken)
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
                return false;
            }

            string msg = await res.Content.ReadAsStringAsync();
            var response = JsonConvert.DeserializeObject<ApiResponse>(msg);
            
            if(response.errorCode != 0)
            {
                Console.WriteLine("Error: " + response.errorMsg);
                return false;
            }

            return true;
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