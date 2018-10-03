using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Messaging;
using System.IO;
using System.Security.Cryptography;

namespace Agent
{   
    public class Agent
    {
        private string _ip;
        private MessageQueue _queue;
        private int _core;
        private int _speed;
        private string _password;

        char[] alphabet = { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z',
                            'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z',
                            'а', 'б', 'в', 'г', 'д', 'е', 'ё', 'ж', 'з', 'и', 'й', 'к', 'л', 'м', 'н', 'о', 'п', 'р', 'с', 'т', 'у', 'ф', 'х', 'ц', 'ч', 'ш', 'щ', 'ъ', 'ь', 'ы', 'э', 'ю', 'я',
                            'А', 'Б', 'В', 'Г', 'Д', 'Е', 'Ё', 'Ж', 'З', 'И', 'Й', 'К', 'Л', 'М', 'Н', 'О', 'П', 'Р', 'С', 'Т', 'У', 'Ф', 'Х', 'Ц', 'Ч', 'Ш', 'Щ', 'Ъ', 'Ь', 'Ы', 'Э', 'Ю', 'Я',
                            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
        private void SetIp()
        {
            string Host = System.Net.Dns.GetHostName();
            _ip = System.Net.Dns.GetHostByName(Host).AddressList[0].ToString();
        }
        private void SetCoreCount()
        {
            _core = Environment.ProcessorCount / 2;            
        }

        private void SendStatusMessage()
        {
            _queue.Send(_ip + " " + _speed + " " + _core, _ip);//"Status");
        }

        private void SetPassPerSeconds()
        {
            long startPos = (long)Math.Pow(128, 6) / 2;
            //long startPos = 0;
            long countPasswords = 1000000;
            DateTime start = DateTime.Now;
            if (CheckPasswords("0CC175B9C0F1B6A831C399E269772661", startPos, countPasswords))
                Console.WriteLine("Кажется мы нашли коллизию");
            DateTime end = DateTime.Now;
            _speed = (int)countPasswords * 1000 / (int)(end.Subtract(start)).TotalMilliseconds;
        }

        public bool CheckPasswords(string hash, long startPos, long countPasswords)
        {
            //long countAllPasswords = (long)Math.Pow(128, 1) + (long)Math.Pow(128, 2) + (long)Math.Pow(128, 3) + (long)Math.Pow(128, 4) + (long)Math.Pow(128, 5) + (long)Math.Pow(128, 6);
            long countAllPasswords = 4432676798592;
            if (countAllPasswords < startPos + countPasswords)
                countPasswords = countAllPasswords - startPos;
            for(long i = 0; i <= countPasswords; i++)
            {
                long numberOfPassword = startPos + i;
                string password = "";
                do
                {
                    password = alphabet[numberOfPassword % 128] + password;
                    numberOfPassword = numberOfPassword / 128;
                }
                while (numberOfPassword != 0);
                if (CalculateMD5Hash(password) == hash)
                {
                    _password = password;
                    return true;
                }
            }
            return false;
        }

        public string CalculateMD5Hash(string password)
        {

            MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(password);
            byte[] hash = md5.ComputeHash(inputBytes);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }

            return sb.ToString();
        }

        public Agent()
        {
            string path = File.ReadAllText("settings.ini");

            /*if (MessageQueue.Exists(path))
            {
                var queue = new MessageQueue(path);
            }
            else
            {
               Console.WriteLine("Очередь не существует");
            }*/
            _queue = new MessageQueue(path);//нет никаких проверок, надо потом подумать
            SetCoreCount();
            SetPassPerSeconds();
            SetIp();
            SendStatusMessage();
            Console.WriteLine("Подключились к очереди: {0}", path);
            Console.WriteLine("Скорость: {0} паролей в секунду", _speed);
            Console.WriteLine("Доступно ядер: {0}", _core);
            Console.WriteLine("ip: {0}", _ip);

            foreach (Message message in _queue)
                if(message.Label == _ip)
                    Console.WriteLine(message.Body);
        }
    }
    public class Queue
    {
        public Queue()
        {
            if (MessageQueue.Exists(@".\private$\MyNewPrivateQueueTest1"))
            {
                MessageQueue.Delete(@".\private$\MyNewPrivateQueueTest1");
            }
            using (var queue = MessageQueue.Create(".\\private$\\MyNewPrivateQueueTest1"))
            {
                queue.Label = "Demo Queue";
                Console.WriteLine("Очередь создана:");
                Console.WriteLine("Путь: {0}", queue.Path);
                Console.WriteLine("Форматное имя: {0}", queue.FormatName);
                Console.WriteLine("Проверка: {0}", queue.Id);
                //queue.SetPermissions(".", MessageQueueAccessRights.FullControl);
                queue.SetPermissions("Все", MessageQueueAccessRights.FullControl);
            }
            Console.ReadLine();
        }
        public void OpenQueue()
        {
            if (MessageQueue.Exists(@".\private$\MyNewPrivateQueueTest1"))
            {
                var queue = new MessageQueue(@".\private$\MyNewPrivateQueueTest1");
            }
            else
            {
                Console.WriteLine("Очередь не существует");
            }
        }
        ~Queue()
        {
            // MessageQueue.Delete(".\\private$\\MyNewPrivateQueue");
            Console.ReadLine();
        }
        public void GetQueue()
        {
            foreach (var queue in MessageQueue.GetPrivateQueuesByMachine(Environment.MachineName))
                Console.WriteLine("Очередь: {0}\n", queue.Path);

            Console.ReadLine();
        }
        public void Send()
        {
            try
            {
                if (!MessageQueue.Exists(@".\private$\MyNewPrivateQueueTest1"))
                {
                    MessageQueue.Create(@".\private$\MyNewPrivateQueueTest1");
                }

                var queue = new MessageQueue(@".\private$\MyNewPrivateQueueTest1");
                queue.Send("Sample Message", "Label");
                Console.WriteLine("qwe");
            }
            catch (MessageQueueException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            Agent agent = new Agent();
            //Queue queue = new Queue();
            //queue.GetQueue();
            //queue.OpenQueue();
            //queue.Send();
            /*MessageQueue queue1 = new MessageQueue(@"FormatName:DIRECT=OS:ostan\private$\mynewprivatequeuetest1");
            queue1.Send("Sample Message", "Label");
            Console.WriteLine("qwe"); */
        }
    }
}