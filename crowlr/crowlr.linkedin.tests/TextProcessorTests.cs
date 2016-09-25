using System;
using System.IO;
using crowlr.contracts;
using crowlr.core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace crowlr.linkedin.tests
{
    [TestClass]
    public class TextProcessorTests
    {
        [TestMethod]
        public void SeniorAspNetDev_ShouldAccept()
        {
            var html = File.ReadAllText("../../Content/ASP.NET_Dev_ValidProfile.html");
            var page = new AccountPage(html);

            var processor = new TextProcessor(new CategoryProvider());

            var result = processor.Process(page, new[,]
            {
                { "secondary", ".net,c#" },
                { "except", "qa,manager" },
                { "category", "asp.net" },
                { "seniority", "senior" },
                { "bottom", "10" }
            });

            Assert.IsNotNull(html);
            Assert.IsNotNull(page);

            Assert.IsNotNull(result);
            Assert.IsTrue(result is AcceptedResult, result.DumpState());
            Assert.IsNotNull(result.Data.Key("fullname"));
            Assert.IsNotNull(result.Data.Key("title"));
        }

        [TestMethod]
        public void SeniorAspNetDev_CountKeywordsRight_ShouldAccept()
        {
            var html = File.ReadAllText("../../Content/ASP.NET_Dev_ValidProfile.html");
            var page = new AccountPage(html);

            var processor = new TextProcessor(new CategoryProvider());

            var result = processor.Process(page, new[,]
            {
                { "category", "asp.net" },
                { "secondary", ".net,c#" },
                { "seniority", "senior" },
                { "except", "qa,manager" },
                { "bottom", "10" }
            });

            Assert.IsNotNull(html);
            Assert.IsNotNull(page);

            Assert.IsNotNull(result);
            Assert.IsTrue(result is AcceptedResult, result.DumpState());
            Assert.IsNotNull(result.Data.Key("positiveMatches"), result.DumpState());
            Assert.IsTrue(result.Data.Key("positiveMatches").Contains(".net"), result.DumpState());
            Assert.IsTrue(result.Data.Key("positiveMatches").Contains("c#"), result.DumpState());
        }

        [TestMethod]
        public void SecondaryIsEmpty_ShouldAccept()
        {
            var html = File.ReadAllText("../../Content/ASP.NET_Dev_ValidProfile.html");
            var page = new AccountPage(html);

            var processor = new TextProcessor(new CategoryProvider());

            var result = processor.Process(page, new[,]
            {
                { "category", ".Net" },
                { "secondary", "" },
                { "seniority", "senior" },
                { "keywords", "ukraine kyiv kiev" },
                { "except", "qa,pm,devops,ruby,Project manager,C,C++Designer, Oracle" },
                { "bottom", "10" },
                { "total", "100" },
                { "start", "0" }
            });

            Assert.IsNotNull(html);
            Assert.IsNotNull(page);

            Assert.IsNotNull(result);
            Assert.IsTrue(result is AcceptedResult, result.DumpState());
        }

        [TestMethod]
        public void CountryIsNotUA_ShouldSkip()
        {
            var html = File.ReadAllText("../../Content/Java_Dev_NotUA_Country.html");
            var page = new AccountPage(html);

            var processor = new TextProcessor(new CategoryProvider());

            var result = processor.Process(page, new[,]
            {
                { "secondary", ".net,c#" },
                { "except", "qa,manager" },
                { "category", "asp.net" },
                { "seniority", "senior" },
                { "bottom", "10" }
            });

            Assert.IsNotNull(html);
            Assert.IsNotNull(page);

            Assert.IsNotNull(result);
            Assert.IsTrue(result is SkippedResult, result.DumpState());
            Assert.IsTrue(result.Reason.ToString() == "country", result.DumpState());
        }
    }

    public static class MsTestPrettyPrintExtensions
    {
        public static string MsTest_Wrap(this string @this)
        {
            return $@"{Environment.NewLine + Environment.NewLine}{@this}{Environment.NewLine}";
        }
        
        public static string DumpState(this IOperationResult @this)
        {
            if (@this.IsNull())
                return string.Empty;

            return $@"{@this.Status} -> {@this.Reason} :: {@this.Data.Print()} :: {@this.Reason.Data.Print()}".MsTest_Wrap();
        }
    }
}
