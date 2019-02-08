using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;

namespace TemplateServer
{
    class Server
    {
        public string IPAdress { get; private set; }
        public int Port { get; private set; }
        public List<ServerClient> ConnectedClients { get; private set; }

        private TcpListener ServerPoint;
        private Thread ThreadCheckNewClient;
        //конструктор
        public Server(string ipAdress, int port)
        {
            IPAdress = ipAdress;
            Port = port;
            ConnectedClients = new List<ServerClient>();
        }

        //запускает сервер
        public bool StartServer()
        {
            try
            {
                ServerPoint = new TcpListener(IPAddress.Parse(IPAdress), Port);
                ServerPoint.Start();
            }
            catch (Exception)
            {
                return false;
            }
            //запускаем поток обработки подлючений
            ThreadCheckNewClient = new Thread(CheckerNewClient);
            ThreadCheckNewClient.Start();
            return true;
        }
        private void CheckerNewClient()
        {
            while (ThreadCheckNewClient.ThreadState == ThreadState.Running)
            {
                if (ServerPoint.Pending())
                {
                    //добавляем нового клиента
                    ServerClient Client = new ServerClient(ServerPoint.AcceptTcpClient());
                    Client.EventEndSession += HandlerEndSessionClient;
                    ConnectedClients.Add(Client); 
                }
                else Thread.Sleep(500);
            }
        }
       
        private void HandlerEndSessionClient(ServerClient client)
        {
            ConnectedClients.Remove(client);
            client = null;
        }
    }
}
