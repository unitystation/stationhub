using System;
using System.IO;
using System.Threading.Tasks;
using Firebase.Auth;
using Newtonsoft.Json;

namespace UnitystationLauncher.Models
{
    public class AuthManager
    {
        readonly FirebaseAuthProvider authProvider;
        public LoginMsg LoginMsg { get; set; }
        public bool AttemptingAutoLogin { get; set; }
        
        public AuthManager(FirebaseAuthProvider authProvider)
        {
            this.authProvider = authProvider;
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

        internal Task<FirebaseAuthLink> CreateAccount(string username, string email, string password) =>
            authProvider.CreateUserWithEmailAndPasswordAsync(email, password, username, true);

        internal Task<User> GetUpdatedUser() => authProvider.GetUserAsync(AuthLink);
    }

    public class LoginMsg
    {
        public string Email { get; set; }
        public string Pass { get; set; }
    }
}