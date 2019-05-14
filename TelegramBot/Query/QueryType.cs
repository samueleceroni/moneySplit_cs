using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBot.Query
{
    /// <summary>
    /// List of the bot's commands
    /// </summary>
    public enum QueryType
    {
        /// <summary>
        /// Creates a new recurring transaction
        /// </summary>
        NewSubscription,
        /// <summary>
        /// Creates a new transaction
        /// </summary>
        NewTransaction,
        /// <summary>
        /// Shows a list's details
        /// </summary>
        ShowDetail,
        /// <summary>
        /// Shows a list's balance
        /// </summary>
        Balance,
        /// <summary>
        /// Creates a new list
        /// </summary>
        NewList,
        /// <summary>
        /// Deletes a list
        /// </summary>
        Delete,
        /// <summary>
        /// Replaces a list with a new one with same name and different version
        /// </summary>
        Reset,
        /// <summary>
        /// Starts the bot, shows a welcome message and some infos
        /// </summary>
        Start,
        /// <summary>
        /// Shows every command with a brief description
        /// </summary>
        Help,
        /// <summary>
        /// Shows a list's trasactions
        /// </summary>
        Show,
        /// <summary>
        /// Represents an invalid command
        /// </summary>
        None,
        /// <summary>
        /// Shows every list of a user
        /// </summary>
        All
    }
}
