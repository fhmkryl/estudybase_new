namespace EStudyBase.UI.ViewModels
{
    public class KeywordSearchCriteriaViewModel
    {
        public string Term { get; set; }
        public int? KeywordId { get; set; }
        public int? ContentId { get; set; }

        private int? _languageId;
        public int? LanguageId {
            get { return _languageId == 0 ? 2 : _languageId; }
            set { _languageId = value; }
        }

        public string PageDirection { get; set; }

        private int _currentPage;
        public int CurrentPage {
            get { return _currentPage == 0 ? 1 : _currentPage; }
            set { _currentPage = value; }
        }

        public string OperationType { get; set; }
        public int? TagId { get; set; }
    }
}