using ClientNS;
using ServerNS;

namespace ServerTest
{
    [TestClass]
    public class UnitTestServer
    {
        [TestMethod]
        public void Check_If_The_Server_Has_Started()
        {
            var result = Server.Start_server();
            Assert.AreEqual(true, result);
        }
    }
}