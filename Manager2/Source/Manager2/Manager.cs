using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Text;
using System.Messaging;

namespace Manager2
{
    public class Manager
    {
        private List<Agent> _agents;
        private MessageQueue _queue;
        private List<Hash> _packageHash;
        private List<FoundHashes> _solved;
        public static List<Agent> _freeAgent = new List<Agent>();
        public static List<Agent> _tempFreeAgent = new List<Agent>();
        public int _countHash = 0; 
        static public string digit = "0123456789";
        static public string engLow = "abcdefghijklmnopqrstuvwxyz";
        static public string engUpp = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        static public string rusLow = "абвгдеёжзийклмнопрстуфхцчшщъьыэюя";
        static public string rusUpp = "АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЬЫЭЮЯ";

        public Manager(MessageQueue queue)
        {
            _agents = new List<Agent>();
            _queue = queue;
            _packageHash = new List<Hash>();
            _solved = new List<FoundHashes>();
        }

        public List<Agent> GetList() { return _agents; }

        public List<Hash> GetListHash() { return _packageHash; }

        public List<FoundHashes> GetListSolved() { return _solved; }

        public MessageQueue GetQueue() { return _queue; }

        public void SetAgents(List<Agent> agent) { _agents = agent; }

        public void AddHashInSolvedPackage(FoundHashes hash) { _solved.Add(hash); }

        public void AddHashInPackage(Hash hash)
        {
            _packageHash.Add(hash);
            ++_countHash;
        }

        //добавление агента или обновление старого
        public void AddOrUpdateAgents(string msgBody)
        {
            string[] dataArr = msgBody.Split(' ');
            int countTask = 0;
            Agent agent = new Agent();

            bool existAgent = false;
            foreach (Agent oneAgent in _agents)
            {
                if (oneAgent.GetIp() == dataArr[0])
                {
                    existAgent = true;
                    oneAgent.SetCore(int.Parse(dataArr[2]));
                    oneAgent.SetSpeed(int.Parse(dataArr[1]) * 60);

                    agent = oneAgent;
                    countTask = 1;
                    break;
                }
            }

            if (!existAgent)
            {
                agent = new Agent(int.Parse(dataArr[2]), int.Parse(dataArr[1]) * 60, dataArr[0]);
                _agents.Add(agent);
                countTask = agent.GetCore();
            }

            if (!Distribution.NewRangeForAgent(_queue, agent, _packageHash, countTask))
            {
                _freeAgent.AddRange(_tempFreeAgent);
                _tempFreeAgent.Clear();
            }
        }

