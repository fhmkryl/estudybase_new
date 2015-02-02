using System;

namespace EStudyBase.Core.DomainModels
{
    public class KeywordTag
    {
        public int KeywordId { get; set; }
        public int TagId { get; set; }
        public DateTime CreateDate { get; set; }
        public virtual Keyword Keyword { get; set; }
        public virtual Tag Tag { get; set; }
    }
}
