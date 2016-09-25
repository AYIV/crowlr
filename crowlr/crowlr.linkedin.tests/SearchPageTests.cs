using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace crowlr.linkedin.tests
{
    [TestClass]
    public class SearchPageTests
    {
        [TestMethod]
        public void Process_EmptyJson_EmptyDictionary()
        {
            var page = new SearchPage(string.Empty, isJson: true);

            var result = page.Process();

            Assert.IsNotNull(page);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void Process_ValidJsonWithoutElementsProperty_EmptyDictionary()
        {
            var page = new SearchPage(@"{ ""test"": ""test"" }", isJson: true);

            var result = page.Process();

            Assert.IsNotNull(page);
            Assert.IsNotNull(result);
        }
    }
}
