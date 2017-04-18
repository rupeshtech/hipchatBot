using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XmppBot.GoogleApi;
using static XmppBot.GoogleApi.DeptResource;

namespace XmppBot.GetParsedText
{
    public class RoomInfo
    {
        Dictionary<int, string> QuestionsList;
        public RoomInfo()
        {
            QuestionsList = QuestionsList ?? GetDurationList();
        }

        private Dictionary<int, string> GetDurationList()
        {
            return null;
        }

        public List<Room> GetAvailableRooms(string city)
        {
            city = city.Replace(" ","");
            var cityToSearch = city.Split('-').First();
            var typeOfSearch = city.Split('-').Last();
            int timeMax = 15;
            switch(typeOfSearch.ToLower())
            {
                case "quick":
                case "quck":
                case "quik":
                case "quic":
                case "qick":
                case "quk":
                case "qik":
                case "qk":
                case "qc":
                case "qu":
                    timeMax = 10;
                    break;
                case "short":
                case "shor":
                case "shrt":
                case "sort":
                case "shot":
                    timeMax = 30;
                    break;
                case "medium":
                case "mediu":
                case "medim":
                case "medum":
                case "mdium":
                case "meium":
                    timeMax = 60;
                    break;
                case "long":
                case "lon":
                case "lng":
                case "ong":
                case "log":
                    timeMax = 90;
                    break;
                default:
                    timeMax = 15;
                    break;
            }
            var ca = new DeptResource();
            var roomHandler = new GoogleHandler();
            List<KeyValuePair<string, string>> roomsList = new List<KeyValuePair<string, string>>();
            if (cityToSearch.ToLower() == "amsterdam")
                roomsList = DeptResource.RoomList.FirstOrDefault(x => x.Key == RoomLocation.Amsterdam).Value;
            else if (cityToSearch.ToLower() == "rotterdam")
                roomsList = DeptResource.RoomList.FirstOrDefault(x => x.Key == RoomLocation.Rotterdam).Value;
            else if (cityToSearch.ToLower() == "manchester")
                roomsList = DeptResource.RoomList.FirstOrDefault(x => x.Key == RoomLocation.Manchester).Value;

            var roomsInfo = roomHandler.GetAvailableRoomsInfo(roomsList.Select(x => x.Key).ToList(), "", timeMax);
            if (roomsInfo.Calendars == null) return null;
            var unavailableRooms = roomsInfo.Calendars.Where(x => x.Value.Busy.Count != 0).Select(x => x.Key).ToList();

            var availableRooms = roomsInfo.Calendars.Where(x => x.Value.Busy.Count == 0).Select(x => x.Key);
            var rooms = roomsList.Where(x => availableRooms.Contains(x.Key)).Select(r => new Room { Name = r.Value }).ToList();
            var roomss = roomsList.Where(x => unavailableRooms.Contains(x.Key)).Select(r => new Room { Name = r.Value }).ToList();
            return rooms;
        }
    }
}
