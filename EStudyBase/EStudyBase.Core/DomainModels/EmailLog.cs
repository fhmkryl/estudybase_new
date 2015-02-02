using System;

namespace EStudyBase.Core.DomainModels
{
    public class EmailLog
    {
        public int EmailId { get; set; }
        public int? FromUserId { get; set; }
        public int? ToUserId { get; set; }
        public string Header { get; set; }
        public string Content { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? ModifyDate { get; set; }
        public int EmailTypeId { get; set; }
        public bool Read { get; set; }
        public bool Deleted { get; set; }
        public virtual UserProfile FromUser { get; set; }
        public virtual UserProfile ToUser { get; set; }
        public virtual EmailLogType EmailLogType { get; set; }
    }
}