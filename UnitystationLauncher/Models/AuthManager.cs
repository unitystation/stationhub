using System;
using System.IO;
using System.Threading.Tasks;
using Firebase.Auth;
using Newtonsoft.Json;
using Serilog;
using UnitystationLauncher.ViewModels;

namespace UnitystationLauncher.Models
{
    public class AuthManager
    {
        readonly FirebaseAuthProvider authProvider;
        private static AuthManager authManager;

        public static AuthManager Instance
        {
            get { return authManager; }
        }

        public AuthManager(FirebaseAuthProvider authProvider)
        {
            authManager = this;
            this.authProvider = authProvider;
            if (File.Exists("settings.json"))
            {
                var json = File.ReadAllText("settings.json");
                var authLink = JsonConvert.DeserializeObject<FirebaseAuthLink>(json);
                AuthLink = authLink;
            }
        }
        
        public FirebaseAuthLink? AuthLink { get; set; }

        public static string RefreshToken
        {
            get { return Instance.AuthLink.RefreshToken; }
        }

        public static string UID
        {
            get { return Instance.AuthLink.User.LocalId; }
        }

        public void Store()
        {
            var json = JsonConvert.SerializeObject(AuthLink);

            using (StreamWriter writer = System.IO.File.CreateText("settings.json"))
            {
                writer.WriteLine(json);
            }
        }

        internal Task<FirebaseAuthLink> SignInWithEmailAndPasswordAsync(string email, string password) =>
            authProvider.SignInWithEmailAndPasswordAsync(email, password);
    }
}