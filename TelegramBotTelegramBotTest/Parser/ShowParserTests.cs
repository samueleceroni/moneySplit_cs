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
    public class ShowParserTests
    {
        private readonly string CorrectTestText = "ABC #A #B";
        private readonly string WrongTestText1 = "#ABC #A asd #B";
        private readonly string WrongTestText2 = "ABC 2 #A asd #B";
        private readonly string CorrectListName = "ABC";

        private readonly Chat TestChat = new Chat() { Id = 123456 };
        private readonly State TestState = new State();
        private readonly ChatMember TestChatMemeber = new ChatMember();

        [TestMethod()]
        public void GetQueryObjectTest()
        {
            Result<QueryObject> result;
            List<string> CorrectTags = new List<string>();
            CorrectTags.Add("#A");
            CorrectTags.Add("#B");

            //Tests with no paramenters
            string expectedError = AbstractQueryParser.WrongArgsFormat;
            result = (new ShowParser(TestState, String.Empty, TestChat, TestChatMemeber)).GetQueryObject();
            Assert.IsTrue(result.IsFailure);
            Assert.AreEqual(expectedError, result.Error);

            //Tests with wrong parameters: Tags before list name
            result = (new ShowParser(TestState, WrongTestText1, TestChat, TestChatMemeber)).GetQueryObject();
            Assert.IsTrue(result.IsFailure);
            Assert.AreEqual(expectedError, result.Error);

            //Tests with wrong parameters: Non tagged words
            result = (new ShowParser(TestState, WrongTestText2, TestChat, TestChatMemeber)).GetQueryObject();
            Assert.IsTrue(result.IsFailure);
            Assert.AreEqual(expectedError, result.Error);
            //Tests with correct parameters
            var expected = new QueryObject(TestChat, TestChatMemeber, QueryType.Show, CorrectListName, 0, String.Empty, CorrectTags);
            result = (new ShowParser(TestState, CorrectTestText, TestChat, TestChatMemeber)).GetQueryObject();
            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(expected.Equals(result.Value));
        }
    }
}