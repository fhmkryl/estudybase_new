using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Web.Mvc;
using EStudyBase.Core.DomainModels;
using EStudyBase.Infrastructure.Mappings;
using EStudyBase.UI.Attributes;
using EStudyBase.UI.ViewModels;
using WebMatrix.WebData;

namespace EStudyBase.UI.Controllers
{
    [InitializeSimpleMembership]
    public class KeywordController : Controller
    {
        private readonly EStudyBaseContext _context = new EStudyBaseContext();

        //[OutputCache(Duration = 3600, VaryByParam = "none")]
        public ActionResult GetTotalKeywordCount() {
            try {
                var totalKeywordCount = _context.Keywords.Count();
                var totalContentCount = _context.Contents.Count();

                var content = string.Format(@"Toplam <b>{0}</b> başlık ve <b>{1}</b> içerik ile her gün büyüyen sözlük!",
                                            totalKeywordCount, totalContentCount);
                return Content(content);
            } catch(Exception exception) {
                throw new Exception("Error in KeywordController.GetTotalKeywordCount", exception);
            }
        }

        public ActionResult Search(KeywordSearchCriteriaViewModel searchCriteria) {
            try {
                
                // If request type is post try to get term from form collection
                if(HttpContext.Request.RequestType == "POST") {
                    searchCriteria.Term = HttpContext.Request.Form["Term"];
                }

                // Redirect to most popular keyword
                if(searchCriteria.KeywordId == null && string.IsNullOrWhiteSpace(searchCriteria.Term)) {
                    var keyword = _context.Database.SqlQuery<Keyword>("SELECT * FROM dbo.GetMostPopularKeyword()").FirstOrDefault();

                    searchCriteria.KeywordId = keyword != null ? keyword.KeywordId : 0;
                }

                SearchViewModel searchViewModel = null;
                // Search logic
                if(searchCriteria.KeywordId.HasValue && searchCriteria.KeywordId > 0) {
                    searchViewModel = _context
                        .Keywords
                        .Include(p => p.Contents)
                        .Include(p => p.Contents.Select(q => q.ContentCategory))
                        .Include(p => p.CreateUser)
                        .Where(p => p.KeywordId == searchCriteria.KeywordId && p.Approved)
                        .Select(p => new SearchViewModel {
                            KeywordId = p.KeywordId,
                            Definition = p.Definition,
                            Synonym = p.Synonym,
                            Antonym = p.Antonym,
                            Order = p.Order,
                            CreateUser = p.CreateUser
                        })
                        .FirstOrDefault();
                } else if(!string.IsNullOrWhiteSpace(searchCriteria.Term)) {
                    searchViewModel = _context
                        .Keywords
                        .Include(p => p.Contents)
                        .Include(p => p.Contents.Select(q => q.ContentCategory))
                        .Include(p => p.CreateUser)
                        .Where(p => p.Definition.ToLower().Equals(searchCriteria.Term.ToLower()) && p.Approved)
                        .Select(p => new SearchViewModel {
                            KeywordId = p.KeywordId,
                            Definition = p.Definition,
                            Synonym = p.Synonym,
                            Antonym = p.Antonym,
                            Order = p.Order,
                            CreateUser = p.CreateUser
                        })
                        .FirstOrDefault();
                }

                // No keyword found for this search
                // Suggest related keywords
                if(searchViewModel == null) {
                    DbSqlQuery<Keyword> relatedKeywords = _context.Keywords.SqlQuery(
                        string.Format(@"SELECT TOP 5 * FROM Keyword
                                        ORDER BY dbo.LevenshteinSearchAlgorithm('{0}',Definition)",
                            ViewBag.Term));
                    searchViewModel = new SearchViewModel {
                        RelatedKeywords = relatedKeywords.ToList()
                    };
                }

                searchCriteria.Term = searchViewModel.Definition;
                searchViewModel.SearchCriteria = searchCriteria;

                return View(searchViewModel);
            } catch(Exception exception) {
                throw new Exception("Error in KeywordController.Search", exception);
            }
        }

