using System;
using System.Messaging;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.IO;

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

    public class HashConvol//+++++++++++++++++++++++++++++++++++++++++++
    {
        private string _hashStr;
        private bool _solvedBool;

        public HashConvol(string hash)
        {
            _hashStr = hash;
            _solvedBool = false;
        }

        public string GetHashStr()
        {
            return _hashStr;
        }

        public bool GetSolved()
        {
            return _solvedBool;
        }

        public void SetSolved()
        {
            _solvedBool = true;
        }       
    }


    public class Manager
    {
        private List<Agent> _agents; //контейнер агентов
        private MessageQueue _queue;//очередь
        private List<HashConvol> _packageHash;//контейнер сверток
        private List<SolvedConvol> _solved;
        public bool _resolution = false;
        public int _countHash = 0;

        public Manager(MessageQueue queue)//+++++++++++++++++++++++++++++++++++++++++++
        {
            _agents = new List<Agent>();
            _queue = queue;
            _packageHash = new List<HashConvol>();
            _solved = new List<SolvedConvol>();
        }

        public List<Agent> GetList()//+++++++++++++++++++++++++++++++++++++++++++
        {
            return _agents;
        }

        public List<HashConvol> GetListHash()//+++++++++++++++++++++++++++++++++++++++++++
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

        public void AddHashInPackage(HashConvol hash)//+++++++++++++++++++++++++++++++++++++++++++
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

                //Distribution.NewRangeForAgent(_queue, agent, _packageHash, agent.GetCore()); //убрал несколько заданий
                Distribution.NewRangeForAgent(_queue, agent, _packageHash, 1);
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

        //считывание сверток
        public void ReadPackage()
        {
            Console.WriteLine("Задайте свертку");
            string inputValue = "";
            inputValue = Console.ReadLine();
            _packageHash.Add(new HashConvol(inputValue));
            Distribution._usedRange.Add(";" + inputValue + ";" +  "0 " + Distribution._currentPosInRange.ToString());
            ++_countHash;
            //_resolution = true;

            while (true)
            {
                Console.WriteLine("Добавить хеш - 1, Удалить хеш - 2");
                string flag = Console.ReadLine();
                switch (flag)
                {
                    case "1":
                        {
                            Console.WriteLine("Задайте новую свертку:");
                            inputValue = Console.ReadLine();
                            _packageHash.Add(new HashConvol(inputValue));
                            ++_countHash;
                            Distribution._usedRange.Add(";" + inputValue + ";" + "0 " + Distribution._currentPosInRange.ToString());
                            break;
                        }
                    case "2":
                        {
                            Console.WriteLine("Задайте свертку, которую нужно удалить:");
                            inputValue = Console.ReadLine();
                            UpdateHashPackage(inputValue);
                            UpdateSolvedPackage(inputValue);
                            Distribution.UpdateUsedRange(inputValue);
                            --_countHash;
                            break;
                        }
                    default:
                        {
                            Console.WriteLine("Invalid button. Для добавления новой свертки нажмите 1, для удаления нажмите 2");
                            break;
                        }
                }         
            }
        }

        //обновление контейнера с хешами(удаление хешей, к которым найдено решение или пользователь сам хочет удалить данный хеш из обработки)
        public void UpdateHashPackage(string hash)//+++++++++++++++++++++++++++++++++++++++++++
        {
            foreach(HashConvol oneHash in _packageHash)
            {
                if (oneHash.GetHashStr() == hash)
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

        public void CHUDO()
        {
            while(Distribution._usedRange.Count() != 0)
            {
                Thread.Sleep(65000);
                Distribution.DistrOfRemainRange(this, _queue, _agents);
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

        public void ReadingMessages()//+++++++++++++++++++++++++++++++++++++++++++
        {
            bool solved = false;

            while (!solved)
            {
                foreach (Message message in _queue)
                {
                    if (message.Label == "ManagerStart")
                    {
                        AddOrUpdateAgents(message.Body.ToString());
                        Message mes = _queue.ReceiveById(message.Id);
                        Console.WriteLine("Удалили Start: '{0}' имеющее id: '{1}'", mes.Body, mes.Id);
                    }

                    if (message.Label == "ManagerSOLVED")
                    {                      
                        string[] dataArr = message.Body.ToString().Split(' ');
                        _solved.Add(new SolvedConvol(dataArr[0], dataArr[1]));
                        UpdateHashPackage(dataArr[0]);
                        Distribution.UpdateUsedRange(dataArr[0]);
                        _queue.ReceiveById(message.Id);

                        solved = AllSolved();
                    }

                    if (message.Label == "ManagerDELETE")
                    {
                        string id = message.Body.ToString();

                        Message mes = _queue.ReceiveById(message.Id);
                        Console.WriteLine("Удалили запрос: '{0}' имеющее id: '{1}'", mes.Body, mes.Id);
                        mes = _queue.ReceiveById(id);
                        Console.WriteLine("Удалили задание: '{0}' имеющее id: '{1}'", mes.Body, mes.Id);
                    }

                    if (message.Label == "Range")
                    {
                        Distribution.UpdateUsedRange(message.Body.ToString());
                        Message mes = _queue.ReceiveById(message.Id);
                        Console.WriteLine("Удалили ответ: '{0}' имеющее id: '{1}'", mes.Body, mes.Id);
                    }

                    if(Distribution._endAllVariant)
                    {
                        Thread range = new Thread(new ThreadStart(CHUDO));
                        range.Start();
                    }
                }            
            }

            Console.WriteLine("Решение найдено:");
            foreach(SolvedConvol solv in GetListSolved())
            {
                Console.WriteLine(solv.GetHashStr() + " - " + solv.GetSolvedStr());
            }
        }
    }

    class Program
    { 
        static void Main(string[] args)
        {
            Manager manager = new Manager(Manager.CreateQueue("testName"));
            MessageQueue queue = manager.GetQueue();
            queue.Formatter = new XmlMessageFormatter(new String[] { "System.String" });

            manager._resolution = true;
            while (manager._resolution)
            {
                Thread message = new Thread(new ThreadStart(manager.ReadingMessages));
                message.Start();
                manager._resolution = false;
                break;
            }

            manager.ReadPackage();
        }
    }
}
