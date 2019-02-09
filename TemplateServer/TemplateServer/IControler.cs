using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProgramMessage;

namespace TemplateServer
{
    interface IControler
    {
        void HanlderNewMessage(IMessage msg);
        IControler GetNewControler(ServerClient client);
    }
}
