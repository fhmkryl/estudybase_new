using Mvc.Mailer;

namespace EStudyBase.Common.Mailers
{
    public class UserMailer : MailerBase, IUserMailer
    {
        public UserMailer()
        {
            MasterName = "_Layout";
        }

        public virtual MvcMailMessage SendEmail(string viewName,string subject,string mailBody,string to)
        {
            ViewBag.MailBody = mailBody;
            return Populate(x =>
                {
                    x.Subject = subject;
                    x.ViewName = viewName;
                    x.To.Add(to);
                });
        }

        public virtual MvcMailMessage PasswordReset()
        {
            return
                Populate(x =>
                    {
                        x.Subject = "PasswordReset";
                        x.ViewName = "PasswordReset";
                        x.To.Add("fhmkryl@gmail.com");
                    });
        }
    }
}