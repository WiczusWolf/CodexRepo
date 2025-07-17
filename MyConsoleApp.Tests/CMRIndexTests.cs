namespace MyConsoleApp.Tests
{
    [TestClass]
    public class CMRIndexTests
    {
        [TestMethod]
        public void CMRIndexTestAdd0()
        {
            CMRIndex index = new CMRIndex(0, 2, 0, 4);
            CMRIndex expectedIndex = new CMRIndex(0, 2, 0, 4);
            CMRIndex actualIndex = index.AddOffset(0);
            Assert.AreEqual(expectedIndex, actualIndex);
        }
        [TestMethod]
        public void CMRIndexTestAdd1()
        {
            CMRIndex index = new CMRIndex(0, 2, 0, 4);
            CMRIndex expectedIndex = new CMRIndex(0, 2, 1, 4);
            CMRIndex actualIndex = index.AddOffset(1);
            Assert.AreEqual(expectedIndex, actualIndex);
        }
        [TestMethod]
        public void CMRIndexTestAddOverflow()
        {
            CMRIndex index = new CMRIndex(0, 2, 3, 4);
            CMRIndex expectedIndex = new CMRIndex(0, 3, 0, 4);
            CMRIndex actualIndex = index.AddOffset(1);
            Assert.AreEqual(expectedIndex, actualIndex);
        }
        [TestMethod]
        public void CMRIndexTestAddOverflowBigMod()
        {
            CMRIndex index = new CMRIndex(0, 2, 13, 16);
            CMRIndex expectedIndex = new CMRIndex(0, 3, 0, 16);
            CMRIndex actualIndex = index.AddOffset(3);
            Assert.AreEqual(expectedIndex, actualIndex);
        }
        [TestMethod]
        public void CMRIndexTestSub()
        {
            CMRIndex index = new CMRIndex(0, 2, 1, 4);
            CMRIndex expectedIndex = new CMRIndex(0, 2, 0, 4);
            CMRIndex actualIndex = index.AddOffset(-1);
            Assert.AreEqual(expectedIndex, actualIndex);
        }
        [TestMethod]
        public void CMRIndexTestSubOverflow()
        {
            CMRIndex index = new CMRIndex(0, 2, 0, 4);
            CMRIndex expectedIndex = new CMRIndex(0, 1, 3, 4);
            CMRIndex actualIndex = index.AddOffset(-1);
            Assert.AreEqual(expectedIndex, actualIndex);
        }
        [TestMethod]
        public void CMRIndexTestSubOverflowBigMod()
        {
            CMRIndex index = new CMRIndex(0, 2, 4, 16);
            CMRIndex expectedIndex = new CMRIndex(0, 1, 15, 16);
            CMRIndex actualIndex = index.AddOffset(-5);
            Assert.AreEqual(expectedIndex, actualIndex);
        }
    }
}
