using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpFunctionalExtensions;
using System.Threading.Tasks;
using TelegramBot.Query;
using TelegramBot.User;
using Telegram.Bot.Types;

namespace TelegramBot.Parser
{
    public abstract class AbstractQueryParser
    {
        public readonly static string WrongArgsFormat = "Wrong arguments format";

        protected State userState;
        protected Chat chat;
        protected ChatMember chatMember;
        protected string text;

        /// <summary>
        /// Sets the text
        /// </summary>
        /// <param name="text">Bot's New Message</param>
        protected void Init(State userState, string text, Chat chat, ChatMember chatMember)
        {
            this.text = text;
            this.chat = chat;
            this.userState = userState;
            this.chatMember = chatMember;
        }

        /// <summary>
        /// Selects every word that starts with '#'
        /// </summary>
        /// <returns>Returns a list with the tags</returns>
        protected List<string> GetTags() => text.Split(' ')
                                              .Where(i => i.StartsWith("#"))
                                              .ToList();
        /// <summary>
        /// Checks if the first word of the text is a number, if it is we suppose
        /// that the user will use the default list otherwise return the first word
        /// </summary>
        /// <returns>Returns the list name used in the text</returns>
        protected string GetListName()
        {
            string first = text.Split(' ')[0];
            if (double.TryParse(first, out var n))
                return userState.DefaultList;
            return first;
        }

        /// <summary>
        /// Checks if the command has a correct format
        /// </summary>
        /// <returns>true if it's correct, false otherwise</returns>
        protected abstract bool CheckCommandFormat();

        /// <summary>
        /// Parse the text and then creates a QueryObject with the arguments
        /// </summary>
        /// <returns>Returns a QueryObject with the command arguments</returns>
        public abstract Result<QueryObject> GetQueryObject();
    }
}
