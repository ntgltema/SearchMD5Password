﻿using System;
using System.Messaging;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace Manager2
{
    class AgentWithMessage
    {
        private string _ip;
        private List<string> _messages;

        public AgentWithMessage(string ip)
        {
            _ip = ip;
            _messages = new List<string>();
        }

        public void AddMsg(string msg)
        {
            _messages.Add(msg);
        }

        public string GetIp()
        {
            return _ip;
        }

        public List<string> GetList()
        {
            return _messages;
        }
    };
   
    class Distribution
    {
        static public List<string> usedRange = new List<string>();
        static public long _totalCountPsw = 4432676798592;
        static public long _countRangeOutput = 0;
        
        public static void NewRangeForAgent(MessageQueue queue, Agent agent, List<HashConvol> hashPackage, int countTask)
        {
            for (int i = 0; i < countTask; ++i)
            {
                long indexHashW = _countRangeOutput % hashPackage.Count();
                long startRange = hashPackage[(int)indexHashW].GetCurrentPos();
                long countPswInRange = agent.GetSpeed();

                countPswInRange = (startRange + countPswInRange) < _totalCountPsw ? countPswInRange : _totalCountPsw - startRange + 1;

                if (startRange < _totalCountPsw)
                {
                    queue.Send(hashPackage[(int)indexHashW].GetHashStr() + " " + startRange + " " + countPswInRange, agent.GetIp());
                    usedRange.Add(agent.GetIp() + " " + hashPackage[(int)indexHashW].GetHashStr() + " " + startRange + " " + countPswInRange);
                }

                ++_countRangeOutput;
                hashPackage[(int)indexHashW].SetCurrentPos(countPswInRange);
            }
        }

        public static void OldRangeForAgent(List<Agent> agents)
        {
            foreach(string range in usedRange)
            {

            }
        }
   
        /*public static List<AgentWithMessage> MessageWithRangePsw(List<Agent> agents)
        {
            List<AgentWithMessage> resultDistr = new List<AgentWithMessage>();

            foreach (Agent oneAgent in agents)
            {
                AgentWithMessage agentWithRange = new AgentWithMessage(oneAgent.GetIp());
                int countCore = oneAgent.GetCore();

                for (int i = 0; i < countCore; ++i)
                {
                    long startRange = _currentPos;
                    long countPswInRange = oneAgent.GetSpeed();

                    countPswInRange = (startRange + countPswInRange) < _totalCountPsw ? countPswInRange : _totalCountPsw - startRange + 1;

                    if (startRange < _totalCountPsw)
                        agentWithRange.AddMsg(startRange.ToString() + " " + countPswInRange.ToString());

                    _currentPos += countPswInRange + 1;
                }

                resultDistr.Add(agentWithRange);
            }

            return resultDistr;
        }*/
    }
}