        //создание очереди
        public static MessageQueue CreateQueue(string nameQueue)
        {
            try
            {
                if (!MessageQueue.Exists(@".\private$\" + nameQueue))
                {
                    MessageQueue.Create(@".\private$\" + nameQueue);
                }
            }
            catch (MessageQueueException ex)
            {
                Console.WriteLine(ex.Message);
            }

            var queue = new MessageQueue(@".\private$\" + nameQueue);
            queue.SetPermissions("Все", MessageQueueAccessRights.FullControl);
            queue.Purge();
            queue.Send("Start", "Start");

            return queue;
        }

        public void ReadPackage(Thread messageRead)
        {
            bool startMessagesRaed = false;
            Console.WriteLine("Для добавления свертки воспользуйтесь командой: add [-min *] [-max *] [-preg #]\n" +
                "min - минимальная длина пароля\n" +
                "max - максимальная длина пароля\n" +
                "preg - алфавит, из которго состоит пароль\n" +
                "\t1) [] означает, что параметр не обязятелен для ввода\n" +
                "\t2) * - число от 1 до 6\n" +
                "\t3) min <= max\n" +
                "\t4) # - подстрока из подстановок последовательности deErR\n" +
                "\t\td - цифры от 0 до 9\n" +
                "\t\te - строчные буквы английского алфавита\n" +
                "\t\tE - прописные буквы английского алфавита\n" +
                "\t\tr - строчные буквы русского алфавита\n" +
                "\t\tR - прописные буквы русского алфавита\n\n" +
                "\t5) По умолчанию параметры принимают значения:\n" +
                "\t\tmin = 1\n" +
                "\t\tmax = 6\n" +
                "\t\tpreg = deErR\n\n" +
                "Для удаления свертки воспользуйтесь командой: del *\n" +
                "\t1) * - строка со сверткой\n");

            while (true)
            {
                string hash = "";
                string preg = "";
                int minLength = 0;
                int maxLength = 0;

                WriteCommand();
                string[] consolComandArr = Console.ReadLine().Split(' ');

                if (!((consolComandArr[0] != "add" && consolComandArr[0] == "del") || (consolComandArr[0] != "del" && consolComandArr[0] == "add")))
                {
                    Console.WriteLine("'{0}' command not defined", consolComandArr[0]);
                    continue;
                }

                if (consolComandArr.Length % 2 == 1)
                {
                    Console.WriteLine("Too few parameters");
                    continue;
                }

                if (consolComandArr[0] == "add")
                {
                    bool alredayExists = false;
                    foreach (Hash obj in _packageHash)
                    {
                        if (obj.GetHash() == consolComandArr[1])
                        {
                            Console.WriteLine("Hash already exists");
                            alredayExists = true;
                            break;
                        }
                    }

                    if (alredayExists == true)
                        continue;

                    hash = consolComandArr[1].ToUpper();
                    if (consolComandArr.Length == 2)
                    {
                        preg = "deErR";
                        minLength = 1;
                        maxLength = 6;
                    }
                    else
                    {
                        int minPos = Array.IndexOf(consolComandArr, "-min");
                        int maxPos = Array.IndexOf(consolComandArr, "-max");
                        int pregPos = Array.IndexOf(consolComandArr, "-preg");

                        minLength = minPos == -1 ? 1 : int.Parse(consolComandArr[minPos + 1]);
                        maxLength = maxPos == -1 ? 6 : int.Parse(consolComandArr[maxPos + 1]);
                        preg = pregPos == -1 ? "deErR" : consolComandArr[pregPos + 1];
                    }

                    AddHashInPackage(new Hash(hash, preg, minLength, maxLength));

                    if (!startMessagesRaed)
                    {
                        messageRead.Start();
                        startMessagesRaed = true;
                    }
                }

                if (consolComandArr[0] == "del")
                {
                    if (_packageHash.Count != 0)
                    {
                        UpdateHashPackage(consolComandArr[1]);
                        UpdateSolvedPackage(consolComandArr[1]);
                        Distribution.UpdateUsedRange(consolComandArr[1]);
                        --_countHash;
                    }
                    else
                    {
                        ClearLine();
                        Console.WriteLine("Список сверток пуст!");
                        WriteCommand();
                    }
                }
            }
        }

        public void UpdateHashPackage(string hash)
        {
            foreach (Hash oneHash in _packageHash)
            {
                if (oneHash.GetHash() == hash)
                {
                    _packageHash.Remove(oneHash);
                    break;
                }
            }
        }

        public void UpdateSolvedPackage(string hash)
        {
            foreach (FoundHashes oneHash in _solved)
            {
                if (oneHash.GetHashStr() == hash)
                {
                    _solved.Remove(oneHash);
                    break;
                }
            }
        }

        public void WriteCommand()
        {
            Console.Write("command: ");
        }

        public void CheckRemainRange()
        {
            while (true)
            {
                Thread.Sleep(120000);
                if (!Distribution.CheckDateCreate())
                {
                    ClearLine();
                    Console.WriteLine("Количество необработанных диапазонов {0}", Distribution._usedRange.Count);
                    WriteCommand();

                    Distribution.DistrOfRemainRange(this, _queue, _agents);
                }
            }
        }

        static void ClearLine()
        {
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, Console.CursorTop - 1);
        }

        void StartDistrOfRange()
        {
            Thread range = new Thread(new ThreadStart(CheckRemainRange));
            range.Start();
        }
        
        public void ReadingMessages()//+++++++++++++++++++++++++++++++++++++++++++
        {
            StartDistrOfRange();

            while (true)
            {
                foreach (Message message in _queue)
                {
                    if (_freeAgent.Count != 0 && _packageHash.Count != 0)
                    {
                        foreach (Agent agent in _freeAgent)
                        {
                            Distribution.NewRangeForAgent(_queue, agent, _packageHash, 1);
                        }

                        _freeAgent.Clear();
                        _freeAgent.AddRange(_tempFreeAgent);
                        _tempFreeAgent.Clear();
                    }

                    if (message.Label == "ManagerStart")
                    {
                        _queue.ReceiveById(message.Id);
                        AddOrUpdateAgents(message.Body.ToString());
                        continue;
                    }

                    if (message.Label == "ManagerSOLVED")
                    {
                        _queue.ReceiveById(message.Id);

                        string[] dataArr = message.Body.ToString().Split(' ');
                        _solved.Add(new FoundHashes(dataArr[0], dataArr[1]));

                        UpdateHashPackage(dataArr[0].ToUpper());
                        Distribution.UpdateUsedRange(dataArr[0]);

                        ClearLine();
                        Console.WriteLine("Нашли решение: {0} - {1}", dataArr[0], dataArr[1]);
                        WriteCommand();
                        continue;
                    }

                    if (message.Label == "ManagerDELETE")
                    {
                        string id = message.Body.ToString();

                        Message mes = _queue.ReceiveById(message.Id);
                        mes = _queue.ReceiveById(id);
                        continue;
                    }
                                  
                    if (message.Label == "Range")
                    {
                        string[] dataArr = message.Body.ToString().Split(';');
                        string ip = dataArr[0];

                        _queue.ReceiveById(message.Id);
                        Distribution.UpdateUsedRange(message.Body.ToString());
                    }
                }
            }
        }
    }
}

/*
        public bool AllSolved()
        {
            bool solved = true;

            if (_solved.Count == _countHash)
            {
                foreach (FoundHashes hash in _solved)
                    solved = solved && hash.GetSolved();
            }
            else
            {
                solved = false;
            }

            return solved;
        }
*/
