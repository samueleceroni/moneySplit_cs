using Microsoft.VisualStudio.TestTools.UnitTesting;
using TelegramBot.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Telegram.Bot.Types;
using CSharpFunctionalExtensions;
using System.Threading.Tasks;

namespace TelegramBot.Query.Tests
{
    [TestClass()]
    public class QueryObjectBuilderTests
    {
        private readonly string TestListName = "ABC";
        private readonly Chat TestChat = new Chat() { Id = 123456 };
        private readonly QueryType TestQueryType = QueryType.NewList;

        private Result<QueryObject> BuildWithNullChat()
        {
            var builder = new QueryObjectBuilder()
            {
                Chat = null,
                Type = TestQueryType,
                ListName = TestListName
            };

            return builder.Build();
        }

        private Result<QueryObject> BuildWithEmptyListName()
        {
            var builder = new QueryObjectBuilder()
            {
                Chat = TestChat,
                Type = TestQueryType,
                ListName = String.Empty
            };

            return builder.Build();
        }

        private Result<QueryObject> BuildWithNoneQueryType()
        {
            var builder = new QueryObjectBuilder()
            {
                Chat = TestChat,
                Type = QueryType.None,
                ListName = TestListName
            };

            return builder.Build();
        }

        private Result<QueryObject> BuildCorrectly()
        {
            var builder = new QueryObjectBuilder()
            {
                Chat = TestChat,
                Type = TestQueryType,
                ListName = TestListName,
                Value = 0
            };

            return builder.Build();
        }

        [TestMethod()]
        public void BuildTest()
        {
            QueryObject expected;
            string expectedError;
            Result<QueryObject> result;

            //Tests null chat value checks
            expectedError = QueryObjectBuilder.ChatIsNullMessage;
            result = BuildWithNullChat();
            Assert.IsTrue(result.IsFailure);
            Assert.AreEqual(expectedError, result.Error);

            //Tests empty list name checks
            expectedError = QueryObjectBuilder.EmptyListMessage;
            result = BuildWithEmptyListName();
            Assert.IsTrue(result.IsFailure);
            Assert.AreEqual(expectedError, result.Error);
            
            //Tests None Type check
            expectedError = QueryObjectBuilder.NoneTypeMessage;
            result = BuildWithNoneQueryType();
            Assert.IsTrue(result.IsFailure);
            Assert.AreEqual(expectedError, result.Error);
            
            //Tests build with correct params
            expected = new QueryObject(TestChat, TestQueryType, TestListName, 0, String.Empty, new List<string>());
            result = BuildCorrectly();
            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(expected.Equals(result.Value));
        }
    }
}