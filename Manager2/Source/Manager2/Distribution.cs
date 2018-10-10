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
        static public long _totalCountPsw = 4432676798592;
        static public long _currentPosInRange = 0;
        static public bool _endAllVariant = false;
        static private int _countMessage = 0;
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

        public static void NewRangeForAgent(MessageQueue queue, Agent agent, List<HashConvol> hashPackage, int countTask)//+++++++++++++++++++++++++++++++++++++++++++
        {
            string allHash = "";
            for(int i = 0; i < hashPackage.Count(); ++i)
            {
                if (i != hashPackage.Count() - 1)
                    allHash += hashPackage[i].GetHashStr() + " ";
                else
                    allHash += hashPackage[i].GetHashStr();
            }

            for (int i = 0; i < countTask; ++i)
            {
                long startRange = _currentPosInRange;
                long countPswInRange = agent.GetSpeed();

                countPswInRange = (startRange + countPswInRange) < _totalCountPsw ? countPswInRange : _totalCountPsw - startRange + 1;

                if (startRange < _totalCountPsw)
                {
                    queue.Send(allHash + ";" + startRange + " " + countPswInRange, agent.GetIp());
                    _usedRange.Add(agent.GetIp() + ";" + allHash + ";" + startRange + " " + countPswInRange);
                    _currentPosInRange += countPswInRange + 1;
                }

                if (_currentPosInRange >= _totalCountPsw)
                {
                    _endAllVariant = true;
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
                string rangeStr = dataArr[2];

                string[] dataArrRange = rangeStr.Split(' ');
                int startRange = int.Parse(dataArrRange[0]);
                int totalPswInRange = int.Parse(dataArrRange[1]);

                int countPswInRange = 0;
                int indexCurrentAgent = _countMessage % agents.Count();

                while (startRange < totalPswInRange + startRange)
                {
                    indexCurrentAgent = _countMessage % agents.Count();       
                    countPswInRange = agents[indexCurrentAgent].GetSpeed() > totalPswInRange ? totalPswInRange : agents[indexCurrentAgent].GetSpeed();

                    queue.Send(dataArr[1] + ";" + startRange + " " + countPswInRange, agents[indexCurrentAgent].GetIp());
                    tempUsedRange.Add(agents[indexCurrentAgent].GetIp() + ";" + dataArr[1] + ";" + startRange + " " + countPswInRange);

                    totalPswInRange -= countPswInRange;
                    startRange += countPswInRange + 1;
                    ++_countMessage;
                }
            }

            _usedRange = tempUsedRange;
        }   
    }
}
