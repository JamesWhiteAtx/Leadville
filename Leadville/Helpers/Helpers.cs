using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.WebPages;
using CST.Prdn.ViewModels;
using CST.Localization;
using CST.Prdn.Data;
using System.Text;
using System.Web.Routing;
using System.Reflection;

namespace CST.Prdn.Helpers.Extensions
{
    public static class HtmlHelpers
    {
        private const string Nbsp = "&nbsp;";
        private const string dashStr = "---";
        private const string spanIdPrefix = "span";

        public static MvcHtmlString DashStr(this HtmlHelper helper)
        {
            return new MvcHtmlString(dashStr);
        }

        public static MvcHtmlString Concat(this MvcHtmlString first, params MvcHtmlString[] strings)
        {
            var sb = new StringBuilder();
            sb.Append(first.ToHtmlString());
            foreach (var item in strings.Where(i => i != null))
                sb.Append(item.ToHtmlString());
            return MvcHtmlString.Create(sb.ToString());
        }

        public static string GetFullHtmlFieldName<TModel, TProperty>(this HtmlHelper<TModel> helper, Expression<Func<TModel, TProperty>> expression)
        {
            string text = ExpressionHelper.GetExpressionText(expression);
            return helper.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(text);
        }

        public static MvcHtmlString IdFor<TModel, TValue>(this HtmlHelper<TModel> helper, Expression<Func<TModel, TValue>> expression)
        {
            //var name = helper.GetFullHtmlFieldName(expression); // did not handle sub properties '.' '_'
            var name = helper.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldId(
                  ExpressionHelper.GetExpressionText(expression));

            return new MvcHtmlString(name);
        }

        public static MvcHtmlString GetDisplayName<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, TProperty>> expression)
        {
            var metaData = ModelMetadata.FromLambdaExpression<TModel, TProperty>(expression, htmlHelper.ViewData);
            string value = metaData.DisplayName ?? (metaData.PropertyName ?? ExpressionHelper.GetExpressionText(expression));
            return MvcHtmlString.Create(value);
        }

        public static MvcHtmlString PrefixId(this HtmlHelper helper, string prefix, string id)
        {
            return new MvcHtmlString(prefix + id);
        }

        public static MvcHtmlString PrefixIdFor<TModel, TValue>(this HtmlHelper<TModel> helper, Expression<Func<TModel, TValue>> expression, string prefix)
        {
            MvcHtmlString id = helper.IdFor(expression);
            return helper.PrefixId(prefix, id.ToString());
        }

        public static MvcHtmlString SpanPrefixId(this HtmlHelper helper, string id)
        {
            return helper.PrefixId(spanIdPrefix, id);
            //return new MvcHtmlString(spanIdPrefix + id);
        }

        public static MvcHtmlString Span(this HtmlHelper helper, string name, object value = null, string defaultValue = null, object htmlAttributes = null)
        {
            var span = new TagBuilder("span");
            string id = null;
            if (htmlAttributes != null)
            {
                RouteValueDictionary dict = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes);
                if (dict.ContainsKey("id"))
                {
                    id = dict["id"].ToString();
                    dict.Remove("id");
                }
                span.MergeAttributes(dict);
            }
            if (id == null)
            {
                id = spanIdPrefix + name;
            }
            span.GenerateId(id);

            string innerText = null;
            if (value != null)
            {
                innerText = value.ToString();
            }
            if ((!String.IsNullOrEmpty(defaultValue)) && (String.IsNullOrEmpty(innerText)))
            {
                innerText = MvcHtmlString.Create(defaultValue).ToString();
            }
            span.SetInnerText(innerText);

