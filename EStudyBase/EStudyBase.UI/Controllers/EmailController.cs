using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using EStudyBase.Common.Mailers;
using EStudyBase.Core.DomainModels;
using EStudyBase.Infrastructure.Mappings;
using EStudyBase.UI.Attributes;
using EStudyBase.UI.ViewModels;
using Mvc.Mailer;
using WebMatrix.WebData;

namespace EStudyBase.UI.Controllers
{
    [InitializeSimpleMembership]
    [AjaxAuthorize]
    public class EmailController : Controller
    {
        private readonly EStudyBaseContext _context = new EStudyBaseContext();

        private IUserMailer _userMailer = new UserMailer();
        public IUserMailer UserMailer
        {
            get { return _userMailer; }
            set { _userMailer = value; }
        }

        public ActionResult EmailBox(string userName = "", int emailId = 0)
        {
            var loggedInUserId = WebSecurity.CurrentUserId;
            var allCount = _context.EmailLogs.Count(p => p.FromUserId == loggedInUserId || p.ToUserId == loggedInUserId);
            var inboxCount = _context.EmailLogs.Count(p => p.ToUserId == loggedInUserId);
            var outboxCount = _context.EmailLogs.Count(p => p.FromUserId == loggedInUserId);
            var unReadCount = _context.EmailLogs.Count(p => p.ToUserId == loggedInUserId && !p.Read);
            var deletedCount =
                _context.EmailLogs.Count(
                    p => (p.FromUserId == loggedInUserId || p.ToUserId == loggedInUserId) && p.Deleted);

            ViewBag.AllCount = allCount;
            ViewBag.InboxCount = inboxCount;
            ViewBag.OutboxCount = outboxCount;
            ViewBag.UnReadCount = unReadCount;
            ViewBag.DeletedCount = deletedCount;
            ViewBag.ToUserId = WebSecurity.GetUserId(userName);
            ViewBag.EmailId = emailId;

            return View();
        }

        public ActionResult GetEmailBox(string operationType, int currentPage = 1, string pageDirection = null)
        {
            if (pageDirection != null && pageDirection.Equals("Next"))
            {
                currentPage++;
            }
            if (pageDirection != null && pageDirection.Equals("Previous"))
            {
                currentPage--;
            }
            ViewBag.CurrentPage = currentPage;
            ViewBag.OperationType = operationType;
            var skip = (currentPage - 1) * 10;

            int loggedInUserId = WebSecurity.CurrentUserId;
            var query = _context.EmailLogs
                                       .Include(p => p.FromUser)
                                       .Include(p => p.ToUser)
                                       .Include(p => p.EmailLogType);
            switch (operationType)
            {
                case "All":
                    query = query.Where(p => p.ToUserId == loggedInUserId || p.FromUserId == loggedInUserId);
                    ViewBag.ListHeader = "Tüm Mesajlar";
                    ViewBag.ListCount = query.Count();
                    break;
                case "Inbox":
                    query = query.Where(p => p.ToUserId == loggedInUserId);
                    ViewBag.ListHeader = "Gelen Mesajlar";
                    ViewBag.ListCount = query.Count();
                    break;
                case "Outbox":
                    query = query.Where(p => p.FromUserId == loggedInUserId);
                    ViewBag.ListHeader = "Giden Mesajlar";
                    ViewBag.ListCount = query.Count();
                    break;
                case "UnRead":
                    query = query.Where(p => p.ToUserId == loggedInUserId && !p.Read);
                    ViewBag.ListHeader = "Okunmamış Mesajlar";
                    ViewBag.ListCount = query.Count();
                    break;
                case "Deleted":
                    query =
                        query.Where(p => (p.ToUserId == loggedInUserId || p.FromUserId == loggedInUserId) && p.Deleted);
                    ViewBag.ListHeader = "Silinen Mesajlar";
                    ViewBag.ListCount = query.Count();
                    break;
                default:
                    query = query.Where(p => p.ToUserId == loggedInUserId || p.FromUserId == loggedInUserId);
                    ViewBag.ListHeader = "Tüm Mesajlar";
                    ViewBag.ListCount = query.Count();
                    break;
            }

            var emailBoxList =
                query
                    .OrderByDescending(p => p.CreateDate)
                    .Skip(skip)
                    .Take(10)
                    .Select(p => new EmailBoxViewModel
                    {
                        EmailId = p.EmailId,
                        FromUserId = p.FromUserId,
                        FromUserName = p.FromUser.UserName,
                        FromUserEmail = p.FromUser.Email,
                        ToUserId = p.ToUser.UserId,
                        ToUserName = p.ToUser.UserName,
                        ToUserEmail = p.ToUser.Email,
                        Header = p.Header,
                        Content = p.Content,
                        EmailTypeId = p.EmailTypeId,
                        EmailType = p.EmailLogType.Description,
                        CreateDate = p.CreateDate
                    }).ToList();

            return PartialView("_EmailBoxList", emailBoxList);
        }

