using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XmppBot.GetParsedText
{
    public class IndividualInfo
    {
        public IndividualInfo()
        {
        }

        public Individual GetIndividualInfo(string name)
        {
            return new Individual()
            {
                Name = name,
                IsBusy = true,
                Busy_Subject = "Retrospective meeting",
                BusyWith = "Team oxxio",
                RoomName = "Big Room Ground Floor",
                Busy_Till = DateTime.Now.AddMinutes(20)
            };
        }
    }
}
