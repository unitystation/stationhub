using System;
using System.IO;
using System.IO.Pipes;
using System.Reactive.Concurrency;
using System.Threading.Tasks;
using MsBox.Avalonia.Base;
using ReactiveUI;
using Serilog;
using UnitystationLauncher.Infrastructure;
using UnitystationLauncher.Models.Enums;

namespace UnitystationLauncher.GameCommunicationPipe;

public class PipeHubBuildCommunication : IDisposable
{
    private NamedPipeServerStream _serverPipe;
    private StreamReader? _reader;
    private StreamWriter? _writer;

    public PipeHubBuildCommunication()
    {
        _serverPipe = new("Unitystation_Hub_Build_Communication", PipeDirection.InOut, 1,
            PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
    }

    public async Task StartServerPipe()
    {
        await _serverPipe.WaitForConnectionAsync();
        _reader = new(_serverPipe);
        _writer = new(_serverPipe);

        while (true)
        {
            string? request = await _reader.ReadLineAsync();
            if (request == null)
            {
                try
                {
                    await _serverPipe.WaitForConnectionAsync();
                }
                catch (IOException e)
                {
                    Log.Error(e.ToString());
                    _serverPipe.Close();
                    _serverPipe = new("Unitystation_Hub_Build_Communication", PipeDirection.InOut,
                        1,
                        PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
                    await _serverPipe.WaitForConnectionAsync();
                }

                _reader = new(_serverPipe);
                _writer = new(_serverPipe);
                continue;
            }

            string[] requests = request.Split(",");
            Log.Information($"Server: Received request: {request}");

            if (ClientRequest.URL.ToString() == requests[0])
            {
                RxApp.MainThreadScheduler.ScheduleAsync(async (_, _) =>
                {
                    IMsBox<string> msgBox = MessageBoxBuilder.CreateMessageBox(
                        MessageBoxButtons.YesNo,
                        string.Empty,
                        $"would you like to add this Domain to The allowed domains to be opened In your browser, {requests[1]} " +
                        @"
Justification given by the Fork : " + requests[2]);

                    string response = await msgBox.ShowAsync();
                    Log.Information($"response {response}");
                    await _writer.WriteLineAsync(response == "No" ? false.ToString() : true.ToString());
                    await _writer.FlushAsync();
                    return Task.CompletedTask;
                });
            }
            else if (ClientRequest.API_URL.ToString() == requests[0])
            {
                RxApp.MainThreadScheduler.ScheduleAsync(async (_, _) =>
                {
                    IMsBox<string> msgBox = MessageBoxBuilder.CreateMessageBox(
                        MessageBoxButtons.YesNo,
                        string.Empty,
                        $"The build would like to send an API request to, {requests[1]} " + @"
do you allow this fork to now on access this domain
Justification given by the Fork : " + requests[2]);


                    string response = await msgBox.ShowAsync();
                    Log.Information($"response {response}");
                    await _writer.WriteLineAsync(response == "No" ? false.ToString() : true.ToString());
                    await _writer.FlushAsync();
                    return Task.CompletedTask;
                });
            }
            else if (ClientRequest.Host_Trust_Mode.ToString() == requests[0])
            {
                RxApp.MainThreadScheduler.ScheduleAsync(async (_, _) =>
                {
                    IMsBox<string> msgBox = MessageBoxBuilder.CreateMessageBox(
                        MessageBoxButtons.YesNo,
                        string.Empty,
                        @" Trusted mode automatically allows every API and open URL action to happen without prompt, this also enables the 
Variable viewer ( Application that can modify the games Data ) that Could potentially be used to Perform malicious actions on your PC,
  The main purpose of this Prompt is to allow the Variable viewer (Variable editing), 
What follows is given by the build, we do not control what is written in the Following text So treat with caution and use your brain
  Justification : " + requests[1]); //TODO Add text

                    string response = await msgBox.ShowAsync();
                    Log.Information($"response {response}");
                    await _writer.WriteLineAsync(response == "No" ? false.ToString() : true.ToString());
                    await _writer.FlushAsync();
                    return Task.CompletedTask;
                });
            }
        }
    }

    public void Dispose()
    {
        _serverPipe.Dispose();
        _reader?.Dispose();
        _writer?.Dispose();

        GC.SuppressFinalize(this);
    }
}