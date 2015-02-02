namespace EStudyBase.Core.DomainModels
{
    //[Table("UserProfile")]
    //public class UserProfile
    //{
    //    [Key]
    //    [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
    //    public int UserId { get; set; }
    //    public string UserName { get; set; }
    //}

    public class RegisterExternalLoginModel
    {
        //[Required(ErrorMessage = "Kullanıcı adını giriniz.")]
        //[Display(Name = "Kullanıcı Adı")]
        public string UserName { get; set; }

        public string ExternalLoginData { get; set; }
    }

    public class LocalPasswordModel
    {
        //[Required(ErrorMessage = "Mevcut şifrenizi giriniz")]
        //[DataType(DataType.Password)]
        //[Display(Name = "Mevcut Şifre")]
        public string OldPassword { get; set; }

        //[Required(ErrorMessage = "Yeni şifrenizi giriniz.")]
        //[StringLength(100, ErrorMessage = "Şifreniz en az {2} karakter uzunluğunda olmalı.", MinimumLength = 6)]
        //[DataType(DataType.Password)]
        //[Display(Name = "Yeni Şifre")]
        public string NewPassword { get; set; }

        //[DataType(DataType.Password)]
        //[Display(Name = "Yeni Şifre Tekrar")]
        //[Compare("NewPassword", ErrorMessage = "Şifre ve şifre tekrarı uyuşmamaktadır.")]
        public string ConfirmPassword { get; set; }
    }

    public class LoginModel
    {
        //[Required(ErrorMessage = "Kullanıcı adınızı giriniz.")]
        //[Display(Name = "Kullanıcı Adı")]
        public string UserName { get; set; }

        //[Required(ErrorMessage = "Şifrenizi giriniz.")]
        //[DataType(DataType.Password)]
        //[Display(Name = "Şifre")]
        public string Password { get; set; }

        //[Display(Name = "Beni Hatırla?")]
        public bool RememberMe { get; set; }
    }

    public class RegisterModel
    {
        //[Required(ErrorMessage = "Kullanıcı adınızı giriniz.")]
        //[Display(Name = "Kullanıcı Adı")]
        public string UserName { get; set; }

        //[Required(ErrorMessage = "Email adresinizi giriniz.")]
        //[Display(Name = "Email")]
        //[DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        //[Required(ErrorMessage = "Şifrenizi giriniz.")]
        //[StringLength(100, ErrorMessage = "Şifreniz en az {2} karakter uzunluğunda olmalı.", MinimumLength = 6)]
        //[DataType(DataType.Password)]
        //[Display(Name = "Şifre")]
        public string Password { get; set; }

        //[DataType(DataType.Password)]
        //[Display(Name = "Şifre Tekrar")]
        //[Compare("Password", ErrorMessage = "Şifre ve şifre tekrarı uyuşmamaktadır.")]
        public string ConfirmPassword { get; set; }


    }

    public class ExternalLogin
    {
        public string Provider { get; set; }
        public string ProviderDisplayName { get; set; }
        public string ProviderUserId { get; set; }
    }
}