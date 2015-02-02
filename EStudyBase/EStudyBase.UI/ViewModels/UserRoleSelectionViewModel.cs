using System.ComponentModel.DataAnnotations;

namespace EStudyBase.UI.ViewModels
{
    public class UserRoleSelectionViewModel
    {
        public int UserId { get; set; }

        [Display(Name = "Kullanıcı Adı")]
        public string UserName { get; set; }

        [Display(Name = "Rol")]
        public string UserRole { get; set; }
    }
}