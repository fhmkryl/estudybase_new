namespace EStudyBase.UI.ViewModels
{
    public class LikeViewModel
    {
        public int KeywordId { get; set; }
        public int SourceId { get; set; }
        public int SourceTypeId { get; set; }
        public int LikeCount { get; set; }
        public int DislikeCount { get; set; }
        public bool AlreadyVoted { get; set; }
    }
}