using System;
using System.Messaging;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.IO;
using System.Security.Cryptography;
using System.Windows;

namespace Manager2
{
    public class Agent//+++++++++++++++++++++++++++++++++++++++++++
    {
        private int _core;
        private int _speed;
        private string _ip;

        public Agent(int core, int speed, string ip)
        {
            _core = core;
            _speed = speed;
            _ip = ip;
        }

        public int GetCore()
        {
            return _core;
        }

        public void SetCore(int core)
        {
            _core = core;
        }

        public int GetSpeed()
        {
            return _speed;
        }

        public void SetSpeed(int speed)
        {
            _speed = speed;
        }

        public string GetIp()
        {
            return _ip;
        }
    }

    public class SolvedConvol//+++++++++++++++++++++++++++++++++++++++++++
    {
        private string _hashStr;
        private string _solved;
        private bool _solvedBool;

        public SolvedConvol(string hash, string solved)
        {
            _hashStr = hash;
            _solved = solved;
            _solvedBool = true;
        }

        public string GetHashStr()
        {
            return _hashStr;
        }

        public string GetSolvedStr()
        {
            return _solved;
        }

        public bool GetSolved()
        {
            return _solvedBool;
        }        
    }

    public class NewHash
    {
        private string _hash;
        private int _minLength;
        private int _maxLength;
        private long _startPos;
        private long _currentPos;
        private long _totalPsw;
        private string _alphaStr;
        private bool _endAllVariant;
        private List<string> _usedRange;

        public NewHash(string hash, string alpha, int minLength, int maxLength)
        {
            _hash = hash;
            _minLength = minLength;
            _maxLength = maxLength;
            _alphaStr = InitializeAlpha(alpha);
            _startPos = InitializeStartPos(minLength);
            _currentPos = _startPos;
            _totalPsw = InitializeTotalPsw(maxLength);
            _endAllVariant = false;
            _usedRange = new List<string>();
        }

        public string GetHash()
        {
            return _hash;
        }

        public string GetAlphaStr()
        {
            return _alphaStr;
        }

        public int GetMinLength()
        {
            return _minLength;
        }

        public int GetMaxLength()
        {
            return _maxLength;
        }

        public long GetStartPos()
        {
            return _startPos;
        }

        public long GetCurrentPos()
        {
            return _currentPos;
        }

        public void SetCurrentPos(long delta)
        {
            _currentPos += delta;
        }

        public long GetTotalPsw()
        {
            return _totalPsw;
        }

        public bool GetEndAllVariant()
        {
            return _endAllVariant;
        }

        public void SetEndAllVariant()
        {
            _endAllVariant = true;
        }

        public List<string> GetListUsedRange()
        {
            return _usedRange;
        }

        public void SetListUsedRange(List<string> other)
        {
            _usedRange = other;
        }

        private long InitializeStartPos(int length)
        {
            long outStart = 0;

            for (int i = 1; i < length; ++i)
            {
                outStart += (long)Math.Pow(_alphaStr.Length, i);
            }

            return outStart;
        }

        private long InitializeTotalPsw(int maxLenght)
        {
            long outStart = 0;

            for (int i = 1; i < maxLenght + 1; ++i)
            {
                outStart += (long)Math.Pow(_alphaStr.Length, i);
            }

            return outStart;
        }

        private string InitializeAlpha(string alpha)
        {
            string outAlpha = "";

            foreach (char ch in alpha)
            {
                switch (ch)
                {
                    case 'd':
                        {
                            outAlpha += Manager.digit;
                            break;
                        }
                    case 'e':
                        {
                            outAlpha += Manager.engLow;
                            break;
                        }
                    case 'E':
                        {
                            outAlpha += Manager.engUpp;
                            break;
                        }
                    case 'r':
                        {
                            outAlpha += Manager.rusLow;
                            break;
                        }
                    case 'R':
                        {
                            outAlpha += Manager.rusUpp;
                            break;
                        }
                    default:
                        {
                            break;
                        }
                }
            }

            return outAlpha;
        }
    }

    public class Manager
    {
        private List<Agent> _agents; //контейнер агентов
        private MessageQueue _queue;//очередь
        private List<NewHash> _packageHash;//контейнер сверток
        private List<SolvedConvol> _solved;
        private bool _resolution = true;
        public int _countHash = 0;
        static public string digit = "0123456789";
        static public string engLow = "abcdefghijklmnopqrstuvwxyz";
        static public string engUpp = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        static public string rusLow = "абвгдеёжзийклмнопрстуфхцчшщъьыэюя";
        static public string rusUpp = "АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЬЫЭЮЯ";

