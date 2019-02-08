using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProgramMessage
{
    public interface IMessage
    {
        TypesProgramMessage TypeMessage { get; }
    }

    public enum TypesProgramMessage
    {
        TextMessage
    }
}
