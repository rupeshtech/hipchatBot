using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XmppBot.GetParsedText
{
    public class ResponseIssues
    {
        public string expand;

        public int startAt;

        public int maxResults;

        public int total;

        public List<Issue_> issues;

        public ResponseIssues()
        {
        }
    }
}
