using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json.Nodes;
using Newtonsoft.Json;

namespace UnitystationLauncher.Models;

public abstract class JsonObject
{
    public virtual string ToJson()
    {
        return JsonConvert.SerializeObject(this);
    }

    public virtual StringContent ToStringContent()
    {
        return new StringContent(ToJson(), Encoding.UTF8, "application/json");
    }
}


[Serializable]
public class AccountRegister : JsonObject
{
    [JsonProperty("unique_identifier")]
    public string UniqueIdentifier { get; set; }

    [JsonProperty("email")]
    public string Email { get; set; }

    [JsonProperty("username")]
    public string Username { get; set; }

    [JsonProperty("password")]
    public string Password { get; set; }
}


[Serializable]
public class ForgotPasswordModel : JsonObject
{
    [JsonProperty("email")]
    public string Email { get; set; }
}


[Serializable]
public class AccountRegisterResponse : JsonObject
{
    [JsonProperty("account")]
    public AccountRegisterDetails Account { get; set; }
}

[Serializable]
public class AccountRegisterDetails : JsonObject
{
    [JsonProperty("unique_identifier")]
    public string UniqueIdentifier { get; set; }

    [JsonProperty("email")]
    public string Email { get; set; }

    [JsonProperty("username")]
    public string Username { get; set; }
}

[Serializable]
public class AccountLoginResponse : JsonObject
{
    [JsonProperty("token")]
    public string Token { get; set; }

    [JsonProperty("account")]
    public AccountGetResponse Account { get; set; }
}

[Serializable]
public class AccountGetResponse : JsonObject
{
    [JsonProperty("unique_identifier")]
    public string UniqueIdentifier { get; set; }

    [JsonProperty("username")]
    public string Username { get; set; }

    [JsonProperty("is_verified")]
    public bool IsVerified { get; set; }
}

[Serializable]
public class AccountLoginToken : JsonObject, ITokenAuthable
{
    public string Token { get; set; }
}

[Serializable]
public class AccountLogout : JsonObject, ITokenAuthable
{
    public string Token { get; set; }
}

[Serializable]
public class AccountResendEmailConfirmationRequest : JsonObject
{
    [JsonProperty("email")]
    public string Email { get; set; }
}

[Serializable]
public class AccountLoginCredentials : JsonObject
{
    [JsonProperty("email")]
    public string Email { get; set; }

    [JsonProperty("password")]
    public string Password { get; set; }
}

public class ApiResult<T> : JsonObject where T : JsonObject
{
    public HttpStatusCode StatusCode { get; set; }
    public T Data { get; set; }
    public ApiHttpException Exception { get; set; }

    public bool IsSuccess => Exception == null;

    private ApiResult(HttpStatusCode statusCode, T data, ApiHttpException exception = null)
    {
        StatusCode = statusCode;
        Data = data;
        Exception = exception;
    }

    public static ApiResult<T> Success(HttpStatusCode statusCode, T data) => new(statusCode, data);
    public static ApiResult<T> Failure(HttpStatusCode statusCode, T data, ApiHttpException exception) => new(statusCode, data, exception);
}

/// <summary>
/// Error class for any HTTP-related errors as returned by the API server.
/// </summary>
public class ApiHttpException : Exception
{
    public HttpStatusCode StatusCode { get; private set; }

    public ApiHttpException(string message, HttpStatusCode code) : base(message)
    {
        StatusCode = code;
    }
}

/// <summary>
/// Marks an API request as having or requiring an authentication token.
/// </summary>
public interface ITokenAuthable
{
    string Token { get; }
}

/// <summary>
/// Error class for any usage-specific API errors as returned by the API server.
/// </summary>
public class ApiRequestException : ApiHttpException
{
    /// <summary>A list of all error messages returned by the API server.</summary>
    /// <remarks>You can use <c>Message</c> to get the first one.</remarks>
    public List<string> Messages { get; set; }

    public ApiRequestException(string message, HttpStatusCode statusCode) : base(message, statusCode)
    {
        Messages = new List<string>();
    }
}

