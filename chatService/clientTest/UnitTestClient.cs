using ClientNS;
using ServerNS;

namespace clientTest
{
    [TestClass]
    public class UnitTestClient
    {
        [TestMethod]
        public void Verify_That_The_Client_Is_Connecting_To_The_Server()
        {
            var result = Client.Start_client();
            Assert.AreEqual(true, result);
        }
    }
}