using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Agent.Tests
{
    [TestClass()]
    public class AgentTests
    {
        Agent agent = new Agent();

        [TestMethod()]
        public void AgentTest()
        {
            Assert.AreEqual(agent._queue.FormatName, @"FormatName:DIRECT=OS:ostan\private$\testname");

            string Host = System.Net.Dns.GetHostName();
            Assert.AreEqual(agent._ip, System.Net.Dns.GetHostByName(Host).AddressList[0].ToString());

            Assert.AreEqual(agent._core, Environment.ProcessorCount);

            Assert.IsNotNull(agent._speed);
        }

        [TestMethod()]
        public void SetPassPerSecondsTest()
        {
            agent._speed = -5;
            agent.SetPassPerSeconds();
            Assert.AreNotEqual(agent._speed, -5);
        }

        [TestMethod()]
        public void SendGoodMessageTest()
        {

        }

        [TestMethod()]
        public void DelHashTest()
        {
            Task task = new Task();
            task._hash = "098F6BCD4621D373CADE4E832627B4F6";

            Task task1 = new Task();
            task1._hash = "DF64DC2EB4A0B85091DD31EB4923EAAC";

            Task task2 = new Task();
            task2._hash = "202CB962AC59075B964B07152D234B70";

            agent.tasks.Add(task1);
            agent.tasks.Add(task2);
            agent.tasks.Add(task);

            Assert.AreEqual(agent.tasks.Count, 3);

            agent.DelHash("myPass", "DF64DC2EB4A0B85091DD31EB4923EAAC");

            Assert.AreEqual(agent.tasks.Count, 2);

            agent.DelHash("myPass", "202CB962AC59075B964B07152D234B70");

            Assert.AreEqual(agent.tasks.Count, 1);

            agent.DelHash("myPass", "098F6BCD4621D373CADE4E832627B4F6");

            Assert.AreEqual(agent.tasks.Count, 0);

        }

        [TestMethod()]
        public void SolverTest()
        {

        }

        [TestMethod()]
        public void SetStartSymbolsTest()
        {
            int ch1 = -1, ch2 = -1, ch3 = -1, ch4 = -1, ch5 = -1, ch6 = -1;
            agent.SetStartSymbols(ref ch1, ref ch2, ref ch3, ref ch4, ref ch5, ref ch6, 10, 545);

            Assert.AreEqual(ch1, -1);
            Assert.AreEqual(ch2, -1);
            Assert.AreEqual(ch3, -1);
            Assert.AreEqual(ch4, 4);
            Assert.AreEqual(ch5, 3);
            Assert.AreEqual(ch6, 5);
        }

        [TestMethod()]
        public void CheckPasswordsTest()
        {

        }

        [TestMethod()]
        public void CalculateMD5HashTest()
        {
            Assert.AreEqual(agent.CalculateMD5Hash("test"), "098F6BCD4621D373CADE4E832627B4F6");
            Assert.AreEqual(agent.CalculateMD5Hash("123"), "202CB962AC59075B964B07152D234B70");
            Assert.AreEqual(agent.CalculateMD5Hash("привет"), "DF64DC2EB4A0B85091DD31EB4923EAAC");
        }

        [TestMethod()]
        public void CheckMessageTest()
        {
            Assert.IsFalse(agent.CheckMessage());
        }
    }
}