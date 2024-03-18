using BookToAudio.Core.Config;
using System.Diagnostics;

namespace BookToAudio.SeleniumTests.ProcessStart;

internal sealed class ServerManager
{
    public static Process _process = null!;
    
    public static void StartServer()
    {
        ProcessStartInfo startInfo = new ("dotnet")
        {
            Arguments = "run --no-restore -c Debug --launch-profile https",
            UseShellExecute = false,
            WorkingDirectory = ExtensionManager.GetProjectDirectory(SharedConstants.ServerProjectName),
            CreateNoWindow = true,
        };

        _process = new () { StartInfo = startInfo };
        _process.Start();
    }
}