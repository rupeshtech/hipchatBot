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
        public RoomInfo()
        {
        }

        public List<Room> GetAvailableRooms(string city)
        {
            var ca = new DeptResource();
            var roomHandler = new GoogleHandler();
            List<KeyValuePair<string, string>> roomsList = new List<KeyValuePair<string, string>>();
            if (city.ToLower() == "amsterdam")
                roomsList = DeptResource.RoomList.FirstOrDefault(x => x.Key == RoomLocation.Amsterdam).Value;
            else if (city.ToLower() == "rotterdam")
                roomsList = DeptResource.RoomList.FirstOrDefault(x => x.Key == RoomLocation.Rotterdam).Value;
            else if (city.ToLower() == "manchester")
                roomsList = DeptResource.RoomList.FirstOrDefault(x => x.Key == RoomLocation.Manchester).Value;

            var roomsInfo = roomHandler.GetAvailableRoomsInfo(roomsList.Select(x => x.Key).ToList(), "", "");

            var availableRooms = roomsInfo.Calendars.Where(x => x.Value.Busy.Count == 0).Select(x => x.Key);
            var rooms = roomsList.Where(x => availableRooms.Contains(x.Key)).Select(r => new Room { Name = r.Value }).ToList();
            return rooms;
        }
    }
}
