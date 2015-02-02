using System;

namespace EStudyBase.Core.DomainModels
{
    public class Like
    {
        public int KeywordId { get; set; }
        public int SourceId { get; set; }
        public int LikeUserId { get; set; }
        public bool Status { get; set; }
        public int SourceTypeId { get; set; }
        public DateTime CreateDate { get; set; }
        public virtual Keyword Keyword { get; set; }
        public virtual LikeSourceType LikeSourceType { get; set; }
        public virtual UserProfile CreateUser { get; set; }
    }
}
