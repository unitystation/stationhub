
using System;

namespace UnitystationLauncher.Exceptions;

public sealed class UnsupportedMetadataException : Exception
{
    public UnsupportedMetadataException()
    {
    }

    public UnsupportedMetadataException(string message) : base(message)
    {
    }

    public UnsupportedMetadataException(string message, Exception inner) : base(message, inner)
    {
    }
}