        public ActionResult SocialNetwork(int contentId = 0) {
            var contentViewModel =
                _context.Contents
                    .Where(p => p.ContentId == contentId)
                    .Select(p => new ContentViewModel {
                        ContentId = p.ContentId,
                        KeywordId = p.KeywordId
                    })
                    .FirstOrDefault();
            return View("SocialNetwork", contentViewModel);
        }

        public ActionResult GetKeywordTagList(int? keywordId = null) {
            if(keywordId == null) {
                return PartialView("_KeywordTagList");
            }

            var tagList = _context
                .KeywordTags
                .Include(p => p.Tag)
                .Where(p => p.KeywordId == keywordId)
                .ToList();

            return PartialView("_KeywordTagList", tagList);
        }

        public ActionResult GetContentList(ContentSearchCriteriaViewModel searchCriteria) {
            if(searchCriteria == null)
                throw new ArgumentNullException("searchCriteria");

            if(searchCriteria.PageDirection != null) {
                if(searchCriteria.PageDirection.Equals("Next")) {
                    searchCriteria.CurrentPage++;
                } else {
                    searchCriteria.CurrentPage--;
                }
            }

            //...
            // Pages dropdown list
            var totalContentCount = _context.Contents
                .Count(k => (k.KeywordId == searchCriteria.KeywordId || searchCriteria.KeywordId == null)
                            && (k.CreateUserId == searchCriteria.UserId || searchCriteria.UserId  == null)
                            && (k.ContentId == searchCriteria.ContentId || searchCriteria.ContentId == null)
                            && k.Approved);


            IDictionary<int, int> pages = new Dictionary<int, int>();
            for(int i = 1; i <= (totalContentCount / 10) + 1; i++) {
                pages.Add(new KeyValuePair<int, int>(i, i));
            }

            var skip = (searchCriteria.CurrentPage - 1) * 10;


            //...
            // Get content list page
            var contentsQuery = _context.Contents
                .Where(k => (k.KeywordId == searchCriteria.KeywordId || searchCriteria.KeywordId == null)
                            && (k.CreateUserId == searchCriteria.UserId || searchCriteria.UserId == null)
                            && (k.ContentId == searchCriteria.ContentId || searchCriteria.ContentId == null)
                            && k.Approved);

            // Sort by language
            contentsQuery = searchCriteria.LanguageId == 1
                ? contentsQuery
                    .OrderBy(k => k.LanguageId)
                    .ThenBy(k => k.ContentType)
                    .ThenByDescending(k => k.CreateDate)
                    .Skip(skip)
                    .Take(10)
                : contentsQuery
                    .OrderByDescending(k => k.LanguageId)
                    .ThenBy(k => k.ContentType)
                    .ThenByDescending(k => k.CreateDate)
                    .Skip(skip)
                    .Take(10);

            //...
            // Get current keyword
            var keyword = _context.Keywords.FirstOrDefault(p => p.KeywordId == searchCriteria.KeywordId);

            //...
            // Return view model
            var contentListViewModel = new ContentListViewModel {
                KeywordId = searchCriteria.KeywordId,
                UserId = searchCriteria.UserId,
                Term = keyword == null ? string.Empty : keyword.Definition,
                CurrentPage = searchCriteria.CurrentPage,
                FirstPage = 1,
                LastPage = pages.Count,
                Pages = new SelectList(pages, "Key", "Value", searchCriteria.CurrentPage),
                TotalCount = totalContentCount,
                Contents = contentsQuery.ToList(),
            };

            return PartialView("_ContentList", contentListViewModel);
        }

        public ActionResult SearchSuggestions(string term = null) {
            if(string.IsNullOrWhiteSpace(term)) {
                return null;
            }

            try {
                var suggestionList =
                    _context.Database.SqlQuery<string>(string.Format(@"SELECT * FROM dbo.GetKeywordSuggestionList('{0}')", term)).ToList();

                return Json(suggestionList, JsonRequestBehavior.AllowGet);
            } catch(Exception exception) {
                throw new Exception("Error in KeywordController.SearchSuggestions", exception);
            }
        }