        public ActionResult GetUnreadEmailCount()
        {
            int loggedInUserId = WebSecurity.CurrentUserId;
            var unreadEmailCount = _context.EmailLogs.Count(p => p.ToUserId == loggedInUserId && !p.Read);

            return Content(string.Format(@"<a href='/Email/EmailBox' target='_blank'>
                                                    mesajlar <span class='badge'>{0}</span>
                                                </a>", unreadEmailCount));
        }

        public ActionResult GetEmailContent(int emailId)
        {
            int loggedInUserId = WebSecurity.CurrentUserId;
            var emailBoxViewModel = _context.EmailLogs
                                            .Include(p => p.FromUser)
                                            .Include(p => p.ToUser)
                                            .Include(p => p.EmailLogType)
                                            .Where(
                                                p =>
                                                p.EmailId == emailId &&
                                                (p.FromUserId == loggedInUserId || p.ToUserId == loggedInUserId))
                                            .Select(p => new EmailBoxViewModel
                                            {
                                                EmailId = p.EmailId,
                                                FromUserId = p.FromUserId,
                                                FromUserName = p.FromUser.UserName,
                                                FromUserEmail = p.FromUser.Email,
                                                ToUserId = p.ToUser.UserId,
                                                ToUserName = p.ToUser.UserName,
                                                ToUserEmail = p.ToUser.Email,
                                                Header = p.Header,
                                                Content = p.Content,
                                                EmailTypeId = p.EmailTypeId,
                                                EmailType = p.EmailLogType.Description,
                                                CreateDate = p.CreateDate
                                            }).FirstOrDefault();

            //...
            // Mark email as read
            if (emailBoxViewModel != null)
            {
                var emailLog = _context.EmailLogs.FirstOrDefault(p => p.EmailId == emailBoxViewModel.EmailId);
                if (emailLog != null)
                {
                    emailLog.Read = true;
                    _context.SaveChanges();
                }
            }

            return PartialView("_EmailContent", emailBoxViewModel);
        }

        public ActionResult DeleteEmail(int emailid)
        {
            var emailLog = _context.EmailLogs.FirstOrDefault(p => p.EmailId == emailid);
            if (emailLog != null)
            {
                emailLog.Deleted = true;
                _context.SaveChanges();
            }

            return JavaScript("bootbox.alert('Mesajınız başarıyla silinmiştir!')");
        }

        public ActionResult EmailToUser(int userId = 0)
        {
            try
            {
                var user = _context.UserProfiles.FirstOrDefault(p => p.UserId == userId);
                if (user != null)
                {
                    var viewModel = new EmailViewModel
                    {
                        ToUserId = userId,
                        UserName = user.UserName
                    };
                    return PartialView("_EmailToUser", viewModel);
                }
                return PartialView("_EmailToUser");
            }
            catch (Exception exception)
            {
                throw new Exception("Error in EmailController.EmailToUser [Get]", exception);
            }
        }

        [HttpPost]
        public ActionResult EmailToUser(EmailViewModel viewModel)
        {
            // Check model state
            if (!ModelState.IsValid)
            {
                ViewBag.Result = false;
                ViewBag.ResultMessage = "Lütfen belirtilen alanları doldurunuz!";

                return PartialView("_EmailToUser", viewModel);
            }

            try
            {
                var fromUserName = WebSecurity.CurrentUserName;
                var toUser = _context.UserProfiles.FirstOrDefault(p => p.UserId == viewModel.ToUserId);

                const string view = "EmailTemplate";
                var header = string.Format("{0}", viewModel.Subject);
                var content = viewModel.Body;

                //...
                // Check to user email info
                if (toUser == null)
                {
                    ViewBag.Result = false;
                    ViewBag.ResultMessage = "Kullanıcı bilgisi bulunamadı!";
                    return PartialView("_EmailToUser", viewModel);
                }

                if (string.IsNullOrWhiteSpace(toUser.Email))
                {
                    ViewBag.Result = false;
                    ViewBag.ResultMessage = "Kullanıcı email bilgisi bulunamadı!";
                    return PartialView("_EmailToUser", viewModel);
                }

                //...
                // Log sent email
                var emailLog = new EmailLog
                {
                    EmailTypeId = 2,
                    FromUserId = WebSecurity.GetUserId(fromUserName),
                    ToUserId = toUser.UserId,
                    Header = header,
                    Content = content,
                    CreateDate = DateTime.Now
                };
                _context.EmailLogs.Add(emailLog);
                _context.SaveChanges();

                //...
                // Send email
                header = string.Format("{0} kullanıcısı size bir mesaj gönderdi!", fromUserName);
                content =
                    string.Format(
                        @"{0} kullanıcısı size {1} başlıklı mesaj gönderdi.<br/>
                        Mesajı okumak için lütfen <a href='http://estudybase.com/Email/EmailBox?EmailId={2}'>tıklayınız.</a>",
                        fromUserName, viewModel.Subject, emailLog.EmailId);
                MvcMailMessage mvcMailMessage = UserMailer.SendEmail(view, header, content, toUser.Email);
                mvcMailMessage.Send();

                ViewBag.Result = true;
                ViewBag.ResultMessage = "Email başarıyla gönderildi!";
                return PartialView("_EmailToUser", viewModel);
            }
            catch (Exception exception)
            {
                throw new Exception("Error in EmailController.EmailToUser [Post]", exception);
            }

        }
    }
}