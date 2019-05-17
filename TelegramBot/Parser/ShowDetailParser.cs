using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramBot.Query;
using CSharpFunctionalExtensions;
using Telegram.Bot.Types;
using TelegramBot.User;

namespace TelegramBot.Parser
{
    public class ShowDetailParser : AbstractQueryParser
    {
        public ShowDetailParser(State userState, string text, Chat chat) => Init(userState, text, chat);

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
                Type = QueryType.ShowDetail,
                ListName = GetListName(),
                Tags = GetTags()
            };

            return builder.Build();
        }

        /// <summary>
        /// Checks if the first word is a possible list name and if the other words
        /// are tags
        /// </summary>
        /// <returns>true if the format is correct, false otherwise</returns>
        protected override bool CheckCommandFormat()
        {
            if (String.IsNullOrEmpty(text))
                return false;

            var elements = text.Split(' ').ToList();
            var first = elements[0];

            foreach (var str in elements)
                if (str != first && !str.StartsWith("#"))
                    return false;

            return Char.IsLetter(first[0]) && elements.Count>= 1;
        }
    }
}
