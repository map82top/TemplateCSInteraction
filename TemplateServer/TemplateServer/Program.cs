using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TemplateServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Server ObjectServer = new Server("127.0.0.1",1500);
            if (ObjectServer.StartServer())
            {
                Console.WriteLine("Сервер запущен");
            }
            else Console.WriteLine("Ошибка запуска сервера");
        }
    }
}
