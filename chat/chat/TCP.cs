using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace chat
{
    class TCP
    {
        private const int bufLen = 64;
        private const string askHistory = "HISTORY";
        TcpListener tcpListener;
      
        public void StopReceive()
        {
            if (tcpListener != null) tcpListener.Stop();        
        }

        public void SendMessage(ref List<Client> clients, string Message)
        {
            byte[] MessageBytes = Encoding.ASCII.GetBytes(Message);
            foreach (Client client in clients)
            {
                client.Connection.GetStream().Write(MessageBytes, 0, MessageBytes.Length);
            }
        }

        public void ListenTCP(ref List<Client> clients, ref List<String> history, int port)//если отправитель tcp-пакета есть в списке - .., если нету - добавляем его в список 
        {
            tcpListener = new TcpListener(IPAddress.Any, port);
            tcpListener.Start();
            try
            {
                while (true)
                {
                    TcpClient client = tcpListener.AcceptTcpClient();
                    IPAddress SenderIpAdress = ((IPEndPoint)client.Client.RemoteEndPoint).Address;
                    Client Sender = clients.Find(x => x.IpAddr == SenderIpAdress);
                    if (Sender != null)
                    {
                        string SenderName = Sender.Name;
                    }
                    else
                    {
                        lock (Program.locker)
                        {
                            Client item = new Client(null, SenderIpAdress, client);
                            clients.Add(item);
                            Sender = item;
                        }
                    }
                    StartClientReceive(Sender, history, clients);//создается поток для клиента  GetTcpMessages(ref Sender, ref history, ref clients)
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                tcpListener.Stop();
            }
        }

        public void StartClientReceive(Client client, List<String> History, List<Client> clients)
        {
            Thread ClientThread = new Thread(() => { client.GetTcpMessages(ref History, ref clients); });
            ClientThread.IsBackground = true;
            ClientThread.Start();
        }

        public void SendHistoryRequest(Client client)
        {
            byte[] AskHistoryBytes = Encoding.ASCII.GetBytes(askHistory);
            client.Connection.GetStream().Write(AskHistoryBytes, 0, AskHistoryBytes.Length);
        }      
    }
}
