﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Threading;
using ProgramMessage;

namespace TemplateClient
{
    class BaseClient
    {
        public string IPAdress { get; private set; }
        public int Port { get; private set; }
        public StatusClient Status{ get; private set; }
        private TcpClient StreamWithServer;
        //для сериализации сообщение посланных серверу
        private BinaryFormatter formatter;
        private Thread ThreadOfHandlerMsg;
        //уведомляет о получении нового сообщения от сервера
        public event NewMessage EventNewMessage;
        public event EndSession EventEndSession;
        //конструктор
        public BaseClient(string ipAdress, int port)
        {
            IPAdress = ipAdress;
            Port = port;
            Status = StatusClient.Initialize;
        }
        //закрывает подлючение
        public void Close()
        {
            //отправляем серверу сообщение об окончании сессии
            byte[] EndMsg = CreateTitleMessage((byte)InsideTypesMessage.EndSession, 0);
            try
            {
                if (StreamWithServer.Connected)
                {
                    StreamWithServer.GetStream().Write(EndMsg, 0, EndMsg.Length);
                }
            }
            catch (Exception)
            {
                //пропускаем, если уже невозможно отправить сообщение
            }
            //закрываем соединение и освобождаем ресурсы
            HandlerEndSession();
         }
        //подключаемся к серверу
        public bool ConnectToServer()
        {
            if(StreamWithServer == null) StreamWithServer = new TcpClient();
            try
            {
                StreamWithServer.Connect(IPAdress, Port);
            }
            catch (Exception)
            {
                Status = StatusClient.FailConect;
                return false;
            }
            //обработка сообщений производитсва в отдельном потоке
            ThreadOfHandlerMsg = new Thread(StartReadMessage);
            ThreadOfHandlerMsg.Start();
            Status = StatusClient.Connect;
            formatter = new BinaryFormatter();
            return true;
        }
        //отправляет сообщение серверу
        public bool SendMessage(IMessage msg)
        {
            if (StreamWithServer.Connected)
            {
                byte[] BytesMsg = null;
                using (MemoryStream TempStream = new MemoryStream())
                {
                    formatter.Serialize(TempStream, msg);
                    BytesMsg = TempStream.ToArray();
                }
                //создаем и отправляем заголовк сообщения
                byte[] TitleMsg = CreateTitleMessage((byte)InsideTypesMessage.ProgramMessage, BytesMsg.Length);
                try
                {
                    StreamWithServer.GetStream().Write(TitleMsg, 0, TitleMsg.Length);
                    StreamWithServer.GetStream().Write(BytesMsg, 0, BytesMsg.Length);
                }
                catch (Exception)
                {
                    return false;
                }
                return true;
            }
            else return false;
        }
        //соединяет тип сообщения и его длинну в один массив
        private byte[] CreateTitleMessage(byte type, int Length)
        {
            //конвертируем длинну сообщения в байты
            byte[] BytesLenMsg = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(Length));
            //создаем заголовочное сообщение
            byte[] TitleMessage = new byte[BytesLenMsg.Length+1];
            TitleMessage[0] = type;
            for (int i = 0; i < BytesLenMsg.Length; i++)
            {
                TitleMessage[i + 1] = BytesLenMsg[i];
            }
            return TitleMessage;
        }
        //начинает обработку сообщений поступающих от сервера
        private void StartReadMessage()
        {
            NetworkStream StreamOfClient = StreamWithServer.GetStream();
            while (StreamWithServer.Connected && ThreadOfHandlerMsg.ThreadState == ThreadState.Running)
            {
                byte[] TitleMsg = new byte[5];
                //если пришло сообщение от сервера
                if (ReadData(TitleMsg, 5, StreamOfClient) > 0)
                {
                    //определяем тип сообщения
                    switch ((InsideTypesMessage)TitleMsg[0])
                    {
                        case InsideTypesMessage.ProgramMessage:
                            //вызываем функцию для обработки сообщения от пользователя
                            HandlerProgramMessage(IPAddress.NetworkToHostOrder(BitConverter.ToInt32(TitleMsg, 1)), StreamOfClient);
                            break;
                        case InsideTypesMessage.EndSession:
                            //вызов функции обрабатывающей завершения соединения
                            HandlerEndSession();
                            break;
                    }
                }
            }
        }
        //считывает некоторое количество байт из потока и записывает их в массив байт
        private int ReadData(byte[] data, int length, NetworkStream stream)
        {
            try
            {
                int ReadBytes = 0;
                    while (ReadBytes != length)
                    {
                        int readed = stream.Read(data, 0, length - ReadBytes);
                        ReadBytes += readed;
                        if (readed == 0) return 0;
                    }
                return ReadBytes;
            }           
            catch (Exception) 
            {
                EventEndSession();
                return 0; 
            }
        }
        //обрабатывает завершение соединение
        public void HandlerEndSession()
        {
            ThreadOfHandlerMsg.Abort();
            ThreadOfHandlerMsg = null;
            StreamWithServer.Close();
            Status = StatusClient.EndSession;
            //уведомляем о завершении соединения
            EventEndSession();
        }
        //обрабатывает программные сообщения
        public void HandlerProgramMessage(int length, NetworkStream stream)
        {
            //читаем сообщение от сервера
            byte[] Msg = new byte[length];
            //считываем сообщение
            ReadData(Msg, length, stream);
            //десериализуем сообщение
            IMessage ObjectMsg;
            using (MemoryStream MemStream = new MemoryStream())
            {
                MemStream.Write(Msg,0,Msg.Length);
                MemStream.Seek(0,SeekOrigin.Begin);
                ObjectMsg = (IMessage)formatter.Deserialize(MemStream);
            }
            //генерируем событие
            EventNewMessage(ObjectMsg);
        }
    }

    delegate void NewMessage(IMessage msg);
    delegate void EndSession();

    enum StatusClient
    {
        Initialize,
        Connect,
        FailConect,
        EndSession
    }

    enum InsideTypesMessage
    {
        ProgramMessage,
        EndSession
    }
}
