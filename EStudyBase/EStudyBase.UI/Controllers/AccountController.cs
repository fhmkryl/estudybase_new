using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Web.Mvc;
using System.Web.Security;
using DotNetOpenAuth.AspNet;
using EStudyBase.Common.Mailers;
using EStudyBase.Core.DomainModels;
using EStudyBase.Infrastructure.Mappings;
using EStudyBase.UI.Attributes;
using Microsoft.Web.WebPages.OAuth;
using Mvc.Mailer;
using WebMatrix.WebData;

namespace EStudyBase.UI.Controllers
{
    [InitializeSimpleMembership]
    [Authorize]
    public class AccountController : Controller
    {
        private readonly EStudyBaseContext _context = new EStudyBaseContext();


        private IUserMailer _userMailer = new UserMailer();
        public IUserMailer UserMailer
        {
            get { return _userMailer; }
            set { _userMailer = value; }
        }

        public ActionResult UnAuthorized()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        //[ValidateAntiForgeryToken]
        public ActionResult Login(LoginModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Result = false;
                ViewBag.ResultMessage = "Lütfen belirtilen alanları doldurunuz!";
                return PartialView(model);
            }

            if (WebSecurity.Login(model.UserName, model.Password, model.RememberMe))
            {
                return JavaScript("location.href='/'");
            }

            ViewBag.Result = false;
            ViewBag.ResultMessage = "Kullanıcı adı veya şifre bilgisi yanlış!";
            return PartialView(model);
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            WebSecurity.Logout();

            return RedirectToAction("Search", "Keyword");
        }

        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        //[ValidateAntiForgeryToken]
        public ActionResult Register(RegisterModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Result = false;
                ViewBag.ResultMessage = "Lütfen belirtilen alanları doldurunuz!";
                return PartialView(model);
            }