        //[OutputCache(Duration = 3600, VaryByParam = "none")]
        public PartialViewResult GetTags() {
            try {
                var popularTags = _context.Tags;

                return PartialView("_Tags", popularTags);
            } catch(Exception exception) {
                throw new Exception("Error in KeywordController.GetPopularTags", exception);
            }
        }

        public PartialViewResult GetKeywordList(KeywordSearchCriteriaViewModel searchCriteria) {
            if(searchCriteria == null)
                throw new ArgumentNullException("searchCriteria");

            if(searchCriteria.PageDirection != null) {
                if(searchCriteria.PageDirection.Equals("Next")) {
                    searchCriteria.CurrentPage++;
                } else {
                    searchCriteria.CurrentPage--;
                }
            }

            ViewBag.CurrentPage = searchCriteria.CurrentPage;
            var skip = (searchCriteria.CurrentPage - 1) * 20;

            try {
                string sectionHeader;
                List<Keyword> keywords;
                int totalKeywordCount;
                switch(searchCriteria.OperationType) {
                    case "GetPopularKeywordList":
                        sectionHeader = "popüler";
                        keywords =
                            _context.Keywords.SqlQuery(
                                string.Format(@"SELECT * FROM dbo.GetPopularKeywords({0})", searchCriteria.CurrentPage - 1)).ToList();
                        totalKeywordCount = _context.Keywords.Count();
                        break;
                    case "GetRecentlyAddedKeywordList":
                        sectionHeader = "son eklenenler";
                        keywords =
                            _context.Keywords
                                    .Where(p => p.Approved)
                                    .OrderByDescending(p => p.CreateDate)
                                    .Skip(skip)
                                    .Take(20)
                                    .ToList();
                        totalKeywordCount = _context.Keywords.Count();
                        break;
                    case "GetKeywordListByTag":
                        var tag = _context.Tags.FirstOrDefault(p => p.TagId == searchCriteria.TagId);
                        sectionHeader = tag == null ? "Etiket bulunamadı!" : tag.Definition;
                        ViewBag.TagId = searchCriteria.TagId;
                        keywords =
                            _context.KeywordTags
                                .Where(p => p.TagId == searchCriteria.TagId)
                                .Include("Keyword")
                                .OrderByDescending(p => p.CreateDate)
                                .Skip(skip)
                                .Take(20)
                                .Select(p => p.Keyword)
                                .Where(p => p.Approved)
                                .ToList();
                        totalKeywordCount = _context.KeywordTags.Count(p => p.TagId == searchCriteria.TagId);
                        break;
                    default:
                        sectionHeader = "popüler";
                        keywords = _context.Keywords.SqlQuery(
                            string.Format(@"SELECT * FROM dbo.GetPopularKeywords({0})", searchCriteria.CurrentPage - 1)).ToList();
                        totalKeywordCount = _context.Keywords.Count();
                        break;
                }

                ViewBag.OperationType = searchCriteria.OperationType;
                ViewBag.SectionHeader = sectionHeader;

                //...
                // Pages dropdown list
                IDictionary<int, int> pages = new Dictionary<int, int>();
                for(int i = 1; i <= (totalKeywordCount / 20) + 1; i++) {
                    pages.Add(new KeyValuePair<int, int>(i, i));
                }
                ViewBag.FirstPage = 1;
                ViewBag.LastPage = pages.Count;
                ViewBag.Pages = new SelectList(pages, "Key", "Value", searchCriteria.CurrentPage);

                return PartialView("_KeywordList",
                                   keywords.Select(p =>
                                                   new KeywordViewModel {
                                                       KeywordId = p.KeywordId,
                                                       Definition = p.Definition
                                                   }));
            } catch(Exception exception) {
                throw new Exception("Error in KeywordController.GetPopularKeywords", exception);
            }
        }

