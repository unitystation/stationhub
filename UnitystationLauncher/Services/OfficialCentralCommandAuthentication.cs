using System;
using System.Threading.Tasks;
using UnitystationLauncher.Constants;
using UnitystationLauncher.Models;
using UnitystationLauncher.Models.Api;

namespace UnitystationLauncher.Services;

public interface IAuthProvider
{
    public Task<AccountLoginResponse> SignInWithEmailAndPasswordAsync(string emailAddress, string password);
    public Task<ApiResult<AccountLoginResponse>> Login(string token);

    public Task<JsonObject> Logout(string token, bool destroyAllSessions = false);

    public Task<ApiResult<AccountRegisterResponse>> Register(
        string uniqueIdentifier, string emailAddress, string username, string password);

    public Task<ApiResult<JsonObject>> ResendEmailConfirmation(string email);

    public Task<ApiResult<JsonObject>> SendForgotPasswordEmail(string email);

    public Task<ApiResult<JsonObject>> SendRegisterSharedSecret(string token, string SharedSecret);

    public Task<ApiResult<CharacterTokenResponse>> GenerateCharacterSheetTokenForFork(string token, string ForkName);
}

public class OfficialCentralCommandAuthentication : IAuthProvider
{

    public static string Host => ApiUrls.ApiBaseUrlLogin;
    private static UriBuilder UriBuilder = new(Host);
    public static Uri GetUri(string endpoint, string? queries = null, string BeginningOverride = null)
    {

        UriBuilder.Path = $"/accounts/{endpoint}";

        if (string.IsNullOrEmpty(BeginningOverride) == false)
        {
            UriBuilder.Path = BeginningOverride + $"{endpoint}";

        }

        if (string.IsNullOrEmpty(queries) == false)
        {
            UriBuilder.Query = queries;
        }

        return UriBuilder.Uri;
    }

    public async Task<ApiResult<AccountLoginResponse>> Login(string token)
    {
        AccountLoginToken requestBody = new()
        {
            Token = token,
        };

        ApiResult<AccountLoginResponse> response = await ApiServer.Post<AccountLoginResponse>(GetUri("login-token"), requestBody);

        if (response.IsSuccess == false)
        {
            throw response.Exception!;
        }

        return response;
    }

    public static async Task<ApiResult<AccountLoginResponse>> Login(string emailAddress, string password)
    {
        AccountLoginCredentials requestBody = new()
        {
            Email = emailAddress,
            Password = password,
        };

        ApiResult<AccountLoginResponse> response = await ApiServer.Post<AccountLoginResponse>(GetUri("login-credentials"), requestBody);

        if (response.IsSuccess == false)
        {
            throw response.Exception!;
        }

        return response;
    }

    public async Task<AccountLoginResponse> SignInWithEmailAndPasswordAsync(string emailAddress, string password)
    {
        ApiResult<AccountLoginResponse> loginResponse = await Login(emailAddress, password);

        AccountLoginResponse account = loginResponse.Data;

        return account;
    }
    public async Task<JsonObject> Logout(string token, bool destroyAllSessions = false) // TODO: but no response?
    {
        AccountLogout requestBody = new()
        {
            Token = token,
        };

        var response = await ApiServer.Post<JsonObject>(GetUri(destroyAllSessions ? "logoutall" : "logout"), requestBody);

        return response;
    }

    public async Task<ApiResult<JsonObject>> ResendEmailConfirmation(string email)
    {
        AccountResendEmailConfirmationRequest requestBody = new()
        {
            Email = email,
        };

        var response = await ApiServer.Post<JsonObject>(GetUri("resend-account-confirmation"), requestBody);
        return response;
    }

    public async Task<ApiResult<AccountRegisterResponse>> Register(
        string uniqueIdentifier, string emailAddress, string username, string password)
    {
        var requestBody = new AccountRegister
        {
            Email = emailAddress,
            UniqueIdentifier = uniqueIdentifier,
            Username = username,
            Password = password,
        };

        var response = await ApiServer.Post<AccountRegisterResponse>(GetUri("register"), requestBody);

        if (response.IsSuccess == false)
        {
            throw response.Exception!;
        }

        return response;
    }


    public async Task<ApiResult<JsonObject>> SendForgotPasswordEmail(string email)
    {
        try
        {
            var requestBody = new ForgotPasswordModel
            {
                Email = email,
            };

            var response = await ApiServer.Post<JsonObject>(GetUri("reset-password/"), requestBody);

            if (response.IsSuccess == false)
            {
                throw response.Exception!;
            }

            return response;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<ApiResult<JsonObject>> SendRegisterSharedSecret(string token, string SharedSecret)
    {
        try
        {
            var requestBody = new Registersha512token
            {
                sha512_token = SharedSecret,
            };

            var response = await ApiServer.Post<JsonObject>(GetUri("register-SHA512-for-account/"), requestBody, token);

            if (response.IsSuccess == false)
            {
                throw response.Exception!;
            }

            return response;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<ApiResult<CharacterTokenResponse>> GenerateCharacterSheetTokenForFork(string token, string ForkName)
    {
        try
        {
            var requestBody = new GetCharacterForkToken
            {
                fork_compatibility = ForkName,
            };

            var response = await ApiServer.Post<CharacterTokenResponse>(GetUri("GenForkToken", BeginningOverride: "/persistence/characters/"), requestBody, token);

            if (response.IsSuccess == false)
            {
                throw response.Exception!;
            }

            return response;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}