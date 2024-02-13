using NUnit.Framework.Legacy;
using System.Diagnostics;

namespace BookToAudio.SeleniumTests.ProcessStart;

internal class StartClient
{
    private static Process? process;

    public static void StartAngularApp()
    {
        ProcessStartInfo startInfo = new("cmd.exe")
        {
            Arguments = "/c ng serve",
            UseShellExecute = false,
            WorkingDirectory = @"C:\Users\ukrbi\source\repos\BookToAudio\BookToAudio.Web",
            CreateNoWindow = true,
        };

        process = new() { StartInfo = startInfo };
        process.Start();

        ClassicAssert.IsTrue(CheckStartPort.CheckoutLocalPort("localhost", 4200), "Local port is not responding");
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