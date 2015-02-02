using System;

namespace EStudyBase.Core.DomainModels {
    public class Content {
        public int ContentId { get; set; }
        public int KeywordId { get; set; }
        public int LanguageId { get; set; }
        public string Url { get; set; }
        public decimal? FileSize { get; set; }
        public string Description { get; set; }
        public ContentType ContentType { get; set; }
        public int ContentCategoryId { get; set; }
        public int CreateUserId { get; set; }
        public int? ModifyUserId { get; set; }
        public bool Approved { get; set; }
        public int LikeCount { get; set; }
        public int DislikeCount { get; set; }
        public int Complaint { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? ModifyDate { get; set; }
        public virtual ContentCategory ContentCategory { get; set; }
        public virtual Keyword Keyword { get; set; }
        public virtual Language Language { get; set; }
        public virtual UserProfile CreateUser { get; set; }
        public virtual UserProfile ModifyUser { get; set; }
    }
}