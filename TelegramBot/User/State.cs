using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBot.User
{
    public class State
    {
        public string DefaultList { get; private set; }
        public StateType Type { get; private set; }

        public State() : this(String.Empty) { }
        public State(string defaultList)
        {
            this.Type = StateType.None; 
            DefaultList = defaultList;
        }
    }
}