            return MvcHtmlString.Create(span.ToString(TagRenderMode.Normal));
        }

        public static MvcHtmlString SpanOrDash(this HtmlHelper helper, string name, object value = null, object htmlAttributes = null)
        {
            return Span(helper, name: name, defaultValue: dashStr, htmlAttributes: htmlAttributes);
        }

        public static MvcHtmlString SpanAndHidden(this HtmlHelper helper, string name, object value = null, object htmlAttributes = null)
        {
            MvcHtmlString display;
            display = helper.Span(name, value, "\xA0", htmlAttributes);
            display = display.Concat(helper.Hidden(name, value, htmlAttributes));
            return display;
        }

        public static MvcHtmlString SpanOrDashAndHidden(this HtmlHelper helper, string name, object value = null, object htmlAttributes = null)
        {
            MvcHtmlString display;
            display = helper.SpanOrDash(name, value, htmlAttributes);
            display = display.Concat(helper.Hidden(name, value, htmlAttributes));
            return display;
        }

        public static MvcHtmlString SpanId(this HtmlHelper helper, string name)
        {
            return helper.SpanPrefixId(name);
        }

        public static MvcHtmlString IdSpanId(this HtmlHelper helper, string name)
        {
            MvcHtmlString id = new MvcHtmlString(name);
            MvcHtmlString spanId = helper.SpanId(name);

            MvcHtmlString result = new MvcHtmlString("'");
            return result.Concat(id, new MvcHtmlString("', '"), spanId, new MvcHtmlString("'"));
        }

        public static MvcHtmlString SpanIdFor<TModel, TValue>(this HtmlHelper<TModel> helper, Expression<Func<TModel, TValue>> expression)
        {
            MvcHtmlString id = helper.IdFor(expression);
            return helper.SpanPrefixId(id.ToString());
        }

        public static MvcHtmlString IdSpanIdFor<TModel, TValue>(this HtmlHelper<TModel> helper, Expression<Func<TModel, TValue>> expression)
        {
            MvcHtmlString id = helper.IdFor(expression);
            MvcHtmlString spanId = helper.SpanIdFor(expression);

            MvcHtmlString result = new MvcHtmlString("'");
            return result.Concat(id, new MvcHtmlString("', '"), spanId, new MvcHtmlString("'"));
        }

        public static MvcHtmlString SpanFor<TModel, TProperty>(this HtmlHelper<TModel> helper, Expression<Func<TModel, TProperty>> expression, 
            string defaultValue=null, object htmlAttributes=null)
        {
            MvcHtmlString value = helper.DisplayFor(expression);

            var span = new TagBuilder("span");
            string id = null;
            if (htmlAttributes != null) {
                RouteValueDictionary dict = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes);
                if (dict.ContainsKey("id"))
                {
                    id = dict["id"].ToString();
                    dict.Remove("id");
                }
                span.MergeAttributes(dict);
            }
            if (id == null)
            {
                id = spanIdPrefix + helper.GetFullHtmlFieldName(expression);    
            }
            span.GenerateId(id);

            string innerText = null;
            if (value != null) {
                innerText = value.ToString();
            }
            if ((!String.IsNullOrEmpty(defaultValue)) && (String.IsNullOrEmpty(innerText)))
            {
                innerText = MvcHtmlString.Create(defaultValue).ToString();
            }
            span.SetInnerText(innerText);

            return MvcHtmlString.Create(span.ToString(TagRenderMode.Normal));
        }

        public static MvcHtmlString SpanOrDashFor<TModel, TProperty>(this HtmlHelper<TModel> helper, Expression<Func<TModel, TProperty>> expression,
            object htmlAttributes = null)
        {
            return SpanFor<TModel, TProperty>(helper, expression, defaultValue: dashStr, htmlAttributes: htmlAttributes);
        }

        public static MvcHtmlString DisplayOrDashFor<TModel, TValue>(this HtmlHelper<TModel> helper, Expression<Func<TModel, TValue>> expression)
        {
            MvcHtmlString display;
            display = helper.DisplayFor(expression);
            if (MvcHtmlString.IsNullOrEmpty(display))
            {
                display = MvcHtmlString.Create(dashStr);
            }
            return display;
        }

        public static MvcHtmlString DisplayOrNbspFor<TModel, TValue>(this HtmlHelper<TModel> helper, Expression<Func<TModel, TValue>> expression)
        {
            MvcHtmlString display;
            display = helper.DisplayFor(expression);
            if (MvcHtmlString.IsNullOrEmpty(display))
            {
                display = MvcHtmlString.Create(Nbsp);
            }
            return display;
        }

        public static MvcHtmlString DispAndHiddenFor<TModel, TValue>(this HtmlHelper<TModel> helper, Expression<Func<TModel, TValue>> expression)
        {
            MvcHtmlString display;
            display = helper.DisplayOrNbspFor(expression);
            display = display.Concat(helper.HiddenFor(expression));
            return display;
        }

        public static MvcHtmlString SpanAndHiddenFor<TModel, TValue>(this HtmlHelper<TModel> helper, Expression<Func<TModel, TValue>> expression, object htmlAttributes=null)
        {
            MvcHtmlString display;
            display = helper.SpanFor(expression, "\xA0", htmlAttributes);
            display = display.Concat(helper.HiddenFor(expression, htmlAttributes));
            return display;
        }

        public static MvcHtmlString SpanOrDashAndHiddenFor<TModel, TValue>(this HtmlHelper<TModel> helper, Expression<Func<TModel, TValue>> expression, object htmlAttributes = null)
        {
            MvcHtmlString display;
            display = helper.SpanOrDashFor(expression, htmlAttributes);
            display = display.Concat(helper.HiddenFor(expression, htmlAttributes));
            return display;
        }

        public static string DisplayNameFor<TModel, TValue>(this HtmlHelper<TModel> helper, Expression<Func<TModel, TValue>> expression)
        {
            var name = helper.GetFullHtmlFieldName(expression);
            var metadata = ModelMetadataProviders.Current.GetMetadataForProperty(() => Activator.CreateInstance<TModel>(), typeof(TModel), name);
            return metadata.DisplayName;
        }

        public static MvcHtmlString ProdDayTd(this HtmlHelper helper, PrdnCalendarDay day, DateTime today, ProductionCalendar calendar)
        {
            StringBuilder sb = new StringBuilder();

            TagBuilder divDay = new TagBuilder("div");
            divDay.AddCssClass("prod-cal-day-num");
            divDay.InnerHtml = day.CalDay.Day.ToString();
            sb.Append(divDay.ToString(TagRenderMode.Normal));

            bool shipDay = day.ShipDay;

            if (day.ExistsInDB) 
            { 
                TagBuilder divWD = new TagBuilder("div");
                divWD.AddCssClass("prod-cal-ship-div");
                string dayId = day.FormatDateID;
                Dictionary<string, object> attrs = new Dictionary<string, object>() { { "class", "prod-cal-ship-day" } };

                if ((!calendar.AllowEditing) || (!day.Editable))
                {
                    attrs.Add("disabled", "disabled");
                }

                divWD.InnerHtml = helper.CheckBox(dayId, shipDay, attrs).ToString() + helper.Label(dayId, LocalStr.ShippingDay);

                sb.AppendLine(divWD.ToString(TagRenderMode.Normal));

                if (shipDay)
                {
                    TagBuilder divPo = new TagBuilder("div");
                    divPo.AddCssClass("prod-cal-po-num");
                    string orderText = LocalStr.OrderNo + " " + day.ShipPrdnOrdNo;
                    if (calendar.OrderAction != null)
                    {
                        divPo.InnerHtml =
                            helper.ActionLink(
                                linkText: orderText,
                                actionName: calendar.OrderAction,
                                controllerName: calendar.OrderController,
                                routeValues: new { id = day.ShipPrdnOrdNo },
                                htmlAttributes: null
                             ).ToString();
                    }
                    else
                    {
                        divPo.InnerHtml = helper.Label("prod-num", orderText).ToString();
                    }
                    sb.Append(divPo.ToString(TagRenderMode.Normal));

                    TagBuilder divRuns = new TagBuilder("div");
                    divRuns.AddCssClass("prod-cal-po-runs");
                    string runText = LocalStr.RunCount +" " + day.TotalRuns.ToString();
                    if (calendar.RunAction != null)
                    {
                        divRuns.InnerHtml = 
                            helper.ActionLink(
                                linkText: runText,
                                actionName: calendar.RunAction, 
                                controllerName: calendar.RunController,
                                routeValues: new { id = day.ShipPrdnOrdNo },
                                htmlAttributes: null
                             ).ToString();
                    }
                    else {
                        divRuns.InnerHtml = helper.Label("prod-runs", runText).ToString();
                    }
                    sb.Append(divRuns.ToString(TagRenderMode.Normal));

                }
            }

            TagBuilder td = new TagBuilder("td");
            td.InnerHtml = sb.ToString();

            if (day.CalMonth == PrdnCalDayMonth.LastMonth)
            {
                td.AddCssClass("prod-cal-last-month");
                td.AddCssClass("t-alt");
            }

            else if (day.CalMonth == PrdnCalDayMonth.ThisMonth)
            {
                td.AddCssClass("prod-cal-this-month");
            }

            else if (day.CalMonth == PrdnCalDayMonth.NextMonth)
            {
                td.AddCssClass("prod-cal-next-month");
                td.AddCssClass("t-alt");
            }

            if (day.CalDay.Date.Equals(today.Date))
            {
                td.GenerateId("prod-cal-today");
            }

            td.InnerHtml = sb.ToString();
            return MvcHtmlString.Create(td.ToString());
        }

        public static MvcHtmlString ImageLink(this HtmlHelper helper, string imgSrc, string additionalText = null, 
            string actionName = null, string controllerName = null, 
            object routeValues = null, object linkHtmlAttributes = null, object imgHtmlAttributes = null)
        {
            var urlHelper = ((Controller)helper.ViewContext.Controller).Url;
            var url = "#";
            if (!string.IsNullOrEmpty(actionName))
                url = urlHelper.Action(actionName, controllerName, routeValues);

            var imglink = new TagBuilder("a");
            imglink.MergeAttribute("href", url);
            imglink.InnerHtml = helper.Image(imgSrc, imgHtmlAttributes) + " " + additionalText;
            linkHtmlAttributes = HtmlHelper.AnonymousObjectToHtmlAttributes(linkHtmlAttributes);
            imglink.MergeAttributes((IDictionary<string, object>)linkHtmlAttributes, true);

            return MvcHtmlString.Create(imglink.ToString());
        }

        public static void TagLinkVal(this HtmlHelper helper, TagBuilder tag,
            string actionName = null, string controllerName = null,
            object routeTokens = null, object routeValues = null)
        {
            tag.AddCssClass("cst-linkval");

            RouteValueDictionary tokenDict = null;
            if (routeTokens != null)
            {
                tokenDict = new RouteValueDictionary(routeTokens);
                int i = 0;
                string selector;
                foreach (var key in tokenDict.Keys.ToList())
                {
                    selector = "#" + tokenDict[key].ToString();
                    // check for id because "id" will be incorporated into the route path, not the search string
                    if (String.Compare(key, "id", true) == 0)
                    {
                        tag.MergeAttribute("data-linkval-token-id", selector);
                        tokenDict[key] = "linkval-token-id";
                    }
                    else
                    {
                        tag.MergeAttribute("data-linkval-token-" + i.ToString(), selector);
                        tokenDict[key] = "linkval-token-" + i.ToString();
                        i++;
                    }
                }
            }

            RouteValueDictionary valueDict = null;
            if ((routeValues != null) || (tokenDict != null))
            {
                valueDict = new RouteValueDictionary(routeValues);
                if (tokenDict != null)
                {
                    tokenDict.ToList().ForEach(x => valueDict[x.Key] = x.Value);
                }
            }

            UrlHelper urlHelper = new UrlHelper(helper.ViewContext.RequestContext);
            string url = helper.Raw(urlHelper.Action(actionName, controllerName, valueDict)).ToString();

            tag.MergeAttribute("data-linkval-url", url);

        }

        public static TagBuilder ImgBtnTag(this HtmlHelper helper, string imgSrc, object imgHtmlAttributes = null)
        {
            TagBuilder imgBtn = new TagBuilder("input");
            imgBtn.MergeAttribute("type", "image");
            imgBtn.MergeAttribute("src", imgSrc);
            if (imgHtmlAttributes != null)
            {
                imgHtmlAttributes = HtmlHelper.AnonymousObjectToHtmlAttributes(imgHtmlAttributes);
                imgBtn.MergeAttributes((IDictionary<string, object>)imgHtmlAttributes, true);
            }
            return imgBtn;
        }

        public static MvcHtmlString ImageButton(this HtmlHelper helper, string imgSrc, object imgHtmlAttributes = null)
        {
            TagBuilder imgBtn = helper.ImgBtnTag(imgSrc, imgHtmlAttributes);
            return MvcHtmlString.Create(imgBtn.ToString());
        }

        public static MvcHtmlString ImageButtonLinkVal(this HtmlHelper helper, string imgSrc, 
            string actionName = null, string controllerName = null,
            object routeTokens = null, object routeValues = null,
            object imgHtmlAttributes = null)
        {
            TagBuilder imgBtn = helper.ImgBtnTag(imgSrc, imgHtmlAttributes);

            helper.TagLinkVal(imgBtn, actionName: actionName, controllerName: controllerName, routeTokens: routeTokens, routeValues: routeValues);

            return MvcHtmlString.Create(imgBtn.ToString());
        }
        

        public static MvcHtmlString Image(this HtmlHelper helper, string imgSrc, object imgHtmlAttributes = null)
        {
            var imgTag = new TagBuilder("img");
            imgTag.MergeAttribute("src", imgSrc);
            if (imgHtmlAttributes != null)
            {
                imgHtmlAttributes = HtmlHelper.AnonymousObjectToHtmlAttributes(imgHtmlAttributes);
                imgTag.MergeAttributes((IDictionary<string, object>)imgHtmlAttributes, true);
            }
            return MvcHtmlString.Create(imgTag.ToString());
        }

        public static MvcHtmlString ReturnLink(this HtmlHelper helper, string url, string linkText = null)
        {
            if (helper.ViewContext.HttpContext.Request.IsUrlLocalToHost(url))
            {
                var builder = new TagBuilder("a");
                builder.MergeAttribute("href", url);
                builder.SetInnerText(linkText ?? LocalStr.Return);
                return MvcHtmlString.Create(builder.ToString(TagRenderMode.Normal));                
            } 
            else 
            {
                return MvcHtmlString.Create(String.Empty);
            }
        }

        public static string TIconSpan(string tIconClass = null)
        {
            if (tIconClass == null)
            {
                return String.Empty;
            }
            var span = new TagBuilder("span");
            span.AddCssClass("t-icon");
            span.AddCssClass(tIconClass);
            return span.ToString();
        }

        public static MvcHtmlString TIconTag(TagBuilder tag, string tIconClass = null, string id = null, string caption = null, object htmlAttributes = null)
        {
            if (htmlAttributes != null)
            {
                RouteValueDictionary dict = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes);
                if ((dict.ContainsKey("id")) && (id == null))
                {
                    id = dict["id"].ToString();
                    dict.Remove("id");
                }
                if (dict.ContainsKey("class"))
                {
                    string cssClass = dict["class"].ToString();
                    dict.Remove("class");
                    tag.AddCssClass(cssClass);
                }
                tag.MergeAttributes(dict);
            }

            tag.AddCssClass("t-button");

            if (id != null)
            {
                tag.GenerateId(id);
            }

            if ((caption != null) && (tIconClass != null))
            {
                tag.AddCssClass("t-button-icontext");
                tag.InnerHtml = TIconSpan(tIconClass) + caption;
            }
            else if (tIconClass != null)
            {
                tag.AddCssClass("t-button-icon");
                tag.InnerHtml = TIconSpan(tIconClass);
            }
            else
            {
                tag.AddCssClass("t-button");
                tag.InnerHtml = caption;
            }

            return MvcHtmlString.Create(tag.ToString());
        }

        public static MvcHtmlString TIconButton(this HtmlHelper helper, string tIconClass = null, string id = null, string caption = null, object htmlAttributes = null)
        {
            var button = new TagBuilder("button");
            return TIconTag(tag: button, tIconClass: tIconClass, id: id, caption: caption, htmlAttributes: htmlAttributes);
        }

        public static MvcHtmlString TIconSubmit(this HtmlHelper helper, string tIconClass = null, string id = null, string caption = null, object htmlAttributes = null)
        {
            var button = new TagBuilder("button");
            button.Attributes.Add("type", "submit");
            return TIconTag(tag: button, tIconClass: tIconClass, id: id, caption: caption, htmlAttributes: htmlAttributes);
        }

        public static MvcHtmlString TIconSave(this HtmlHelper helper, string id = null, string caption = null, object htmlAttributes = null)
        {
            var button = new TagBuilder("button");
            button.Attributes.Add("type", "submit");
            return TIconTag(tag: button, tIconClass: "t-update", id: id, caption: caption ?? LocalStr.Save, htmlAttributes: htmlAttributes);
        }

        public static MvcHtmlString TIconActionLink(this HtmlHelper helper, string linkText = null, string actionName = null, string controllerName = null, object routeValues = null,
            string tIconClass = null, string id = null, object htmlAttributes = null)
        {
            TagBuilder anchor = new TagBuilder("a");

            UrlHelper urlHelper = new UrlHelper(helper.ViewContext.RequestContext);
            string url = urlHelper.Action(actionName, controllerName, routeValues);

            anchor.MergeAttribute("href", url);             

            return TIconTag(tag: anchor, tIconClass: tIconClass, id: id, caption: linkText, htmlAttributes: htmlAttributes);
        }

        public static MvcHtmlString TIconLink(this HtmlHelper helper, string linkText, string url,
            string tIconClass = null, string id = null, object htmlAttributes = null)
        {
            if (helper.ViewContext.HttpContext.Request.IsUrlLocalToHost(url))
            {
                var anchor = new TagBuilder("a");
                anchor.MergeAttribute("href", url);

                return TIconTag(tag: anchor, tIconClass: tIconClass, id: id, caption: linkText, htmlAttributes: htmlAttributes);
            }
            else
            {
                return MvcHtmlString.Create(String.Empty);
            }
        }

        public static MvcHtmlString TIconReturnLink(this HtmlHelper helper, string url, string linkText=null,
            string id = null, object htmlAttributes = null)
        { 
            return helper.TIconLink(linkText: linkText ?? LocalStr.Return, url: url, tIconClass: "t-go-back", id: id, htmlAttributes: htmlAttributes);
        }

        public static MvcHtmlString TIconCancelLink(this HtmlHelper helper, string url, string linkText=null,
            string id = null, object htmlAttributes = null)
        {
            return helper.TIconLink(linkText: linkText ?? LocalStr.Cancel, url: url, tIconClass: "t-cancel", id: id, htmlAttributes: htmlAttributes);
        }

        public static MvcHtmlString TIconLinkVal(this HtmlHelper helper, string caption = null, string actionName = null, string controllerName = null,
            object routeTokens = null, object routeValues = null,
            string tIconClass = null, string id = null, object htmlAttributes = null)
        {
            TagBuilder button = new TagBuilder("button");
            button.AddCssClass("cst-linkval");

            RouteValueDictionary tokenDict = null;
            if (routeTokens != null)
            {
                tokenDict = new RouteValueDictionary(routeTokens);
                int i = 0;
                string selector;
                foreach (var key in tokenDict.Keys.ToList())
                {
                    selector = "#" + tokenDict[key].ToString();
                    // check for id because "id" will be incorporated into the route path, not the search string
                    if (String.Compare(key, "id", true) == 0)
                    {
                        button.MergeAttribute("data-linkval-token-id", selector); 
                        tokenDict[key] = "linkval-token-id";
                    }
                    else
                    {
                        button.MergeAttribute("data-linkval-token-" + i.ToString(), selector);
                        tokenDict[key] = "linkval-token-" + i.ToString();
                        i++;
                    }
                }
            }

            RouteValueDictionary valueDict = null;
            if ((routeValues != null) || (tokenDict != null))
            {
                valueDict = new RouteValueDictionary(routeValues);
                if (tokenDict != null)
                {
                    tokenDict.ToList().ForEach(x => valueDict[x.Key] = x.Value);
                }
            }
           

            UrlHelper urlHelper = new UrlHelper(helper.ViewContext.RequestContext);
            string url = helper.Raw(urlHelper.Action(actionName, controllerName, valueDict)).ToString();

            button.MergeAttribute("data-linkval-url", url);

            return TIconTag(tag: button, tIconClass: tIconClass, id: id, caption: caption, htmlAttributes: htmlAttributes);
        }

        public static MvcHtmlString ClearButton(this HtmlHelper helper, string selector, string name = null, string caption = null, object htmlAttributes = null)
        {
            TagBuilder button = new TagBuilder("button");
            button.AddCssClass("cst-clearbtn");

            button.MergeAttribute("data-clearbtn-sel", selector);
            button.MergeAttribute("tabindex", "-1");
            button.MergeAttribute("type", "button");

            string title = LocalStr.Clear + (name != null ? " " + name : String.Empty);
            button.MergeAttribute("title", title);

            return TIconTag(tag: button, tIconClass: "t-delete", caption: caption, htmlAttributes: htmlAttributes);
        }

        public static MvcHtmlString ClearButtonFor<TModel, TValue>(this HtmlHelper<TModel> helper, Expression<Func<TModel, TValue>> expression, 
            string caption = null, object htmlAttributes = null)
        { 
            string selector = "#"+helper.IdFor(expression);
            string name = helper.GetDisplayName(expression).ToString();
            return ClearButton(helper, selector, name, caption: caption, htmlAttributes: htmlAttributes);
        }

        const string upDnClass = "updn";
        const string upDnTextClass = "updn-text";
        const string upDnUpClass = "updn-up";
        const string upDnDnClass = "updn-dn";

        public static TagBuilder UpDnLink(string upOrDnClass, object htmlAttributes = null)
        {
            var link = new TagBuilder("a");

            RouteValueDictionary dictAttributes = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes);

            if ((dictAttributes != null) && (dictAttributes.Count > 0))
            {
                if ((dictAttributes.ContainsKey("id")))
                {
                    string id = dictAttributes["id"].ToString();
                    dictAttributes.Remove("id");
                    link.GenerateId(id);
                }
                if (dictAttributes.ContainsKey("class"))
                {
                    string cssClass = dictAttributes["class"].ToString();
                    dictAttributes.Remove("class");
                    link.AddCssClass(cssClass);
                }
                link.MergeAttributes(dictAttributes);
            }

            link.AddCssClass(upDnClass);
            link.AddCssClass(upOrDnClass);

            var span = new TagBuilder("span");
            span.AddCssClass(upDnClass);
            span.AddCssClass(upOrDnClass);
            link.InnerHtml = span.ToString();

            return link;
        }

        public static MvcHtmlString UpDnInnerHtml(this HtmlHelper helper, string text, object htmlAttributes = null)
        {
            TagBuilder spanText = new TagBuilder("span");
            spanText.AddCssClass(upDnTextClass);
            spanText.SetInnerText(text);

            TagBuilder upLink = UpDnLink(upDnUpClass, htmlAttributes);
            TagBuilder dnLink = UpDnLink(upDnDnClass, htmlAttributes);

            return MvcHtmlString.Create(spanText.ToString() + upLink.ToString() + dnLink.ToString());
        }

        public static MvcHtmlString TodayDtTmStr(this HtmlHelper helper, DateTime? date)
        {
            DateTime dt = (DateTime)date;
            return MvcHtmlString.Create( dt.TodayDtTmStr()  );
        }

        //Expression<Func<TModel, TProperty>> expression, object value, IDictionary<string, object> htmlAttributes

        public static MvcHtmlString JobStatusElementFor<TModel, TValue>(this HtmlHelper<TModel> helper, Expression<Func<TModel, TValue>> expression, 
            PrdnJobStatus curStatus, PrdnJobStatus status, bool canStatus)
        {
            if (curStatus == status)
            {
                if (canStatus)
                {
                    return helper.StatusControlFor(expression, status);
                }
                else
                {
                    return helper.CheckDisplay(status);
                }
            }
            else
            {
                if (canStatus)
                {
                    return helper.StatusControlFor(expression, status);
                }
            }
            return MvcHtmlString.Create(String.Empty);
        }

        public static MvcHtmlString CheckDisplay(this HtmlHelper helper, PrdnJobStatus status)
        {
            MvcHtmlString display;
            display = helper.Span(name: status.ToString(), defaultValue: dashStr, htmlAttributes: new { @class = "t-update t-icon" });
            display = display.Concat(helper.Label(status.Description()));
            return display;
        }

        private static MvcHtmlString StatusControlFor<TModel, TValue>(this HtmlHelper<TModel> helper, Expression<Func<TModel, TValue>> expression, PrdnJobStatus status)
        {
            MvcHtmlString display;
            display = helper.RadioButtonFor(expression, status.ToString(), new { id = status.ToString() });
            display = display.Concat(helper.Label(status.ToString(), status.Description()));
            return display;
        }

        /// <summary>
        /// Return the Current Version from the AssemblyInfo.cs file.
        /// </summary>
        public static string CurrentVersion(this HtmlHelper helper)
        {
            try
            {
                var version = Assembly.GetExecutingAssembly().GetName().Version;
                return version.ToString();
            }
            catch
            {
                return "?.?.?.?";
            }
        }

    }

    public static class UrlHelpers {

        public static string ProdImageSetSrcPath(this UrlHelper helper, string imageSetCD)
        {
            if (imageSetCD == "FRONT")
            {
                return helper.Content("~/Content/Images/smallCar.gif");
            }
            else if (imageSetCD == "FRONT_OUT")
            {
                return helper.Content("~/Content/Images/smallSeat.gif");
            }
            else if (imageSetCD == "FLIER")
            {
                return helper.Content("~/Content/Images/flier.gif");
            }
            else if (imageSetCD == "TECH BULLETIN")
            {
                return helper.Content("~/Content/Images/techBulletin.gif");
            }
            else
            {
                return helper.Content("~/Content/Images/pencil.gif");
            }
        }

    }

}