using System.Collections.Generic;

namespace EStudyBase.Core.DomainModels
{
    public class UserProfile
    {
        public UserProfile()
        {
            Keywords = new List<Keyword>();
            Keywords1 = new List<Keyword>();
            Likes = new List<Like>();
            Contents = new List<Content>();
            Contents1 = new List<Content>();
            Tags = new List<Tag>();
            Tags1 = new List<Tag>();
        }

        public int UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }

        public virtual ICollection<Keyword> Keywords { get; set; }
        public virtual ICollection<Keyword> Keywords1 { get; set; }
        public virtual ICollection<Like> Likes { get; set; }
        public virtual ICollection<Content> Contents { get; set; }
        public virtual ICollection<Content> Contents1 { get; set; }
        public virtual ICollection<Tag> Tags { get; set; }
        public virtual ICollection<Tag> Tags1 { get; set; }
        public virtual ICollection<EmailLog>  EmailLogsSent{ get; set; }
        public virtual ICollection<EmailLog> EmailLogsReceived { get; set; }
    }
}
