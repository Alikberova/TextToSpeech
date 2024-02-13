using NUnit.Framework.Legacy;
using System.Diagnostics;

namespace BookToAudio.SeleniumTests.ProcessStart;

internal class StartServer
{
    private static Process? process;

    public static void StartWebAPI()
    {
        ProcessStartInfo startInfo = new ("dotnet")
        {
            Arguments = "run -c Debug --launch-profile https",
            UseShellExecute = false,
            WorkingDirectory = @"C:\Users\ukrbi\source\repos\BookToAudio\BookToAudio.Api",
            CreateNoWindow = true,
        };

        process = new () { StartInfo = startInfo };
        process.Start();

        ClassicAssert.IsTrue(CheckStartPort.CheckoutLocalPort("localhost", 7057), "Local port is not responding");
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