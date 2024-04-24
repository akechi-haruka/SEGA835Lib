namespace _835TestsMaybeLess {
    public class TestTest {

        [SetUp]
        public void Setup() {
        }

        [Test]
        public void T01_TestFramework() {
            Assert.Pass();
        }

        [Test]
        public void T02_TestCWD() {
            Assert.Pass(Environment.CurrentDirectory);
        }

    }
}