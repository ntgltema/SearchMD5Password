using System;
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
        public void NewHashTest()
        {
            NewHash hash = new NewHash("hash","deR", 3, 5);

            Assert.AreEqual("hash", hash.GetHash());
            Assert.AreEqual(3, hash.GetMinLength());
            Assert.AreEqual(5, hash.GetMaxLength());
            Assert.AreEqual(69, hash.GetAlpha().Length);
            Assert.AreEqual(4830, hash.GetStartPos());
            Assert.AreEqual(4830, hash.GetCurrentPos());
            Assert.AreEqual(1587031809, hash.GetTotalPsw());

            hash.SetCurrentPos(20000);
            Assert.AreEqual(24831, hash.GetCurrentPos());
        }
        /*public void AgentTest()
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
        public void SolvedConvolTest()
        {
            SolvedConvol solvedConvol = new SolvedConvol("020MCSMKMVPASQSACKACAC", "test");

            Assert.AreEqual("020MCSMKMVPASQSACKACAC", solvedConvol.GetHashStr());
            Assert.AreEqual("test", solvedConvol.GetSolvedStr());
            Assert.AreEqual(true, solvedConvol.GetSolved());
        }

        [Test]
        public void HashConvolTest()
        {
            HashConvol hashConvol = new HashConvol("020MCSMKMVPASQSACKACAC");

            Assert.AreEqual("020MCSMKMVPASQSACKACAC", hashConvol.GetHashStr());
            Assert.AreEqual(false, hashConvol.GetSolved());

            hashConvol.SetSolved();
            Assert.AreEqual(true, hashConvol.GetSolved());
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

            manager.AddOrUpdateAgents("192.168.25.9 56123 2");

            Assert.AreEqual(1, manager.GetList().Count);
            Agent agent1 = manager.GetList()[0];
            Assert.AreEqual("192.168.25.9", agent1.GetIp());
            Assert.AreEqual(56123, agent1.GetSpeed());
            Assert.AreEqual(2, agent1.GetCore());

            manager.AddOrUpdateAgents("127.0.0.3 86489 4");

            Assert.AreEqual(2, manager.GetList().Count);
            Agent agent2 = manager.GetList()[1];
            Assert.AreEqual("127.0.0.3", agent2.GetIp());
            Assert.AreEqual(86489, agent2.GetSpeed());
            Assert.AreEqual(4, agent2.GetCore());

            manager.AddOrUpdateAgents("192.168.25.9 45685 8");

            Assert.AreEqual(2, manager.GetList().Count);
            agent1 = manager.GetList()[0];
            Assert.AreEqual("192.168.25.9", agent2.GetIp());
            Assert.AreEqual(45685, agent2.GetSpeed());
            Assert.AreEqual(8, agent2.GetCore());
            queueTest.Purge();
        }

        [Test]
        public void UpdateHashPackageTest()
        {
            Manager manager = new Manager(Manager.CreateQueue("myTestQueue"));
            HashConvol hash1 = new HashConvol("hash1");
            HashConvol hash2 = new HashConvol("hash2");
            HashConvol hash3 = new HashConvol("hash3");

            manager.AddHashInPackage(hash1);
            manager.AddHashInPackage(hash2);
            manager.AddHashInPackage(hash3);
            Assert.AreEqual(3, manager.GetListHash().Count);

            manager.UpdateHashPackage("hash2");
            Assert.AreEqual(2, manager.GetListHash().Count);
            List<HashConvol> hashPackage = manager.GetListHash(); 
            foreach(HashConvol hash in hashPackage)
            {
                Assert.AreNotEqual("hash2", hash.GetHashStr());
            }
            manager.GetQueue().Purge();
        }

        [Test]
        public void UpdateSolvedPackageTest()
        {
            Manager manager = new Manager(Manager.CreateQueue("myTestQueue"));
            SolvedConvol hash1 = new SolvedConvol("hash1", "solved1");
            SolvedConvol hash2 = new SolvedConvol("hash2", "solved2");
            SolvedConvol hash3 = new SolvedConvol("hash3", "solved3");

            manager.AddHashInSolvedPackage(hash1);
            Assert.AreEqual(1, manager.GetListSolved().Count);
            Assert.AreEqual("hash1", manager.GetListSolved()[0].GetHashStr());
            manager.AddHashInSolvedPackage(hash2);
            manager.AddHashInSolvedPackage(hash3);
            Assert.AreEqual(3, manager.GetListSolved().Count);

            manager.UpdateSolvedPackage("hash2");
            Assert.AreEqual(2, manager.GetListSolved().Count);
            List<HashConvol> hashPackage = manager.GetListHash();
            foreach (HashConvol hash in hashPackage)
            {
                Assert.AreNotEqual("hash2", hash.GetHashStr());
            }
            manager.GetQueue().Purge();
        }

        public void UpdateUsedPackageTest()
        {
            Distribution._usedRange.Add("ip;хеш1;range1");
            Distribution._usedRange.Add("ip;хеш2;range1");
            Distribution._usedRange.Add("ip;хеш3;range1");
            Distribution._usedRange.Add("ip;хеш2;range1");
            Distribution._usedRange.Add("ip;хеш4;range1");
            Assert.AreEqual(5, Distribution._usedRange.Count);

            Distribution.UpdateUsedRange("хеш2");
            Assert.AreEqual(3, Distribution._usedRange.Count);

            foreach (string message in Distribution._usedRange)
            {
                string[] dataArrMessage = message.Split(';');

                Assert.AreNotEqual("хеш2", dataArrMessage[1]);
            }

            Distribution.UpdateUsedRange("ip;хеш4;range1");
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
            HashConvol hash1 = new HashConvol("hash1");
            HashConvol hash2 = new HashConvol("hash2");
            HashConvol hash3 = new HashConvol("hash3");
            SolvedConvol solved1 = new SolvedConvol("hash1", "solved1");
            SolvedConvol solved2 = new SolvedConvol("hash2", "solved2");
            SolvedConvol solved3 = new SolvedConvol("hash3", "solved3");

            manager.AddHashInPackage(hash1);
            manager.AddHashInPackage(hash2);
            manager.AddHashInPackage(hash3);
            manager.AddHashInSolvedPackage(solved1);
            manager.AddHashInSolvedPackage(solved2);

            Assert.AreEqual(false, manager.AllSolved());

            manager.AddHashInSolvedPackage(solved3);
            Assert.AreEqual(true, manager.AllSolved());
            manager.GetQueue().Purge();
        }

        */[Test]
        public void NewRangeForAgentTest1()
        {
            Manager manager = new Manager(Manager.CreateQueue("myTestQueue"));

            MessageQueue queue = manager.GetQueue();
            queue.Purge();
            Distribution._usedRange.Clear();
            Agent agent = new Agent(2, 80000, "192.168.25.9");
            NewHash hash1 = new NewHash("хеш1", "d", 1, 6);
            NewHash hash2 = new NewHash("хеш2", "d", 2, 5);
            NewHash hash3 = new NewHash("хеш3", "d", 4, 5);
            manager.AddHashInPackage(hash1);
            manager.AddHashInPackage(hash2);
            manager.AddHashInPackage(hash3);

            Distribution.NewRangeForAgent(queue, agent, manager.GetListHash(), agent.GetCore());
            Assert.AreEqual(2, Distribution._usedRange.Count);
            Assert.AreEqual("192.168.25.9;хеш1;0123456789;0 80000", Distribution._usedRange[0]);
            Assert.AreEqual("192.168.25.9;хеш2;0123456789;10 80000", Distribution._usedRange[1]);
            Assert.AreEqual(80001, hash1.GetCurrentPos());

            Message[] allMessgaes = queue.GetAllMessages();
            Assert.AreEqual("192.168.25.9", allMessgaes[0].Label);
            Assert.AreEqual("192.168.25.9", allMessgaes[1].Label);
            Assert.AreEqual("хеш1;0123456789;0 80000", allMessgaes[0].Body.ToString());
            Assert.AreEqual("хеш2;0123456789;10 80000", allMessgaes[1].Body.ToString());
            queue.Purge();
            Distribution._endAllVariant = false;
        }

       /* [Test]
        public void NewRangeForAgentTest2()
        {
            Manager manager = new Manager(Manager.CreateQueue("myTestQueue"));

            MessageQueue queue = manager.GetQueue();
            queue.Purge();
            Distribution._usedRange.Clear();
            Distribution._totalCountPsw = 100000;
            Agent agent = new Agent(2, 80000, "192.168.25.9");
            manager.AddHashInPackage(new HashConvol("хеш1"));
            manager.AddHashInPackage(new HashConvol("хеш2"));
            manager.AddHashInPackage(new HashConvol("хеш3"));

            Distribution.NewRangeForAgent(queue, agent, manager.GetListHash(), agent.GetCore());
            Assert.AreEqual(2, Distribution._usedRange.Count);
            Assert.AreEqual("192.168.25.9;хеш1 хеш2 хеш3;0 80000", Distribution._usedRange[0]);
            Assert.AreEqual("192.168.25.9;хеш1 хеш2 хеш3;80001 20000", Distribution._usedRange[1]);
            Assert.AreEqual(100002, Distribution._currentPosInRange);

            Message[] allMessgaes = queue.GetAllMessages();
            Assert.AreEqual("192.168.25.9", allMessgaes[0].Label);
            Assert.AreEqual("192.168.25.9", allMessgaes[1].Label);
            Assert.AreEqual("хеш1 хеш2 хеш3+0 80000", allMessgaes[0].Body.ToString());
            Assert.AreEqual("хеш1 хеш2 хеш3+80001 20000", allMessgaes[1].Body.ToString());
            Assert.AreEqual(true, Distribution._endAllVariant);
            queue.Purge();
            Distribution._totalCountPsw = 4432676798592;
            Distribution._currentPosInRange = 0;
            Distribution._endAllVariant = false;
            Distribution._usedRange.Clear();
        }

        /*[Test]
        public void DistrOfRemainRangeTest()
        {
            Manager manager = new Manager(Manager.CreateQueue("myTestQueue"));

            MessageQueue queue = manager.GetQueue();
            queue.Purge();
            Distribution._usedRange.Clear();
            Distribution._totalCountPsw = 230000;

            Agent agent1 = new Agent(2, 26000, "192.168.25.9");
            Agent agent2 = new Agent(3, 12345, "137.168.25.9");
            Agent agent3 = new Agent(4, 56783, "145.168.25.9");
            Agent agent4 = new Agent(5, 85743, "158.168.25.9");
            Agent agent5 = new Agent(2, 50300, "237.168.25.9");
            List<Agent> agents = new List<Agent> { agent1, agent2, agent3, agent4, agent5 };

            Distribution._usedRange.Add("137.168.25.9;хеш1 хеш2;10000 30000");
            Distribution._usedRange.Add("158.168.25.9;хеш3;30000 60000");

            Distribution.DistrOfRemainRange(manager, queue, agents);
            Assert.AreEqual(3, manager.GetList().Count);
            Assert.AreEqual(4, Distribution._usedRange.Count);
            Assert.AreEqual("192.168.25.9;хеш1 хеш2;10000 26000", Distribution._usedRange[0]);
            Assert.AreEqual("145.168.25.9;хеш1 хеш2;36001 4000", Distribution._usedRange[1]);
            Assert.AreEqual("237.168.25.9;хеш3;30000 50300", Distribution._usedRange[2]);
            Assert.AreEqual("192.168.25.9;хеш3;80301 9700", Distribution._usedRange[3]);

            queue.Purge();
            Distribution._totalCountPsw = 4432676798592;
            Distribution._currentPosInRange = 0;
            Distribution._endAllVariant = false;
            Distribution._usedRange.Clear();
        }

        [Test]
        public void ReadingMessagesTest()
        {
            Manager manager = new Manager(Manager.CreateQueue("myTestQueue"));

            MessageQueue queue = manager.GetQueue();
            queue.Purge();
            Distribution._totalCountPsw = 5000000;

            manager.AddHashInPackage(new HashConvol("хеш1"));
            manager.AddHashInPackage(new HashConvol("хеш2"));
            manager.AddHashInPackage(new HashConvol("хеш3"));
            queue.Send("192.168.25.9 56783 2", "ManagerStart");
            queue.Send("237.168.25.9 30000 4", "ManagerStart");
            queue.Send("145.168.25.9 40000 2", "ManagerStart");
            queue.Send("хеш1 solved1", "ManagerSOLVED");
            queue.Send("хеш2 solved2", "ManagerSOLVED");

            Thread messageRead =new Thread(new ThreadStart(manager.ReadingMessages));
            messageRead.Start();
            Thread.Sleep(50);
            Assert.AreEqual(3, manager.GetList().Count);
            Assert.AreEqual(1, manager.GetListHash().Count);
            Assert.AreEqual(2, manager.GetListSolved().Count);
            Assert.AreEqual(8, Distribution._usedRange.Count);

            queue.Send("192.168.25.9;хеш1 хеш2 хеш3;0 56783", "Range");
            Thread.Sleep(50);
            Assert.AreEqual(7, Distribution._usedRange.Count);
            queue.Send("хеш3 solved3", "ManagerSOLVED");
            Thread.Sleep(50);

            queue.Purge();
            Distribution._totalCountPsw = 4432676798592;
            Distribution._currentPosInRange = 0;
            Distribution._endAllVariant = false;
            messageRead.Abort();
            Distribution._usedRange.Clear();
        }*/
    }

    class Program
    {
        static void Main(string[] args)
        {
            UnitTest1 test = new UnitTest1();

            test.NewHashTest();
            test.NewRangeForAgentTest1();
            /*test.AgentTest();
            test.SolvedConvolTest();
            test.HashConvolTest();
            test.CreateManager();
            test.UpdateHashPackageTest();
            test.UpdateSolvedPackageTest();
            test.AllSolvedTest();
            test.UpdateUsedPackageTest();
            test.NewRangeForAgentTest1();
            test.NewRangeForAgentTest2();
            test.DistrOfRemainRangeTest();
            test.ReadingMessagesTest();*/
        }
    }
}
