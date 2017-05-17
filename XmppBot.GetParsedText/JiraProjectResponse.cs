using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XmppBot.GetParsedText
{
    public class JiraProjectResponse
    {
        public List<JiraProject> Projects { get; set; }
    }

    public class AvatarUrls
    {
        public string __invalid_name__48x48 { get; set; }
        public string __invalid_name__24x24 { get; set; }
        public string __invalid_name__16x16 { get; set; }
        public string __invalid_name__32x32 { get; set; }
    }

    public class ProjectCategory
    {
        public string self { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
    }

    public class JiraProject
    {
        public string self { get; set; }
        public string id { get; set; }
        public string key { get; set; }
        public string name { get; set; }
        //public AvatarUrls avatarUrls { get; set; }
        public ProjectCategory projectCategory { get; set; }
    }
}
