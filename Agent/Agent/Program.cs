using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Messaging;
using System.IO;
using System.Security.Cryptography;
using System.Threading;

namespace Agent
{   
    public struct Task
    {
        public List<string> _hash;
        public long startPos;
        public long countPasswords;
    }

    public class Agent
    {
        private string _id = null;
        public Task task;
        private string _ip;
        public MessageQueue _queue;
        private int _core;
        private int _speed;
        private string _message;

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
            _queue.Send(_ip + " " + _speed + " " + _core, "ManagerStart");
        }

        public void SetPassPerSeconds()
        {
            task.startPos = (long)Math.Pow(128, 6) / 2;
            task.countPasswords = 1000000;
            task._hash = new List<string>() { "aaa" };
            DateTime start = DateTime.Now;
            Solver();
            DateTime end = DateTime.Now;
            _speed = (int)task.countPasswords * 1000 / (int)(end.Subtract(start)).TotalMilliseconds;
        }
        public void SendGoodMessage(string password, string hash)
        {
            _queue.Send(hash + " " + password, "ManagerSOLVED");
            task._hash.Remove(hash);
        }

        public void Solver()
        {
            if(task._hash.Count() > 0)
            {
                List<Thread> therds = new List<Thread>();
                for (int i = 0; i < _core * 2; i++)
                {
                    Thread thread = new Thread(this.func);
                    therds.Add(thread);
                    string num = (task.startPos + i * task.countPasswords / (_core * 2)) + " " + (task.countPasswords / (_core * 2));
                    thread.Start(num);
                }
                foreach (Thread t in therds)
                {
                    t.Join();
                }
                task._hash.Clear();
                if(0 < _speed)
                {
                    _queue.Send(_ip + ";" + _message, "Range");
                    SendStatusMessage();
                }
            }
        }

        void func(object num)//Функция потока, передаем параметр
        {
            string[] param = num.ToString().Split(' ');
            CheckPasswords(Convert.ToInt64(param[0]), Convert.ToInt64(param[1]));
        }

        public void CheckPasswords(long startPos, long countPasswords)
        {
           long countAllPasswords = 4432676798592;
            if (countAllPasswords < startPos + countPasswords)
                countPasswords = countAllPasswords - startPos;
            for(long i = 0; i <= countPasswords && 0 < task._hash.Count(); i++)
            {
                long numberOfPassword = startPos + i;
                string password = "";
                do
                {
                    password = alphabet[numberOfPassword % 128] + password;
                    numberOfPassword = numberOfPassword / 128;
                }
                while (numberOfPassword != 0);
                string newHash = CalculateMD5Hash(password);
                for(int j = 0; j < task._hash.Count(); j++)
                    if (newHash == task._hash[j])
                    {
                        Console.WriteLine("Подобрали пароль к свертке: {0} : {1}", task._hash[j], password);
                        SendGoodMessage(password, task._hash[j]);
                        j--;
                    }
            }
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
        public void CheckMessage()
        {
            foreach (Message message in _queue)
            {
                if (message.Label == _ip)
                {
                    Console.WriteLine("Получили сообщение");
                    Console.WriteLine(message.Body.ToString());
                    SetParam(message.Body.ToString());
                    _id = message.Id;
                }
            }
            if(_id != null)
            {
                _queue.Send(_id, "ManagerDELETE");
                _id = null;
            }
        }

        private void SetParam(string message)
        {
            _message = message;
            string[] param = message.ToString().Split(';');
            string[] hash = param[0].ToString().Split(' ');
            string[] count = param[1].ToString().Split(' ');
            task.startPos = Convert.ToInt64(count[0]);
            task.countPasswords = Convert.ToInt64(count[1]);
            task._hash.Clear();
            foreach (string data in hash)
                task._hash.Add(data);
        }

        public Agent()
        {
            string path = File.ReadAllText("settings.ini");
            _queue = new MessageQueue(path);//нет никаких проверок, надо потом подумать
            foreach(Message message in _queue)
            {
                if(message.Label == "Start")
                {
                    Console.WriteLine("Нашли стартовое сообщение");
                    //_id = message.Id;
                }
            }
            SetCoreCount();
            SetIp();
            SetPassPerSeconds();
            Console.WriteLine("Подключились к очереди: {0}", path);
            Console.WriteLine("Скорость: {0} паролей в секунду", _speed);
            Console.WriteLine("Доступно ядер: {0}", _core);
            Console.WriteLine("ip: {0}", _ip);
            SendStatusMessage();
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            Agent agent = new Agent();
            while(true)
            {
                agent.CheckMessage();
                agent.Solver();
            }
            //DateTime start = DateTime.Now;
            //agent.Solver();
           // DateTime end = DateTime.Now;
            //int speed = 1000000 * 1000 / (int)(end.Subtract(start)).TotalMilliseconds;
            //Console.WriteLine("Скорость: {0} паролей в секунду", speed);
        }
    }
}