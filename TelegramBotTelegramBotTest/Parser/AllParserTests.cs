using Microsoft.VisualStudio.TestTools.UnitTesting;
using TelegramBot.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using CSharpFunctionalExtensions;
using TelegramBot.Query;
using TelegramBot.User;

namespace TelegramBot.Parser.Tests
{
    [TestClass()]
    public class AllParserTests
    {
        private readonly string TestText = "ABC";
        private readonly Chat TestChat = new Chat() { Id = 123456 };
        private readonly State TestState = new State();

        [TestMethod()]
        public void GetQueryObjectTest()
        {
            Result<QueryObject> result;

            //Tests with wrong paramenters
            string expectedError = AbstractQueryParser.WrongArgsFormat;
            result = (new AllParser(TestState, TestText, TestChat)).GetQueryObject();
            Assert.IsTrue(result.IsFailure);
            Assert.AreEqual(expectedError, result.Error);
            
            //Tests with good parameters
            var expected = (new QueryObjectBuilder() { Type = QueryType.All, Chat = TestChat }).Build().Value;
            result = (new AllParser(TestState, String.Empty, TestChat)).GetQueryObject();
            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(expected.Equals(result.Value));
        }
    }
}