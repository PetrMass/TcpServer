using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;


namespace TCPServer
{
    public class ServerObject
    {
        TcpListener tcpListener;
        internal List<ClientWorker> clients = new List<ClientWorker>(); // все подключения

        internal void AddConnection(ClientWorker clientObject)
        {
            clients.Add(clientObject);
        }
        protected internal void RemoveConnection(string id) // передаелать
        {
            for (int i = 0; i < clients.Count; i++)
            {
                if (clients[i].Id == id)
                {
                    Console.WriteLine("клиент {0} удален", clients[i].userName);
                    clients.Remove(clients[i]);                    
                }
            }
        }
        // прослушивание входящих подключений
        protected internal void Listen()
        {
            try
            {
                tcpListener = new TcpListener(IPAddress.Parse("127.0.0.1"), 8888);
                tcpListener.Start();
                Console.WriteLine("Сервер запущен. Ожидание подключений...");

                while (true)
                {
                    TcpClient tcpClient = tcpListener.AcceptTcpClient();
                    ClientWorker clientObject = new ClientWorker(tcpClient, this);                    
                    Thread clientThread = new Thread(clientObject.Process);
                    clientThread.Start();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Disconnect();
            }
        }

        // трансляция сообщения клиентам
        protected internal void BroadcastMessage(string message, string id, string sendId)
        {
            byte[] data = Encoding.Unicode.GetBytes(message);
            if (sendId == "all")
            {
                for (int i = 0; i < clients.Count; i++)
                {
                    if (clients[i].Id != id & clients[i].userName != null) // если id клиента не равно id отправляющего
                    {                     
                        clients[i].Stream.Write(data, 0, data.Length);
                    }
                }
            }
            else if (sendId == "client") 
            {
                for (int i = 0; i < clients.Count; i++)
                {
                    if (clients[i].Id == id)
                    {                     
                        clients[i].Stream.Write(data, 0, data.Length);
                    }
                }
            }
            else
            {
                for (int i = 0; i < clients.Count; i++)
                {
                    if (clients[i].userName == sendId) // если имя клиента равно имени отправляющего
                    {
                        clients[i].Stream.Write(data, 0, data.Length); //передача данных
                    }
                }
            }
        }
        // отключение всех клиентов
        protected internal void Disconnect()
        {
            tcpListener.Stop(); //остановка сервера

            for (int i = 0; i < clients.Count; i++)
            {
                clients[i].Close(); //отключение клиента
                Console.WriteLine("отключение клиентов");
            }            
        }
    }
}
