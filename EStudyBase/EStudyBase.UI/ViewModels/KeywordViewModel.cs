using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using EStudyBase.Core.DomainModels;

namespace EStudyBase.UI.ViewModels
{
    public class KeywordViewModel
    {
        private string _definition;
        public int? KeywordId { get; set; }

        [Required(ErrorMessage = " ")]
        [DisplayName("Başlık")]
        [StringLength(100,ErrorMessage = "Başlık 100 karakteri aşamaz.")]
        public string Definition
        {
            get
            {
                //return
                //    string.IsNullOrWhiteSpace(_definition)
                //        ? string.Empty
                //        : _definition.ToLower()
                //                     .Replace("?", "")
                //                     .Replace("mı", "")
                //                     .Replace("mi", "")
                //                     .Replace("mu", "")
                //                     .Replace("mü", "")
                //                     .Trim();
                return _definition;
            }
            set { _definition = value; }
        }

        [Display(Name = "Dil")]
        public int LanguageId { get; set; }
        public IEnumerable<SelectListItem> Languages { get; set; }

        [DisplayName("#Etiket")]
        public List<Tag> Tags { get; set; }
        [Required(ErrorMessage = " ")]
        public Int32[] SelectedTags { get; set; }

        [DisplayName("Başlık eş anlamı")]
        public string Synonym { get; set; }

        [DisplayName("Başlık zıt anlamı")]
        public string Antonym { get; set; }

        [Required(ErrorMessage = " ")]
        [DisplayName("Başlık sırası")]
        public int Order { get; set; }

        [DisplayName("Onayla")]
        public bool Approved { get; set; }

        public int LikeCount { get; set; }

        public int DislikeCount { get; set; }
    }
}