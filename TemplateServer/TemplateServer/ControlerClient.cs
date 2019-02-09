using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProgramMessage;

namespace TemplateServer
{
    class ControlerClient:IControler
    {
        private ServerClient client;
        public ControlerClient(ServerClient client)
        {
            this.client = client;
        }
        public ServerClient Client
        {
            get {return client; }
            set {client = value; }
        }
        public void HanlderNewMessage(IMessage msg)
        {
            switch (msg.TypeMessage)
            {
                case TypesProgramMessage.TextMessage:
                    TextMessage TextMsg = new TextMessage((msg as TextMessage).Text+" Обработано сервером");
                    Client.SendMessage(TextMsg);
                    break;
            }
        }
        public IControler GetNewControler(ServerClient client)
        {
            return new ControlerClient(client);
        }
    }
}
