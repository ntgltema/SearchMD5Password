using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Messaging;
using System.Security.Cryptography;
using System.Text;
using System.Threading;


namespace Agent
{
    public struct Task
    {
        public string _hash;
        public long _startPos;
        public long _countPasswords;
        public string _alphabet;
        public string _message;
        public bool _isTask;
    }

    public class Agent
    {
        public MessageQueue _queue;
        public string _ip;
        public int _core;
        public List<Task> tasks;
        public int _speed;

        public Agent()
        {
            string path;
            try
            {
                path = File.ReadAllText("settings.ini");
            }
            catch(FileNotFoundException)
            {
                Console.WriteLine("File settings.ini not found. Place the file in the program folder and restart the agent.");
                path = "";
                Environment.Exit(-1);
            }

            _queue = new MessageQueue(path);

            try
            {
                foreach (Message message in _queue)
                {
                    if (message.Label == "Start")
                    {
                        Console.WriteLine("Нашли стартовое сообщение");
                    }
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Problem connecting to queue. Check the parameters and restart the agent.");
                Environment.Exit(-1);
            }
            

            SetIp();
            SetCoreCount();
            tasks = new List<Task>();
            SetPassPerSeconds();

            Console.WriteLine("Подключились к очереди: {0}", path);
            Console.WriteLine("Скорость: {0} паролей в секунду", _speed);
            Console.WriteLine("Доступно ядер: {0}", _core);
            Console.WriteLine("ip: {0}", _ip);

            SendStatusMessage();
        }

        private void SetIp()
        {
            string Host = System.Net.Dns.GetHostName();
#pragma warning disable CS0618 // Тип или член устарел
            _ip = System.Net.Dns.GetHostByName(Host).AddressList[0].ToString();
#pragma warning restore CS0618 // Тип или член устарел
        }

        private void SetCoreCount()
        {
            _core = Environment.ProcessorCount;
        }

        public void SetPassPerSeconds()
        {
            List<Thread> threads = new List<Thread>();
            DateTime start = DateTime.Now;

            for (int i = 0; i < _core; i++)
            {
                Task task = new Task
                {
                    _startPos = i * 2500000,
                    _countPasswords = 2500000,
                    _alphabet = "qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM1234567890",
                    _hash = "aaa",
                    _isTask = false
                };

                tasks.Add(task);

                Thread thread = new Thread(this.CheckPasswords);
                threads.Add(thread);

                thread.Start();
            }

            foreach (Thread thread in threads)
                thread.Join();

            DateTime end = DateTime.Now;

            _speed = (int)2500000 * _core / (int)(end.Subtract(start)).TotalMilliseconds * 1000;
        }

        private void SendStatusMessage()
        {
            _queue.Send(_ip + " " + _speed + " " + _core, "ManagerStart");
        }

        public void SendGoodMessage(string myPassword, string myHash)
        {
            _queue.Send(myHash + " " + myPassword, "ManagerSOLVED");
        }

        public void DelHash(string myPassword, string myHash)
        {
            foreach (Task task in tasks)
            {
                if (myHash == task._hash)
                {
                    _queue.Send(_ip + ";" + task._message, "Range");

                    tasks.Remove(task);

                    return;
                }
            }
        }

        public void Solver()
        {
            if (tasks.Count < _core)
            {
                if (CheckMessage())
                {
                    Thread thread = new Thread(this.CheckPasswords);
                    thread.Start();
                }
            }
        }

        public void SetStartSymbols(ref int ch1, ref int ch2, ref int ch3, ref int ch4, ref int ch5, ref int ch6, int countOfChar, long startPos)
        {
            long count = startPos;
            ch6 = (int)(count % countOfChar);
            count -= countOfChar;

            if (count > 0)
            {
                count /= countOfChar;
                ch5 = (int)(count % countOfChar);
                count -= countOfChar;
            }

            if (count > 0)
            {
                count /= countOfChar;
                ch4 = (int)(count % countOfChar);
                count -= countOfChar;
            }

            if (count > 0)
            {
                count /= countOfChar;
                ch3 = (int)(count % countOfChar);
                count -= countOfChar;
            }

            if (count > 0)
            {
                count /= countOfChar;
                ch2 = (int)(count % countOfChar);
                count -= countOfChar;
            }

            if (count > 0)
            {
                count /= countOfChar;
                ch1 = (int)(count % countOfChar);
            }
        }

        public void CheckPasswords()
        {
            Task task = tasks.Last();

            int ch1 = -1, ch2 = -1, ch3 = -1, ch4 = -1, ch5 = -1, ch6 = -1;
            long counter = 0;
            int countOfChar = task._alphabet.Length;
            string password = "";

            SetStartSymbols(ref ch1, ref ch2, ref ch3, ref ch4, ref ch5, ref ch6, countOfChar, task._startPos);

            for (; ch1 < countOfChar; ch1++)
            {
                string password1 = password;
                if (ch1 >= 0)
                    password1 += task._alphabet[ch1];

                for (; ch2 < countOfChar; ch2++)
                {

                    string password2 = password1;
                    if (ch2 >= 0)
                        password2 += task._alphabet[ch2];

                    for (; ch3 < countOfChar; ch3++)
                    {
                        string password3 = password2;
                        if (ch3 >= 0)
                            password3 += task._alphabet[ch3];

                        for (; ch4 < countOfChar; ch4++)
                        {
                            string password4 = password3;
                            if (ch4 >= 0)
                                password4 += task._alphabet[ch4];

                            for (; ch5 < countOfChar; ch5++)
                            {
                                string password5 = password4;
                                if (ch5 >= 0)
                                    password5 += task._alphabet[ch5];

                                for (; ch6 < countOfChar; ch6++)
                                {
                                    if (counter == task._countPasswords)
                                    {
                                        EndOfTask(task._message);
                                        return;
                                    }

                                    string password6 = password5 + task._alphabet[ch6];
                                    string newHash = CalculateMD5Hash(password6);

                                    if (newHash == task._hash)
                                    {
                                        Console.WriteLine("Подобрали пароль к свертке: {0} : {1}", newHash, password6);

                                        SendGoodMessage(password6, newHash);

                                        DelHash(password6, newHash);
                                        return;
                                    }
                                    counter++;
                                }
                                ch6 = 0;
                            }
                            ch5 = 0;
                        }
                        ch4 = 0;
                    }
                    ch3 = 0;
                }
                ch2 = 0;
            }
            EndOfTask(task._message);
        }

        private void EndOfTask(string message)
        {
            foreach (Task task in tasks)
                if (task._message == message)
                {
                    if(task._isTask)
                        _queue.Send(_ip + ";" + task._message, "Range");

                    tasks.Remove(task);

                    return;
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
            Thread.Sleep(1000);

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

            string[] param = message.ToString().Split(';');
            string[] count = param[2].ToString().Split(' ');

            task._message = message;
            task._alphabet = param[1];
            task._startPos = Convert.ToInt64(count[0]);
            task._countPasswords = Convert.ToInt64(count[1]);
            task._hash = param[0].ToUpper();
            task._isTask = true;

            tasks.Add(task);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Agent agent = new Agent();
            while (true)
            {
                agent.Solver();
            }
        }
    }
}