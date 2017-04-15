using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XmppBot.GetParsedText
{
    public class Individual
    {
        public string Name;

        public bool IsBusyNow;

        public List<CalendarEvent> Events;

        public string Busy_Subject;

        public DateTime? Busy_Till;

        public DateTime? Busy_From;

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

    public class CalendarEvent
    {
        public string Busy_Subject;

        public DateTime? Busy_Till;

        public DateTime? Busy_From;

        public string RoomName;

        public List<string> Attendees;

        public string BusyWith
        {
            get;
            set;
        }
    }
}
