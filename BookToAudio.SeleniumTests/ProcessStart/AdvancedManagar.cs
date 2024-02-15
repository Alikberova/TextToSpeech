using System.Net.Sockets;
using System.Diagnostics;

namespace BookToAudio.SeleniumTests.ProcessStart;

internal sealed class AdvancedManagar
{
    public static bool PortChecker(string ip, int port)
    {
        const int tries = 120;
        TcpClient client = new TcpClient();

        for (int i = 0; i <= tries; i++)
        {
            try
            {
              client.Connect(ip, port);
            }
            catch (Exception)
            {
                Thread.Sleep(1000);
            }
            if (client.Connected)
            {
                client.Close();
                return true;
            }
        }
        client.Close();
        return false;
    }

    public static string GetRootDirectory(string nameDirectory)
    {
        var angularPath = Directory.GetCurrentDirectory();
        
        while (angularPath != null && !Directory.Exists(Path.Combine(angularPath, "BookToAudio")))
        {
            angularPath = Directory.GetParent(angularPath)?.FullName;
        }
        angularPath = Path.Combine(angularPath!, nameDirectory);
        
        return angularPath;
    }

    public static void ProcessIsStopped(params Process[] process) 
    {
        foreach (var proc in process)
        {
            if (proc != null && !proc.HasExited)
            {
                proc.Kill(true);
                proc.Dispose();
            }
        }
    }
}
