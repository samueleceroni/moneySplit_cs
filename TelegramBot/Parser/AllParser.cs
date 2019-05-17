using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using TelegramBot.Query;
using Telegram.Bot.Types;
using TelegramBot.User;

namespace TelegramBot.Parser
{
    public class AllParser : AbstractQueryParser
    {
        public AllParser(State userState, string text, Chat chat) => Init(userState, text, chat);

        /// <summary>
        /// Creates a QueryObject from text
        /// </summary>
        /// <returns></returns>
        public override Result<QueryObject> GetQueryObject()
        {
            if (!CheckCommandFormat())
                return Result.Fail<QueryObject>(WrongArgsFormat);
            var builder = new QueryObjectBuilder()
            {
                Chat = chat,
                Type = QueryType.All
            };

            return builder.Build();
        }

        /// <summary>
        /// There must be no parameters
        /// </summary>
        /// <returns></returns>
        protected override bool CheckCommandFormat() => String.IsNullOrEmpty(text);
    }
}
