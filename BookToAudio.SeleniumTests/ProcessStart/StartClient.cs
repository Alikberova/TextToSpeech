using System.Diagnostics;

namespace BookToAudio.SeleniumTests.ProcessStart;

internal class StartClient
{
    private static Process? process;

    public static string AngularDirrectory()
    {
        var angularPath = Directory.GetCurrentDirectory();

        while (angularPath != null && !Directory.Exists(Path.Combine(angularPath, "BookToAudio")))
        {
            angularPath = Directory.GetParent(angularPath)?.FullName;
        }

        Assert.That(!string.IsNullOrEmpty(angularPath), Is.True);
        angularPath = Path.Combine(angularPath!, "BookToAudio.Web");

        return angularPath;
    }
    public static void StartAngularApp()
    {
        ProcessStartInfo startInfo = new("cmd.exe")
        {
            Arguments = "/c ng serve",
            UseShellExecute = false,
            WorkingDirectory = AngularDirrectory(),
            CreateNoWindow = true,
        };
        process = new() { StartInfo = startInfo };
        process.Start();
    }
   
    public static void StopAngularApp()
    {
        if (process != null && !process.HasExited)
        {
            process.Kill(true);
            process.Dispose();
        }
    }
}