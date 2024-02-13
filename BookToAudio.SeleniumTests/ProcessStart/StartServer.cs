using System.Diagnostics;

namespace BookToAudio.SeleniumTests.ProcessStart;
internal class StartServer
{
    private static Process? process;

    public static string WebApiDirrectory()
    {
        var webApiPath = Directory.GetCurrentDirectory();

        while (webApiPath != null && !Directory.Exists(Path.Combine(webApiPath, "BookToAudio")))
        {
            webApiPath = Directory.GetParent(webApiPath)?.FullName;
        }

        Assert.That(!string.IsNullOrEmpty(webApiPath), Is.True);
        webApiPath = Path.Combine(webApiPath!, "BookToAudio.Api");

        return webApiPath;
    }
    public static void StartWebAPI()
    {
        ProcessStartInfo startInfo = new ("dotnet")
        {
            Arguments = "run -c Debug --launch-profile https",
            UseShellExecute = false,
            WorkingDirectory = WebApiDirrectory(),
            CreateNoWindow = true,
        };

        process = new () { StartInfo = startInfo };
        process.Start();
    }

    public static void StopWebAPI()
    {
        if (process != null && !process.HasExited)
        {
            process.Kill(true);
            process.Dispose();
        }
    }
}