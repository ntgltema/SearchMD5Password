using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manager2
{
    public class FoundHashes
    {
        private string _hashStr;
        private string _solved;
        private bool _solvedBool;

        public FoundHashes(string hash, string solved)
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
}
