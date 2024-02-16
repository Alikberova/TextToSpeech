using System.Diagnostics;

namespace BookToAudio.SeleniumTests.ProcessStart;

internal sealed class ClientManager
{
    public static Process process;
    
    public static void StartClient()
    {
        ProcessStartInfo startInfo = new("cmd.exe")
        {
            Arguments = "/c ng serve",
            UseShellExecute = false,
            WorkingDirectory = ExtensionManager.GetRootDirectory(ConstantsTests.DirectoryClient),
            CreateNoWindow = true,
        };
        process = new() { StartInfo = startInfo };
        process.Start();
    }
}
