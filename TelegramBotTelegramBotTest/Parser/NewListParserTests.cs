using Microsoft.VisualStudio.TestTools.UnitTesting;
using TelegramBot.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using TelegramBot.Query;
using CSharpFunctionalExtensions;
using TelegramBot.User;

namespace TelegramBot.Parser.Tests
{
    [TestClass()]
    public class NewListParserTests
    {
        private readonly string CorrectTestText = "ABC";
        private readonly string WrongTestText = "ABC ABC";
        private readonly Chat TestChat = new Chat() { Id = 123456 };
        private readonly State TestState = new State();

        [TestMethod()]
        public void GetQueryObjectTest()
        {
            Result<QueryObject> result;

            //Tests with wrong paramenters
            string expectedError = AbstractQueryParser.WrongArgsFormat;
            result = (new NewListParser(TestState, WrongTestText, TestChat)).GetQueryObject();
            Assert.IsTrue(result.IsFailure);
            Assert.AreEqual(expectedError, result.Error);

            //Tests with good parameters
            var expected = (new QueryObjectBuilder() { ListName = CorrectTestText, Type = QueryType.NewList, Chat = TestChat }).Build().Value;
            result = (new NewListParser(TestState, CorrectTestText, TestChat)).GetQueryObject();
            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(expected.Equals(result.Value));
        }
    }
}