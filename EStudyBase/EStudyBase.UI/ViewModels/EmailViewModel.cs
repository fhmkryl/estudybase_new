using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace EStudyBase.UI.ViewModels
{
    public class EmailViewModel
    {
        public int ToUserId { get; set; }

        public string UserName { get; set; }

        [Required(ErrorMessage = " ")]
        [DisplayName("Konu")]
        public string Subject { get; set; }

        [Required(ErrorMessage = " ")]
        [DisplayName("İçerik")]
        public string Body { get; set; }
    }
}