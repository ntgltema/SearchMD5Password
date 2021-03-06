﻿using System;
using System.Collections.Generic;
using Manager2;
using NUnit.Framework;
using System.Messaging;
using System.Threading;

namespace Manager2.Test
{
    public class UnitTest1
    {
        [SetUp]
        public void HashTest()
        {
            Hash hash = new Hash("hash","deR", 3, 5);

            Assert.AreEqual("hash", hash.GetHash());
            Assert.AreEqual(3, hash.GetMinLength());
            Assert.AreEqual(5, hash.GetMaxLength());
            Assert.AreEqual("0123456789abcdefghijklmnopqrstuvwxyzАБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЬЫЭЮЯ", hash.GetAlphaStr());
            Assert.AreEqual(4830, hash.GetStartPos());
            Assert.AreEqual(4830, hash.GetCurrentPos());
            Assert.AreEqual(1587031809, hash.GetTotalPsw());

            hash.SetCurrentPos(20000);
            Assert.AreEqual(24830, hash.GetCurrentPos());
        }

        public void AgentTest()
        {
            Agent agent = new Agent(2, 57865, "192.168.25.9");

            Assert.AreEqual(2, agent.GetCore());
            Assert.AreEqual(57865, agent.GetSpeed());
            Assert.AreEqual("192.168.25.9", agent.GetIp());

            agent.SetCore(4);
            Assert.AreEqual(4, agent.GetCore());

            agent.SetSpeed(88926);
            Assert.AreEqual(88926, agent.GetSpeed());
        }

        [Test]
        public void FoundHashesTest()
        {
            FoundHashes solvedConvol = new FoundHashes("020MCSMKMVPASQSACKACAC", "test");

            Assert.AreEqual("020MCSMKMVPASQSACKACAC", solvedConvol.GetHashStr());
            Assert.AreEqual("test", solvedConvol.GetSolvedStr());
            Assert.AreEqual(true, solvedConvol.GetSolved());
        }

        [Test]
        public void CreateManager()
        {
            Manager manager = new Manager(Manager.CreateQueue("myTestQueue"));
            MessageQueue queueTest = manager.GetQueue();

            Assert.AreEqual("DIRECT=OS:beliy-pc\\private$\\myTestQueue", queueTest.FormatName);
            Assert.AreEqual(".\\private$\\myTestQueue", queueTest.Path);

            Assert.AreEqual(0, manager.GetList().Count);
            Assert.AreEqual(0, manager.GetListHash().Count);
            Assert.AreEqual(0, manager.GetListSolved().Count);
            queueTest.Purge();
        }

        [Test]
        public void AddNewAgentAndUpadateTest()
        {
            Manager manager = new Manager(Manager.CreateQueue("myTestQueue"));
            MessageQueue queueTest = manager.GetQueue();

            Hash hash1 = new Hash("hash1", "d", 1, 5);
            manager.AddHashInPackage(hash1);

            manager.AddOrUpdateAgents("192.168.25.9 56123 2");

            Assert.AreEqual(1, manager.GetList().Count);
            Agent agent1 = manager.GetList()[0];
            Assert.AreEqual("192.168.25.9", agent1.GetIp());
            Assert.AreEqual(3367380, agent1.GetSpeed());
            Assert.AreEqual(2, agent1.GetCore());

            manager.AddOrUpdateAgents("127.0.0.3 86489 4");

            Assert.AreEqual(2, manager.GetList().Count);
            Agent agent2 = manager.GetList()[1];
            Assert.AreEqual("127.0.0.3", agent2.GetIp());
            Assert.AreEqual(5189340, agent2.GetSpeed());
            Assert.AreEqual(4, agent2.GetCore());

            manager.AddOrUpdateAgents("192.168.25.9 45685 8");

            Assert.AreEqual(2, manager.GetList().Count);
            agent1 = manager.GetList()[0];
            Assert.AreEqual("192.168.25.9", agent1.GetIp());
            Assert.AreEqual(2741100, agent1.GetSpeed());
            Assert.AreEqual(8, agent1.GetCore());
            queueTest.Purge();
            Distribution._usedRange.Clear();
        }

