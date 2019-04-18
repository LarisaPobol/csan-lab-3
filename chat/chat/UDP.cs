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
    class UDP
    {
        private UdpClient UdpSender;
        private readonly IPAddress IpAdressBroadcast = IPAddress.Parse("192.168.43.255");//IPAddress.Broadcast;
        private IPEndPoint ipEndPointBroadcast;
        private UdpClient UdpListener = null;

        public void StopReceive()
        {
            if (UdpListener != null) UdpListener.Close();
        }

        public void ConnectToChat(string login, int port, ref List<string> history)//отправление udp-пакета
        {
            int sendedData;
            byte[] LoginBytes;
            try
            {
                UdpSender = new UdpClient(port, AddressFamily.InterNetwork);
                ipEndPointBroadcast = new IPEndPoint(IpAdressBroadcast, port);
                LoginBytes = Encoding.ASCII.GetBytes(login);
                sendedData = UdpSender.Send(LoginBytes, LoginBytes.Length, ipEndPointBroadcast);
                if (sendedData == LoginBytes.Length)
                {
                    Console.WriteLine("You are connected to the chat " + DateTime.Now);
                    history.Add(login + " connected to the chat " + DateTime.Now + "\n");
                }
                UdpSender.Close();
            }
            catch
            {
                throw;
            }
        }

        public void Receive(ref List<Client> clients, ref List<String> history, string login, int TcpPort, int UdpPort)//если пришел udp-пакет, добавляем в отправителя в список, отправляем ему tcp-пакет  с своим логином
        {
            byte[] LoginBytes;
            TcpClient NewTcp;
            string NewUserName;
            UdpListener = new UdpClient();
            try
            {
                IPEndPoint ClientEndPoint = new IPEndPoint(IPAddress.Any, UdpPort);
                UdpListener.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                UdpListener.ExclusiveAddressUse = false;
                UdpListener.Client.Bind(ClientEndPoint);
                while (true)
                {
                    Byte[] data = UdpListener.Receive(ref ClientEndPoint);
                    NewUserName = Encoding.ASCII.GetString(data);
                    clients.Add(new Client(NewUserName, ClientEndPoint.Address, null));
                    NewTcp = new TcpClient();
                    NewTcp.Connect(new IPEndPoint(ClientEndPoint.Address, TcpPort));
                    clients[clients.Count - 1].Connection = NewTcp;
                    Console.WriteLine(NewUserName + " connected to this chat " + DateTime.Now);
                    history.Add(NewUserName + " connected to this chat " + DateTime.Now + "\n");
                    StartClientReceive(clients[clients.Count - 1], history, clients);
                    LoginBytes = Encoding.ASCII.GetBytes(login);
                    NewTcp.GetStream().Write(LoginBytes, 0, LoginBytes.Length);
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                UdpListener.Close();
            }
        }

        public void StartClientReceive(Client client, List<String> History, List<Client> clients)
        {
            Thread ClientThread = new Thread(() => { client.GetTcpMessages(ref History, ref clients); });
            ClientThread.Start();
        }

    }
}
