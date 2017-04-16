using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XmppBot.GoogleApi;

namespace XmppBot.GetParsedText
{
    public class IndividualInfo
    {
        public IndividualInfo()
        {
        }

        public Individual GetIndividualInfo(string id)
        {
            var calendarinfo = new GoogleHandler();
            var events = calendarinfo.GetCalendarInfo(id,"","");
            var individualInfo = new Individual { Name = id };
            individualInfo.Events = new List<CalendarEvent>();
            foreach (var eventItem in events.Items.OrderByDescending(x=>x.Start.Date))
            {
                var calendarEvent = new CalendarEvent();
                var when = eventItem.Start.DateTime;
                if (when==null || when==DateTime.MinValue || when==DateTime.MaxValue)
                {
                    when = Convert.ToDateTime(eventItem.Start.Date);
                }
                calendarEvent.Busy_From = eventItem.Start.DateTime==null|| eventItem.Start.DateTime==DateTime.MinValue|| 
                    eventItem.Start.DateTime==DateTime.MaxValue? Convert.ToDateTime(eventItem.Start.Date) : eventItem.Start.DateTime;
                calendarEvent.Busy_Till = eventItem.End.DateTime == null || eventItem.End.DateTime == DateTime.MinValue ||
                    eventItem.End.DateTime == DateTime.MaxValue ? Convert.ToDateTime(eventItem.End.Date) : eventItem.End.DateTime;

                calendarEvent.Busy_Subject = eventItem.Summary;
                calendarEvent.BusyWith = eventItem.Attendees?.First()?.DisplayName;
                calendarEvent.RoomName = eventItem.Location;
                calendarEvent.Attendees = new List<string>();
                if (eventItem.Attendees != null)
                {
                    if (eventItem.Attendees.Count<11)
                    {
                        foreach (var attendee in eventItem.Attendees)
                        {
                            var attendeeName = attendee.DisplayName ?? attendee.Email?.Substring(0, attendee.Email.IndexOf('@'));
                            if (!string.IsNullOrEmpty(attendeeName) && !attendeeName.ToLower().Contains("room"))
                                calendarEvent.Attendees.Add(attendeeName);
                        } 
                    }
                    else
                    calendarEvent.Attendees.Add("more than 10 people");
                }
                individualInfo.Events.Add(calendarEvent);
            }
            if (individualInfo.Events.Any(x => x.Busy_From < DateTime.Now))
            {
                individualInfo.IsBusyNow = true;
                var currentEvent = individualInfo.Events.FirstOrDefault(x => x.Busy_From < DateTime.Now && x.Busy_Till >DateTime.Now);
                individualInfo.Busy_From = currentEvent.Busy_From;
                individualInfo.Busy_Till = currentEvent.Busy_Till;
                individualInfo.RoomName = currentEvent.RoomName;
                foreach (var attendee in currentEvent.Attendees)
                    if (attendee != null)
                        individualInfo.BusyWith += attendee + "; ";
            }
            return individualInfo;
        }
    }
}
