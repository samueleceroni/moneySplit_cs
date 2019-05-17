using Microsoft.VisualStudio.TestTools.UnitTesting;
using TelegramBot.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using TelegramBot.Query;
using TelegramBot.User;
using Telegram.Bot.Types;

namespace TelegramBot.Parser.Tests
{
    [TestClass()]
    public class BalanceParserTests
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
            result = (new BalanceParser(TestState, WrongTestText, TestChat)).GetQueryObject();
            Assert.IsTrue(result.IsFailure);
            Assert.AreEqual(expectedError, result.Error);

            //Tests with good parameters
            var expected = (new QueryObjectBuilder() { ListName = CorrectTestText, Type = QueryType.Balance, Chat = TestChat }).Build().Value;
            result = (new  BalanceParser(TestState, CorrectTestText, TestChat)).GetQueryObject();
            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(expected.Equals(result.Value));
        }
    }
}