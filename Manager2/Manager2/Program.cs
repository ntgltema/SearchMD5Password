using System;
using System.Messaging;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

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

    class Manager
    {
        private List<Agent> _agents;
        private MessageQueue _queue;

        public Manager(MessageQueue queue)
        {
            _agents = new List<Agent>();
            _queue = queue;
        }

        public List<Agent> GetList()
        {
            return _agents;
        }

        public MessageQueue GetQueue()
        {
            return _queue;
        }

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
                    Distribution.NewRangeForAgent(_queue, oneAgent, 1);
                    break;
                }                             
            }

            if (!existAgent)
            {
                Agent agent = new Agent(int.Parse(dataArr[2]), int.Parse(dataArr[1]), dataArr[0]);

                Distribution.NewRangeForAgent(_queue, agent, agent.GetCore());

                _agents.Add(agent);
            }       
        }            

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

        public void SendMessage(List<AgentWithMessage> agents)
        {
            foreach(AgentWithMessage agent in agents)
            {
                for(int i = 0; i< agent.GetList().Count(); ++i)
                {
                    _queue.Send(agent.GetList()[i], agent.GetIp());
                }
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

            bool solved = false;
            while (!solved)
            {
                foreach (Message message in queue)
                {
                    if (message.Label == "ManagerStart")
                    {
                        manager.AddOrUpdateAgents(message.Body.ToString());
                    }

                    if(message.Label == "ManagerSOLVED")
                    {
                        Console.WriteLine(message.Body.ToString());
                        solved = true;
                    }
                }

                solved = true;
            }
        }
    }
}
