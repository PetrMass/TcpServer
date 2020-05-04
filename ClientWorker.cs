using System;
using System.Linq;
using System.Net.Sockets;
using System.Text;


namespace TCPServer
{

    public class ClientWorker
    {
        protected internal string Id { get; private set; }
        protected internal string senderName = "all"; // задает кому отправлять сообщения
        protected internal NetworkStream Stream { get; private set; }
        protected internal string userName;
        TcpClient client;
        ServerObject server;

        public ClientWorker(TcpClient tcpClient, ServerObject serverObject)
        {
            Id = Guid.NewGuid().ToString();
            client = tcpClient;
            server = serverObject;
            serverObject.AddConnection(this);
        }

        public void Process()
        {
            try
            {
                Stream = client.GetStream();
                userName = CheckName();

                string message = userName + " вошел в чат"; // посылаем сообщение о входе в чат всем подключенным пользователям
                server.BroadcastMessage(message, this.Id, this.senderName);
                Console.WriteLine(message);
                
                while (true) // в бесконечном цикле получаем сообщения от клиента
                {
                    try
                    { 
                        message = GetMessage();
                        if (ChangeSenderName(ref message) & message != null)// поверяем,введена ли команда на изменение поля senderName. 
                        {
                            senderName = message.Substring(7); // извлекает все начиная с 7 символа
                        }
                        else if (message != null)
                        {
                            Console.WriteLine("{0}(to {1}): {2}", userName, senderName, message);
                            message = String.Format("{0}: {1}", userName, message);
                            server.BroadcastMessage(message, this.Id, this.senderName);
                        }

                    }
                    catch
                    {
                        message = String.Format("{0}: покинул чат", userName);
                        Console.WriteLine(message);
                        server.BroadcastMessage(message, this.Id, "all");
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                // в случае выхода из цикла отключаем клиента
                server.RemoveConnection(this.Id);
                Close();
            }
        }

        // чтение входящего сообщения и преобразование в строку
        private string GetMessage()
        {
            byte[] data = new byte[64];
            StringBuilder builder = new StringBuilder();
            int bytes = 0;
            do
            {
                bytes = Stream.Read(data, 0, data.Length);
                builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
            }
            while (Stream.DataAvailable);

            return builder.ToString();
        }

        
        protected internal void Close()
        {
            if (Stream != null)
                Stream.Close();
            if (client != null)
                client.Close();
            Console.WriteLine("соединение закрыто");
        }

        private bool ChangeSenderName(ref string message) // Проверка введенного имени получателя
        {
            try
            {
                string result = new string(message.Take(6).ToArray());// проверяем, что первые 6 символов это команда sendto
                                                                      
                if (result == "sendto")
                {
                    string name = message.Substring(7);
                    bool a = false;
                    for (int i = 0; i < server.clients.Count; i++)
                    {
                        if (server.clients[i].userName == name)
                        {
                            a = true;
                            return true;                           
                        }
                    }
                    if (a == false)
                    {
                        message = String.Format("{0} нет в чате", name);
                        server.BroadcastMessage(message, this.Id, "client");
                        message = null;
                    }
                }
            }
            catch
            {
                message = ("команда введена неверно");
                server.BroadcastMessage(message, this.Id, "client");
                Console.WriteLine(message);
                message = null;
            }
            return false;
        }

        private string CheckName() // проверка имени отправителя
        {
            bool a;
            string message = "Введите имя";
            server.BroadcastMessage(message, this.Id, "client");
            do
            {
                message = GetMessage();
                a = false;
                for (int i = 0; i < server.clients.Count; i++)
                {
                    if (server.clients[i].userName == message) // если имя клиента уже есть в базе
                    {
                        a = true;
                        message = "Вы ввели неуникальное имя, повтрите ввод";
                        Console.WriteLine("Попытка ввода неуникального имени");
                        server.BroadcastMessage(message, this.Id, "client");
                    }
                }
            } while (a);
            return message;
        }

    }
}
