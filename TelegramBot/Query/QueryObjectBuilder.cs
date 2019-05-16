using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpFunctionalExtensions;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace TelegramBot.Query
{
    public class QueryObjectBuilder
    {
        public static string ChatIsNullMessage = "Chat is null";
        public static string NoneTypeMessage = "Type can't be set to None";
        public static string EmptyListMessage = "List name is empty";

        /// <summary>
        /// Query
        /// </summary>
        public QueryType Type { get; set; }
        /// <summary>
        /// List used for the query
        /// </summary>
        public string ListName { get; set; }
        /// <summary>
        /// Query's amount 
        /// </summary>
        public double Value { get; set; }
        /// <summary>
        /// Query's description
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Query's tags
        /// </summary>
        public List<string> Tags { get; set; }
        /// <summary>
        /// User/Group's chat Id
        /// </summary>
        public ChatId Chat { get; set; }
        /// <summary>
        /// User that executed the command
        /// </summary>
        public ChatMember ChatMember { get; set; }

        /// <summary>
        /// Creates a QueryObjectBuilder object an set to default values some property
        /// </summary>
        public QueryObjectBuilder()
        {
            Tags = null;
            Type = QueryType.None;
            Chat = null;
            ChatMember = null;
            Description = String.Empty;
        }

        /// <summary>
        /// Creates a QueryObject with the builder's properties
        /// </summary>
        /// <returns>Ok if the object was built successfully, Fail otherwise</returns>
        public Result<QueryObject> Build()
        {
            return Result.Ok<QueryObject>(
                        new QueryObject(
                            Chat,
                            ChatMember,
                            Type,
                            ListName,
                            Value,
                            Description, 
                            Tags
                        )).Ensure<QueryObject>(query => query.Chat != null, ChatIsNullMessage)
                          .Ensure<QueryObject>(query => query.Type != QueryType.None, NoneTypeMessage)
                          .Ensure<QueryObject>(query => !String.IsNullOrEmpty(query.ListName), EmptyListMessage);
        }
    }
}
