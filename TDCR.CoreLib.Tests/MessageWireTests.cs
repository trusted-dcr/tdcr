using NUnit.Framework;
using System.Net;
using TDCR.CoreLib.Messages.Network;

namespace TDCR.CoreLib.Tests
{
    [TestFixture]
    public class MessageWireTests
    {
        [Test]
        public void TestAddr()
        {
            Addr addr = new Addr
            {
                IP = new IPAddress(new byte[] { 127, 0, 0, 1 }),
                Port = 8888
            };
            Assert.That(Addr.FromWire(addr.ToWire()), Is.EqualTo(addr));
        }

        [Test]
        public void TestUid()
        {
            Uid uid = new Uid
            {
                Part1 = ulong.MaxValue,
                Part2 = ulong.MinValue
            };
            Assert.That(Uid.FromWire(uid.ToWire()), Is.EqualTo(uid));
        }
    }
}
