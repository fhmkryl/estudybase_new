using System;
using System.Collections.Generic;

namespace EStudyBase.Core.DomainModels
{
    public class Keyword
    {
        public Keyword()
        {
            KeywordTags = new List<KeywordTag>();
            Likes = new List<Like>();
            Contents = new List<Content>();
        }

        public int KeywordId { get; set; }
        public string Definition { get; set; }
        public string Synonym { get; set; }
        public string Antonym { get; set; }
        public int LanguageId { get; set; }
        public int CreateUserId { get; set; }
        public int? ModifyUserId { get; set; }
        public bool Approved { get; set; }
        public int Order { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? ModifyDate { get; set; }
        public virtual ICollection<KeywordTag> KeywordTags { get; set; }
        public virtual UserProfile CreateUser { get; set; }
        public virtual Language Language { get; set; }
        public virtual UserProfile ModifyUser { get; set; }
        public virtual ICollection<Like> Likes { get; set; }
        public virtual ICollection<Content> Contents { get; set; }
    }
}
