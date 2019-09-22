using System;
using System.Threading.Tasks;
using Firebase.Auth;

namespace UnitystationLauncher.Models{
    public class AuthManager
    {
        readonly FirebaseAuthProvider authProvider;
        public AuthManager(FirebaseAuthProvider authProvider)
        {
            this.authProvider = authProvider;
        }
        public FirebaseAuthLink? AuthLink { get; set; }

        internal Task<FirebaseAuthLink> SignInWithEmailAndPasswordAsync(string email, string password) => 
            authProvider.SignInWithEmailAndPasswordAsync(email, password);
    }
}