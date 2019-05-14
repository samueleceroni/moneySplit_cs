using System;
using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace TelegramBot.QueryObject
{
    class QueryObject
    {
        public QueryType Type { get; private set; }
        public String ListName { get;  private set; }
        public double Value { get; private set; }
        public string Description { get; private set; }
        public List<string> Tags { get; private set; }
        public ChatId Chat { get; private set; }
        public ChatMember ChatMember { get; private set; }

        private QueryObject(ChatId chat, ChatMember chatmember, QueryType type, string listName, double value, string description, List<string> tags)
        {
            Type = type;
            ListName = listName;
            Value = value;
            Description = description;
            Tags = tags;
            Chat = chat;
            ChatMember = chatmember;
        }

        public static Result<QueryObject> BuildQueryObject(ChatId chat, ChatMember chatMember, QueryType type, string listName, double value, string description, List<string> tags)
        {
            if (chat == null)
                return Result.Fail<QueryObject>("Chat is null");
            if (type == QueryType.None)
                return Result.Fail<QueryObject>("Type can't be set to None");
            if (string.IsNullOrEmpty(listName))
                return Result.Fail<QueryObject>("List name is empty");

            return Result.Ok<QueryObject>(
                    new QueryObject(
                            chat, 
                            chatMember,
                            type,
                            listName, 
                            value, 
                            description, 
                            tags    
                   ));
        }
    }
}
