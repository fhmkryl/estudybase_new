using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.Mvc;

namespace EStudyBase.Common.Extensions
{
    public static class CheckBoxListExtensions
    {
        public static MvcHtmlString CheckBoxListFor<T>(this HtmlHelper helper, String name, IEnumerable<T> items,
                                                       String textField, String valueField, Int32[] selectedItems = null)
        {
            if (items != null)
            {
                Type itemstype = typeof (T);
                PropertyInfo textfieldInfo = itemstype.GetProperty(textField, typeof (String));
                PropertyInfo valuefieldInfo = itemstype.GetProperty(valueField);

                TagBuilder tag;
                StringBuilder checklist = new StringBuilder();

                foreach (var item in items)
                {
                    var tempitem = item;
                    tag = new TagBuilder("input");
                    tag.Attributes["type"] = "checkbox";
                    tag.Attributes["value"] = valuefieldInfo.GetValue(tempitem, null).ToString();
                    tag.Attributes["name"] = name;
                    if (selectedItems != null && selectedItems.Contains((Int32) valuefieldInfo.GetValue(tempitem, null)))
                    {
                        tag.Attributes["checked"] = "checked";
                    }
                    tag.InnerHtml = textfieldInfo.GetValue(tempitem, null).ToString();
                    checklist.Append(tag);
                    checklist.Append("<br />");
                }
                return MvcHtmlString.Create(checklist.ToString());
            }
            return MvcHtmlString.Empty;
        }
    }
}