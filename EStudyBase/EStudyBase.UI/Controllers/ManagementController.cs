using System;
using System.Collections.Generic;
using System.Data.Entity.SqlServer;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Security;
using Datatables.Mvc;
using EStudyBase.Core.DomainModels;
using EStudyBase.Infrastructure.Mappings;
using EStudyBase.UI.Attributes;
using EStudyBase.UI.ViewModels;
using WebMatrix.WebData;

namespace EStudyBase.UI.Controllers
{
    [InitializeSimpleMembership]
    [AjaxAuthorize(Roles = "Admin")]
    public class ManagementController : Controller
    {
        private readonly EStudyBaseContext _context = new EStudyBaseContext();
        private const string UploadFilePath = "/UploadFiles";

        public ActionResult KeywordList()
        {
            return View();
        }

        [HttpPost]
        public ActionResult KeywordList(DataTable dataTable, string userName = "")
        {
            var isSearchEmpty = string.IsNullOrWhiteSpace(dataTable.sSearch);
            
            //...
            // User portfolio
            var createUserId = WebSecurity.GetUserId(userName);
            
            var totalCount =
                _context
                .Keywords
                .Count(p => (p.Definition.ToLower().Contains(dataTable.sSearch.ToLower()) || isSearchEmpty) && (createUserId == -1 || p.CreateUserId == createUserId));

            //...
            // Where clause
            var keywordList = _context.Keywords
                .Include("Language")
                .Include("CreateUser")
                .Where(p => (p.Definition.ToLower().Contains(dataTable.sSearch.ToLower()) || isSearchEmpty) && (createUserId == -1 || p.CreateUserId == createUserId));

            //...
            // Order by clause
            var sortColumn = HttpContext.Request.Form["iSortCol_0"];
            var sortDirection = HttpContext.Request.Form["sSortDir_0"];
            switch (sortColumn)
            {
                case "0":
                    keywordList = sortDirection.Equals("asc")
                                      ? keywordList.OrderBy(p => p.KeywordId)
                                      : keywordList.OrderByDescending(p => p.KeywordId);
                    break;
                case "1":
                    keywordList = sortDirection.Equals("asc")
                                      ? keywordList.OrderBy(p => p.Definition)
                                      : keywordList.OrderByDescending(p => p.Definition);
                    break;
                case "2":
                    keywordList = sortDirection.Equals("asc")
                                      ? keywordList.OrderBy(p => p.LanguageId)
                                      : keywordList.OrderByDescending(p => p.LanguageId);
                    break;
                case "3":
                    keywordList = sortDirection.Equals("asc")
                                      ? keywordList.OrderBy(p => p.CreateUserId)
                                      : keywordList.OrderByDescending(p => p.CreateUserId);
                    break;
                default:
                    keywordList = sortDirection.Equals("asc")
                                      ? keywordList.OrderBy(p => p.CreateDate)
                                      : keywordList.OrderByDescending(p => p.CreateDate);
                    break;
            }

            //...
            // Filtering
            keywordList = keywordList
                .Skip(dataTable.iDisplayStart)
                .Take(dataTable.iDisplayLength);

            var resultTable = new List<DataTableRow>();
            foreach (var keyword in keywordList)
            {
                var operations = new StringBuilder();
                operations.Append("<table>");
                operations.Append("<tr>");

                operations.Append(
                    string.Format(
                        @"<td>
                              <a href='/Management/KeywordContentList?keywordId={0}' target='_blank'>İçerikler</a>
                         </td>",
                        keyword.KeywordId));

                operations.Append(
                    string.Format(
                        @"<td>
                              <a target='_blank' href='/Management/KeywordTagSelection?KeywordId={0}'>Tag Ekle</a>
                         </td>", keyword.KeywordId));

                operations.Append(
                    string.Format(
                        @"<td>
                              <a target='_blank' href='/Management/CreateContent?KeywordId={0}'>İçerik Ekle</a>
                         </td>", keyword.KeywordId));

                operations.Append(
                    string.Format(
                        @"<td>
                              <input type='checkbox' title='Onayla' data-toggle='tooltip' data-keyword-id='{0}' onclick='ToggleKeywordActivation(this);' {1} />
                        </td>",
                        keyword.KeywordId, keyword.Approved ? "checked" : ""));

                operations.Append(
                    string.Format(
                        @"<td>
                               <div class='icon-remove-sign' title='Sil' data-toggle='tooltip' style='cursor:pointer;' data-keyword-id='{0}' onclick='DeleteKeyword(this);'></div>
                          </td>", keyword.KeywordId));
                operations.Append("</tr>");
                operations.Append("</table>");

                var row = new DataTableRow
                    {
                        keyword.KeywordId.ToString(CultureInfo.InvariantCulture),
                        keyword.Definition,
                        keyword.Language.Definition,
                        keyword.CreateUser == null ? "Anonym" : keyword.CreateUser.UserName,
                        String.Format("{0:d/M/yyyy HH:mm}", keyword.CreateDate),
                        operations.ToString()
                    };
                resultTable.Add(row);
            }
            

            return new DataTableResultExt(dataTable, totalCount, totalCount, resultTable);
        }

        public ActionResult ToggleKeywordActivation()
        {
            var request = HttpContext.Request;
            try
            {
                int keywordId = Convert.ToInt32(request.Form["KeywordId"]);
                bool approve = Convert.ToBoolean(request.Form["Approve"]);
                
                var keyword = _context.Keywords.FirstOrDefault(p => p.KeywordId == keywordId);
                if (keyword != null)
                    keyword.Approved = approve;

                _context.SaveChanges();

                return
                    Json(
                        new
                            {
                                Result = true,
                                ResultMessage = approve ? "Başlık aktifleştirildi." : "Başlık pasifleştirildi"
                            },
                        JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return
                    Json(new {Result = false, ResultMessage = "Başlığın aktiflik durumu değiştirilirken hata oluştu!"},
                         JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult DeleteKeyword()
        {
            var request = HttpContext.Request;
            try
            {
                int keywordId = Convert.ToInt32(request.Form["KeywordId"]);

                var keyword = _context.Keywords.FirstOrDefault(p => p.KeywordId == keywordId);
                if (keyword != null)
                {
                    //...
                    // Remove keyword
                    _context.Keywords.Remove(keyword);

                    //...
                    // Remove shares
                    var shares = _context.Contents.Where(p => p.KeywordId == keyword.KeywordId);
                    foreach (var share in shares)
                    {
                        _context.Contents.Remove(share);
                    }

                    _context.SaveChanges();

                    return Json(
                        new
                            {
                                Result = true,
                                ResultMessage = "Başlık silindi!"
                            },
                        JsonRequestBehavior.AllowGet);
                }

                return
                    Json(
                        new
                            {
                                Result = false,
                                ResultMessage = "Başlık bulunamadı!"
                            },
                        JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(
                    new
                        {
                            Result = false,
                            ResultMessage = "Başlık silinirken hata oluştu!"
                        },
                    JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult KeywordTagSelection(int keywordId = 0)
        {
            try
            {
                var keywordTagSelectionListViewModel = new KeywordTagSelectionViewModel
                {
                    KeywordId = keywordId,
                    Tags = _context.Tags.Where(p=>p.TagParentId != null).ToList(),
                    SelectedTags = _context.KeywordTags.Where(p => p.KeywordId == keywordId).Select(p => p.TagId).ToArray()
                };

                return View(keywordTagSelectionListViewModel);
            }
            catch(Exception exception)
            {
                throw new Exception("Error in ManagementController.CreateKeywordTagSelection [Get]", exception);
            }
        }

        [HttpPost]
        public ActionResult KeywordTagSelection(FormCollection form, KeywordTagSelectionViewModel viewModel)
        {
            //...
            // Restore tag list
            viewModel.Tags = _context.Tags.Where(p => p.TagParentId != null).ToList();

            if (!ModelState.IsValid)
            {
                ViewBag.Result = false;
                ViewBag.ResultMessage = "Lütfen belirtilen alanları doldurunuz!";
                return PartialView(viewModel);
            }

            try
            {
                //... 
                // Delete previously added tags
                var previousSelectedTags =
                    _context.KeywordTags.Where(p => p.KeywordId == viewModel.KeywordId).ToList();
                foreach (var previousSelectedTag in previousSelectedTags)
                {
                    _context.KeywordTags.Remove(previousSelectedTag);
                    _context.SaveChanges();
                }

                //...
                // Create Tags for newly created keyword
                foreach (var item in form["SelectedTags"].Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
                {
                    var keywordTag = new KeywordTag
                    {
                        KeywordId = viewModel.KeywordId,
                        TagId = Convert.ToInt32(item),
                        CreateDate = DateTime.Now
                    };

                    _context.KeywordTags.Add(keywordTag);
                    _context.SaveChanges();
                }

                ViewBag.Result = true;
                ViewBag.ResultMessage = "Girdiğiniz içerik başarıyla kaydedilmiştir.";

                return PartialView(viewModel);
            }
            catch (Exception exception)
            {
                throw new Exception("Error in ManagementController.CreateKeywordTagSelection [Post]", exception);
            }
        }

        public ActionResult KeywordContentList(int keywordId = 0)
        {

            return View();
        }

        [HttpPost]
        public ActionResult KeywordContentList(DataTable dataTable, int keywordId = 0)
        {
            var isSearchEmpty = string.IsNullOrWhiteSpace(dataTable.sSearch);

            var totalCount =
                _context.Contents.Count(p => p.KeywordId == keywordId && (p.Description.ToLower().Contains(dataTable.sSearch.ToLower()) || isSearchEmpty));

            var contentList = _context.Contents
                                      .Include("Language")
                                      .Include("CreateUser")
                                      .Where(
                                          p =>
                                          p.KeywordId == keywordId &&
                                          (p.Description.ToLower().Contains(dataTable.sSearch.ToLower()) ||
                                           isSearchEmpty))
                                      .Select(p => new ContentViewModel
                                          {
                                              KeywordId = p.KeywordId,
                                              LanguageDefinition = p.Language.Definition,
                                              Description = p.Description,
                                              UserName = p.CreateUser.UserName,
                                              ContentId = p.ContentId,
                                              Approved = p.Approved,
                                              Complaint = p.Complaint,
                                              CreateDate = p.CreateDate,
                                              ContentType = p.ContentType,
                                              Url = p.Url
                                          });

            //...
            // Order by clause
            var sortColumn = HttpContext.Request.Form["iSortCol_0"];
            var sortDirection = HttpContext.Request.Form["sSortDir_0"];
            switch (sortColumn)
            {
                case "0":
                    contentList = sortDirection.Equals("asc")
                                      ? contentList.OrderBy(p => p.LanguageDefinition)
                                      : contentList.OrderByDescending(p => p.LanguageDefinition);
                    break;
                case "1":
                    contentList = sortDirection.Equals("asc")
                                      ? contentList.OrderBy(p => p.Description)
                                      : contentList.OrderByDescending(p => p.Description);
                    break;
                case "2":
                    contentList = sortDirection.Equals("asc")
                                      ? contentList.OrderBy(p => p.UserName)
                                      : contentList.OrderByDescending(p => p.UserName);
                    break;
                case "3":
                    contentList = sortDirection.Equals("asc")
                                      ? contentList.OrderBy(p => p.Complaint)
                                      : contentList.OrderByDescending(p => p.Complaint);
                    break;
                default:
                    contentList = sortDirection.Equals("asc")
                                      ? contentList.OrderBy(p => p.CreateDate)
                                      : contentList.OrderByDescending(p => p.CreateDate);
                    break;
            }

            //...
            // Filtering
            contentList = contentList
                .Skip(dataTable.iDisplayStart)
                .Take(dataTable.iDisplayLength);

            var resultTable = new List<DataTableRow>();
            foreach (var content in contentList)
            {
                var operations = new StringBuilder();
                operations.Append("<table>");
                operations.Append("<tr>");

                operations.Append(
                    string.Format(
                        @"<td>
                              <input type='checkbox' title='Onayla' data-toggle='tooltip' data-content-id='{0}' data-content-type='{1}' onclick='ToggleContentActivation(this);' {2} />
                        </td>",
                        content.ContentId, content.ContentType, content.Approved ? "checked" : ""));

                operations.Append(
                    string.Format(
                        @"<td>
                               <div class='icon-remove-sign' title='Sil' data-toggle='tooltip' style='cursor:pointer;' data-content-id='{0}' data-content-type='{1}' onclick='DeleteContent(this);'></div>
                          </td>", content.ContentId, content.ContentType));

                operations.Append("</tr>");
                operations.Append("</table>");

                //...
                // Reset content description
                content.Description = string.Format("<div class='icon-comment'></div> {0}", content.Description);


                var row = new DataTableRow
                    {
                        content.LanguageDefinition,
                        content.Description,
                        string.Format("<a target='_blank' href='/Management/KeywordList/?UserName={0}'>{0}</a>", content.UserName ?? "Anonym"),
                        content.Complaint.ToString(CultureInfo.InvariantCulture),
                        String.Format("{0:d/M/yyyy HH:mm}", content.CreateDate),
                        operations.ToString()
                    };
                resultTable.Add(row);
            }


            return new DataTableResultExt(dataTable, totalCount, totalCount, resultTable);
        }

        public ActionResult ToggleContentActivation()
        {
            var request = HttpContext.Request;
            try
            {
                int contentId = Convert.ToInt32(request.Form["ContentId"]);
                var contentType = Convert.ToInt32(request.Form["ContentType"]);
                bool approve = Convert.ToBoolean(request.Form["Approve"]);

                var content = _context.Contents.FirstOrDefault(p => p.ContentId == contentId);
                if (content != null)
                    content.Approved = approve;

                _context.SaveChanges();
                
                return
                    Json(
                        new
                        {
                            Result = true,
                            ResultMessage = approve ? "İçerik aktifleştirildi." : "İçerik pasifleştirildi"
                        },
                        JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return
                    Json(new { Result = false, ResultMessage = "İçerik aktiflik durumu değiştirilirken hata oluştu!" },
                         JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult DeleteContent()
        {
            var request = HttpContext.Request;
            try
            {
                int contentId = Convert.ToInt32(request.Form["ContentId"]);
                var contentType = Convert.ToInt32(request.Form["ContentType"]);

                var content = _context.Contents.FirstOrDefault(p => p.ContentId == contentId);
                if (content != null)
                    _context.Contents.Remove(content);

                _context.SaveChanges();

                return Json(
                    new
                        {
                            Result = true,
                            ResultMessage = "İçerik silindi!"
                        },
                    JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(
                    new
                        {
                            Result = false,
                            ResultMessage = "İçerik silinirken hata oluştu!"
                        },
                    JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult UserList()
        {
            return View();
        }

        [HttpPost]
        public ActionResult UserList(DataTable dataTable)
        {
            var isSearchEmpty = string.IsNullOrWhiteSpace(dataTable.sSearch);

            var totalCount =
                _context.UserProfiles.Count(
                    p => p.UserName.ToLower().Contains(dataTable.sSearch.ToLower()) || isSearchEmpty);

            //...
            // Where clause
            var userList = _context.UserProfiles
                .Where(p => p.UserName.ToLower().Contains(dataTable.sSearch.ToLower()) || isSearchEmpty);

            //...
            // Order by clause
            var sortColumn = HttpContext.Request.Form["iSortCol_0"];
            var sortDirection = HttpContext.Request.Form["sSortDir_0"];
            switch (sortColumn)
            {
                case "0":
                    userList = sortDirection.Equals("asc")
                                      ? userList.OrderBy(p => p.UserId)
                                      : userList.OrderByDescending(p => p.UserId);
                    break;
                case "1":
                    userList = sortDirection.Equals("asc")
                                      ? userList.OrderBy(p => p.UserName)
                                      : userList.OrderByDescending(p => p.UserName);
                    break;
                case "2":
                    userList = sortDirection.Equals("asc")
                                      ? userList.OrderBy(p => p.Email)
                                      : userList.OrderByDescending(p => p.Email);
                    break;
                default:
                    userList = sortDirection.Equals("asc")
                                      ? userList.OrderBy(p => p.UserName)
                                      : userList.OrderByDescending(p => p.UserName);
                    break;
            }

            //...
            // Filtering
            userList = userList
                .Skip(dataTable.iDisplayStart)
                .Take(dataTable.iDisplayLength);

            var resultTable = new List<DataTableRow>();
            foreach (var user in userList)
            {
                var operations = new StringBuilder();
                operations.Append("<table>");
                operations.Append("<tr>");

                operations.Append(
                    string.Format(
                        @"<td>
                              <a href='{0}' class='btn open-modal' title='Rol Tanımla' data-toggle='tooltip'>Rol</a>
                         </td>",
                        string.Format("/Management/UserRoleSelection?userId={0}", user.UserId)));

                operations.Append(
                    string.Format(
                        @"<td>
                               <div class='icon-remove-sign' title='Sil' data-toggle='tooltip' style='cursor:pointer;' data-user-id='{0}' onclick='DeleteUser(this);'></div>
                          </td>", user.UserId));

                operations.Append("</tr>");
                operations.Append("</table>");

                var row = new DataTableRow
                    {
                        user.UserId.ToString(CultureInfo.InvariantCulture),
                        user.UserName,
                        user.Email,
                        operations.ToString()
                    };
                resultTable.Add(row);
            }


            return new DataTableResultExt(dataTable, totalCount, totalCount, resultTable);
        }

        public ActionResult DeleteUser()
        {
            var request = HttpContext.Request;
            try
            {
                int userId = Convert.ToInt32(request.Form["UserId"]);
                
                var user = _context.UserProfiles.FirstOrDefault(p => p.UserId == userId);
                if (user != null)
                    _context.UserProfiles.Remove(user);

                _context.SaveChanges();

                return Json(
                    new
                    {
                        Result = true,
                        ResultMessage = "Kullanıcı silindi!"
                    },
                    JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(
                    new
                    {
                        Result = false,
                        ResultMessage = "Kullanıcı silinirken hata oluştu!"
                    },
                    JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult UserRoleSelection(int userId = 0)
        {
            //...
            // Language list
            ViewBag.Roles =
                Roles.GetAllRoles().ToList()
                    .Select(p => new SelectListItem
                        {
                            Text = p,
                            Value = p
                        })
                    .ToList();

            try
            {
                var user = _context.UserProfiles.FirstOrDefault(p => p.UserId == userId);
                if (user == null)
                {
                    return PartialView("_UserRoleSelection");
                }

                var userRoleSelectionListViewModel = new UserRoleSelectionViewModel
                    {
                        UserId = userId,
                        UserName = user.UserName,
                        UserRole = Roles.GetRolesForUser(user.UserName).FirstOrDefault()
                    };

                return PartialView("_UserRoleSelection", userRoleSelectionListViewModel);
            }
            catch
            {
                return PartialView("_UserRoleSelection");
            }
        }

        [HttpPost]
        public ActionResult UserRoleSelection(FormCollection form, UserRoleSelectionViewModel viewModel)
        {
            //...
            // Language list
            ViewBag.Roles =
                Roles.GetAllRoles().ToList()
                    .Select(p => new SelectListItem
                        {
                            Text = p,
                            Value = p
                        })
                    .ToList();

            if (!ModelState.IsValid)
            {
                ViewBag.Result = false;
                ViewBag.ResultMessage = "Lütfen belirtilen alanları doldurunuz!";
                return PartialView("_UserRoleSelection", viewModel);
            }

            var user = _context.UserProfiles.FirstOrDefault(p => p.UserId == viewModel.UserId);
            if (user == null)
            {
                ViewBag.Result = false;
                ViewBag.ResultMessage = "Kullanıcı bilgisi bulunamadı!";
                return PartialView("_UserRoleSelection", viewModel);
            }

            try
            {
                //... 
                // Delete previously added tags
                var preRole = Roles.GetRolesForUser(user.UserName).FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(preRole))
                {
                    Roles.RemoveUserFromRole(user.UserName, preRole);
                }

                //...
                // Add user to role
                Roles.AddUserToRole(user.UserName, viewModel.UserRole);

                ViewBag.Result = true;
                ViewBag.ResultMessage = "Kullanıcının rol bilgisi tanımlanmıştır.";

                return PartialView("_UserRoleSelection", viewModel);
            }
            catch
            {
                ViewBag.Result = false;
                ViewBag.ResultMessage = "Hata oluştu lütfen tekrar deneyiniz!";
                return PartialView("_UserRoleSelection", viewModel);
            }
        }

        public ActionResult CreateKeyword()
        {
            try
            {
                var viewModel = new KeywordViewModel
                    {
                        Languages = _context
                            .Languages
                            .Select(language => new SelectListItem
                                {
                                    Text = language.Definition,
                                    Value = SqlFunctions.StringConvert((double?) language.LanguageId).Trim()
                                })
                            .ToList(),
                        Tags = _context.Tags.Where(p => p.TagParentId != null).ToList()
                    };

                return View(viewModel);
            }
            catch (Exception exception)
            {
                throw new Exception("Error in ManagementController.CreateKeyword [Get]", exception);
            }
        }

        [HttpPost]
        public ActionResult CreateKeyword(FormCollection form, KeywordViewModel viewModel)
        {
            try
            {
                //...
                // Languages
                viewModel.Languages = _context
                    .Languages
                    .Select(language => new SelectListItem
                        {
                            Text = language.Definition,
                            Value = SqlFunctions.StringConvert((double?) language.LanguageId).Trim()
                        })
                    .ToList();

                //...
                // Tags
                viewModel.Tags = _context.Tags.Where(p => p.TagParentId != null).ToList();

                // Check model state
                if (!ModelState.IsValid)
                {
                    ViewBag.Result = false;
                    ViewBag.ResultMessage = "Lütfen belirtilen alanları doldurunuz!";
                    return PartialView(viewModel);
                }

                // Check keyword for existence
                var keywordExists = _context.Keywords.Any(p => p.Definition.ToLower().Equals(viewModel.Definition.ToLower()));

                if (keywordExists)
                {
                    ViewBag.Result = false;
                    ViewBag.ResultMessage = "Bu başlık daha önceden girilmiş.";
                    return PartialView(viewModel);
                }

                //...
                // Create Keyword
                var keyword = new Keyword
                {
                    Definition = viewModel.Definition,
                    LanguageId = viewModel.LanguageId,
                    Synonym = viewModel.Synonym,
                    Antonym = viewModel.Antonym,
                    Order = viewModel.Order,
                    Approved = viewModel.Approved,
                    CreateUserId = WebSecurity.CurrentUserId,
                    CreateDate = DateTime.Now
                };

                _context.Keywords.Add(keyword);
                _context.SaveChanges();

                //...
                // Create Tags for newly created keyword
                foreach (var item in form["SelectedTags"].Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
                {
                    var keywordTag = new KeywordTag
                    {
                        KeywordId = keyword.KeywordId,
                        TagId = Convert.ToInt32(item),
                        CreateDate = DateTime.Now
                    };
                    _context.KeywordTags.Add(keywordTag);
                }

                //...
                // Save changes
                _context.SaveChanges();

                ViewBag.Result = true;
                ViewBag.ResultMessage = "Girdiğiniz içerik başarıyla kaydedilmiştir.";
                return PartialView(viewModel);
            }
            catch (Exception exception)
            {
                throw new Exception("Error in ManagementController.CreateKeyword [Post]", exception);
            }

        }

        public ActionResult CreateContent(int keywordId = 0)
        {
            var viewModel = new CreateContentViewModel
            {
                KeywordId = keywordId,
                Languages = _context
                    .Languages
                    .Select(language => new SelectListItem
                    {
                        Text = language.Definition,
                        Value = SqlFunctions.StringConvert((double?)language.LanguageId).Trim()
                    })
                    .ToList(),
                ContentCategories = _context
                    .ContentCategories
                    .Select(category => new SelectListItem
                    {
                        Text = category.Definition,
                        Value = SqlFunctions.StringConvert((double?)category.ContentCategoryId).Trim()
                    })
                    .ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult CreateContent(CreateContentViewModel viewModel)
        {
            viewModel.Languages = _context
                .Languages
                .Select(language => new SelectListItem
                {
                    Text = language.Definition,
                    Value = SqlFunctions.StringConvert((double?)language.LanguageId).Trim()
                })
                .ToList();
            viewModel.ContentCategories = _context
                .ContentCategories
                .Select(category => new SelectListItem
                {
                    Text = category.Definition,
                    Value = SqlFunctions.StringConvert((double?)category.ContentCategoryId).Trim()
                })
                .ToList();

            if (string.IsNullOrWhiteSpace(viewModel.Description))
            {
                ViewBag.Result = false;
                ViewBag.ResultMessage = "Lütfen açıklama giriniz!";
                return PartialView("CreateContent", viewModel);
            }

            if (viewModel.PostedFile != null)
            {
                viewModel.PostedFile.SaveAs(Path.Combine(Server.MapPath(UploadFilePath),
                                                         viewModel.PostedFile.FileName));
            }

            try
            {
                var contentModel = new Content
                {
                    KeywordId = viewModel.KeywordId,
                    LanguageId = viewModel.LanguageId,
                    ContentCategoryId = viewModel.ContentCategoryId,
                    Url =
                        viewModel.PostedFile != null
                            ? Path.Combine(UploadFilePath, viewModel.PostedFile.FileName)
                            : viewModel.Url,
                    Description = viewModel.Description,
                    ContentType = ContentType.Translation,
                    CreateUserId = WebSecurity.CurrentUserId,
                    Approved = true,
                    LikeCount = 0,
                    DislikeCount = 0,
                    Complaint = 0,
                    CreateDate = DateTime.Now
                };

                _context.Contents.Add(contentModel);
                _context.SaveChanges();

                ViewBag.Result = true;
                ViewBag.ResultMessage = "Girdiğiniz bilgiler kaydedilmiştir!";
                return PartialView("CreateContent", viewModel);
            }
            catch (Exception exception)
            {
                throw new Exception("Error in ManagementController.CreateContent [Post]", exception);
            }
        }
    }
}