        [Test]
        public void UpdateHashPackageTest()
        {
            Manager manager = new Manager(Manager.CreateQueue("myTestQueue"));
            Hash hash1 = new Hash("hash1", "d", 2, 3);
            Hash hash2 = new Hash("hash2", "er", 3,4);
            Hash hash3 = new Hash("hash3", "R", 2, 5);

            manager.AddHashInPackage(hash1);
            manager.AddHashInPackage(hash2);
            manager.AddHashInPackage(hash3);
            Assert.AreEqual(3, manager.GetListHash().Count);

            manager.UpdateHashPackage("hash2");
            Assert.AreEqual(2, manager.GetListHash().Count);
            List<Hash> hashPackage = manager.GetListHash(); 
            foreach(Hash hash in hashPackage)
            {
                Assert.AreNotEqual("hash2", hash.GetHash());
            }
            manager.GetQueue().Purge();
        }

        [Test]
        public void UpdateSolvedPackageTest()
        {
            Manager manager = new Manager(Manager.CreateQueue("myTestQueue"));
            FoundHashes hash1 = new FoundHashes("hash1", "solved1");
            FoundHashes hash2 = new FoundHashes("hash2", "solved2");
            FoundHashes hash3 = new FoundHashes("hash3", "solved3");

            manager.AddHashInSolvedPackage(hash1);
            Assert.AreEqual(1, manager.GetListSolved().Count);
            Assert.AreEqual("hash1", manager.GetListSolved()[0].GetHashStr());
            manager.AddHashInSolvedPackage(hash2);
            manager.AddHashInSolvedPackage(hash3);
            Assert.AreEqual(3, manager.GetListSolved().Count);

            manager.UpdateSolvedPackage("hash2");
            Assert.AreEqual(2, manager.GetListSolved().Count);
            List<Hash> hashPackage = manager.GetListHash();
            foreach (Hash hash in hashPackage)
            {
                Assert.AreNotEqual("hash2", hash.GetHash());
            }
            manager.GetQueue().Purge();
        }

        public void UpdateUsedPackageTest()
        {
            Distribution._usedRange.Add("ip;хеш1;alpha1;range1;time1");
            Distribution._usedRange.Add("ip;хеш2;alpha2;range1;time2");
            Distribution._usedRange.Add("ip;хеш3;alpha3;range1;time3");
            Distribution._usedRange.Add("ip;хеш2;alpha4;range1;time4");
            Distribution._usedRange.Add("ip;хеш4;alpha5;range1;time5");
            Assert.AreEqual(5, Distribution._usedRange.Count);

            Distribution.UpdateUsedRange("хеш2");
            Assert.AreEqual(3, Distribution._usedRange.Count);

            foreach (string message in Distribution._usedRange)
            {
                string[] dataArrMessage = message.Split(';');

                Assert.AreNotEqual("хеш2", dataArrMessage[1]);
            }

            Distribution.UpdateUsedRange("ip;хеш4;alpha5;range1;time5");
            Assert.AreEqual(2, Distribution._usedRange.Count);

            foreach (string message in Distribution._usedRange)
            {
                string[] dataArrMessage = message.Split(';');

                Assert.AreNotEqual("хеш4", dataArrMessage[1]);
            }
        }

        [Test]
        public void AllSolvedTest()
        {
            Manager manager = new Manager(Manager.CreateQueue("myTestQueue"));
            Hash hash1 = new Hash("hash1", "der", 1, 2);
            Hash hash2 = new Hash("hash2", "r", 2, 3);
            Hash hash3 = new Hash("hash3", "R", 1, 4);
            FoundHashes solved1 = new FoundHashes("hash1", "solved1");
            FoundHashes solved2 = new FoundHashes("hash2", "solved2");
            FoundHashes solved3 = new FoundHashes("hash3", "solved3");

            manager.AddHashInPackage(hash1);
            manager.AddHashInPackage(hash2);
            manager.AddHashInPackage(hash3);
            manager.AddHashInSolvedPackage(solved1);
            manager.AddHashInSolvedPackage(solved2);

           // Assert.AreEqual(false, manager.AllSolved());

            manager.AddHashInSolvedPackage(solved3);
          //  Assert.AreEqual(true, manager.AllSolved());
            manager.GetQueue().Purge();
        }

