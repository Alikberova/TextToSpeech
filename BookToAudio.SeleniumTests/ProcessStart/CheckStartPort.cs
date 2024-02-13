using System.Net.Sockets;

namespace BookToAudio.SeleniumTests.ProcessStart;

internal class CheckStartPort
{
    public static bool CheckoutLocalPort(string ip, int port)
    {
        int waitTime = 120;
        TcpClient client = new TcpClient();

        for (int i = 0; i <= waitTime; i++)
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
}
