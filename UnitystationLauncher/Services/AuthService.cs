using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Mail;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using UnitystationLauncher.Constants;
using UnitystationLauncher.Models;
using UnitystationLauncher.Models.Api;
using UnitystationLauncher.Models.ConfigFile;
using UnitystationLauncher.Services.Interface;

namespace UnitystationLauncher.Services
{
    public class AuthService
    {
        private readonly HttpClient _http;
        public LoginMsg? LoginMsg { get; set; }
        public bool AttemptingAutoLogin { get; set; }
        private readonly IAuthProvider _IAuthProvider;
        private readonly IPreferencesService _preferencesService;

        public AuthService(HttpClient http, IAuthProvider IAuthProvider, IPreferencesService preferencesService)
        {
            _http = http;
            _IAuthProvider = IAuthProvider;
            _preferencesService = preferencesService;
            LoadAuthSettings();
        }


        private string AuthSettingsPath => Path.Combine(_preferencesService.GetPreferences().InstallationPath, "authSettings.json");

        public AccountLoginResponse? AccountLoginResponse { get; set; }


        private void LoadAuthSettings()
        {
            try
            {
                if (File.Exists(AuthSettingsPath))
                {
                    var json = File.ReadAllText(AuthSettingsPath);
                    var AccountLoginResponseA = JsonSerializer.Deserialize<AccountLoginResponse>(json);
                    AccountLoginResponse = AccountLoginResponseA;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                // Something went wrong reading the auth settings. Just ask the user to log in again.
                // The auth settings file will get overwritten after they do so we don't need to clean it up.
            }

        }

        public void SaveAuthSettings()
        {
            var json = JsonSerializer.Serialize(AccountLoginResponse);

            using (StreamWriter writer = File.CreateText(AuthSettingsPath))
            {
                writer.WriteLine(json);
            }
        }

        public async Task<CharacterTokenResponse> GenerateCharacterSheetTokenForFork(string Fork)
        {
            return (await _IAuthProvider.GenerateCharacterSheetTokenForFork(AccountLoginResponse.Token, Fork)).Data;
        }

        public void RegisterJoiningServerWithSecret(string SharedSecret)
        {
            _IAuthProvider.SendRegisterSharedSecret(AccountLoginResponse.Token, SharedSecret);
        }

        public void ResendVerificationEmail(string email)
        {
            _IAuthProvider.ResendEmailConfirmation(email);
        }

        public void SendForgotPasswordEmail(string email)
        {
            _IAuthProvider.SendForgotPasswordEmail(email);
        }

        internal Task<AccountLoginResponse> SignInWithEmailAndPasswordAsync(string email, string password)
        {
            return _IAuthProvider.SignInWithEmailAndPasswordAsync(email, password);
        }




        internal async Task<ApiResult<AccountRegisterResponse>> CreateAccountAsync(string userId, string username, string email, string password)
        {
            ApiResult<AccountRegisterResponse> registerResponse = await _IAuthProvider.Register(userId, email, username, password);

            if (registerResponse.IsSuccess == false)
            {
                throw new InvalidOperationException("Failed to register account");
            }

            return registerResponse;
        }



        public async Task<string> GetCustomTokenAsync(string refreshToken)
        {
            try
            {
                ApiResult<AccountLoginResponse> Response = await _IAuthProvider.Login(refreshToken);
                if (Response.IsSuccess == false)
                {
                    Log.Error("Error: {Error}", Response.Exception);
                    return "";
                }
                else
                {
                    AccountLoginResponse = Response.Data;
                    return Response.Data.Token;
                }
            }
            catch (Exception e)
            {
                Log.Error(e, "Failed when sending token validation request");
                return "";
            }

        }

        public async Task SignOutUserAsync()
        {
            await _IAuthProvider.Logout(AccountLoginResponse.Token);
            AccountLoginResponse = null;
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