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

   /* public class HashConvol//+++++++++++++++++++++++++++++++++++++++++++
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
    }*/

    public class NewHash
    {
        private string _hash;
        private char[] _alpha;
        private int _minLength;
        private int _maxLength;
        private long _startPos;
        private long _currentPos;
        private long _totalPsw;
        private string _alphaStr;
        private bool _endAllVariant;

        public NewHash(string hash, string alpha, int minLength, int maxLength)
        {
            _hash = hash;
            _minLength = minLength;
            _maxLength = maxLength;
            _alphaStr = InitializeAlpha(alpha);
            _startPos = InitializeStartPos(minLength);
            _currentPos = _startPos;
            _totalPsw = InitializeTotalPsw(maxLength);
            _endAllVariant = true;
        }

        public string GetHash()
        {
            return _hash;
        }

        public char[] GetAlpha()
        {
            return _alpha;
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
            _currentPos += delta + 1;
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

        private long InitializeStartPos(int length)
        {
            long outStart = 0;

            for (int i = 1; i < length; ++i)
            {
                outStart += (long)Math.Pow(_alpha.Length, i);
            }

            return outStart;
        }

        private long InitializeTotalPsw(int maxLenght)
        {
            long outStart = 0;

            for(int i = 1; i < maxLenght + 1; ++i)
            {
                outStart += (long)Math.Pow(_alpha.Length, i);  
            }

            return outStart;
        }

        private string InitializeAlpha(string alpha)
        {
            List<char> outAlpha = new List<char>();

            foreach (char ch in alpha)
            {
                switch (ch)
                {
                    case 'd':
                        {
                            outAlpha.AddRange(Manager.digit);
                            break;
                        }
                    case 'e':
                        {
                            outAlpha.AddRange(Manager.engLow);
                            break;
                        }
                    case 'E':
                        {
                            outAlpha.AddRange(Manager.engUpp);
                            break;
                        }
                    case 'r':
                        {
                            outAlpha.AddRange(Manager.rusLow);
                            break;
                        }
                    case 'R':
                        {
                            outAlpha.AddRange(Manager.rusUpp);
                            break;
                        }
                    default:
                        {
                            break;
                        }
                }
            }

            string outAplhaStr = "";
            int capacityList = outAlpha.Count;
            for (int i = 0; i < capacityList; ++i)
            {
                outAplhaStr += outAlpha[i];
            }
            _alpha = outAlpha.ToArray();

            return outAplhaStr;
        }
    }


    public class Manager
    {
        private List<Agent> _agents; //контейнер агентов
        private MessageQueue _queue;//очередь
        private List<NewHash> _packageHash;//контейнер сверток
        private List<SolvedConvol> _solved;
        public bool _resolution = false;
        public int _countHash = 0;
        static public char[] digit  = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
        static public char[] engLow = { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z' };
        static public char[] engUpp = { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };
        static public char[] rusLow = { 'а', 'б', 'в', 'г', 'д', 'е', 'ё', 'ж', 'з', 'и', 'й', 'к', 'л', 'м', 'н', 'о', 'п', 'р', 'с', 'т', 'у', 'ф', 'х', 'ц', 'ч', 'ш', 'щ', 'ъ', 'ь', 'ы', 'э', 'ю', 'я' };
        static public char[] rusUpp = { 'А', 'Б', 'В', 'Г', 'Д', 'Е', 'Ё', 'Ж', 'З', 'И', 'Й', 'К', 'Л', 'М', 'Н', 'О', 'П', 'Р', 'С', 'Т', 'У', 'Ф', 'Х', 'Ц', 'Ч', 'Ш', 'Щ', 'Ъ', 'Ь', 'Ы', 'Э', 'Ю', 'Я' };

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

        private void InitialazeHash()
        {
            Console.WriteLine("Вы сможете нам помочь быстрее подобрать пароль, если укажите известные вам данные о пароле.\n" +
               "Если информация о пароле вам не известна, то введите цифру 1 - \"по умолчанию\", это будет означать, что пароль будет подбираться из всех возможных вариантов\n\n" +
               "Если вы все-таки обладаете информацией о пароле, то введите цифру 2 и заполните колонки:");

            string minLength = "";
            string maxLength = "";
            string preg = "";
            string flag2 = "";
            while (flag2 != "1" || flag2 != "2")
            {
                flag2 = Console.ReadLine();
                switch (flag2)
                {
                    case "1":
                        {
                            minLength = "1";
                            maxLength = "6";
                            preg = "deErR";
                            break;
                        }
                    case "2":
                        {
                            Console.Write("Введите минимальную длину пароля(число от 1 до 5): ");
                            minLength = Console.ReadLine();

                            Console.Write("Введите максимальную длину пароля(minLength < число < 6): ");
                            maxLength = Console.ReadLine();

                            Console.Write("Введите из каких символов составлен пароль: ");
                            preg = Console.ReadLine();

                            break;
                        }
                    default:
                        {

                            break;
                        }
                }
            }
        }

        public void InformationAboutHash(string inputValue)
        {
            Console.WriteLine("Информация не известна - 1, Добавить информацию о пароле - 2");
            int minLength = 0;
            int maxLength = 0;
            string preg = "";
            string flag2 = Console.ReadLine();
            switch (flag2)
            {
                case "1":
                    {
                        _packageHash.Add(new NewHash(inputValue, "deErR", 1, 6));
                        break;
                    }
                case "2":
                    {
                        Console.Write("minLength = ");
                        minLength = int.Parse(Console.ReadLine());

                        Console.Write("maxLength = ");
                        minLength = int.Parse(Console.ReadLine());

                        Console.Write("preg = ");
                        preg = Console.ReadLine();
                        _packageHash.Add(new NewHash(inputValue, preg, minLength, maxLength));
                        break;
                    }
            }
        }
        //считывание сверток
        public void ReadPackage()
        {
            Console.WriteLine("Вы сможете нам помочь быстрее подобрать пароль, если укажите известные вам данные о пароле.\n" +
                "Если информация о пароле вам не известна, то введите цифру 1 - \"по умолчанию\", это будет означать, что пароль будет подбираться из всех возможных вариантов\n\n" + 
                "Если вы все-таки обладаете информацией о пароле, то введите цифру 2 и заполните колонки:\n 1) минимальная длина пароля\n 2) максимальная длина пароля\n" + 
                " 3) маска, задающая из каких символов состаит пароль:\n\t*\td - цифры [0,9]\n\t*\te - строчные буквы английского алфавита" +
                "\n\t*\tE - просписные буквы английского алфавита\n\t*\tr - строчные буквы русского алфавита\n\t*\tR - просписные буквы русского алфавита\n\n\n");
                              
            Console.WriteLine("Задайте свертку");
            string inputValue = Console.ReadLine();
            InformationAboutHash(inputValue);
            ++_countHash;

            while (true)
            {
                Console.WriteLine("Добавить хеш - 1, Удалить хеш - 2");
                string flag = Console.ReadLine();
                switch (flag)
                {
                    case "1":
                        {
                            Console.WriteLine("Задайте новую свертку:");
                            InformationAboutHash(inputValue);
                            ++_countHash;
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

        public bool AllEndAllVariant()//+++++++++++++++++++++++++++++++++++++++++++
        {
            bool end = true;

            foreach (NewHash hash in _packageHash)
                end = end && hash.GetEndAllVariant();
           
            return end;
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

                    if(AllEndAllVariant())
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