        public Manager(MessageQueue queue)//+++++++++++++++++++++++++++++++++++++++++++
        {
            _agents = new List<Agent>();
            _queue = queue;
            _packageHash = new List<NewHash>();
            _solved = new List<SolvedConvol>();
        }

        public List<Agent> GetList()//+++++++++++++++++++++++++++++++++++++++++++
        {
            return _agents;
        }

        public List<NewHash> GetListHash()//+++++++++++++++++++++++++++++++++++++++++++
        {
            return _packageHash;
        }

        public List<SolvedConvol> GetListSolved()//+++++++++++++++++++++++++++++++++++++++++++
        {
            return _solved;
        }

        public MessageQueue GetQueue()//+++++++++++++++++++++++++++++++++++++++++++
        {
            return _queue;
        }

        public void SetAgents(List<Agent> agent)
        {
            _agents = agent;
        }

        public void AddHashInPackage(NewHash hash)//+++++++++++++++++++++++++++++++++++++++++++
        {
            _packageHash.Add(hash);
            ++_countHash;
        }

        public void AddHashInSolvedPackage(SolvedConvol hash)//+++++++++++++++++++++++++++++++++++++++++++
        {
            _solved.Add(hash);
        }

        //добавление агента или обновление старого
        public void AddOrUpdateAgents(string msgBody)//+++++++++++++++++++++++++++++++++++++++++++
        {
            string[] dataArr = msgBody.Split(' ');

            bool existAgent = false;
            foreach (Agent oneAgent in _agents)
            {
                if (oneAgent.GetIp() == dataArr[0])
                {
                    existAgent = true;
                    oneAgent.SetCore(int.Parse(dataArr[2]));
                    oneAgent.SetSpeed(int.Parse(dataArr[1]) * 60);
                    Distribution.NewRangeForAgent(_queue, oneAgent, _packageHash , 1);
                    break;
                }
            }

            if (!existAgent)
            {
                Agent agent = new Agent(int.Parse(dataArr[2]), int.Parse(dataArr[1]) * 60, dataArr[0]);
                Distribution.NewRangeForAgent(_queue, agent, _packageHash, agent.GetCore()); 
                _agents.Add(agent);
            }
        }

        //создание очереди
        public static MessageQueue CreateQueue(string nameQueue) //+++++++++++++++++++++++++++++++++++++++++++
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
            queue.Send("Start", "Start");

            return queue;
        }
        
        public void ReadPackage(Thread messageRead)
        {
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

                Console.Write("command: ");
                string[] consolComandArr = Console.ReadLine().Split(' ');
                
                if(!((consolComandArr[0] != "add" && consolComandArr[0] == "del") || (consolComandArr[0] != "del" && consolComandArr[0] == "add")))
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
                    foreach (NewHash obj in _packageHash)
                    {
                        if (obj.GetHash() == consolComandArr[1])
                        {
                            Console.WriteLine("Hash already exists");
                            alredayExists = true;
                            break;
                        }
                    }
                   
                    if (alredayExists == true)
                    {
                        continue;
                    }

                    hash = consolComandArr[1];
                    if (consolComandArr.Length == 2)
                    {
                        preg = "deErR";
                        minLength = 1;
                        maxLength = 6;
                    } else
                    {
                        int minPos = Array.IndexOf(consolComandArr, "-min");
                        int maxPos = Array.IndexOf(consolComandArr, "-max");
                        int pregPos = Array.IndexOf(consolComandArr, "-preg");

                        minLength = minPos == -1 ? 1 : int.Parse(consolComandArr[minPos + 1]);
                        maxLength = maxPos == -1 ? 6 : int.Parse(consolComandArr[maxPos + 1]);
                        preg      = pregPos == -1 ? "deErR" : consolComandArr[pregPos + 1];
                    }
                   
                    AddHashInPackage(new NewHash(hash, preg, minLength, maxLength));

                    if(_resolution)
                    {
                        messageRead.Start();
                        _resolution = false;
                    }
                }

