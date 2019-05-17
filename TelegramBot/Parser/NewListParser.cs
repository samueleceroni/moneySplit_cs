using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramBot.Query;
using TelegramBot.User;
using CSharpFunctionalExtensions;
using Telegram.Bot.Types;

namespace TelegramBot.Parser
{
    public class NewListParser : AbstractQueryParser
    {
        public NewListParser(State userState, string text, Chat chat) => Init(userState, text, chat);

        /// <summary>
        /// Creates a QueryObject from text
        /// </summary>
        /// <returns></returns>
        public override Result<QueryObject> GetQueryObject()
        {
            if (!CheckCommandFormat())
                return Result.Fail<QueryObject>(WrongArgsFormat);

            var builder = new QueryObjectBuilder() {
                Chat = chat,
                Type = QueryType.NewList,
                ListName = GetListName()
            };

            return builder.Build();
        }

        protected override bool CheckCommandFormat() => text.Split(' ').Length == 1;
    }
}