        public PartialViewResult GetLikeDislikeList(int sourceId, int sourceTypeId) {
            try {
                //...
                // Check whether content is already voted
                var loggedInUserId = WebSecurity.CurrentUserId;
                var alreadyVoted = _context.Likes
                                           .Any(
                                               p =>
                                               p.LikeUserId == loggedInUserId && p.SourceId == sourceId &&
                                               p.SourceTypeId == sourceTypeId);

                //...
                // Get LikeViewModel
                LikeViewModel likeViewModel = _context.Contents
                                                      .Where(p => p.ContentId == sourceId)
                                                      .Select(p => new LikeViewModel {
                                                          KeywordId = p.KeywordId,
                                                          SourceId = p.ContentId,
                                                          SourceTypeId = sourceTypeId,
                                                          LikeCount = p.LikeCount,
                                                          DislikeCount = p.DislikeCount,
                                                          AlreadyVoted = alreadyVoted
                                                      }).FirstOrDefault()
                                              ?? new LikeViewModel {
                                                  KeywordId =
                                                      _context.Contents.Where(p => p.ContentId == sourceId).Select(
                                                          p => p.KeywordId).
                                                               FirstOrDefault(),
                                                  SourceId = sourceId,
                                                  SourceTypeId = sourceTypeId,
                                                  LikeCount = 0,
                                                  DislikeCount = 0,
                                                  AlreadyVoted = false
                                              };

                return PartialView("_Like", likeViewModel);
            } catch(Exception exception) {
                throw new Exception("Error in KeywordController.GetLikeDislikeList [Get]", exception);
            }
        }

        [AjaxAuthorize]
        public ActionResult LikeDislike(int keywordId = 0, int sourceId = 0, int sourceTypeId = 0, int status = 0) {
            try {
                var likeUserId = WebSecurity.CurrentUserId;
                if(keywordId > 0 && sourceId > 0) {
                    //...
                    // Check whether content is already voted by current user
                    var alreadyVoted =
                        _context.Likes.Any(
                            p => p.LikeUserId == likeUserId && p.SourceId == sourceId && p.SourceTypeId == sourceTypeId);
                    if(alreadyVoted) {
                        return JavaScript("bootbox.alert('Bu içerik için daha önce oy kullandınız!')");
                    }

                    var likeModel = new Like {
                        KeywordId = keywordId,
                        LikeUserId = likeUserId,
                        SourceId = sourceId,
                        SourceTypeId = sourceTypeId,
                        Status = status == 1,
                        CreateDate = DateTime.Now
                    };

                    _context.Likes.Add(likeModel);
                    _context.SaveChanges();
                }

                return RedirectToAction("GetLikeDislikeList",
                                        new { sourceId = sourceId, sourceTypeId = sourceTypeId });
            } catch(Exception exception) {
                throw new Exception("Error in KeywordController.LikeDislike [Post]", exception);
            }
        }

        [AjaxAuthorize]
        public ActionResult Complain(int contentId = 0) {
            try {
                //...
                // Shared content
                var contentModel =
                    _context.Contents.FirstOrDefault(p => p.ContentId == contentId);
                if(contentModel != null) {
                    contentModel.Complaint++;
                    _context.SaveChanges();
                }

                return JavaScript("bootbox.alert('Şikayetiniz alınmıştır!')");
            } catch(Exception exception) {
                throw new Exception("Error in KeywordController.Complain", exception);
            }
        }

        public ActionResult UserPortfolio(string userName = "") {
            try {
                //...
                // Check whether user name is not empty
                if(string.IsNullOrWhiteSpace(userName)) {
                    return View();
                }

                //...
                // Get User Id
                var userId = WebSecurity.GetUserId(userName);
                if(userId <= 0) {
                    return View();
                }

                //... 
                // Get keyword list created by this user
                var keywords = _context.Keywords
                                       .Where(p => p.CreateUserId == userId)
                                       .Take(5)
                                       .OrderByDescending(p => p.CreateDate);

                //... 
                // Get keyword mean list created by this user
                var contents = _context.Contents
                                       .Where(p => p.CreateUserId == userId)
                                       .OrderBy(k => k.ContentType)
                                       .ThenByDescending(k => k.CreateDate)
                                       .Take(10);

                //...
                // Create view model
                var viewModel = new UserPortfolioViewModel {
                    UserId = userId,
                    UserName = userName,
                    Keywords = keywords.ToList(),
                    Contents = contents.ToList()
                };

                return View(viewModel);
            } catch(Exception exception) {
                throw new Exception("Error in KeywordController.UserPortfolio [Get]", exception);
            }
        }
    }
}
