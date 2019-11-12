using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Firebase.Auth;

namespace UnitystationLauncher.Models{
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
        }
        public FirebaseAuthLink? AuthLink { get; set; }

        public static string RefreshToken
        {
            get { return Instance.AuthLink.RefreshToken; }
        }
        
        public static string UID {
            get { return Instance.AuthLink.User.LocalId; }
        }

        internal Task<FirebaseAuthLink> SignInWithEmailAndPasswordAsync(string email, string password) => 
            authProvider.SignInWithEmailAndPasswordAsync(email, password);
    }
}