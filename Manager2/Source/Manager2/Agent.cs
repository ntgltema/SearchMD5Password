using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manager2
{
    public class Agent
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

        public Agent()
        {
            _core = 0;
            _speed = 0;
            _ip = "";
        }

        public Agent(Agent other)
        {
            _core = other._core;
            _speed = other._speed;
            _ip = other._ip;
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
}
