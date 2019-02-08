using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProgramMessage;

namespace TemplateServer
{
    class ControlerClient
    {
        private ServerClient Client;
        public ControlerClient(ServerClient client)
        {
            Client = client;
            Client.EventNewMessage += HanlderNewMessage;
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
    }
}
