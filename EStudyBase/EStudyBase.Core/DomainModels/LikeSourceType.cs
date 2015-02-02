using System.Collections.Generic;

namespace EStudyBase.Core.DomainModels
{
    public class LikeSourceType
    {
        public int LikeSourceTypeId { get; set; }
        public string Definition { get; set; }
        public virtual ICollection<Like> Likes { get; set; }
    }
}
