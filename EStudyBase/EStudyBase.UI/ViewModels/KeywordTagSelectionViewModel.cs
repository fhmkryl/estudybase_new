using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using EStudyBase.Core.DomainModels;

namespace EStudyBase.UI.ViewModels
{
    public class KeywordTagSelectionViewModel
    {
        public int KeywordId { get; set; }
        [DisplayName("#Tag")]
        public List<Tag> Tags { get; set; }
        [Required(ErrorMessage = " ")]
        public Int32[] SelectedTags { get; set; }
    }
}