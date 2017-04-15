using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XmppBot.GetParsedText
{
    public class Epic
    {
        public int id;

        public string self;

        public string name;

        public string summary;

        public Color color;

        public bool done;

        public Epic()
        {
        }
    }
    public class Color
    {
        public string key;

        public Color()
        {
        }
    }
}
