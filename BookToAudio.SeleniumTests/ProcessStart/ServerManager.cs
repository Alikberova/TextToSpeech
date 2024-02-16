using System.Diagnostics;

namespace BookToAudio.SeleniumTests.ProcessStart;

internal sealed class ServerManager
{
    public static Process process;
    
    public static void StartServer()
    {
        ProcessStartInfo startInfo = new ("dotnet")
        {
            Arguments = "run -c Debug --launch-profile https",
            UseShellExecute = false,
            WorkingDirectory = ExtensionManager.GetRootDirectory(ConstantsTests.DirectoryServer),
            CreateNoWindow = true,
        };
        process = new () { StartInfo = startInfo };
        process.Start();
    }
}