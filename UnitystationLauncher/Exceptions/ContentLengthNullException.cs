using System;

namespace UnitystationLauncher.Exceptions;

public class ContentLengthNullException : Exception
{
    public ContentLengthNullException(string url)
    {
        Url = url;
    }

    public ContentLengthNullException(string url, string message)
        : base(message)
    {
        Url = url;
    }

    public ContentLengthNullException(string url, string message, Exception inner)
        : base(message, inner)
    {
        Url = url;
    }

    public string Url { get; }

    public override String Message => $"{base.Message} Url='{Url}";
}
