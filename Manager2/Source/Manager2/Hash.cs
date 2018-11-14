using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace Manager2
{
    public class Hash
    {
        private string _hash;
        private int _minLength;
        private int _maxLength;
        private long _startPos;
        private long _currentPos;
        private long _totalPsw;
        private string _alphaStr;
        private bool _endAllVariant;
        private List<string> _usedRange;

        public Hash(string hash, string alpha, int minLength, int maxLength)
        {
            _hash = hash;
            _minLength = minLength;
            _maxLength = maxLength;
            _alphaStr = InitializeAlpha(alpha);
            _startPos = InitializeStartPos(minLength);
            _currentPos = _startPos;
            _totalPsw = InitializeTotalPsw(maxLength);
            _endAllVariant = false;
            _usedRange = new List<string>();
        }

        public string GetHash() { return _hash; }

        public string GetAlphaStr() { return _alphaStr; }

        public int GetMinLength() { return _minLength; }

        public int GetMaxLength() { return _maxLength; }

        public long GetStartPos() { return _startPos; }

        public long GetCurrentPos() { return _currentPos; }

        public void SetCurrentPos(long delta) { _currentPos += delta; }

        public long GetTotalPsw() { return _totalPsw; }

        public bool GetEndAllVariant() { return _endAllVariant; }

        public void SetEndAllVariant() { _endAllVariant = true; }

        public List<string> GetListUsedRange() { return _usedRange; }

        public void SetListUsedRange(List<string> other) { _usedRange = other; }

        private long InitializeStartPos(int length)
        {
            long outStart = 0;

            for (int i = 1; i < length; ++i)
            {
                outStart += (long)Math.Pow(_alphaStr.Length, i);
            }

            return outStart;
        }

        private long InitializeTotalPsw(int maxLenght)
        {
            long outStart = 0;

            for (int i = 1; i < maxLenght + 1; ++i)
            {
                outStart += (long)Math.Pow(_alphaStr.Length, i);
            }

            return outStart;
        }

        private string InitializeAlpha(string alpha)
        {
            string outAlpha = "";

            foreach (char ch in alpha)
            {
                switch (ch)
                {
                    case 'd':
                        {
                            outAlpha += Manager.digit;
                            break;
                        }
                    case 'e':
                        {
                            outAlpha += Manager.engLow;
                            break;
                        }
                    case 'E':
                        {
                            outAlpha += Manager.engUpp;
                            break;
                        }
                    case 'r':
                        {
                            outAlpha += Manager.rusLow;
                            break;
                        }
                    case 'R':
                        {
                            outAlpha += Manager.rusUpp;
                            break;
                        }
                    default:
                        {
                            break;
                        }
                }
            }

            return outAlpha;
        }
    }
}
