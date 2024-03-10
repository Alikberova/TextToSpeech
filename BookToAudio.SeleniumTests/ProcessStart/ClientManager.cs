using System.Diagnostics;

namespace BookToAudio.SeleniumTests.ProcessStart;

internal sealed class ClientManager
{
    public static Process _process = null!;
    
    public static void StartClient()
    {
        var isWindows = Environment.OSVersion.ToString().Contains("Windows"); //Microsoft Windows NT 10.0.19045.0
        var fileName = isWindows ? "cmd" : "ng";
        var arguments = $"serve --port {Constants.ClientPort}";

        ProcessStartInfo startInfo = new(fileName)
        {
            Arguments = isWindows ? "/c ng " + arguments : arguments,
            UseShellExecute = false,
            WorkingDirectory = ExtensionManager.GetProjectDirectory(Constants.ClientProjectName),
            CreateNoWindow = true,
        };

        _process = new() { StartInfo = startInfo };
        _process.Start();
    }
}
