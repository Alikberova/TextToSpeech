using BookToAudio.Core.Config;
using System.Diagnostics;

namespace BookToAudio.SeleniumTests.ProcessStart;

internal sealed class ClientManager
{
    public static Process _process = null!;
    
    public static void StartClient()
    {
        // todo if thhis method is not used, remove reference to TestingInfra
        ProcessStartInfo startInfo = new("ng")
        {
            Arguments = "serve",
            UseShellExecute = false,
            WorkingDirectory = ExtensionManager.GetProjectDirectory(SharedConstants.ClientProjectName),
            CreateNoWindow = true,
        };

        _process = new() { StartInfo = startInfo };
        _process.Start();
    }
}
