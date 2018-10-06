using System;
using System.Messaging;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.IO;

namespace Manager2
{
    class Agent
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

    class HashConvol
    {
        private string _hashStr;
        private long _currentPos;

        public HashConvol(string hash)
        {
            _hashStr = hash;
            _currentPos = 0;
        }

        public string GetHashStr()
        {
            return _hashStr;
        }

        public long GetCurrentPos()
        {
            return _currentPos;
        }

        public void SetCurrentPos(long offset/*смещение*/)
        {
            _currentPos += offset + 1;
        }
    }


    class Manager
    {
        private List<Agent> _agents; //контейнер агентов
        private MessageQueue _queue;//очередь
        private List<HashConvol> _packageHash;//контейнер сверток

        public Manager(MessageQueue queue)
        {
            _agents = new List<Agent>();
            _queue = queue;
            _packageHash = ReadPackage("packageHash.txt");
        }

        public List<Agent> GetList()
        {
            return _agents;
        }

        public List<HashConvol> GetListHash()
        {
            return _packageHash;
        }

        public MessageQueue GetQueue()
        {
            return _queue;
        }


        //добавление агента или обновление старого
        public void AddOrUpdateAgents(string msgBody)
        {
            string[] dataArr = msgBody.Split(' ');

            bool existAgent = false;
            foreach (Agent oneAgent in _agents)
            {
                if (oneAgent.GetIp() == dataArr[0])
                {
                    existAgent = true;
                    oneAgent.SetCore(int.Parse(dataArr[2]));
                    oneAgent.SetSpeed(int.Parse(dataArr[1]));
                    Distribution.NewRangeForAgent(_queue, oneAgent, _packageHash , 1);
                    break;
                }
            }

            if (!existAgent)
            {
                Agent agent = new Agent(int.Parse(dataArr[2]), int.Parse(dataArr[1]), dataArr[0]);

                Distribution.NewRangeForAgent(_queue, agent, _packageHash, 7);//agent.GetCore());

                _agents.Add(agent);
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

            return queue;
        }

        //отправка сообщений с диапазонами в очередь 
       /* public void SendMessage(List<AgentWithMessage> agents)
        {
            foreach (AgentWithMessage agent in agents)
            {
                for (int i = 0; i < agent.GetList().Count(); ++i)
                {
                    _queue.Send(agent.GetList()[i], agent.GetIp());
                }
            }
        }*/

        //считывание сверток
        private List<HashConvol> ReadPackage(string fileName)
        {
            List<HashConvol> packageHash = new List<HashConvol>();

            StreamReader file = new StreamReader(fileName);

            string hash = "";
            while((hash = file.ReadLine()) != null)
            {
                packageHash.Add(new HashConvol(hash));
            }

            file.Close();

            Console.WriteLine("Задайте свертку искомого пароля: ");

            string inputValue = "";
            string flag = "";
            while(flag != "0")
            {
                if(flag == "1")
                {
                    Console.WriteLine("Задайте свертку:");
                }
                inputValue = Console.ReadLine();
                packageHash.Add(new HashConvol(inputValue));

                Console.WriteLine("Если хотите задать еще одну свертку, то нажмите 1, если нет, то - 0.");
                flag = Console.ReadLine();
            }

            return packageHash;
        }

        //обновление контейнера с хешами(удаление хешей, к которым найдено решение)
        public void UpdateHashPackage(string hash)
        {
            foreach(HashConvol oneHash in _packageHash)
            {
                if (oneHash.GetHashStr() == hash)
                    _packageHash.Remove(oneHash);
            }
        }
    }

    class Program
    { 
        static void Main(string[] args)
        {
            Manager manager = new Manager(Manager.CreateQueue("MyNewPrivateQueueTest"));
            MessageQueue queue = manager.GetQueue();
            queue.Formatter = new XmlMessageFormatter(new String[] { "System.String" });

            List<HashConvol> map = manager.GetListHash();

            foreach(HashConvol one in map)
            {
                Console.WriteLine(one.GetHashStr());
            }

           bool solved = false;
            while (!solved)
            {
                foreach (Message message in queue)
                {
                    if (message.Label == "ManagerStart")
                    {
                        manager.AddOrUpdateAgents(message.Body.ToString());
                       // queue.ReceiveById(message.Id);
                    }

                    if(message.Label == "ManagerSOLVED")
                    {
                        Console.WriteLine(message.Body.ToString());

                        string[] dataArr = message.Body.ToString().Split(' ');
                        manager.UpdateHashPackage(dataArr[0]);

                        solved = true;
                    }

                    if(message.Label == "Range")
                    {
                        Distribution.usedRange.Remove(message.Body.ToString());
                        queue.ReceiveById(message.Id);
                    }                    
                }

               /* if (!solved && (Distribution._currentPos < Distribution._totalCountPsw) && Distribution.usedRange.Count != 0)
                {
                    Distribution.OldRangeForAgent(manager.GetList());
                }*/

                solved = true;
            }
        }
    }
}
