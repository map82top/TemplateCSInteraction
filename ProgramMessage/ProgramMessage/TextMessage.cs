using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProgramMessage
{
    [Serializable]
    public class TextMessage : IMessage
    {
        public string Text { get; private set; }
        public TypesProgramMessage TypeMessage { get; private set; }
        public TextMessage(string text)
        {
            Text = text;
            TypeMessage = TypesProgramMessage.TextMessage;
        }
    }
}
