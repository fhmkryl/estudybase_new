using System;
using System.Collections.Generic;

namespace EStudyBase.Core.DomainModels
{
    public class ContentCategory
    {
        public int ContentCategoryId { get; set; }
        public int? ContentCategoryParentId { get; set; }
        public string Definition { get; set; }
        public int LanguageId { get; set; }
        public int CreateUserId { get; set; }
        public int? ModifyUserId { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? ModifyDate { get; set; }
        public virtual ICollection<Content> Contents { get; set; }
    }
}