using System;
using System.Messaging;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace Manager2
{
    public class Distribution
    {
        static public List<string> _usedRange = new List<string>();
        static public int _countMessage = 0;
        static private List<Agent> _agents = new List<Agent>();

        static void ClearLine()
        {
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, Console.CursorTop - 1);
        }

        static void WriteCommand()
        {
            Console.Write("command: ");
        }

        public static void UpdateUsedRange(string removeHash)
        {
            _usedRange.RemoveAll(delegate (string message)
            {
                string[] dataArrMessage = message.Split(';');
                if (dataArrMessage[1] == removeHash || message == removeHash)
                    return true;

                return false;
            });
        }

        public static bool NewRangeForAgent(MessageQueue queue, Agent agent, List<Hash> hashPackage, int countTask)
        {
            if (hashPackage.Count > 0)
            {
                for (int i = 0; i < countTask; ++i)
                {
                    Hash hash = hashPackage[_countMessage % hashPackage.Count];
                    long startRange = hash.GetCurrentPos();
                    long countPswInRange = agent.GetSpeed();

                    countPswInRange = (startRange + countPswInRange) < hash.GetTotalPsw() ? countPswInRange : hash.GetTotalPsw() - startRange + 1;

                    if (startRange < hash.GetTotalPsw())
                    {
                        long totalSecond = (long)DateTime.Now.Subtract(new DateTime()).TotalSeconds;
                        queue.Send(hash.GetHash() + ";" + hash.GetAlphaStr() + ";" + startRange + " " + countPswInRange + ";" + totalSecond, agent.GetIp());
                        _usedRange.Add(agent.GetIp() + ";" + hash.GetHash() + ";" + hash.GetAlphaStr() + ";" + startRange + " " + countPswInRange + ";" + totalSecond);
                        hash.SetCurrentPos(countPswInRange);
                        ++_countMessage;
                    }

                    if (hash.GetCurrentPos() >= hash.GetTotalPsw())
                    {
                        hash.SetEndAllVariant();
                    }

                    if (startRange >= hash.GetTotalPsw())
                    {
                        for (int j = i; j < countTask; ++j)
                        {
                            Manager._tempFreeAgent.Add(agent);
                        }

                        return false;
                    }
                }
            }
            else
            {
                for (int j = 0; j < countTask; ++j)
                {
                    Manager._tempFreeAgent.Add(agent);
                }

                return false;
            }

            return true;
        }

        public static bool CheckDateCreate()
        {
            List<string> tempUsedRange = new List<string>();
            tempUsedRange.AddRange(_usedRange);

            foreach (string range in tempUsedRange)
            {
                string[] dataArr = range.Split(';');
                string ipRemove = dataArr[0];
                long dataOfCreateRange = long.Parse(dataArr[4]);

                if (DateTime.Now.Subtract(new DateTime()).TotalSeconds - dataOfCreateRange > 120)
                    return false;
            }

            return true;
        }

        public static void DistrOfRemainRange(Manager manager, MessageQueue queue, List<Agent> agents)//+++++++++++++++++++++++++++++++++++++++++++
        {
            _countMessage = 0;
            List<string> tempUsedRange = new List<string>();
            tempUsedRange.AddRange(_usedRange);
            _usedRange.Clear();

            foreach (string range in tempUsedRange)
            {
                string[] dataArr = range.Split(';');
                string ipRemove = dataArr[0];
                long dataOfCreateRange = long.Parse(dataArr[4]);

                if (DateTime.Now.Subtract(new DateTime()).TotalSeconds - dataOfCreateRange > 120)
                {
                    if (ipRemove != "")
                    {
                        agents.RemoveAll(delegate (Agent agent)
                        {
                            if (agent.GetIp() == ipRemove)
                                return true;
                            return false;
                        });
                    }
                }
            }

            manager.SetAgents(agents);
            Console.WriteLine("Количество агентов {0}", manager.GetList().Count);

            if (agents.Count != 0)
            {
                List<string> temp = new List<string>();

                foreach (string range in tempUsedRange)
                {
                    string[] dataArr = range.Split(';');
                    string rangeStr = dataArr[3];
                    long dataOfCreateRange = long.Parse(dataArr[4]);

                    if (DateTime.Now.Subtract(new DateTime()).TotalSeconds - dataOfCreateRange > 120)
                    {
                        string[] dataArrRange = rangeStr.Split(' ');
                        long startRange = long.Parse(dataArrRange[0]);
                        long totalPswInRange = long.Parse(dataArrRange[1]);

                        long countPswInRange = 0;
                        int indexCurrentAgent = 0;

                        while (startRange < totalPswInRange + startRange)
                        {
                            indexCurrentAgent = _countMessage % agents.Count();
                            countPswInRange = agents[indexCurrentAgent].GetSpeed() > totalPswInRange ? totalPswInRange : agents[indexCurrentAgent].GetSpeed();

                            long totalSecond = (long)DateTime.Now.Subtract(new DateTime()).TotalSeconds;

                            queue.Send(dataArr[1] + ";" + dataArr[2] + ";" + startRange + " " + countPswInRange + ";" + totalSecond.ToString(), agents[indexCurrentAgent].GetIp());
                            temp.Add(agents[indexCurrentAgent].GetIp() + ";" + dataArr[1] + ";" + dataArr[2] + ";" + startRange + " " + countPswInRange + ";" + totalSecond.ToString());

                            totalPswInRange -= countPswInRange;
                            startRange += countPswInRange;
                            ++_countMessage;
                        }
                    }
                    else
                    {
                        temp.Add(range);
                    }
                }

                tempUsedRange.Clear();
                tempUsedRange.AddRange(temp);
            }
            else
            {
                Console.WriteLine("Нет активыных агентов!!!");
            }

            _usedRange.AddRange(tempUsedRange);
        }
    }
}
