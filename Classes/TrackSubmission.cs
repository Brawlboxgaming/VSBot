using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace VPBot.Classes
{
    public class TrackSubmission
    {
        public TrackSubmission(string name, string wikiPage, string video, string comments)
        {
            Name = name;
            WikiPage = wikiPage;
            Video = video;
            Comments = comments;
            TimeSubmitted = DateTime.Now;
            Pending = true;
            Accepted = false;
            Rejected = false;
            Added = false;
        }
        [Key] public int ID { get; set; }
        public string SubmitterID { get; set; }
        public string? PollID { get; set; }
        public string Name { get; set; }
        public string WikiPage { get; set; }
        public string Video { get; set; }
        public string Comments { get; set; }
        public DateTime TimeSubmitted { get; set; }
        public bool Pending { get; set; }
        public bool Accepted { get; set; }
        public bool Rejected { get; set; }
        public bool Added { get; set; }
        public int? FinalScore { get; set; }
        public string? RejectionReason { get; set; }

        public NewTrack ToNewTrack()
        {
            return new(Name, WikiPage, Video, Comments);
        }
    }
}
