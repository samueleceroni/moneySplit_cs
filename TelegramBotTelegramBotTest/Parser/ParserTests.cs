using Microsoft.VisualStudio.TestTools.UnitTesting;
using TelegramBot.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Telegram.Bot.Types;
using TelegramBot.Query;
using TelegramBot.User;

namespace TelegramBot.Parser.Tests
{
    [TestClass()]
    public class ParserTests
    {
        private readonly State TestState = new State("ABC");
        private readonly Chat TestChat = new Chat();
        private readonly ChatMember TestChatMember = new ChatMember();

        private readonly string TestListName = "ABC";
        private readonly string TestDescription = "#test desc";
        private readonly double TestValue = 2;
        private List<string> TestEmptyTag = new List<string>();
        private List<string> TestTags;

        private readonly string TestDeleteCommand = "/delete ABC";
        private readonly string TestShowCommand = "/show ABC";
        private readonly string TestShowDetailCommand = "/show_detail ABC";
        private readonly string TestAllCommand = "/all";
        private readonly string TestNewListCommand = "/new_list ABC";
        private readonly string TestNewTransactionLongCommand = "/new_transaction ABC 2 #test desc";
        private readonly string TestResetCommand = "/reset ABC";
        private readonly string TestBalanceCommand = "/balance ABC";
        //private readonly string TestNewTransacionShortCommand = "ABC 2 #test desc";

        private Result<Parser> BuildWithNullText() => Parser.BuildParser(TestState, String.Empty, TestChat, TestChatMember);
        private Result<Parser> BuildWithNullChat() => Parser.BuildParser(TestState, TestAllCommand, null, TestChatMember);
        private Result<Parser> BuildWithNullChatMember() => Parser.BuildParser(TestState, TestAllCommand, TestChat, null);
        private Result<Parser> BuildWithNullState() => Parser.BuildParser(null, TestAllCommand, TestChat, TestChatMember);
        private Result<Parser> BuildCorrectly(string text) => Parser.BuildParser(TestState, text, TestChat, TestChatMember);

        [TestMethod()]
        public void BuildParserTest()
        {
            Parser expected;
            string expectedError;
            Result<Parser> result;

            //Tests null text checks
            result = BuildWithNullText();
            expectedError = Parser.TextIsEmptyMessage;
            Assert.IsTrue(result.IsFailure);
            Assert.AreEqual(result.Error, expectedError);

            //Tests null chat checks
            result = BuildWithNullChat();
            expectedError = Parser.ChatIsNullMessage;
            Assert.IsTrue(result.IsFailure);
            Assert.AreEqual(result.Error, expectedError);

            //Tests null chat member checks
            result = BuildWithNullChatMember();
            expectedError = Parser.ChatMemberIsNullMessage;
            Assert.IsTrue(result.IsFailure);
            Assert.AreEqual(result.Error, expectedError);

            //Tests null text checks
            result = BuildWithNullState();
            expectedError = Parser.StateIsNullMessage;
            Assert.IsTrue(result.IsFailure);
            Assert.AreEqual(result.Error, expectedError);
            
            //Tests build with correct parameters
            result = BuildCorrectly(TestDeleteCommand);
            expected = new Parser(TestState, TestDeleteCommand, TestChat, TestChatMember);
            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(expected.Equals(result.Value));            
        }

        [TestMethod()]
        public void ParseTest()
        {
            Result<QueryObject> result;
            QueryObject expected;

            TestTags = new List<string>();
            TestTags.Add("#test");

            //Tests Delete command parser
            result = Parser.BuildParser(TestState, TestDeleteCommand, TestChat, TestChatMember).Value.Parse();
            expected = new QueryObject(TestChat, TestChatMember, QueryType.Delete, TestListName, 0, String.Empty, TestEmptyTag);
            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(expected.Equals(result.Value));

            //Tests Show command parser
            result = Parser.BuildParser(TestState, TestShowCommand, TestChat, TestChatMember).Value.Parse();
            expected = new QueryObject(TestChat, TestChatMember, QueryType.Show, TestListName, 0, String.Empty, TestEmptyTag);
            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(expected.Equals(result.Value));

            //Tests Show Detail command parser
            result = Parser.BuildParser(TestState, TestShowDetailCommand, TestChat, TestChatMember).Value.Parse();
            expected = new QueryObject(TestChat, TestChatMember, QueryType.ShowDetail, TestListName, 0, String.Empty, TestEmptyTag);
            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(expected.Equals(result.Value));
            
            //Tests All command parser
            result = Parser.BuildParser(TestState, TestAllCommand, TestChat, TestChatMember).Value.Parse();
            expected = new QueryObject(TestChat, TestChatMember, QueryType.All, null, 0, String.Empty, TestEmptyTag);
            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(expected.Equals(result.Value));
            
            //Tests NewList command parser
            result = Parser.BuildParser(TestState, TestNewListCommand, TestChat, TestChatMember).Value.Parse();
            expected = new QueryObject(TestChat, TestChatMember, QueryType.NewList, TestListName, 0, String.Empty, TestEmptyTag);
            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(expected.Equals(result.Value));

            //Tests NewTransaction command parser
            result = Parser.BuildParser(TestState, TestNewTransactionLongCommand, TestChat, TestChatMember).Value.Parse();
            expected = new QueryObject(TestChat, TestChatMember, QueryType.NewTransaction, TestListName, TestValue, TestDescription, TestTags);
            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(expected.Equals(result.Value));

            //Tests Reset command parser
            result = Parser.BuildParser(TestState, TestResetCommand, TestChat, TestChatMember).Value.Parse();
            expected = new QueryObject(TestChat, TestChatMember, QueryType.Reset, TestListName, 0, String.Empty, TestEmptyTag);
            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(expected.Equals(result.Value));

            //Tests Balance command parser
            result = Parser.BuildParser(TestState, TestBalanceCommand, TestChat, TestChatMember).Value.Parse();
            expected = new QueryObject(TestChat, TestChatMember, QueryType.Balance, TestListName, 0, String.Empty, TestEmptyTag);
            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(expected.Equals(result.Value));
        }
    }
}