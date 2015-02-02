using System;

namespace EStudyBase.UI.ViewModels
{
    public class EmailBoxViewModel
    {
        public int EmailId { get; set; }
        public int? FromUserId { get; set; }
        public string FromUserName { get; set; }
        public string FromUserEmail { get; set; }
        public int ToUserId { get; set; }
        public string ToUserName { get; set; }
        public string ToUserEmail { get; set; }
        public string Header { get; set; }
        public string Content { get; set; }
        public DateTime CreateDate { get; set; }
        public int EmailTypeId { get; set; }
        public string EmailType { get; set; }
    }
}