            // Attempt to register the user
            try
            {
                // Check whether username already exists
                var user = _context.UserProfiles.FirstOrDefault(p => p.UserName == model.UserName);
                if (user != null)
                {
                    ViewBag.Result = false;
                    ViewBag.ResultMessage = "Bu kullanıcı adı ile daha önceden kayıt oluşturulmuş!";
                    return PartialView(model);
                }


                // Check user by email address
                var userName = (from u in _context.UserProfiles
                                where u.Email.Equals(model.Email)
                                select u).FirstOrDefault();
                if (userName != null)
                {
                    ViewBag.Result = false;
                    ViewBag.ResultMessage = "Bu email adresi ile daha önceden kullanıcı oluşturulmuş!";
                    return PartialView(model);
                }

                WebSecurity.CreateUserAndAccount(model.UserName, model.Password, new { model.Email });
                WebSecurity.Login(model.UserName, model.Password);

                return
                    JavaScript(
                        "bootbox.alert('Kullanıcı kaydınız başarıyla oluşturuldu, devam etmek için tıklayınız!',function(){location.href='/'});");
            }
            catch (MembershipCreateUserException exception)
            {
                throw new Exception("Error in AccountController.Register [Post]", exception);
            }
        }

        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        //[ValidateAntiForgeryToken]
        public ActionResult ForgotPassword(string userName)
        {
            //...
            // Check user existance
            var user = Membership.GetUser(userName);
            if (user == null)
            {
                ViewBag.Result = false;
                ViewBag.ResultMessage = "Kullanıcı bilgisi bulunamadı!";
                return PartialView();
            }
            else
            {
                //...
                // Generate password token
                var token = WebSecurity.GeneratePasswordResetToken(userName);
                
                //...
                // Create url with above token
                var resetLink = string.Format("<a href='{0}'>Şifre Sıfırla</a>", Url.Action("ResetPassword", "Account", new { un = userName, rt = token }, "http"));
                
                //...
                // Get user email address
                var userEmail = (from i in _context.UserProfiles
                               where i.UserName == userName
                               select i.Email).FirstOrDefault();
                
                //...
                // Send mail
                const string view = "EmailTemplate";
                const string header = "Şifre Tanımlama";
                var body = string.Format("Şifrenizi yeniden tanımlamak için lütfen linke tıklayınız <br/>{0}", resetLink);
                try
                {
                    //...
                    // Send email
                    MvcMailMessage mvcMailMessage = UserMailer.SendEmail(view, header, body, userEmail);
                    mvcMailMessage.Send();

                    //...
                    // Log sent email
                    var emailLog = new EmailLog
                        {
                            EmailTypeId = 1,
                            ToUserId = WebSecurity.GetUserId(user.UserName),
                            Header = header,
                            Content = body,
                            CreateDate = DateTime.Now
                        };
                    _context.EmailLogs.Add(emailLog);
                    _context.SaveChanges();

                    ViewBag.Result = true;
                    ViewBag.ResultMessage = "Şifrenizi tanımlamak için emailinize gönderilen linki tıklayınız.";
                    return PartialView();
                }
                catch (Exception ex)
                {
                    ViewBag.Result = false;
                    ViewBag.ResultMessage = string.Format("Email gönderilirken hata oluştu, lütfen tekrar deneyiniz.");
                    return PartialView();
                }
            }
        }

        [AllowAnonymous]
        public ActionResult ResetPassword(string un, string rt)
        {
            //...
            // Get userid of received username
            var userId = (from i in _context.UserProfiles
                          where i.UserName == un
                          select i.UserId).FirstOrDefault();
            
            //...
            // Check UserId and token matches
            bool any = (from j in _context.WebpagesMemberships
                        where (j.UserId == userId)
                              && (j.PasswordVerificationToken == rt)
                        //&& (j.PasswordVerificationTokenExpirationDate < DateTime.Now)
                        select j).Any();

            if (!any)
            {
                ViewBag.Result = false;
                ViewBag.ResultMessage = "Kullanıcı adı ve kod uyuşmuyor!";
                return View();
            }

            // ...
            // Generate random password
            string newpassword = GenerateRandomPassword(6);
            
            //...
            // Reset password
            bool response = WebSecurity.ResetPassword(rt, newpassword);
            if (response)
            {
                //...
                // Get user email address to send password
                var email = (from i in _context.UserProfiles
                             where i.UserName == un
                             select i.Email).FirstOrDefault();
                
                //...
                //Send email
                const string view = "EmailTemplate";
                const string header = "Şifreniz değiştirildi!";
                var content = string.Format("Yeni şifreniz <b>{0}</b>", newpassword);
                try
                {
                    //...
                    // Send email
                    MvcMailMessage mvcMailMessage = UserMailer.SendEmail(view, header, content, email);
                    mvcMailMessage.Send();

                    //...
                    // Log sent email
                    var emailLog = new EmailLog
                    {
                        EmailTypeId = 1,
                        ToUserId = userId,
                        Header = header,
                        Content = content,
                        CreateDate = DateTime.Now
                    };
                    _context.EmailLogs.Add(emailLog);
                    _context.SaveChanges();

                    ViewBag.Result = true;
                    ViewBag.ResultMessage = string.Format("Şifreniz değiştirildi! Yeni şifreniz <b>{0}</b>", newpassword);
                    return View();
                }
                catch (Exception ex)
                {
                    ViewBag.Result = false;
                    ViewBag.ResultMessage = string.Format("Email gönderilirken hata oluştu, lütfen tekrar deneyiniz.");
                    return View();
                }
            }
            else
            {
                ViewBag.Result = false;
                ViewBag.ResultMessage = "Yanlış kullanıcı adı girdiniz!";
                return View();
            }
        }

        private string GenerateRandomPassword(int length)
        {
            const string allowedChars = "abcdefghijkmnopqrstuvwxyzABCDEFGHJKLMNOPQRSTUVWXYZ0123456789!@$?_-*&#+";
            var chars = new char[length];
            
            var rd = new Random();
            for (int i = 0; i < length; i++)
            {
                chars[i] = allowedChars[rd.Next(0, allowedChars.Length)];
            }

            return new string(chars);
        }

        //
        // POST: /Account/Disassociate

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult Disassociate(string provider, string providerUserId)
        {
            string ownerAccount = OAuthWebSecurity.GetUserName(provider, providerUserId);
            ManageMessageId? message = null;

            // Only disassociate the account if the currently logged in user is the owner
            if (ownerAccount == User.Identity.Name)
            {
                // Use a transaction to prevent the user from deleting their last login credential
                using (var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Serializable }))
                {
                    bool hasLocalAccount = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
                    if (hasLocalAccount || OAuthWebSecurity.GetAccountsFromUserName(User.Identity.Name).Count > 1)
                    {
                        OAuthWebSecurity.DeleteAccount(provider, providerUserId);
                        scope.Complete();
                        message = ManageMessageId.RemoveLoginSuccess;
                    }
                }
            }

            return RedirectToAction("Manage", new { Message = message });
        }

        //
        // GET: /Account/Manage

        public ActionResult Manage(ManageMessageId? message)
        {
            return View();
        }

        //
        // POST: /Account/Manage

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult Manage(LocalPasswordModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Result = false;
                ViewBag.ResultMessage = "Lütfen belirtilen alanları doldurunuz!";
                return PartialView(model);
            }

            // ChangePassword will throw an exception rather than return false in certain failure scenarios.
            bool changePasswordSucceeded;
            try
            {
                changePasswordSucceeded = WebSecurity.ChangePassword(User.Identity.Name, model.OldPassword, model.NewPassword);
            }
            catch (Exception)
            {
                changePasswordSucceeded = false;
            }

            if (changePasswordSucceeded)
            {
                ViewBag.Result = true;
                ViewBag.ResultMessage = "Şifreniz başarıyla değiştirildi.";
                return PartialView(model);
            }

            ViewBag.Result = false;
            ViewBag.ResultMessage = "Şifreniz değiştirilemedi, lütfen tekrar deneyiniz.";
            return PartialView(model);
        }

        //
        // POST: /Account/ExternalLogin

        [HttpPost]
        [AllowAnonymous]
        //[ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            return new ExternalLoginResult(provider, Url.Action("ExternalLoginCallback", new { ReturnUrl = returnUrl }));
        }


        [AllowAnonymous]
        public ActionResult ExternalLoginCallback(string returnUrl)
        {
            AuthenticationResult result = OAuthWebSecurity.VerifyAuthentication(Url.Action("ExternalLoginCallback", new { ReturnUrl = returnUrl }));
            if (!result.IsSuccessful)
            {
                return RedirectToAction("ExternalLoginFailure");
            }

            if (OAuthWebSecurity.Login(result.Provider, result.ProviderUserId, createPersistentCookie: false))
            {
                return RedirectToLocal(returnUrl);
            }

            if (User.Identity.IsAuthenticated)
            {
                // If the current user is logged in add the new account
                OAuthWebSecurity.CreateOrUpdateAccount(result.Provider, result.ProviderUserId, User.Identity.Name);
                return RedirectToLocal(returnUrl);
            }

            // User is new, ask for their desired membership name
            string loginData = OAuthWebSecurity.SerializeProviderUserId(result.Provider, result.ProviderUserId);
            ViewBag.ProviderDisplayName = OAuthWebSecurity.GetOAuthClientData(result.Provider).DisplayName;
            ViewBag.ReturnUrl = returnUrl;
            return View("ExternalLoginConfirmation", new RegisterExternalLoginModel { UserName = result.UserName, ExternalLoginData = loginData });
        }

        //
        // POST: /Account/ExternalLoginConfirmation

        [HttpPost]
        [AllowAnonymous]
        //[ValidateAntiForgeryToken]
        public ActionResult ExternalLoginConfirmation(RegisterExternalLoginModel model, string returnUrl)
        {
            string provider;
            string providerUserId;

            if (User.Identity.IsAuthenticated || !OAuthWebSecurity.TryDeserializeProviderUserId(model.ExternalLoginData, out provider, out providerUserId))
            {
                return RedirectToAction("Manage");
            }

            if (ModelState.IsValid)
            {
                // Insert a new user into the database
                using (var db = new EStudyBaseContext())
                {
                    UserProfile user = db.UserProfiles.FirstOrDefault(u => u.UserName.ToLower() == model.UserName.ToLower());
                    // Check if user already exists
                    if (user == null)
                    {
                        // Insert name into the profile table
                        db.UserProfiles.Add(new UserProfile { UserName = model.UserName, Email = "user@twitter.com" });
                        db.SaveChanges();

                        OAuthWebSecurity.CreateOrUpdateAccount(provider, providerUserId, model.UserName);
                        OAuthWebSecurity.Login(provider, providerUserId, createPersistentCookie: false);

                        return RedirectToLocal(returnUrl);
                    }

                    ModelState.AddModelError("UserName", "Bu kullanıcı adı ile daha önceden kayıt oluşturulmuş. Lütfen farklı bir kullanıcı adı giriniz.");
                }
            }

            ViewBag.ProviderDisplayName = OAuthWebSecurity.GetOAuthClientData(provider).DisplayName;
            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // GET: /Account/ExternalLoginFailure

        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        [AllowAnonymous]
        [ChildActionOnly]
        public ActionResult ExternalLoginsList(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return PartialView("_ExternalLoginsListPartial", OAuthWebSecurity.RegisteredClientData);
        }

        [ChildActionOnly]
        public ActionResult RemoveExternalLogins()
        {
            ICollection<OAuthAccount> accounts = OAuthWebSecurity.GetAccountsFromUserName(User.Identity.Name);
            var externalLogins = new List<ExternalLogin>();

            foreach (OAuthAccount account in accounts)
            {
                AuthenticationClientData clientData = OAuthWebSecurity.GetOAuthClientData(account.Provider);

                externalLogins.Add(new ExternalLogin
                {
                    Provider = account.Provider,
                    ProviderDisplayName = clientData.DisplayName,
                    ProviderUserId = account.ProviderUserId,
                });
            }

            ViewBag.ShowRemoveButton = externalLogins.Count > 1 || OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
            return PartialView("_RemoveExternalLoginsPartial", externalLogins);
        }

        #region Helpers
        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Search", "Keyword");
        }

        public enum ManageMessageId
        {
            ChangePasswordSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
        }

        internal class ExternalLoginResult : ActionResult
        {
            public ExternalLoginResult(string provider, string returnUrl)
            {
                Provider = provider;
                ReturnUrl = returnUrl;
            }

            public string Provider { get; private set; }
            public string ReturnUrl { get; private set; }

            public override void ExecuteResult(ControllerContext context)
            {
                OAuthWebSecurity.RequestAuthentication(Provider, ReturnUrl);
            }
        }

        private static string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // See http://go.microsoft.com/fwlink/?LinkID=177550 for
            // a full list of status codes.
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return "User name already exists. Please enter a different user name.";

                case MembershipCreateStatus.DuplicateEmail:
                    return "A user name for that e-mail address already exists. Please enter a different e-mail address.";

                case MembershipCreateStatus.InvalidPassword:
                    return "The password provided is invalid. Please enter a valid password value.";

                case MembershipCreateStatus.InvalidEmail:
                    return "The e-mail address provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidAnswer:
                    return "The password retrieval answer provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidQuestion:
                    return "The password retrieval question provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidUserName:
                    return "The user name provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.ProviderError:
                    return "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                case MembershipCreateStatus.UserRejected:
                    return "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                default:
                    return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
            }
        }
        #endregion
    }
}