        [Test]
        public void NewRangeForAgentTest1()
        {
            Manager manager = new Manager(Manager.CreateQueue("myTestQueue"));

            MessageQueue queue = manager.GetQueue();
            queue.Purge();
            Distribution._usedRange.Clear();
            Agent agent = new Agent(2, 80000, "192.168.25.9");
            Hash hash1 = new Hash("хеш1", "d", 1, 6);
            Hash hash2 = new Hash("хеш2", "d", 2, 5);
            Hash hash3 = new Hash("хеш3", "d", 4, 5);
            manager.AddHashInPackage(hash1);
            manager.AddHashInPackage(hash2);
            manager.AddHashInPackage(hash3);

            Distribution.NewRangeForAgent(queue, agent, manager.GetListHash(), agent.GetCore());
            Assert.AreEqual(2, Distribution._usedRange.Count);

            string timeSendMessage0 = (Distribution._usedRange[0]).Split(';')[4];
            string timeSendMessage1 = (Distribution._usedRange[1]).Split(';')[4];

            Assert.AreEqual("192.168.25.9;хеш1;0123456789;0 80000" + ";" + timeSendMessage0.ToString(), Distribution._usedRange[0]);
            Assert.AreEqual("192.168.25.9;хеш2;0123456789;10 80000" + ";" + timeSendMessage1.ToString(), Distribution._usedRange[1]);
            Assert.AreEqual(80000, hash1.GetCurrentPos());

            Message[] allMessgaes = queue.GetAllMessages();
            Assert.AreEqual("192.168.25.9", allMessgaes[0].Label);
            Assert.AreEqual("192.168.25.9", allMessgaes[1].Label);
            Assert.AreEqual("хеш1;0123456789;0 80000" + ";" + timeSendMessage0.ToString(), allMessgaes[0].Body.ToString());
            Assert.AreEqual("хеш2;0123456789;10 80000" + ";" + timeSendMessage1.ToString(), allMessgaes[1].Body.ToString());
            queue.Purge();
        }

        [Test]
        public void NewRangeForAgentTest2()
        {
            Manager manager = new Manager(Manager.CreateQueue("myTestQueue"));

            MessageQueue queue = manager.GetQueue();
            queue.Purge();
            Distribution._usedRange.Clear();
            Distribution._countMessage = 0;
            Agent agent = new Agent(2, 80000, "192.168.25.9");
            manager.AddHashInPackage(new Hash("хеш1", "d", 1, 3));
            manager.AddHashInPackage(new Hash("хеш2", "e", 2, 3));
            manager.AddHashInPackage(new Hash("хеш3", "R", 4, 6));

            Distribution.NewRangeForAgent(queue, agent, manager.GetListHash(), agent.GetCore());
            Assert.AreEqual(2, Distribution._usedRange.Count);

            string timeSendMessage0 = (Distribution._usedRange[0]).Split(';')[4];
            string timeSendMessage1 = (Distribution._usedRange[1]).Split(';')[4];

            Assert.AreEqual("192.168.25.9;хеш1;0123456789;0 1111" + ";" + timeSendMessage0.ToString(), Distribution._usedRange[0]);
            Assert.AreEqual("192.168.25.9;хеш2;abcdefghijklmnopqrstuvwxyz;26 18253" + ";" + timeSendMessage1.ToString(), Distribution._usedRange[1]);

            Message[] allMessgaes = queue.GetAllMessages();
            Assert.AreEqual("192.168.25.9", allMessgaes[0].Label);
            Assert.AreEqual("192.168.25.9", allMessgaes[1].Label);
            Assert.AreEqual("хеш1;0123456789;0 1111" + ";" + timeSendMessage0.ToString(), allMessgaes[0].Body.ToString());
            Assert.AreEqual("хеш2;abcdefghijklmnopqrstuvwxyz;26 18253" + ";" + timeSendMessage1.ToString(), allMessgaes[1].Body.ToString());
            queue.Purge();
            Distribution._usedRange.Clear();
        }

