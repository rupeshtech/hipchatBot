using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace XmppBot.GoogleApi
{
    public class GoogleHandler
    {
        public string[] Scopes = { CalendarService.Scope.CalendarReadonly };
        public const string ApplicationName = "CalendarDept";
        private static CalendarService _service;
        public GoogleHandler()
        {
            _service = _service ?? GetCalendarService();
        }
        public FreeBusyResponse GetAvailableRoomsInfo(List<string> roomIds, string startTime, int duration)
        {
            FreeBusyRequest freeBusyRequest = new FreeBusyRequest();
            freeBusyRequest.TimeMin = DateTime.Now;
            freeBusyRequest.TimeMax = DateTime.Now.AddMinutes(duration);
            freeBusyRequest.Items = new List<FreeBusyRequestItem>();
            foreach (var roomId in roomIds)
                freeBusyRequest.Items.Add(new FreeBusyRequestItem { Id = roomId });
            var request = _service.Freebusy.Query(freeBusyRequest);

            var events = request.Execute();
            return events;

        }
        public Events GetCalendarInfo(string id,string startTime, string endTime)
        {
            EventsResource.ListRequest request = _service.Events.List(id);
            request.TimeMin = DateTime.Now;
            request.ShowDeleted = false;
            request.SingleEvents = true;
            request.MaxResults = 10;
            request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

            // List events.
            Events events = request.Execute();
            if (events.Items != null && events.Items.Count > 0)
                return events;
            return null;

        }

        private CalendarService GetCalendarService()
        {
            UserCredential credential;

            using (var stream =
                new FileStream(@"C:\Temp\client_id.json", FileMode.Open, FileAccess.Read))
            {
                string credPath = System.Environment.GetFolderPath(
                    System.Environment.SpecialFolder.Personal);
                credPath = @"C:\Temp\CalendarDept.json";//Path.Combine(credPath, ".credentials/calendar-dotnet-quickstart.json");

                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "bot@deptagency.com",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
            }

            // Create Google Calendar API service.
            var service = new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });
            return service;
        }
    }
}
