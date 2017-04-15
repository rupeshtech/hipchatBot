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
            List<Room> rooms = new List<Room>();
            if (city.ToLower() == "amsterdam")
            {
                var roomsList = DeptResource.RoomList.FirstOrDefault(x => x.Key == RoomLocation.Amsterdam).Value;
                rooms.Add(new Room()
                {
                    Name = "Big Room"
                });
                rooms.Add(new Room()
                {
                    Name = "Blue Room"
                });
                rooms.Add(new Room()
                {
                    Name = "Control Room"
                });
                rooms.Add(new Room()
                {
                    Name = "Foest Green Room"
                });
            }
            else if (city.ToLower() == "rotterdam")
            {
                rooms.Add(new Room()
                {
                    Name = "Admiral Blue Room"
                });
                rooms.Add(new Room()
                {
                    Name = "Coral Pink Room"
                });
                rooms.Add(new Room()
                {
                    Name = "Merlot red Room"
                });
                rooms.Add(new Room()
                {
                    Name = "Midnight Blue Room"
                });
            }
            else if (city.ToLower() == "manchester")
            {
                rooms.Add(new Room()
                {
                    Name = "Focus Room1"
                });
                rooms.Add(new Room()
                {
                    Name = "Focus Room2"
                });
                rooms.Add(new Room()
                {
                    Name = "Focus Room3"
                });
                rooms.Add(new Room()
                {
                    Name = "M1"
                });
            }
            return rooms;
        }
    }
}