                if (consolComandArr[0] == "del")
                {
                    UpdateHashPackage(consolComandArr[1]);
                    UpdateSolvedPackage(consolComandArr[1]);
                    Distribution.UpdateUsedRange(consolComandArr[1]);
                    --_countHash;
                    continue;
                }
            }
        }

        //обновление контейнера с хешами(удаление хешей, к которым найдено решение или пользователь сам хочет удалить данный хеш из обработки)
        public void UpdateHashPackage(string hash)//+++++++++++++++++++++++++++++++++++++++++++
        {
            foreach(NewHash oneHash in _packageHash)
            {
                if (oneHash.GetHash() == hash)
                {
                    _packageHash.Remove(oneHash);
                    break;
                }               
            }
        }

        public void UpdateSolvedPackage(string hash)//+++++++++++++++++++++++++++++++++++++++++++
        {
            foreach (SolvedConvol oneHash in _solved)
            {
                if (oneHash.GetHashStr() == hash)
                {
                    _solved.Remove(oneHash);
                    break;
                }
            }
        }

        public void CHUDO()//--------------------------------------------------
        {
            while (true)
            {
                while (Distribution._usedRange.Count() != 0)
                {
                    Console.WriteLine("Перед запуском чуда {0}", Distribution._usedRange.Count);
                    Thread.Sleep(65000);
                    Console.WriteLine("Запустили ЧУДОООООООООО");
                    Distribution.DistrOfRemainRange(this, _queue, _agents);
                }
            }
        }

        public bool AllSolved()//+++++++++++++++++++++++++++++++++++++++++++
        {
            bool solved = true;

            if (_solved.Count == _countHash)
            { 
                foreach (SolvedConvol hash in _solved)
                    solved = solved && hash.GetSolved();
            } else
            {
                solved = false;
            }

            return solved;
        }

        public bool AllEndAllVariant()//********************************************
        {
            bool end = true;
            
            if(_packageHash.Count == 0)
            {
                return false;
            }

            foreach (NewHash hash in _packageHash)
                end = end && hash.GetEndAllVariant();
           
            return end;
        }

        static void ClearLine()
        {
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, Console.CursorTop - 1);
        }

        public void ReadingMessages()//+++++++++++++++++++++++++++++++++++++++++++
        {            
           // bool solved = false;

            while (true)
            {
                foreach (Message message in _queue)
                {
                    if (message.Label == "ManagerStart")
                    {                       
                        if (_packageHash.Count != 0)
                        {
                            AddOrUpdateAgents(message.Body.ToString());
                            _queue.ReceiveById(message.Id);
                        }
                        continue;
                    }

                    if (message.Label == "ManagerSOLVED")
                    {                      
                        string[] dataArr = message.Body.ToString().Split(' ');
                        _solved.Add(new SolvedConvol(dataArr[0], dataArr[1]));
                        UpdateHashPackage(dataArr[0]);
                        Distribution.UpdateUsedRange(dataArr[0]);
                        _queue.ReceiveById(message.Id);
                        ClearLine();
                        Console.WriteLine("Нашли решение: {0} - {1}", dataArr[0], dataArr[1]);
                        Console.Write("command: ");
                        continue;
                    }

                    if (message.Label == "ManagerDELETE")
                    {
                        string id = message.Body.ToString();

                        Message mes = _queue.ReceiveById(message.Id);
                      //  Console.WriteLine("Удалили запрос: '{0}' имеющее id: '{1}'", mes.Body, mes.Id);
                        mes = _queue.ReceiveById(id);
                        // Console.WriteLine("Удалили задание: '{0}' имеющее id: '{1}'", mes.Body, mes.Id);
                        continue;
                    }

                    if (message.Label == "Range")
                    {
                        string[] dataArr = message.Body.ToString().Split(';');
                        string ip = dataArr[0];
                      
                                   

                        if (_packageHash.Count != 0 && !AllEndAllVariant())
                        {
                            foreach (Agent agent in _agents)
                            {
                                if (agent.GetIp() == ip)
                                {
                                    Distribution.NewRangeForAgent(_queue, agent, _packageHash, 1);
                                    break;
                                }
                            }
                                                                                 
                            _queue.ReceiveById(message.Id);
                        }

                        Distribution.UpdateUsedRange(message.Body.ToString());
                      //  Console.WriteLine("После отработки range {0}", Distribution._usedRange.Count);
                    }


                    if (AllEndAllVariant())
                    {
                        if (Distribution._endAllVariant)
                        { 
                            Thread range = new Thread(new ThreadStart(CHUDO));
                            range.Start();
                            Distribution._endAllVariant = false;
                        }
                    }
                }            
            }

           /* Console.WriteLine("Решение найдено:");
            foreach(SolvedConvol solv in GetListSolved())
            {
                Console.WriteLine(solv.GetHashStr() + " - " + solv.GetSolvedStr());
            }*/
        }
    }

    class Program
    { 
        static void Main(string[] args)
        {
            Manager manager = new Manager(Manager.CreateQueue("testName"));
            MessageQueue queue = manager.GetQueue();
            queue.Formatter = new XmlMessageFormatter(new String[] { "System.String" });

            Thread message = new Thread(new ThreadStart(manager.ReadingMessages));   
            manager.ReadPackage(message);
        }
    }
}
