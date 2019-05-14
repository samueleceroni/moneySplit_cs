using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramBot.Query;

namespace TelegramBot.Parser
{
    abstract class AbstractQueryParser
    {
        protected string text;

        /// <summary>
        /// Sets the text
        /// </summary>
        /// <param name="text">Bot's New Message</param>
        protected void Init(string text) => this.text = text;

        /// <summary>
        /// Selects every word that starts with '#'
        /// </summary>
        /// <returns>Returns a list with the tags</returns>
        private List<string> GetTags() => text.Split(' ')
                                              .Where(i => i.StartsWith("#"))
                                              .ToList();
        /// <summary>
        /// Checks if the first word of the text is a number, if it is we suppose
        /// that the user will use the default list otherwise return the first word
        /// </summary>
        /// <returns>Returns the list name used in the text</returns>
        private string GetListName()
        {
            string first = text.Split(' ')[0];
            if (double.TryParse(first, out var n))
                return String.Empty;
            return first;
        }

        /// <summary>
        /// Parse the text and then creates a QueryObject with the arguments
        /// </summary>
        /// <returns>Returns a QueryObject with the command arguments</returns>
        public abstract QueryObject GetQueryObject();
    }
}
