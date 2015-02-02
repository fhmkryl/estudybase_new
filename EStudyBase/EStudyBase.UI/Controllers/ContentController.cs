using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EStudyBase.Core.DomainModels;
using EStudyBase.Infrastructure.Mappings;
using EStudyBase.UI.Attributes;
using EStudyBase.UI.ViewModels;
using WebMatrix.WebData;

namespace EStudyBase.UI.Controllers {
    [InitializeSimpleMembership]
    public class ContentController : Controller {
        private readonly EStudyBaseContext _context = new EStudyBaseContext();
        private const string UploadFilePath = "/UploadFiles";
        private readonly List<string> _allowedFileExtensions = new List<string>
            {
                ".JPG",
                ".JPE",
                ".BMP",
                ".GIF",
                ".PNG",
                ".AVI",
                ".MP4",
                ".FLV",
                ".MOV",
                ".M4V",
                ".F4V"
            };

        [AjaxAuthorize]
        public ActionResult CreateKeyword() {
            try {
                var viewModel = new KeywordViewModel {
                    Languages = _context
                        .Languages
                        .Select(language => new SelectListItem {
                            Text = language.Definition,
                            Value = SqlFunctions.StringConvert((double?)language.LanguageId).Trim()
                        })
                        .ToList(),
                    Tags = _context.Tags.Where(p => p.TagParentId != null).ToList()
                };

                return PartialView("_NewKeyword", viewModel);
            }
            catch (Exception exception) {
                throw new Exception("Error in ManagementController.CreateKeyword [Get]", exception);
            }
        }

