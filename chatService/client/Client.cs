using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ClientNS
{
    public class Client
    {
        static TcpClient client = new TcpClient();
        public static bool Start_client()
        {
            try
            {
                IPAddress ip = IPAddress.Parse("127.0.0.1");
                int port = 5000;
                client.Connect(ip, port);
                Console.WriteLine("client connected!!");
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return false;
            }
        }

        public static void Receive_data(TcpClient client)
        {
            NetworkStream ns = client.GetStream();
            Thread thread = new Thread(o => ReceiveData((TcpClient)o));

            thread.Start(client);

            string s;
            while (!string.IsNullOrEmpty((s = Console.ReadLine())))
            {
                byte[] buffer = Encoding.ASCII.GetBytes(s);
                ns.Write(buffer, 0, buffer.Length);
            }

            client.Client.Shutdown(SocketShutdown.Send);
            thread.Join();
            ns.Close();
            client.Close();
            Console.WriteLine("disconnect from server!!");
            Console.ReadKey();
        }

        public static void Main(string[] args)
        {
            var result = Start_client();

            if(result)
            {
                Receive_data(client);
            }
        }

        static void ReceiveData(TcpClient client)
        {
            NetworkStream ns = client.GetStream();
            byte[] receivedBytes = new byte[1024];
            int byte_count;

            while ((byte_count = ns.Read(receivedBytes, 0, receivedBytes.Length)) > 0)
            {
                Console.Write(Encoding.ASCII.GetString(receivedBytes, 0, byte_count));
            }
        }
    }
}

