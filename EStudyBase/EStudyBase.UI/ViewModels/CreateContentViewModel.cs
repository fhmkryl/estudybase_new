using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc;
using EStudyBase.Core.DomainModels;

namespace EStudyBase.UI.ViewModels
{
    public class CreateContentViewModel
    {
        public int KeywordId { get; set; }

        [Required(ErrorMessage = " ")]
        [Display(Name = "Dil")]
        public int LanguageId { get; set; }
        public IEnumerable<SelectListItem> Languages { get; set; }

        [Required(ErrorMessage = " ")]
        [Display(Name = "Kategori")]
        public int ContentCategoryId { get; set; }
        public IEnumerable<SelectListItem> ContentCategories { get; set; }

        [Required(ErrorMessage = " ")]
        [Display(Name = "Açıklama")]
        public string Description { get; set; }

        [Display(Name = "Web adresi")]
        public string Url { get; set; }

        public ContentType ContentType { get; set; }

        public HttpPostedFileBase PostedFile { get; set; }
    }
}