        [AjaxAuthorize]
        [HttpPost]
        public ActionResult CreateKeyword(FormCollection form, KeywordViewModel viewModel) {
            try {
                //...
                // Languages
                viewModel.Languages = _context
                    .Languages
                    .Select(language => new SelectListItem {
                        Text = language.Definition,
                        Value = SqlFunctions.StringConvert((double?)language.LanguageId).Trim()
                    })
                    .ToList();

                //...
                // Tags
                viewModel.Tags = _context.Tags.Where(p => p.TagParentId != null).ToList();

                // ...
                // Validations
                if (string.IsNullOrWhiteSpace(viewModel.Definition)) {
                    ViewBag.Result = false;
                    ViewBag.ResultMessage = "Lütfen başlık bölümünü doldurunuz!";
                    return PartialView("_NewKeyword", viewModel);
                }

                if (viewModel.SelectedTags == null) {
                    ViewBag.Result = false;
                    ViewBag.ResultMessage = "Lütfen en az bir etiket seçiniz!";
                    return PartialView("_NewKeyword", viewModel);
                }

                // Check keyword for existence
                var keywordExists = _context.Keywords.Any(p => p.Definition.ToLower().Equals(viewModel.Definition.ToLower()));

                if (keywordExists) {
                    ViewBag.Result = false;
                    ViewBag.ResultMessage = "Bu başlık daha önceden girilmiş.";
                    return PartialView("_NewKeyword", viewModel);
                }

                //...
                // Create Keyword
                var keyword = new Keyword {
                    Definition = viewModel.Definition,
                    LanguageId = viewModel.LanguageId,
                    Synonym = viewModel.Synonym,
                    Antonym = viewModel.Antonym,
                    //Order = viewModel.Order,
                    Approved = true,
                    CreateUserId = WebSecurity.CurrentUserId,
                    CreateDate = DateTime.Now
                };

                _context.Keywords.Add(keyword);
                _context.SaveChanges();

                //...
                // Create Tags for newly created keyword
                foreach (var item in form["SelectedTags"].Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)) {
                    var keywordTag = new KeywordTag {
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
                return PartialView("_NewKeyword", viewModel);
            }
            catch (Exception exception) {
                throw new Exception("Error in ContentController.CreateKeyword [Post]", exception);
            }

        }

        public JsonResult GetContentDetailsByContentId(int contentId = 0) {
            var content =
                _context.Contents
                    .Include(p => p.Keyword)
                    .FirstOrDefault(p => p.ContentId == contentId);
            if (content != null) {
                return
                    Json(
                        new {
                            keywordId = content.Keyword.KeywordId,
                            ShareUrl = string.Format("http://estudybase.com/?Term={0}&ContentId={1}", content.Keyword.Definition, contentId),
                            ShareDescription = string.Format("{0} {1}", content.Keyword.Definition, content.Description),
                            Result = true
                        },
                        JsonRequestBehavior.AllowGet);
            }

            return
                Json(
                    new {
                        Result = false,
                        ResultMessage = "İçerik bilgisi bulunamadı, lütfen daha sonra tekrar deneyiniz!"
                    },
                    JsonRequestBehavior.AllowGet);
        }

        [AjaxAuthorize]
        public PartialViewResult CreateContent(int keywordId = 0) {
            var viewModel = new CreateContentViewModel {
                KeywordId = keywordId,
                Languages = _context
                    .Languages
                    .Select(language => new SelectListItem {
                        Text = language.Definition,
                        Value = SqlFunctions.StringConvert((double?)language.LanguageId).Trim()
                    })
                    .ToList(),
                ContentCategories = _context
                    .ContentCategories
                    .Select(category => new SelectListItem {
                        Text = category.Definition,
                        Value = SqlFunctions.StringConvert((double?)category.ContentCategoryId).Trim()
                    })
                    .ToList()
            };

            return PartialView("_NewContent", viewModel);
        }

        [AjaxAuthorize]
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult CreateContent(CreateContentViewModel viewModel) {
            viewModel.Languages = _context
                .Languages
                .Select(language => new SelectListItem {
                    Text = language.Definition,
                    Value = SqlFunctions.StringConvert((double?)language.LanguageId).Trim()
                })
                .ToList();
            viewModel.ContentCategories = _context
                .ContentCategories
                .Select(category => new SelectListItem {
                    Text = category.Definition,
                    Value = SqlFunctions.StringConvert((double?)category.ContentCategoryId).Trim()
                })
                .ToList();

            if (string.IsNullOrWhiteSpace(viewModel.Description)) {
                ViewBag.Result = false;
                ViewBag.ResultMessage = "Lütfen açıklama giriniz!";
                return PartialView("_NewContent", viewModel);
            }

            var fileName = string.Empty;
            if (viewModel.PostedFile != null) {
                //... 
                // Check File Size
                if (viewModel.PostedFile.ContentLength > 10 * 1024 * 1024) {
                    ViewBag.Result = false;
                    ViewBag.ResultMessage = "Dosya boyutu max 10 MB olabilir!";
                    return PartialView("_NewContent", viewModel);
                }

                //...
                // Check file extensions
                if (!_allowedFileExtensions.Contains(new FileInfo(viewModel.PostedFile.FileName).Extension.ToUpper())) {
                    ViewBag.Result = false;
                    ViewBag.ResultMessage = "Bu dosya formatı desteklenmiyor, lütfen resim veya video dosyası ekleyiniz!";
                    return PartialView("_NewContent", viewModel);
                }

                //...
                // Get Keyword Definition
                var keyword = _context.Keywords.FirstOrDefault(p => p.KeywordId == viewModel.KeywordId);
                if (keyword != null) {
                    fileName = SaveUploadedFile(HttpContext.ApplicationInstance.Context, keyword.KeywordId, keyword.Definition);
                    if (string.IsNullOrWhiteSpace(fileName)) {
                        ViewBag.Result = false;
                        ViewBag.ResultMessage = "Dosya yüklenirken hata oluştu, lütfen daha sonra tekrar deneyiniz!";
                        return PartialView("_NewContent", viewModel);
                    }
                }
                else {
                    ViewBag.Result = false;
                    ViewBag.ResultMessage = "başlık bilgisi bulunamadı, lütfen daha sonra tekrar deneyiniz!";
                    return PartialView("_NewContent", viewModel);
                }
            }

            try {
                var contentModel = new Content {
                    KeywordId = viewModel.KeywordId,
                    LanguageId = viewModel.LanguageId,
                    ContentCategoryId = viewModel.ContentCategoryId,
                    Url = string.IsNullOrWhiteSpace(fileName) ? string.Empty : UploadFilePath + "/" + fileName,
                    Description = viewModel.Description,
                    ContentType = ContentType.FileUpload,
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
                return PartialView("_NewContent", viewModel);
            }
            catch (Exception exception) {
                throw new Exception("Error in ContentController.CreateContent [Post]", exception);
            }
        }

        [AjaxAuthorize]
        public ActionResult DeleteContent(int contentId = 0) {
            try {
                var content = _context.Contents.FirstOrDefault(p => p.ContentId == contentId);

                if (content != null) {
                    if (content.CreateUserId != WebSecurity.CurrentUserId) {
                        return JavaScript("bootbox.alert('İçeriği silmek için gerekli yetkiye sahip değilsiniz!");
                    }

                    _context.Contents.Remove(content);
                    _context.SaveChanges();
                    return JavaScript("bootbox.alert('İçerik başarıyla silinmiştir!');location.href = location.href;");
                }
                return JavaScript("bootbox.alert('İçerik bulunamadı!");
            }
            catch {
                return JavaScript("bootbox.alert('İçerik silinirken hata oluştu, lütfen daha sonra tekrar deneyiniz')");
            }
        }

        [AjaxAuthorize]
        public PartialViewResult CreateRecord(int keywordId = 0) {

            var viewModel = new CreateContentViewModel {
                KeywordId = keywordId,
                Languages = _context
                    .Languages
                    .Select(language => new SelectListItem {
                        Text = language.Definition,
                        Value = SqlFunctions.StringConvert((double?)language.LanguageId).Trim()
                    })
                    .ToList(),
                ContentCategories = _context
                    .ContentCategories
                    .Select(category => new SelectListItem {
                        Text = category.Definition,
                        Value = SqlFunctions.StringConvert((double?)category.ContentCategoryId).Trim()
                    })
                    .ToList()
            };
            return PartialView("_NewRecord", viewModel);
        }

        [AjaxAuthorize]
        [HttpPost]
        [ValidateInput(false)]
        public JsonResult CreateRecord(FormCollection form, int keywordId = 0) {
            if (string.IsNullOrWhiteSpace(form["Description"])) {
                return Json(new { Result = false, ResultMessage = "Lütfen açıklama alanını doldurunuz." }, JsonRequestBehavior.AllowGet);
            }
            //...
            // Check File Existence
            if (HttpContext.ApplicationInstance.Context.Request.Files.Count != 1) {
                return Json(new { Result = false, ResultMessage = "Ses kaydınız alınamadı, lütfen tekrar deneyiniz." }, JsonRequestBehavior.AllowGet);
            }

            try {
                string fileName;

                //...
                // Get Keyword Definition
                var keyword = _context.Keywords.FirstOrDefault(p => p.KeywordId == keywordId);
                if (keyword != null) {
                    //... 
                    // Check File Size
                    if (HttpContext.ApplicationInstance.Context.Request.Files[0].ContentLength > 10 * 1024 * 1024) {
                        return Json(new { Result = false, ResultMessage = "Ses kaydı boyutu max 10 MB olabilir!" }, JsonRequestBehavior.AllowGet);
                    }

                    fileName = SaveUploadedFile(HttpContext.ApplicationInstance.Context, keyword.KeywordId, keyword.Definition);
                }
                else {
                    return Json(new { Result = false, ResultMessage = "başlık bilgisi bulunamadı, lütfen daha sonra tekrar deneyiniz!" }, JsonRequestBehavior.AllowGet);
                }

                bool result = !string.IsNullOrWhiteSpace(fileName);
                string resultMessage = !string.IsNullOrWhiteSpace(fileName)
                                           ? "Ses kaydınız başarıyla alınmıştır!"
                                           : "Hata oluştu, lütfen tekrar deneyiniz!";
                if (result) {
                    var contentModel = new Content {
                        KeywordId = keywordId,
                        LanguageId = Convert.ToInt32(form["LanguageId"]),
                        ContentCategoryId = Convert.ToInt32(form["ContentCategoryId"]),
                        Url = UploadFilePath + "/" + fileName,
                        Description = form["Description"],
                        ContentType = ContentType.VoiceRecord,
                        CreateUserId = WebSecurity.CurrentUserId,
                        Approved = true,
                        Complaint = 0,
                        CreateDate = DateTime.Now
                    };

                    _context.Contents.Add(contentModel);
                    _context.SaveChanges();
                }

                return Json(new { Result = result, ResultMessage = resultMessage }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exception) {
                throw new Exception("Error in ContentController.CreateRecord [Post]", exception);
            }
        }

        public string SaveUploadedFile(HttpContext context, int keywordId, string keywordDefinition) {
            //...
            // Check upload files count
            if (context.Request.Files.Count <= 0 || context.Request.Files.Count > 1) {
                return string.Empty;
            }

            try {
                var file = context.Request.Files[0];
                var fileExtension = new FileInfo(file.FileName).Extension;
                var fileName = string.Format("{0}_{1}_{2}{3}", keywordDefinition, keywordId, DateTime.Now.Ticks,
                                             fileExtension);
                file.SaveAs(Path.Combine(Server.MapPath(UploadFilePath), fileName));
                //file.SaveAs(string.Format("{0}/{1}", UploadFilePath, fileName));
                return fileName;
            }
            catch (Exception exception) {
                throw new Exception("Error in UploadController.UploadFile", exception);
            }
        }

        public ActionResult DownloadFile(string file) {
            return File(file, System.Net.Mime.MediaTypeNames.Application.Octet);
        }
    }
}
