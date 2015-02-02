using System.Collections.Generic;
using System.Web.Mvc;
using EStudyBase.Core.DomainModels;

namespace EStudyBase.UI.ViewModels
{
    public class ContentListViewModel
    {
        public int? KeywordId { get; set; }
        public string Term { get; set; }
        public int? UserId { get; set; }
        public int TotalCount { get; set; }
        public int CurrentPage { get; set; }
        public int FirstPage { get; set; }
        public int LastPage { get; set; }
        public IEnumerable<SelectListItem> Pages { get; set; }
        public ICollection<Content> Contents { get; set; }
    }
}