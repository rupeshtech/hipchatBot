using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XmppBot.GetParsedText
{
    public class Individual
    {
        public string Name;

        public bool IsBusy;

        public string Busy_Subject;

        public DateTime Busy_Till;

        public string RoomName;

        public string BusyWith
        {
            get;
            set;
        }

        public Individual()
        {
        }
    }
}
