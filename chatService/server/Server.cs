using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ServerNS
{
    public class Server
    {
        public class StateObject
        {
            public int id = 1;

            public TcpClient client = null;

            public long last_received_message_time = 0;

            public bool warning = false;

            public string ToString()
            {
                return "id: " + this.id.ToString() + "last_received_message_time: " + this.last_received_message_time.ToString();
            }

        }
        static int count = 1;
        static List<StateObject> list_clients = new List<StateObject>();
        static TcpListener ServerSocket = new TcpListener(IPAddress.Any, 5000);

        public static bool Start_server()
        {
            try
            {
                ServerSocket.Start();
                return true;

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return false;
            }

        }


        public static void Accept_client(TcpListener ServerSocket)
        {
            while (true)
            {
                TcpClient client = ServerSocket.AcceptTcpClient();
                StateObject state = new StateObject();
                state.id = count;
                state.client = client;
                state.last_received_message_time = (long)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                state.warning = false;
                list_clients.Add(state);
                Console.WriteLine("Someone connected!!");

                Thread t = new Thread(() => handle_clients(state));
                t.Start();
                count++;
            }
        }

        public static void handle_clients(StateObject state)
        {
            while (true)
            {
                NetworkStream stream = state.client.GetStream();
                byte[] buffer = new byte[1024];
                int byte_count = stream.Read(buffer, 0, buffer.Length);

                if (byte_count == 0)
                {
                    break;
                }

                string data = Encoding.ASCII.GetString(buffer, 0, byte_count);
                broadcast(data, state);
                Console.WriteLine(data);
            }

            list_clients.Remove(state);
            state.client.Client.Shutdown(SocketShutdown.Both);
            state.client.Close();
        }

        public static void broadcast(string data, StateObject state)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(state.ToString() + ": " + data + Environment.NewLine);

            if ((long)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - state.last_received_message_time < 1000)
            {
                NetworkStream stream = state.client.GetStream();
                state.last_received_message_time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                if (state.warning)
                {
                    stream.Write(Encoding.ASCII.GetBytes("second warning unfortunately your connection is being closed" + Environment.NewLine), 0, Encoding.ASCII.GetBytes("second warning unfortunately your connection is being closed" + Environment.NewLine).Length);
                    list_clients.Remove(state);
                    state.client.Client.Shutdown(SocketShutdown.Both);
                    state.client.Close();
                }
                else
                {
                    state.warning = true;
                    stream.Write(Encoding.ASCII.GetBytes("first and last warning please send one message per second" + Environment.NewLine), 0, Encoding.ASCII.GetBytes("first and last warning please send one message per second" + Environment.NewLine).Length);
                }
            }
            else
            {
                foreach (StateObject c in list_clients)
                {
                    if (c.id != state.id)
                    {
                        NetworkStream stream = c.client.GetStream();
                        stream.Write(buffer, 0, buffer.Length);
                    }
                    else
                    {
                        c.last_received_message_time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    }
                }
            }
        }

        public static void Main(string[] args)
        {
            var result = Start_server();

            if(result)
            {
                Accept_client(ServerSocket);
            }
        }
    }
}
