using System;
using System.Threading.Tasks;
using UnitystationLauncher.Models;

namespace UnitystationLauncher.Services;

public interface IAuthProvider
{
    public Task<AccountLoginResponse> SignInWithEmailAndPasswordAsync(string email, string password);
}

public class OfficialCentralCommandAuthentication : IAuthProvider
{		
    
    public static string Host => "INhere";
    public static UriBuilder UriBuilder = new("https", Host);
    public static Uri GetUri(string endpoint, string queries = null)
    {

        UriBuilder.Path = $"/accounts/{endpoint}";

        if (string.IsNullOrEmpty(queries) == false)
        {
            UriBuilder.Query = queries;
        }

        return UriBuilder.Uri;
    }
    
    public static async Task<ApiResult<AccountLoginResponse>> Login(string emailAddress, string password)
    {
        AccountLoginCredentials requestBody = new()
        {
            Email = emailAddress,
            Password = password,
        };

        ApiResult<AccountLoginResponse> response = await ApiServer.Post<AccountLoginResponse>(GetUri("login-credentials"), requestBody);

        if (!response.IsSuccess)
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
}