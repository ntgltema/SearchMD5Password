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
        public long _startPos;
        public long _countPasswords;
        public string _alphabet;
        public string _message;
        public bool _inWork;
    }

    public class Agent
    {
        public List<Task> tasks;
        private string _id = null;
        //public Task task;
        private string _ip;
        public MessageQueue _queue;
        private int _core;
        private int _speed;

        private void SetIp()
        {
            string Host = System.Net.Dns.GetHostName();
            _ip = System.Net.Dns.GetHostByName(Host).AddressList[0].ToString();
        }
        private void SetCoreCount()
        {
            _core = Environment.ProcessorCount;
        }

        private void SendStatusMessage()
        {
            _queue.Send(_ip + " " + _speed + " " + _core, "ManagerStart");
        }

        public void SetPassPerSeconds()
        {
            //for (int i = 0; i < _core; i++)
            //{
            //    Task task = new Task();
            //    task._startPos = i * 250000;
            //    task._countPasswords = 250000;
            //    task._hash = new List<string>() { "aaa" };
            //    tasks.Add(task);
            //}
            //DateTime start = DateTime.Now;
            //Solver();
            //DateTime end = DateTime.Now;
            //_speed = (int)250000 * 1000 * _core / (int)(end.Subtract(start)).TotalMilliseconds;
            _speed = 100000;
        }

        public void SendGoodMessage(string myPassword, string myHash)
        {
            _queue.Send(myHash + " " + myPassword, "ManagerSOLVED");
        }

        public void DelHash(string myPassword, string myHash)
        {
            foreach(Task task in tasks)
            {
                foreach (string hash in task._hash)
                {
                    if (myHash == hash)
                        task._hash.Remove(myHash);
                }
                if (task._hash.Count == 0)
                {
                    _queue.Send(_ip + ";" + task._message, "Range");
                    tasks.Remove(task);
                }
            }    
    }

        public void Solver()
        {
            if (tasks.Count < _core)
            {
                if(CheckMessage())
                {
                    Thread thread = new Thread(this.CheckPasswords);
                    thread.Start();
                }
            }
            //if(task._hash.Count() > 0)
            //{
            //    List<Thread> therds = new List<Thread>();
            //    for (int i = 0; i < _core * 2; i++)
            //    {
            //        Thread thread = new Thread(this.func);
            //        therds.Add(thread);
            //        string num = (task.startPos + i * task.countPasswords / (_core * 2)) + " " + (task.countPasswords / (_core * 2));
            //        thread.Start(num);
            //    }
            //    foreach (Thread t in therds)
            //    {
            //        t.Join();
            //    }
            //    task._hash.Clear();
            //    if(0 < _speed)
            //    {
            //        _queue.Send(_ip + ";" + _message, "Range");
            //        SendStatusMessage();
            //    }
            //}
        }

        //void func(object num)//Функция потока, передаем параметр
        //{
        //    Task task = (Task)num;
        //    CheckPasswords(task);
        //}

        public void CheckPasswords()
        {
            Task task = tasks[0];
            foreach(Task FreeTask in tasks)
            {
                if (!FreeTask._inWork)
                    task = FreeTask;
            }

            for(long i = 0; i <= task._countPasswords && 0 < task._hash.Count(); i++)
            {
                int countOfChar = task._alphabet.Length;
                long numberOfPassword = task._startPos + i;
                string password = "";
                do
                {
                    password = task._alphabet[(int)(numberOfPassword % 128)] + password;
                    numberOfPassword = numberOfPassword / 128;
                }
                while (numberOfPassword != 0);
                string newHash = CalculateMD5Hash(password);
                for(int j = 0; j < task._hash.Count(); j++)
                    if (newHash == task._hash[j])
                    {
                        Console.WriteLine("Подобрали пароль к свертке: {0} : {1}", task._hash[j], password);
                        task._hash.Remove(newHash);
                        SendGoodMessage(password, task._hash[j]);
                        DelHash(password, task._hash[j]);
                        j--;
                    }
            }
            EndOfTask(task._message);
        }

        private void EndOfTask(string message)
        {
            foreach(Task task in tasks)
                if(task._message == message)
                {
                    _queue.Send(_ip + ";" + task._message, "Range");
                    tasks.Remove(task);
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
        public bool CheckMessage()
        {
            foreach (Message message in _queue)
            {
                if (message.Label == _ip)
                {
                    Console.WriteLine("Получили сообщение");
                    Console.WriteLine(message.Body.ToString());
                    SetParam(message.Body.ToString());
                    _queue.Send(message.Id, "ManagerDELETE");
                    return true;
                }
            }
            return false;
        }

        private void SetParam(string message)
        {
            Task task = new Task();
            task._message = message;
            string[] param = message.ToString().Split(';');
            string[] hash = param[0].ToString().Split(' ');
            string[] count = param[2].ToString().Split(' ');
            task._alphabet = param[1];
            task._startPos = Convert.ToInt64(count[0]);
            task._countPasswords = Convert.ToInt64(count[1]);
            task._hash.Clear();
            task._inWork = false;
            foreach (string data in hash)
                task._hash.Add(data);
            tasks.Add(task);
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
                }
            }
            SetCoreCount();
            SetIp();
            SetPassPerSeconds();
            tasks = new List<Task>();
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