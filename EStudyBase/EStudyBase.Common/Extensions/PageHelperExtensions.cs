using System;
using System.Text;
using System.Web.Mvc;

namespace EStudyBase.Common.Extensions
{
    public static class PageHelperExtensions
    {
        public static MvcHtmlString PageLinks(this HtmlHelper html, int totalItems, int itemsPerPage, int currentPage,
                                              string cssActive, int scrollPages, Func<int, string> pageUrl)
        {
            if (totalItems == 0)
                return MvcHtmlString.Create("");
            if (currentPage < 1)
                currentPage = 1;
            var totalPages = (int)Math.Ceiling((decimal)totalItems / itemsPerPage);
            var startIndex = 1;
            var endIndex = totalPages;
            if (scrollPages > 0)
            {
                bool isOdd = scrollPages % 2 != 0;
                int countRef = (int)Math.Ceiling((decimal)(((isOdd) ? scrollPages - 1 : scrollPages) / 2));
                if (isOdd)
                    countRef++;
                if (currentPage > countRef)
                {
                    startIndex = currentPage - countRef + 1;
                    if (isOdd)
                        countRef--;
                    endIndex = currentPage + countRef;
                }
                else
                {
                    startIndex = 1;
                    endIndex = scrollPages;
                }
                if (endIndex > totalPages)
                {
                    endIndex = totalPages;
                    startIndex = totalPages - scrollPages + 1;
                }
                if (startIndex < 0)
                {
                    currentPage = 1;
                    startIndex = 1;
                }
            }
            var result = new StringBuilder();
            result.AppendLine("<ul>");
            for (var i = startIndex; i <= endIndex; i++)
            {
                var liTag = new TagBuilder("li");
                var tag = new TagBuilder("a");
                tag.MergeAttribute("href", pageUrl(i));
                tag.InnerHtml = i.ToString();
                if (i == currentPage && !String.IsNullOrEmpty(cssActive))
                    liTag.AddCssClass(cssActive);
                liTag.InnerHtml = tag.ToString();
                result.AppendLine(liTag.ToString());
            }
            result.AppendLine("</ul>");
            return MvcHtmlString.Create(result.ToString());
        }
    }
}