using BookToAudio.Core.Config;
using BookToAudio.Infra.Services.Common;
using System.Diagnostics;

namespace BookToAudio.SeleniumTests.ProcessStart;

internal sealed class ServerManager
{
    public static Process _process = null!;

    public static void StartServer()
    {
        string projectDirectory = PathService.GetProjectDirectory(SharedConstants.ServerProjectName);
        ProcessStartInfo startInfo = new("dotnet")
        {
            Arguments = "run -c Debug --launch-profile https",
            UseShellExecute = false,
            WorkingDirectory = projectDirectory,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        _process = new() { StartInfo = startInfo };
        _process.Start();

        // Capture and log the output and error.
        string output = _process.StandardOutput.ReadToEnd();
        string error = _process.StandardError.ReadToEnd();

        Console.WriteLine($"Output: {output}");
        if (!string.IsNullOrEmpty(error))
        {
            Console.WriteLine($"Error: {error}");
        }
    }

    public static void StartServer1()
    {
        ProcessStartInfo startInfo = new ("dotnet")
        {
            Arguments = "run --no-build --no-restore -c Debug --launch-profile https",
            UseShellExecute = false,
            WorkingDirectory = PathService.GetProjectDirectory(SharedConstants.ServerProjectName),
            CreateNoWindow = true,
        };

        _process = new () { StartInfo = startInfo };
        _process.Start();
    }
}