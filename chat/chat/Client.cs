using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace chat
{
    class Client
    {
        public string Name;
        public IPAddress IpAddr;
        public TcpClient Connection;
        private const int bufLen = 64;
        private const string askHistory = "HISTORY";

        public Client(string name, IPAddress ipAddr, TcpClient connection)
        {
            Name = name;
            IpAddr = ipAddr;
            Connection = connection;
        }

        public void GetTcpMessages(ref List<String> History, ref List<Client> clients)
        {
            NetworkStream ClientStream = this.Connection.GetStream();
            try
            {
                while (true)
                {
                    byte[] byteMessage = new byte[bufLen];
                    StringBuilder MessageBuilder = new StringBuilder();
                    string AllMessage;
                    int RecvBytes = 0;
                    do
                    {
                        RecvBytes = ClientStream.Read(byteMessage, 0, byteMessage.Length);
                        MessageBuilder.Append(Encoding.UTF8.GetString(byteMessage, 0, RecvBytes));
                    }
                    while (ClientStream.DataAvailable);
                    AllMessage = MessageBuilder.ToString();
                    if (AllMessage != askHistory)
                    {
                        if (this.Name == null)
                        {
                            this.Name = AllMessage;
                        }
                        else
                        {
                            Console.WriteLine(this.Name + ": " + AllMessage + " " + DateTime.Now);
                            History.Add(this.Name + ": " + AllMessage + " " + DateTime.Now + "\n");
                        }
                    }
                    else
                    {
                        SendHistoryReplay(ref History, this);
                    }
                }
            }
            catch
            {
                Console.WriteLine(this.Name + " disconnected from this chat " + DateTime.Now);
                History.Add(this.Name + " disconnected from this chat " + DateTime.Now + "\n");
                var address = ((IPEndPoint)this.Connection.Client.RemoteEndPoint).Address;
                lock (Program.locker)
                {
                    clients.RemoveAll(X => X.IpAddr.ToString() == address.ToString());
                }
            }
            finally
            {
                StopClientConnection(ClientStream, this.Connection);
            }
        }

        private void StopClientConnection(NetworkStream stream, TcpClient connection)
        {
            if (stream != null)
                stream.Close();
            if (connection != null)
                connection.Close();
        }

        public void SendHistoryReplay(ref List<string> History, Client client)
        {
            byte[] HistoryItemBytes;
            foreach (string HistoryItem in History)
            {
                HistoryItemBytes = Encoding.ASCII.GetBytes(HistoryItem);
                client.Connection.GetStream().Write(HistoryItemBytes, 0, HistoryItemBytes.Length);
            }           
        }

    }
}
