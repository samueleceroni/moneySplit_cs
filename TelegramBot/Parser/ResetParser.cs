using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using TelegramBot.User;
using TelegramBot.Query;
using CSharpFunctionalExtensions;

namespace TelegramBot.Parser
{
    public class ResetParser : AbstractQueryParser
    {
        public ResetParser(State userState, string text, Chat chat, ChatMember chatMember) => Init(userState, text, chat, chatMember);

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
                ChatMember = chatMember,
                Type = QueryType.Reset,
                ListName = GetListName()
            };

            return builder.Build();
        }

        protected override bool CheckCommandFormat() => text.Split(' ').Length == 1;
    }
}
