using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using TelegramBot.Query;
using TelegramBot.User;
using Telegram.Bot.Types;

namespace TelegramBot.Parser
{
    class DeleteParser : AbstractQueryParser
    {
        public DeleteParser(State userState, string text, Chat chat, ChatMember chatMember) => Init(userState, text, chat, chatMember);

        public override Result<QueryObject> GetQueryObject()
        {
            if(!CheckCommandFormat())
                return Result.Fail<QueryObject>(WrongArgsFormat);

            var builder = new QueryObjectBuilder()
            {
                Chat = chat,
                ChatMember = chatMember,
                Type = QueryType.Delete,
                ListName = GetListName()
            };

            return builder.Build();
        }

        protected override bool CheckCommandFormat() => text.Split(' ').Length > 1;
    }
}
