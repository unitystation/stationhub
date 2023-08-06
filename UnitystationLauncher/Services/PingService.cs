using System;
using System.Diagnostics;
using System.IO;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Serilog;
using UnitystationLauncher.Models.Api;
using UnitystationLauncher.Models.Enums;
using UnitystationLauncher.Services.Interface;

namespace UnitystationLauncher.Services;

public class PingService : IPingService
{
    private readonly IEnvironmentService _environmentService;

    public PingService(IEnvironmentService environmentService)
    {
        _environmentService = environmentService;
    }

    public async Task<string> GetPing(Server server)
    {
        if (server is { HasValidDomainName: false, HasValidIpAddress: false })
        {
            Log.Error("Error: {Error}", $"Server '{server.ServerName}' has an invalid ip address, skipping ping...");
            return "Bad IP";
        }

        if (_environmentService.GetCurrentEnvironment() == CurrentEnvironment.LinuxFlatpak)
        {
            return await FlatpakGetPingTime(server);
        }

        try
        {
            using Ping ping = new();
            PingReply reply = await ping.SendPingAsync(server.ServerIp, 1000);

            return $"{reply.RoundtripTime}ms";
        }
        catch (ArgumentException e)
        {
            Log.Error("Error: {Error}", $"Invalid IP address when trying to ping server: {e.Message}");
            return "Error";
        }
    }

    // Ping does not work in the Flatpak sandbox so we have to reconstruct its functionality in that case.
    // Surprisingly, this is basically what that does. Looks for your system's ping tool and parses its output.
    private async Task<string> FlatpakGetPingTime(Server server)
    {
        using Process pingSender = new()
        {
            StartInfo = new()
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                FileName = "ping",
                Arguments = $"{server.ServerIp} -c 1"
            }
        };

        pingSender.Start();
        await pingSender.WaitForExitAsync();

        StreamReader reader = pingSender.StandardOutput;
        string pingRawOutput = await reader.ReadToEndAsync();
        Match matchedPingOutput = new Regex(@"time=(.*?)\ ").Match(pingRawOutput);
        string pingOut = matchedPingOutput.Groups[1].ToString();
        return $"{pingOut}ms";
    }
}