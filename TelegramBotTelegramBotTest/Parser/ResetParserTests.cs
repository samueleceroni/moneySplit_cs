using Microsoft.VisualStudio.TestTools.UnitTesting;
using Telegram.Bot.Types;
using TelegramBot.Query;
using CSharpFunctionalExtensions;
using TelegramBot.User;

namespace TelegramBot.Parser.Tests
{
    [TestClass()]
    public class ResetParserTests
    {
        private readonly string CorrectTestText = "ABC";
        private readonly string WrongTestText = "ABC ABC";
        private readonly Chat TestChat = new Chat() { Id = 123456 };
        private readonly State TestState = new State();
        private readonly ChatMember TestChatMemeber = new ChatMember();

        [TestMethod()]
        public void GetQueryObjectTest()
        {
            Result<QueryObject> result;

            //Tests with wrong paramenters
            string expectedError = AbstractQueryParser.WrongArgsFormat;
            result = (new ResetParser(TestState, WrongTestText, TestChat, TestChatMemeber)).GetQueryObject();
            Assert.IsTrue(result.IsFailure);
            Assert.AreEqual(expectedError, result.Error);

            //Tests with good parameters
            var expected = (new QueryObjectBuilder() { ListName = CorrectTestText, Type = QueryType.Reset, Chat = TestChat, ChatMember = TestChatMemeber }).Build().Value;
            result = (new ResetParser(TestState, CorrectTestText, TestChat, TestChatMemeber)).GetQueryObject();
            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(expected.Equals(result.Value));
        }
    }
}