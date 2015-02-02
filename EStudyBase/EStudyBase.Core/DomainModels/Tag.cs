using System;
using System.Collections.Generic;

namespace EStudyBase.Core.DomainModels
{
    public class Tag
    {
        public Tag()
        {
            KeywordTags = new List<KeywordTag>();
        }

        public int TagId { get; set; }
        public int? TagParentId { get; set; }
        public string Definition { get; set; }
        public int LanguageId { get; set; }
        public int CreateUserId { get; set; }
        public int? ModifyUserId { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? ModifyDate { get; set; }
        public virtual ICollection<KeywordTag> KeywordTags { get; set; }
        public virtual Language Language { get; set; }
        public virtual UserProfile CreateUser { get; set; }
        public virtual UserProfile ModifyUser { get; set; }
    }
}
