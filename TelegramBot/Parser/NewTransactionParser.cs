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
    class NewTransactionParser : AbstractQueryParser
    {
        private NewTransactionParser(State userState, string text, Chat chat, ChatMember chatMember) => Init(userState, text, chat, chatMember);

        public override Result<QueryObject> GetQueryObject()
        {
            if (!CheckCommandFormat())
                return Result.Fail<QueryObject>(WrongArgsFormat);

            var builder = new QueryObjectBuilder()
            {
                Chat = chat,
                ChatMember = chatMember,
                Type = QueryType.NewTransaction,
                Value = GetValue(),
                ListName = GetListName(),
                Description = GetDescription(),
                Tags = GetTags()
            };

            return builder.Build();
        }
    
        /// <summary>
        /// Gets the amount of the transaction from the command looking within the first 
        /// 2 words
        /// </summary>
        /// <returns>Transaction Value</returns>
        private double GetValue()
        {
            var elements = text.Split(' ');
            double value;

            if (!double.TryParse(elements[0], out value))
                double.TryParse(elements[1], out value);
            return value;
        }

        /// <summary>
        /// Returns the whole command after the value as a comment
        /// </summary>
        /// <returns>Description in the command</returns>
        private string GetDescription()
        {
            var elements = text.Split(' ');
            int valuePosition = double.TryParse(elements[0], out var n) ? 1 : 2 ;
            int start = Math.Min(text.Length, text.IndexOf(elements[valuePosition]) + elements[valuePosition].Length);

            return text.Substring(start);
        }

        protected override bool CheckCommandFormat()
        {
            double n;
            var elements = text.Split(' ');
            var first = elements[0];

            if (elements.Length == 1)
                return double.TryParse(first, out n);
            else
                return Char.IsLetter(first[0]) ? double.TryParse(elements[1], out n)
                                                 : double.TryParse(first, out n);
        }
    }
}
