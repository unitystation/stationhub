using UnitystationLauncher.Exceptions;

namespace UnitystationLauncher.ContentScanning;

internal sealed class SandboxError
{
    public string Message;

    public SandboxError(string message)
    {
        Message = message;
    }

    public SandboxError(UnsupportedMetadataException ume) : this($"Unsupported metadata: {ume.Message}")
    {
    }
}