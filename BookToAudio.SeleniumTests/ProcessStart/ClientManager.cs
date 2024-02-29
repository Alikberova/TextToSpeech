using System.Diagnostics;

namespace BookToAudio.SeleniumTests.ProcessStart;

internal sealed class ClientManager
{
    public static Process _process = null!;
    
    public static void StartClient()
    {
        ProcessStartInfo startInfo = new("ng")
        {
            Arguments = "serve",
            UseShellExecute = false,
            WorkingDirectory = ExtensionManager.GetProjectDirectory(ConstantsTests.ClientProjectName),
            CreateNoWindow = true,
        };

        _process = new() { StartInfo = startInfo };
        _process.Start();
    }
}
