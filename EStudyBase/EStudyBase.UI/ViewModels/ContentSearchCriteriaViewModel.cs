namespace EStudyBase.UI.ViewModels
{
    public class ContentSearchCriteriaViewModel
    {
        private int _currentPage;
        private int? _languageId;

        public int CurrentPage {
            get { return _currentPage == 0 ? 1 : _currentPage; }
            set { _currentPage = value; }
        }

        public string PageDirection { get; set; }
        public int? KeywordId { get; set; }
        public int? ContentId { get; set; }

        public int? LanguageId {
            get { return _languageId == 0 ? 2 : _languageId; }
            set { _languageId = value; }
        }

        public int? UserId { get; set; }
    }
}