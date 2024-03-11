using System.Net.Sockets;
using System.Diagnostics;

namespace BookToAudio.SeleniumTests.ProcessStart;

internal sealed class ExtensionManager
{
    public static bool IsPortAvailable(string ip, int port)
    {
        const int tries = 90;
        TcpClient client = new ();

        for (int i = 0; i <= tries; i++)
        {
            try
            {
              client.Connect(ip, port);
            }
            catch (SocketException)
            {
                Thread.Sleep(500);
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

    public static void StopProcess(params Process[] process) 
    {
        foreach (var proc in process)
        {
            try
            {
                if (proc != null && !proc.HasExited)
                {
                    proc.Kill(true);
                    proc.Dispose();
                }
            }
            catch (InvalidOperationException)
            {
            }
        }
    }
}
