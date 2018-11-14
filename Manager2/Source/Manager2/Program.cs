using System;
using System.Messaging;
using System.Threading;
using System.Security.Cryptography;
using System.Windows;

namespace Manager2
{
    class Program
    {
        static void Main(string[] args)
        {
            Manager manager = new Manager(Manager.CreateQueue("testName"));
            MessageQueue queue = manager.GetQueue();
            queue.Formatter = new XmlMessageFormatter(new String[] { "System.String" });

            Thread message = new Thread(new ThreadStart(manager.ReadingMessages));
            manager.ReadPackage(message);
        }
    }
}
