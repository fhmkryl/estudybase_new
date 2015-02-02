using System.Collections.Generic;
using EStudyBase.Core.DomainModels;

namespace EStudyBase.UI.ViewModels
{
    public class SearchViewModel
    {
        public int KeywordId { get; set; }
        public string Definition { get; set; }
        public string Synonym { get; set; }
        public string Antonym { get; set; }
        public int Order { get; set; }
        public UserProfile CreateUser { get; set; }
        public ICollection<Content> Contents { get; set; }
        public ICollection<Keyword> RelatedKeywords { get; set; }
        public int PageNumber { get; set; }        
        public int PageSize { get; set; }
        public int TotalPageNumber { get; set; }
        public int TotalRecords { get; set; }

        public KeywordSearchCriteriaViewModel SearchCriteria { get; set; }
    }
}