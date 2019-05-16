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
    public class NewTransactionParserTests
    {
        private readonly string CorrectTestText1 = "ABC 2";
        private readonly string CorrectTestText2 = "2";
        private readonly string WrongTestText1 = "ABC ABC";
        private readonly string WrongTestText2 = "ABC";
        private readonly string TestDescription1 = "ABC 2 Desc";
        private readonly string TestDescription2 = "2 Desc";
        private readonly string CorrectDescription = "Desc";
        private readonly string CorrectListName = "ABC";

        private readonly Chat TestChat = new Chat() { Id = 123456 };
        private readonly State TestState = new State("ABC");
        private readonly ChatMember TestChatMemeber = new ChatMember();

        [TestMethod()]
        public void GetQueryObjectTest()
        {
            Result<QueryObject> result;

            //Tests with wrong paramenters: No value long format
            string expectedError = AbstractQueryParser.WrongArgsFormat;
            result = (new NewTransactionParser(TestState, WrongTestText1, TestChat, TestChatMemeber)).GetQueryObject();
            Assert.IsTrue(result.IsFailure);
            Assert.AreEqual(expectedError, result.Error);

            //Tests with wrong paramenters: No value long format
            result = (new NewTransactionParser(TestState, WrongTestText2, TestChat, TestChatMemeber)).GetQueryObject();
            Assert.IsTrue(result.IsFailure);
            Assert.AreEqual(expectedError, result.Error);

            //Tests with good parameters: long format
            var expected = new QueryObject(TestChat, TestChatMemeber, QueryType.NewTransaction, CorrectListName, 2, String.Empty, new List<string>());
            result = (new NewTransactionParser(TestState, CorrectTestText1, TestChat, TestChatMemeber)).GetQueryObject();
            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(expected.Equals(result.Value));

            //Tests with good parameters: short format
            expected = new QueryObject(TestChat, TestChatMemeber, QueryType.NewTransaction, CorrectListName, 2, String.Empty, new List<string>());
            result = (new NewTransactionParser(TestState, CorrectTestText2, TestChat, TestChatMemeber)).GetQueryObject();
            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(expected.Equals(result.Value));

            //Test with good parameter: description long format
            expected = new QueryObject(TestChat, TestChatMemeber, QueryType.NewTransaction, CorrectListName, 2, CorrectDescription, new List<string>());
            result = (new NewTransactionParser(TestState, TestDescription1, TestChat, TestChatMemeber)).GetQueryObject();
            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(expected.Equals(result.Value));

            //Test with good parameter: description sort format
            expected = new QueryObject(TestChat, TestChatMemeber, QueryType.NewTransaction, CorrectListName, 2, CorrectDescription, new List<string>());
            result = (new NewTransactionParser(TestState, TestDescription2, TestChat, TestChatMemeber)).GetQueryObject();
            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(expected.Equals(result.Value));
        }
    }
}