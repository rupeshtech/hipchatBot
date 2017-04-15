using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XmppBot.GetParsedText
{
    public class Issue_
    {
        public string expand;

        public string id;

        public string self;

        public string key;

        public Field fields;

        public string IssueUrl
        {
            get
            {
                return string.Format("https://tamtam.atlassian.net/browse/{0}", this.key);
            }
            set
            {
            }
        }

        public Issue_()
        {
        }
    }
    public class Field
    {
        public Sprint sprint;

        public Epic epic;

        public Field()
        {
        }
    }
    public class Sprint
    {
        public int id;

        public string self;

        public string state;

        public string name;

        public Sprint()
        {
        }
    }
}
