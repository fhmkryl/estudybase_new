using Mvc.Mailer;

namespace EStudyBase.Common.Mailers
{
    public interface IUserMailer
    {
        MvcMailMessage SendEmail(string viewName, string subject, string mailBody,string to);
        MvcMailMessage PasswordReset();
    }
}