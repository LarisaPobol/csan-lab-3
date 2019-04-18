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
    class Program
    {
        const int TcpPort = 49000;
        const int UdpPort = 49001;
        public static object locker = new object();
        static string Login;

        static void Main(string[] args)
        {
            List<Client> Clients = new List<Client>();
            List<String> History = new List<String>();
            UDP Udp = new UDP();
            TCP Tcp = new TCP();
            bool continueChat = true;
            string Message;
            Thread UdpListenThread = null;
            Thread TcpListenThread = null;
            try
            {
                Login = GetLoginFromUser();
                Udp.ConnectToChat(Login, UdpPort, ref History);
                UdpListenThread = new Thread(() => { Udp.Receive(ref Clients, ref History, Login, TcpPort, UdpPort); });
                UdpListenThread.Start();
                TcpListenThread = new Thread(() => { Tcp.ListenTCP(ref Clients, ref History, TcpPort); });
                TcpListenThread.Start();
                Thread.Sleep(1000);
                if (Clients.Count != 0)
                {
                    Tcp.SendHistoryRequest(Clients[0]); //запрос истории у первого клиента из списка
                }
                while (continueChat)
                {
                    Message = Console.ReadLine();
                    if (Message != "exit")
                    {
                        Tcp.SendMessage(ref Clients, Message);//отправление сообщения
                        History.Add(Login + ": " + Message + "\n");
                    }
                    else
                    {
                        TcpListenThread.IsBackground = true;
                        UdpListenThread.IsBackground = true;
                        Console.WriteLine("You sucsesfully left this chat");
                        continueChat = false;
                    }
                }
            }
            catch
            {
                Console.WriteLine("Error! Restart your program please");
            }
            Console.ReadKey();
        }

        static string GetLoginFromUser()
        {
            Console.WriteLine("Enter your login please");
            string UserName = Console.ReadLine();
            return UserName;
        }
    }
}
