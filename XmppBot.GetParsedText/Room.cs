using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XmppBot.GetParsedText
{
    public class RoomsStatus
    {
        public List<Room> Rooms;
        public int Minutes;
    }
    public class Room
    {
        public string Name;

        public Room()
        {
        }
    }
}
