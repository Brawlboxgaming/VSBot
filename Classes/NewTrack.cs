using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace VPBot.Classes
{
    public class NewTrack
    {
        public NewTrack(string name, string wikiPage, string video, string comments)
        {
            Name = name;
            WikiPage = wikiPage;
            Video = video;
            Comments = comments;
        }
        [Key] public int ID { get; set; }
        public string Name { get; set; }
        public string WikiPage { get; set; }
        public string Video { get; set; }
        public string Comments { get; set; }
    }
}