        [Test]
        public void DistrOfRemainRangeTest()
        {
            Manager manager = new Manager(Manager.CreateQueue("myTestQueue"));

            MessageQueue queue = manager.GetQueue();
            queue.Purge();
            Distribution._usedRange.Clear();
        
            Agent agent1 = new Agent(2, 24000, "192.168.25.9");
            Agent agent2 = new Agent(3, 12345, "137.168.25.9");
            Agent agent3 = new Agent(4, 56783, "145.168.25.9");
            Agent agent4 = new Agent(5, 85743, "158.168.25.9");
            Agent agent5 = new Agent(2, 45000, "237.168.25.9");
            List<Agent> agents = new List<Agent> { agent1, agent2, agent3, agent4, agent5 };

            Distribution._usedRange.Add("137.168.25.9;хеш2;abcdefghijklmnopqrstuvwxyz;10000 30000;643");
            Distribution._usedRange.Add("158.168.25.9;хеш3;0123456789;30000 60000;643");

            long totalSecond = (long)DateTime.Now.Subtract(new DateTime()).TotalSeconds;
            Distribution._usedRange.Add("158.168.25.9;хеш4;prostavbnam5;30000 60000;" + totalSecond.ToString());

            Distribution.DistrOfRemainRange(manager, queue, agents);
            Assert.AreEqual(3, manager.GetList().Count);
            Assert.AreEqual(5, Distribution._usedRange.Count);


            string timeSendMessage0 = (Distribution._usedRange[0]).Split(';')[4];
            string timeSendMessage1 = (Distribution._usedRange[1]).Split(';')[4];
            string timeSendMessage2 = (Distribution._usedRange[2]).Split(';')[4];
            string timeSendMessage3 = (Distribution._usedRange[3]).Split(';')[4];

            Assert.AreEqual("192.168.25.9;хеш2;abcdefghijklmnopqrstuvwxyz;10000 24000" + ";" + timeSendMessage0.ToString(), Distribution._usedRange[0]);
            Assert.AreEqual("145.168.25.9;хеш2;abcdefghijklmnopqrstuvwxyz;34000 6000" + ";" + timeSendMessage1.ToString(), Distribution._usedRange[1]);
            Assert.AreEqual("237.168.25.9;хеш3;0123456789;30000 45000" + ";" +  timeSendMessage2.ToString(), Distribution._usedRange[2]);
            Assert.AreEqual("192.168.25.9;хеш3;0123456789;75000 15000" + ";" + timeSendMessage3.ToString(), Distribution._usedRange[3]);
            Assert.AreEqual("158.168.25.9;хеш4;prostavbnam5;30000 60000;" + totalSecond.ToString(), Distribution._usedRange[4]);

            queue.Purge();
            Distribution._usedRange.Clear();
        }

        [Test]
        public void ReadingMessagesTest()
        {
            Manager manager = new Manager(Manager.CreateQueue("myTestQueue"));

            MessageQueue queue = manager.GetQueue();
            queue.Purge();

            Hash hash1 = new Hash("hash1", "er", 2, 3);
            Hash hash2 = new Hash("hash2", "d", 3, 4);
            Hash hash3 = new Hash("hash3", "R", 2, 5);
            manager.AddHashInPackage(hash1);
            manager.AddHashInPackage(hash2);
            manager.AddHashInPackage(hash3);

            queue.Send("192.168.25.9 56783 2", "ManagerStart");
            queue.Send("237.168.25.9 30000 4", "ManagerStart");
            queue.Send("145.168.25.9 40000 2", "ManagerStart");
            queue.Send("hash1 solved1", "ManagerSOLVED");
            queue.Send("hash3 solved3", "ManagerSOLVED");

            Thread messageRead = new Thread(new ThreadStart(manager.ReadingMessages));
            messageRead.Start();
            Thread.Sleep(50);
            Assert.AreEqual(3, manager.GetList().Count);
            Assert.AreEqual(1, manager.GetListHash().Count);
            Assert.AreEqual(2, manager.GetListSolved().Count);
            Assert.AreEqual(1, Distribution._usedRange.Count);

            queue.Send(Distribution._usedRange[0], "Range");
            Thread.Sleep(50);
            Assert.AreEqual(0, Distribution._usedRange.Count);
            queue.Send("hash2 solved3", "ManagerSOLVED");
            Thread.Sleep(50);
            Assert.AreEqual(0, manager.GetListHash().Count);
            Assert.AreEqual(3, manager.GetListSolved().Count);

            messageRead.Abort();
            queue.Purge();
            messageRead.Abort();
            Distribution._usedRange.Clear();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            UnitTest1 test = new UnitTest1();

            test.HashTest();
            test.NewRangeForAgentTest1();
            test.AgentTest();
            test.FoundHashesTest();
            test.CreateManager();
            test.AddNewAgentAndUpadateTest();
            test.UpdateHashPackageTest();
            test.UpdateSolvedPackageTest();
          //  test.AllSolvedTest();
            test.UpdateUsedPackageTest();
            test.NewRangeForAgentTest1();
            test.NewRangeForAgentTest2();
            test.DistrOfRemainRangeTest();
            test.ReadingMessagesTest();
        }
    }
}
