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
        static public bool _endAllVariant = true;
        static public int _countMessage = 0;
        static private List<Agent> _agents = new List<Agent>();

        public static void UpdateUsedRange(string removeHash)//+++++++++++++++++++++++++++++++++++++++++++
        {
            _usedRange.RemoveAll(delegate (string message)
            {
                string[] dataArrMessage = message.Split(';');
                if (dataArrMessage[1] == removeHash || message == removeHash)
                    return true;

                return false;
            });            
        }

        public static void NewRangeForAgent(MessageQueue queue, Agent agent, List<NewHash> hashPackage, int countTask)//+++++++++++++++++++++++++++++++++++++++++++
        {
            if (hashPackage.Count > 0)
            {
                for (int i = 0; i < countTask; ++i)
                {
                    NewHash hash = hashPackage[_countMessage % hashPackage.Count];
                    long startRange = hash.GetCurrentPos();
                    long countPswInRange = agent.GetSpeed();

                    countPswInRange = (startRange + countPswInRange) < hash.GetTotalPsw() ? countPswInRange : hash.GetTotalPsw() - startRange + 1;

                    if (startRange < hash.GetTotalPsw())
                    {
                        queue.Send(hash.GetHash() + ";" + hash.GetAlphaStr() + ";" + startRange + " " + countPswInRange, agent.GetIp());
                        _usedRange.Add(agent.GetIp() + ";" + hash.GetHash() + ";" + hash.GetAlphaStr() + ";" + startRange + " " + countPswInRange);
                        hash.SetCurrentPos(countPswInRange);
                        ++_countMessage;
                        Console.WriteLine("Выдали задание");
                    }

                    if (startRange >= hash.GetTotalPsw())
                    {
                        Console.WriteLine("Не выдали задание по причине конца заданий");
                    }

                    if (hash.GetCurrentPos() >= hash.GetTotalPsw())
                    {
                        hash.SetEndAllVariant();
                    }
                }
            }
        }

        public static void DistrOfRemainRange(Manager manager, MessageQueue queue, List<Agent> agents)//+++++++++++++++++++++++++++++++++++++++++++
        {
            _countMessage = 0;

            foreach(string range in _usedRange)
            {
                string[] dataArr = range.Split(';');
                string ipRemove = dataArr[0];

                if (ipRemove != "")
                { 
                    agents.RemoveAll(delegate(Agent agent) {
                        if (agent.GetIp() == ipRemove)
                            return true;
                        return false;
                    });
                }
            }

            manager.SetAgents(agents);
            List<string> tempUsedRange = new List<string>();
                                         
            foreach (string range in _usedRange)
            {
                string[] dataArr = range.Split(';');
                string rangeStr = dataArr[3];

                string[] dataArrRange = rangeStr.Split(' ');
                long startRange = long.Parse(dataArrRange[0]);
                long totalPswInRange = long.Parse(dataArrRange[1]);

                long countPswInRange = 0;
                int indexCurrentAgent = 0;
               
                while (startRange < totalPswInRange + startRange)
                {
                    indexCurrentAgent = _countMessage % agents.Count();       
                    countPswInRange = agents[indexCurrentAgent].GetSpeed() > totalPswInRange ? totalPswInRange : agents[indexCurrentAgent].GetSpeed();

                    queue.Send(dataArr[1]+";" + dataArr[2] + ";" + startRange + " " + countPswInRange, agents[indexCurrentAgent].GetIp());
                    tempUsedRange.Add(agents[indexCurrentAgent].GetIp() + ";" + dataArr[1] + ";" + dataArr[2] + ";" + startRange + " " + countPswInRange);

                    totalPswInRange -= countPswInRange;
                    startRange += countPswInRange;
                    ++_countMessage;
                }
            }

            _usedRange = tempUsedRange;
        }   
    }
}
