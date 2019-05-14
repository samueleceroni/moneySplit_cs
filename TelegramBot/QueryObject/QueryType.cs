using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBot.QueryObject
{
    public enum QueryType
    {
        NewSubscription,
        NewTransaction,
        ShowDetail,
        Balance,
        NewList,
        Delete,
        Reset,
        Start,
        Help,
        Show,
        None,
        All
    }
}
