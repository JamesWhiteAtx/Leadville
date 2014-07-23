using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;
using System.Data;
using System.Text.RegularExpressions;
using System.Web.Security;
using System.Threading;
using CST.Prdn.Data;
using CST.Prdn.ISIS;
using CST.ISIS.Data;
using CST.Prdn.ViewModels;
using CST.Security;
using CST.Localization;
using CST.ActionFilters;
using AutoMapper;
using Telerik.Web.Mvc;
using System.IO;
using CST.ZebraUtils;

namespace System.Web.Mvc
{
    public enum ActionUpdateType { Success, Invalid, Exception };

    public class ActionUpdateResult
    {
        public ActionUpdateResult() { }
        public ActionUpdateResult(ActionUpdateType type)
	    {
            Type = type;
    	}

        public ActionUpdateResult(ActionUpdateType type, string message)
            : this(type)
        {
            Message = message;
        }

        public ActionUpdateType Type { get; private set; }
       
        public string Message { get; set; }
    }

    public abstract class CstControllerBase : Controller
    {
        private PrdnEntities _prdnDbContext = null;

        protected PrdnEntities PrdnDBContext
        {
            get
            {
                if (_prdnDbContext == null)
                {
                    _prdnDbContext = new PrdnEntities();
                }
                return _prdnDbContext;
            }
        }

        protected void EnsurePrdnDBContextOpen() 
        {
            if (PrdnDBContext.Connection.State != ConnectionState.Open)
            {
                PrdnDBContext.Connection.Open();
            }
        }

        protected IsisEntities _isisDbContext = null;

        protected IsisEntities IsisDbContext
        {
            get
            {
                if (_isisDbContext == null)
                {
                    _isisDbContext = IsisRepository.CreateIsisContext();
                }
                return _isisDbContext;
            }
        }

        protected override void ExecuteCore()
        {
            string cultureName = GetCookieCulture();

            SetCurrentCulture(cultureName);

            base.ExecuteCore();
        }

        protected string SetCurrentCulture(string cultureName)
        {
            if ((cultureName == null) && (Request.UserLanguages.IfNotNull(l => l.Length) > 1))
                cultureName = Request.UserLanguages[0]; // obtain it from HTTP header AcceptLanguages

            // Validate culture name
            cultureName = LocalizationHelper.GetValidCulture(cultureName); // This is safe

            return LocalizationHelper.SetCurrentCulture(cultureName);// Thread.CurrentThread.CurrentUICulture.Name;
        }

        protected string SetSaveCurrentCulture(string cultureName)
        {
            string curCulture = SetCurrentCulture(cultureName);

            SaveCultureCookie(cultureName);

            return curCulture;
        }

        protected void SaveCultureCookie(string cultureName)
        {
            HttpCookie cultureCookie = new HttpCookie(LocalizationHelper.CultureCookieName, cultureName);
            Response.Cookies.Add(cultureCookie);
        }

        protected string GetCookieCulture()
        {
            // Attempt to read the culture cookie from Request
            HttpCookie cultureCookie = Request.Cookies[LocalizationHelper.CultureCookieName];
            if (cultureCookie != null)
            {
                return cultureCookie.Value;
            }
            else 
            {
                return null;
            }
        }

        protected bool CultureEsEspanol() 
        {
            string cult = LocalizationHelper.GetCurrentCulture();
            if (cult.Length > 1)
            {
                return cult.Substring(0, 2).ToUpper().Equals("ES");
            } else { 
                return false; 
            }
        }

        public static System.Globalization.DateTimeFormatInfo DateFormat { get { return System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat; } }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (_prdnDbContext != null)
            {
                _prdnDbContext.Dispose();
                _prdnDbContext = null;
            }
            if (_isisDbContext != null)
            {
                _isisDbContext.Dispose();
                _isisDbContext = null;
            }
        }

        //protected override void OnAuthorization(AuthorizationContext filterContext)
        //{
        //    HttpCookie authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];

        //    if (authCookie != null)
        //    {
        //        FormsAuthenticationTicket authTicket = FormsAuthentication.Decrypt(authCookie.Value);

        //        CustomPrincipal newUser = CustomPrincipalSerializeModel.DeserializedCustomPrincipal(authTicket.UserData);

        //        HttpContext.User = newUser;
        //    }

        //    base.OnAuthorization(filterContext);
        //}

        protected RedirectToRouteResult ErrMsgView(string errMsg)
        {
            return RedirectToAction("Message", "Error", new { message = errMsg });
        }

        protected bool IsLocalUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return false;
            }

            Uri absoluteUri;
            if (Uri.TryCreate(url, UriKind.Absolute, out absoluteUri))
            {
                return String.Equals(this.Request.Url.Host, absoluteUri.Host,
                            StringComparison.OrdinalIgnoreCase);
            }
            else
            {
                bool isLocal = !url.StartsWith("http:", StringComparison.OrdinalIgnoreCase)
                    && !url.StartsWith("https:", StringComparison.OrdinalIgnoreCase)
                    && Uri.IsWellFormedUriString(url, UriKind.Relative);
                return isLocal;
            }
        }

        public string RenderPartialView(string viewName, object model)
        {
            if (string.IsNullOrEmpty(viewName))
                viewName = this.ControllerContext.RouteData.GetRequiredString("action");

            this.ViewData.Model = model;
            using (var sw = new StringWriter())
            {
                ViewEngineResult viewResult = ViewEngines.Engines.FindPartialView(this.ControllerContext, viewName);
                var viewContext = new ViewContext(this.ControllerContext, viewResult.View, this.ViewData, this.TempData, sw);
                viewResult.View.Render(viewContext, sw);

                return sw.GetStringBuilder().ToString();
            }
        }

        protected ActionResult RedirectIfLocal(string url, Func<ActionResult> redirFunc)
        {
            if (this.IsLocalUrl(url))
            {
                return Redirect(url);
            }
            else if (redirFunc != null)
            {
                return redirFunc();
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        protected ActionResult ViewAttachment(Attachment attch)
        {
            if (!attch.IsNotNull(a => a.Data))
            {
                return ErrMsgView("Error Displaying Attachment: No attachment data.");
            }

            string mimeType = attch.MimeType;
            if (String.IsNullOrEmpty(mimeType))
                mimeType = System.Net.Mime.MediaTypeNames.Application.Octet;

            var cd = new System.Net.Mime.ContentDisposition();
            cd.FileName = HttpUtility.UrlEncode(attch.FileName);
            cd.Inline = (mimeType != System.Net.Mime.MediaTypeNames.Application.Octet);

            Response.AppendHeader("Content-Disposition", cd.ToString());

            //if (cd.Inline)
            //{
                return File(attch.Data, mimeType);
            //}
            //else
            //{
            //    return File(attch.Data, mimeType, HttpUtility.UrlEncode(attch.FileName)); // Error 349 (net::ERR_RESPONSE_HEADERS_MULTIPLE_CONTENT_DISPOSITION
            //}
        }

        protected SelectList GetAttTypes(string groups)
        {
            string[] groupArr = groups.Split(',');
            var types = (from t in PrdnDBContext.PrdnAttachmentTypes
                         where groupArr.Contains(t.GroupCD)
                         orderby t.SelectOrder
                         select new { t.ID, t.Description }).ToList();

            SelectList l = new SelectList(types, "ID", "Description");
            return l;
        }

        protected void ProcessAttacherForSave(FileAttacher attacher, IEnumerable<HttpPostedFileBase> uploadedFiles)
        {
            if (attacher == null)
            {
                return;
            }

            if (attacher.ExtantFiles != null)
            {
                var delFiles = attacher.ExtantFiles.Where(item => !String.IsNullOrEmpty(item.Deleted)).ToList();

                if (delFiles.Count() > 0)
                {
                    if (attacher.DeleteFiles == null)
                    {
                        attacher.DeleteFiles = new List<DeleteFileInfo>();
                    }
                    foreach (var f in delFiles)
                    {
                        attacher.DeleteFiles.Add(new DeleteFileInfo { ID = f.ID });
                    }
                    attacher.ExtantFiles.RemoveAll(item => !String.IsNullOrEmpty(item.Deleted));
                }
            }

            if (attacher.CachedFiles != null)
            {
                attacher.CachedFiles.RemoveAll(item => !String.IsNullOrEmpty(item.Deleted));

                foreach (var cacheFile in attacher.CachedFiles)
                {
                    var data = TempData[cacheFile.CacheID];
                    Byte[] fileData = data as Byte[];
                    cacheFile.FileData = fileData;
                }
            }

            if ((attacher.NewFiles != null) && (uploadedFiles != null))
            {
                var upldFileList = uploadedFiles.ToList();
                int cnt = Math.Min(attacher.NewFiles.Count(), upldFileList.Count());
                if (cnt > 0)
                {
                    if (attacher.CachedFiles == null)
                    {
                        attacher.CachedFiles = new List<CachedFileInfo>();
                    }
                    for (int i = 0; i < cnt; i++)
                    {
                        var upldFile = upldFileList[i];
                        if ((upldFile != null) && (upldFile.ContentLength > 0))
                        {
                            attacher.CachedFiles.Add(
                                new CachedFileInfo(Guid.NewGuid().ToString("n"), upldFile, attacher.NewFiles[i])
                            );
                        }
                    }
                }
            }

            attacher.NewFiles = null;
        }

        protected void ProcessAttacherForPost(FileAttacher attacher)
        {
            if (attacher == null)
            {
                return;
            }

            attacher.AttTypesFunc = GetAttTypes;

            if (attacher.CachedFiles != null)
            {
                foreach (var cacheFile in attacher.CachedFiles)
                {
                    TempData[cacheFile.CacheID] = cacheFile.FileData;
                }
            }

            // clear attacher model state info so Post view render does not display old list info
            foreach (var delPair in ModelState.Where(p => (p.Key.Contains("Attacher"))).ToList())
            {
                ModelState.Remove(delPair.Key);
            }
        }

        protected string CurrentActionUrl(object routeValues=null)
        {
            return Url.Action(
                    actionName: RouteData.Values["action"].ToString(),
                    controllerName: RouteData.Values["controller"].ToString(),
                    routeValues: routeValues
            );
        }

        protected bool ValidPasswordPolicy(string newPassword, string propName)
        {
            if (string.IsNullOrEmpty(newPassword))
            {
                ModelState.AddModelError(propName ?? "", "Password is required");
                return false;
            }
            else
            {
                string passwordRegex = GetPasswordRegex();
                if (!String.IsNullOrEmpty(passwordRegex))
                {
                    if (Regex.IsMatch(newPassword, passwordRegex))
                    {
                        return true;
                    }
                    else
                    {
                        ModelState.AddModelError(propName ?? "", "Password does not meet policy");
                        return false;
                    }
                }
                else
                {
                    return true;
                }
            }
        }

        private string GetPasswordRegex()
        {
            PasswordSettings passwordSetting = CST.Security.PasswordSettings.GetPasswordSettings();
            return passwordSetting.PasswordRegex;
        }

        protected string GetCurrentUserLogin()
        {
            return User.Identity.Name;
        }

        protected decimal GetCurrentUserID() 
        {
            string login = GetCurrentUserLogin();
            var id = PrdnDBContext.Users.Where(u => u.Login == login).Select(u => u.ID);
            if (id.IsAny())
            {
                return id.First();
            }
            else
            {
                throw new Exception("Invalid User Login");
            }
        }

        protected bool AuthorizedForRole(string role)
        {
            if ((Request.IsAuthenticated) && (User != null))
            {
                return CstAuthorizeAttribute.Authorized(User, role);
            }
            return false;
        }

        protected bool AtionIsAccessible(string controllerName, string actionName)
        { 
            var requestContext = ControllerContext.RequestContext;
            var controllerAuthorization = Telerik.Web.Mvc.Infrastructure.DI.Current.Resolve<Telerik.Web.Mvc.Infrastructure.IControllerAuthorization>();
            return controllerAuthorization.IsAccessibleToUser(requestContext, controllerName, actionName);
        }

        protected bool AtionIsAccessible(string actionName)
        {
            string controllerName = RouteData.Values["controller"].ToString();
            return AtionIsAccessible(controllerName, actionName);
        }

        #region User Settings

        protected UserSettingsModel RetreiveUserSettingsModel()
        {
            if (!Request.IsAuthenticated)
            {
                return null;
            }

            UserSettingsModel userSettings = new UserSettingsModel();
            
            string userLogin = GetCurrentUserLogin();

            var user = (from u in PrdnDBContext.Users.Include("PrdnSettings.DefaultRun")
                        where u.Login == userLogin
                        select u).FirstOrDefault();

            if (user != null)
            {
                userSettings.UserID = user.ID;
                userSettings.Login = user.Login;

                if (user.PrdnSettings != null) 
                {
                    userSettings.JobPageSize = (user.PrdnSettings.JobPageSize == null) ? UserSettingsModel.DefaultPageSize : Convert.ToInt32(user.PrdnSettings.JobPageSize);
                    userSettings.RequestPageSize = (user.PrdnSettings.RequestPageSize == null) ? UserSettingsModel.DefaultPageSize : Convert.ToInt32(user.PrdnSettings.RequestPageSize);
                    userSettings.LabelPrinterID = (user.PrdnSettings.LabelPrinterID == null) ? (int?)null : Convert.ToInt32(user.PrdnSettings.LabelPrinterID);

                    if (user.PrdnSettings.DefaultRun != null)
                    {
                        userSettings.DefaultRunOrderNo = user.PrdnSettings.DefaultRun.PrdnOrderNo;
                        userSettings.DefaultRunID = user.PrdnSettings.DefaultRun.ID;
                        userSettings.DefaultRunDescr = user.PrdnSettings.DefaultRun.RunDescr;
                    }
                }
            }
            else
            {
                throw new Exception("Invalid User Login");
            }

            return userSettings;
        }

        protected string UserSettingsKey { get { return User.Identity.Name+"_SettingsIdx"; } }

        protected void StoreUserSettings(UserSettingsModel userSettings)
        {
            Session[UserSettingsKey] = userSettings;
        }

        protected void ClearUserSettings()
        {
            Session.Remove(UserSettingsKey);
        }

        protected UserSettingsModel GetUserSettings()
        {
            if (!Request.IsAuthenticated)
            {
                return null;
            }

            UserSettingsModel userSettings = Session[UserSettingsKey] as UserSettingsModel;

            if (userSettings == null) {
                userSettings = RetreiveUserSettingsModel();
                StoreUserSettings(userSettings);
            }
            
            return userSettings;
        }

        protected UserSettingsViewModel GetUserSettingsViewModel(bool editable=true)
        {
            UserSettingsViewModel model = Mapper.Map<UserSettingsModel, UserSettingsViewModel>(GetUserSettings());
            model.UserEditable = editable;
            return model;
        }

        protected UserDefaultPrdnRun MakeUserDefaultPrdnRun()
        {
            UserDefaultPrdnRun defRun = new UserDefaultPrdnRun();

            UserSettingsModel userSettings = GetUserSettings();
            if (userSettings != null)
            {
                defRun.UserID =  userSettings.UserID ?? -1;
                defRun.DefaultRunID = userSettings.DefaultRunID;
                defRun.DefaultRunDescr = userSettings.DefaultRunDescr;
            }
            return defRun;
        }

        protected DefaultRunViewModel MakeDefaultRunViewModel(bool userEditable)
        {
            DefaultRunViewModel model = Mapper.Map<DefaultRunViewModel>(MakeUserDefaultPrdnRun());
            model.UserEditable = userEditable;
            return model;
        }

        protected int UpdateUserSettings(decimal userID, Action<PrdnUserSetting> updProc)
        {
            PrdnUserSetting setting;

            setting = (from s in PrdnDBContext.PrdnUserSettings
                       where s.UserID == userID
                       select s).FirstOrDefault();

            if (setting == null)
            {
                setting = new PrdnUserSetting
                {
                    UserID = userID
                };
            }

            updProc(setting);

            if (setting.EntityState == EntityState.Detached)
            {
                PrdnDBContext.PrdnUserSettings.AddObject(setting);
            }

            ClearUserSettings();
            return PrdnDBContext.SaveChanges();
        }

        protected int SaveDefaultUserPrdnRun(decimal userID, decimal? defaultRunID)
        {
            return UpdateUserSettings(userID, x => x.DefaultRunID = defaultRunID);
        }

        protected ActionResult ClearDefaultRunBase(string urlReturn)
        {
            SaveDefaultUserPrdnRun(GetCurrentUserID(), null);

            return RedirectIfLocal(urlReturn, () => RedirectToAction("Index", "Home"));
        }

        protected int GetJobPageSize()
        {
            UserSettingsModel userSettings = GetUserSettings();
            return userSettings.JobPageSize;
        }

        protected int GetRequestPageSize()
        {
            UserSettingsModel userSettings = GetUserSettings();
            return userSettings.RequestPageSize;
        }

        protected ProductionRun MakeProductionNewRun(string orderNo, decimal typeID, string note)
        {
            ProductionRun newRun = new ProductionRun();
            newRun.PrdnOrderNo = orderNo;
            newRun.PrdnTypeID = typeID;
            newRun.Description = note;

            PrdnDBContext.ProductionRuns.AddObject(newRun);
            PrdnDBContext.SaveChanges();
            
            return newRun;
        }

        /// 
        // Settings
        [HttpGet]
        public virtual ActionResult EditSettings(string urlReturn)
        {
            return EditSettingsBase(urlReturn);
        }

        [HttpPost]
        public virtual ActionResult EditSettings(UserSettingsEditViewModel model)
        {
            return EditSettingsBase(model);
        }

        #endregion

        #region Base Action Methods

        protected ActionResult EditSettingsBase(string urlReturn)
        {
            UserSettingsEditViewModel model = Mapper.Map<UserSettingsViewModel, UserSettingsEditViewModel>(GetUserSettingsViewModel());
            if (IsLocalUrl(urlReturn))
            {
                model.urlReturn = urlReturn;
            }

            model.LabelPrinters = null;
            if (AtionIsAccessible(controllerName: "Make", actionName: "Scan"))
            {
                if (AtionIsAccessible(controllerName: "Schedule", actionName: "EditSettings"))
                {
                    model.CanCreateRun = true;
                    model.Requests = true;
                }
                else
                {
                    model.CanCreateRun = false;
                    model.Requests = false;
                }

                var printers = (from p in PrdnDBContext.LabelPrinters
                               where p.ActiveFlag == PrdnDataHelper.BoolYNTue
                               orderby p.Name
                               select p).ToList();
                
                if (printers.IsAny()) 
                {
                    var list = from p in printers
                            select new
                            {
                                p.ID,
                                Display = p.Name+" "+p.HostName+" "+p.Port.ToString()
                            };
                    model.LabelPrinters = new SelectList(list, "ID", "Display");
                }
            }

            return View(model);
        }

        protected ActionResult ProductBase(ProductLookupModel model, string id)
        {
            if (id != null)
            {
                var prodQry = from p in PrdnDBContext.Products
                              where p.ProdCD == id
                              select p;
                
                if (!String.IsNullOrWhiteSpace(model.ProdTypeCD))
                {
                    prodQry = from p in prodQry 
                              where p.ProdTypeCD == model.ProdTypeCD
                              select p;
                }
                
                var prod = (from p in prodQry
                            select new {
                                p.ProdCD,
                                p.ProdTypeCD
                            }).FirstOrDefault();
                
                if (prod != null) {
                    model.ProdCD = prod.ProdCD;
                    model.ProdTypeCD = prod.ProdTypeCD;
                }
            }

            if (String.IsNullOrWhiteSpace(model.ProdTypeCD))
            {
                model.ProdTypeCD = PrdnDataHelper.LeatherProdTypeCd;
            }

            return View(model);
        }

        protected ActionResult PatternBase(PatternLookupModel model, string id)
        {
            string prodCD = id.IfNotNull(c => c.Trim().ToUpper());
            if (prodCD != null)
            {
                if (!prodCD.EndsWith(PrdnDataHelper.LeatherPatternSuffix, StringComparison.OrdinalIgnoreCase))
                {
                    prodCD = prodCD + PrdnDataHelper.LeatherPatternSuffix;
                }
            }

            model.ProdTypeCD = PrdnDataHelper.LeatherPatternProdTypeCd;
            model.ProdCdDisplayTerm = LocalStr.Pattern;

            return ProductBase(model, prodCD);

        }

        protected ActionResult InvItemBase(string id)
        {
            InvItemViewModel model = LookupInvItem(id);
            return View(model);
        }

        protected ActionResult EditSettingsBase(UserSettingsEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                if ((model.NewRunOrderNo != null) && (model.NewRunPrdnTypeID != null))
                {
                    ProductionRun newRun = MakeProductionNewRun(model.NewRunOrderNo, (decimal)model.NewRunPrdnTypeID, model.NewRunNote);

                    model.DefaultRunID = newRun.ID;                
                }

                decimal? oldDefRnID = null;
                UpdateUserSettings(model.UserID, settings => {
                    oldDefRnID = settings.DefaultRunID;
                    Mapper.Map(model, settings); 
                });

                if (model.DefaultRunID != oldDefRnID)
                {
                    return RedirectIfNewRunID(model.urlReturn, model.DefaultRunID);
                }
                
                return RedirectIfLocal(model.urlReturn, () => RedirectToAction("Index", "Home"));
            }
            return View(model);
        }

        protected ActionResult RedirectIfNewRunID(string urlReturn, decimal? runID)
        {
            if(!String.IsNullOrWhiteSpace(urlReturn) && (runID != null))
            {
                RouteData routeData = RouteTable.Routes.ParseRouteData(urlReturn, Request);
                if ((routeData != null) && ("Jobs".Equals(routeData.Values["action"])) && (routeData.Values["id"] != null))
                {
                    routeData.Values["id"] = runID;
                    return RedirectToRoute(routeData.Values);
                }
            }            
            return RedirectIfLocal(urlReturn, () => RedirectToAction("Index", "Home"));
        }

        public IQueryable<Product> ProdsForParent(string parentProdCd)
        {
            var prodList = from p in PrdnDBContext.Products
                           where p.ParentProdCD == parentProdCd
                           select p;
            return prodList;
        }

        public IEnumerable<LeatherListModel> LeathersForPatten(string parentProdCd)
        {
            var prods = (from p in ProdsForParent(parentProdCd) orderby p.ProdCD select p).ToList();
            var models = from p in prods
                         select new LeatherListModel
                         {
                             ProdTypeCD = p.ProdTypeCD,
                             ProdCD = p.ProdCD,
                             ProdDescr = p.Description,
                             ParentProdCD = p.ParentProdCD,
                             ColorCDStr = p.LeatherCharVW.ColorCdDisplay,
                             ColorDescrStr = p.LeatherCharVW.ColorDescrDisplay,
                             DecorStr = p.LeatherCompVW.AbrevStr,
                             Cost = p.Cost
                         };
            return models;
        }

        public virtual ActionResult _PatternLeatherPartial(string prodCD)
        {
            IEnumerable<LeatherListModel> leatherList = LeathersForPatten(prodCD);
            return Json(RenderPartialView("LeatherList", leatherList), JsonRequestBehavior.AllowGet);
        }

        protected void PrintJobLabel(IPrinterInfo printerInfo, ProductionJob job)
        {
            InvItemViewModel itemModel = null;
            EditPrdnJobViewModel viewJob = null;

            if (job.CustID == PrdnDataHelper.PrdnCustIDCST)
            {
                itemModel = LookupInvItem(job.SerialNo);
            }
            else 
            {
                viewJob = Mapper.Map<ProductionJob, EditPrdnJobViewModel>(job);
            }

            if (itemModel != null)
            {
                PrintItemViewLabel(printerInfo, itemModel, job);
            }
            else if (viewJob != null)        
            {
                PrintJobViewItemLabel(printerInfo, viewJob, job);
            }
        }

        protected void PrintJobViewItemLabel(IPrinterInfo printerInfo, EditPrdnJobViewModel viewJob, ProductionJob job = null)
        {
            if ((viewJob.PrdnTypeCode == null) && (job.IfNotNull(j => j.Run) != null))
            {
                viewJob.loadFromRun(job.Run);
            }
            if ((viewJob.WorksheetID != null) && (viewJob.EditWorksheet == null))
            {
                WorksheetEditViewModel.LoadViewJobEditWorksheet(viewJob, job);
            }

            PrdnZplMultiParam parm = new PrdnZplMultiParam(viewJob);
            ZplPrinterHelper.NetworkMultiZpl(printerInfo.PrinterHostName, (int)printerInfo.PrinterPort, parm);
        }

        protected void PrintItemViewLabel(IPrinterInfo printerInfo, InvItemViewModel item, ProductionJob job = null)
        {
            PrdnZplMultiParam parm = new PrdnZplMultiParam(item);

            if (job != null)
            {
                parm.Priority = job.IfNotNull(j => j.Priority).IfNotNull(p => p.Name);

                parm.SetPrdnOrderDisp(
                    job.IfNotNull(j => j.Run).IfNotNull(r => r.PrdnOrderNo),
                    job.IfNotNull(j => j.Run).IfNotNull(r => r.PrdnType.IfNotNull(t => t.Code)),
                    job.IfNotNull(j => j.RunSeqNo));

                parm.ShipCD = job.IfNotNull(j => j.ShipMethodCD);
            }

            ZplPrinterHelper.NetworkMultiZpl(printerInfo.PrinterHostName, (int)printerInfo.PrinterPort, parm);
        }

        protected void LoadPrinterInfo(IPrinterInfo info)
        {
            if (info.LabelPrinterID == null)
            {
                UserSettingsModel settings = GetUserSettings();
                info.LabelPrinterID = settings.IfNotNull(s => s.LabelPrinterID);
            }
            
            if ((String.IsNullOrWhiteSpace(info.PrinterHostName) || (info.PrinterPort == null))
            &&
            (info.IfNotNull(s => s.LabelPrinterID) > 0))
            {
                var printer = (from p in PrdnDBContext.LabelPrinters
                               where p.ID == info.LabelPrinterID 
                               select p).FirstOrDefault();
                if (printer != null)
                {
                    info.LabelPrinterID = printer.ID.ToInt();
                    info.PrinterName = printer.Name;
                    info.PrinterHostName = printer.HostName;
                    info.PrinterPort = printer.Port.ToInt();
                }
            }
        
        }

        protected ActionResult FindItemSerialBase(string id)
        {
            InvLookupItemViewModel model = new InvLookupItemViewModel();
            //model.SerialNo = id;

            if (!String.IsNullOrWhiteSpace(id))
            {
                InvItemViewModel item = LookupInvItem(id);
                if (item != null)
                {
                    Mapper.Map(item, model);     
                }
            }

            LoadPrinterInfo(model);
            return View(model);
        }

        protected ActionResult ItemLabelBase(string id, PrinterInfo printerInfo)
        {
            printerInfo = printerInfo ?? new PrinterInfo();
            LoadPrinterInfo(printerInfo);

            ProductionJob job = (from j in PrdnDBContext.ProductionJobs.Include("Product").Include("Priority").Include("Run.PrdnType")
                                 where j.SerialNo == id
                                 select j).FirstOrDefault();
            try {
                if (job != null)
                {
                    PrintJobLabel(printerInfo, job);
                }
                else {
                    InvItemViewModel itemModel = LookupInvItem(id);
                    PrintItemViewLabel(printerInfo, itemModel);
                }

                printerInfo.Message = SystemExtensions.Sentence(LocalStr.Label, LocalStr.Printed, LocalStr.For, LocalStr.SerialNo, id, ".",
                    LocalStr.LabelPrinter, printerInfo.PrinterName, printerInfo.PrinterHostName, printerInfo.IsNotNull(p => p.PrinterPort) ? printerInfo.PrinterPort.ToString() : null);
            }
            catch (Exception ex)
            {
                printerInfo.Message = SystemExtensions.Sentence(LocalStr.Label, LocalStr.Print, LocalStr.Error, ":", ex.GetType().Name + " - " + ex.Message).SafeSub(0, 450);
            }

            return View(printerInfo);
        }

        #endregion

        #region View Models

        protected IEnumerable<ProdTypeListModel> GetProdTypeList()
        {
            return from type in PrdnDBContext.ProductTypes
                   where (type.Status == PrdnDataHelper.StatusActive)
                   orderby type.ProdTypeCD
                   select new ProdTypeListModel
                   {
                       Code = type.ProdTypeCD,
                       Name = type.Description
                   };
        }

        protected IEnumerable<ProdTypeListModel> GetDIProdTypeList()
        {
            return from type in PrdnDBContext.ProductTypes
                   where (type.DIFlag == PrdnDataHelper.BoolYNTue) && (type.Status == PrdnDataHelper.StatusActive)
                   orderby type.ProdTypeCD
                   select new ProdTypeListModel
                   {
                       Code = type.ProdTypeCD,
                       Name = type.Description
                   };
        }

        protected IQueryable<ProdTypeListModel> GetPrdnProdTypeList()
        {
            return (from type in PrdnDBContext.ProductionTypes
                    where (type.ActiveFlag == PrdnDataHelper.BoolYNTue)
                    orderby type.ProdTypeCD 
                    select new ProdTypeListModel
                    {
                        Code = type.ProductType.ProdTypeCD,
                        Name = type.ProductType.Description
                    }).Distinct(); 
        }

        protected List<string> GetPrdnProdTypeCDs()
        {
            return GetPrdnProdTypeList().Select(t => t.Code).ToList();
        }

        /// <summary>
        /// Production Runs
        /// </summary>
        protected IEnumerable<PrdnRunViewModel> RunViewList()
        {
            var runs = from r in PrdnDBContext.ProductionRuns
                       orderby r.PrdnOrderNo, r.PrdnType.SortOrder, r.PrdnType.Code
                       select new PrdnRunViewModel
                       {
                           ID = r.ID,
                           PrdnOrderNo = r.PrdnOrderNo,
                           PrdnTypeID = r.PrdnTypeID,
                           PrdnTypeCode = r.PrdnType.Code,
                           PrdnTypeDescr = r.PrdnType.Description,
                           Description = r.Description,
                           HasJobs = r.Jobs.Any(),
                           ProdTypeCD = r.PrdnType.ProdTypeCD,
                           ProdTypeDescr = r.PrdnType.ProductType.Description
                       };

            return runs;
        }

        protected PrdnRunViewModel GetRunViewModel(int id)
        {
            var run = (from r in RunViewList()
                       where r.ID == id
                       select r).FirstOrDefault();
            return run;
        }

        protected IEnumerable<PrdnRunViewModel> RunViewList(string prdnNo)
        {
            var runs = from r in RunViewList()
                       where r.PrdnOrderNo == prdnNo
                       select r;
            return runs;
        }

        protected GridModel RunGridList(string prdnNo)
        {
            GridModel g = new GridModel(RunViewList(prdnNo).ToList());
            return g;
        }

        /// <summary>
        /// Production Jobs
        /// </summary>
        protected IQueryable<ProductionJob> JobsForRun(decimal? runID, List<string> statusList)
        {
            if ((runID == null) || (runID < 1))
            {
                return null;
            }
            else
            {
                var jobs = from j in PrdnDBContext.ProductionJobs
                            .Include("Customer")
                            .Include("Product.LeatherCharVW")
                            .Include("Product.LeatherCompVW")
                            .Include("Worksheet.WorksheetCharVW")
                            .Include("Worksheet.WorksheetCompVW")
                            .Include("Priority")
                            .Include("PrdnInvItem")
                        where j.RunID == runID
                        //orderby j.RunSeqNo
                        select j;

                if (statusList.IsAny())
                {
                    jobs = from j in jobs
                           where statusList.Contains(j.StatusStr)
                           select j;
                }

                jobs = from j in jobs
                       orderby j.RunSeqNo
                       select j;

                return jobs;
            }
        }

        protected int JobsForRunCount(decimal? runID, List<string> statusList)
        {
            if ((runID == null) || (runID < 1))
            {
                return 0;
            }
            else
            {
                return JobsForRun(runID, statusList).Count();
            }
        }

        protected virtual PrdnJobStatusViewModel MakeStatusModel()
        {
            return new PrdnJobStatusViewModel();
        }

        protected int LastPageForRun(decimal runID)
        {
            List<string> statusList = MakeStatusModel().RetrieveDBStatusList(Session);

            int jobCount = JobsForRunCount(runID, statusList);
            if (jobCount > 0)
            {
                return (int)(Math.Ceiling(jobCount / (double)GetJobPageSize()));
            }
            else
            {
                return 1;
            }
        }

        protected List<ListPrdnJobViewModel> JobViewList(GridCommand command, decimal? runID, int count, List<string> statusList)
        {
            IQueryable<ProductionJob> jobs = JobsForRun(runID, statusList);

            if (command.PageSize > 0 && command.Page > 0)
            {
                int skip = (command.Page - 1) * command.PageSize;
                if (skip < count)
                {
                    jobs = jobs.Skip(skip);
                }
            }

            jobs = jobs.Take(command.PageSize);

            var j = jobs.ToList();
            List<ListPrdnJobViewModel> viewJobs = Mapper.Map<List<ProductionJob>, List<ListPrdnJobViewModel>>(j);

            return viewJobs;
        }

        protected GridModel JobGridList(GridCommand command, decimal? runID, List<string> statusList)
        {
            List<ListPrdnJobViewModel> l;
            int jobCount;

            if ((runID == null) || (runID < 1))
            {
                jobCount = 0;
                l = new List<ListPrdnJobViewModel>();
            }
            else
            {
                jobCount = JobsForRunCount(runID, statusList);
                l = JobViewList(command, runID, jobCount, statusList);
            }

            return new GridModel
            {
                Data = l,
                Total = jobCount
            };
        }

        protected IQueryable<ProductionJob> JobsForLookup(JobLookupModel model)
        {
            if (model.IfNotNull(m => m.AnyCriteria) == false)
            {
                return Enumerable.Empty<ProductionJob>().AsQueryable();
            }

            var jobQry = from j in PrdnDBContext.ProductionJobs select j;

            if (model.CustID != null) {
                jobQry = from j in jobQry 
                         where j.CustID == model.CustID
                         select j;
            }
            if (!String.IsNullOrWhiteSpace(model.CustLocation)) {
                jobQry = from j in jobQry 
                         where j.CustLocation.StartsWith(model.CustLocation)
                         select j;
            }
            if (!String.IsNullOrWhiteSpace(model.OrderNo)) {
                jobQry = from j in jobQry 
                         where j.OrderNo.StartsWith(model.OrderNo)
                         select j;
            }
            if (!String.IsNullOrWhiteSpace(model.PurchaseOrder))
            {
                jobQry = from j in jobQry
                         where j.PurchaseOrder.StartsWith(model.PurchaseOrder)
                         select j;
            }
            if (!String.IsNullOrWhiteSpace(model.CstRequestID)) {
                jobQry = from j in jobQry 
                         where j.CstRequestID.StartsWith(model.CstRequestID)
                         select j;
            }
            if (!String.IsNullOrWhiteSpace(model.DropShipCustID)) {
                jobQry = from j in jobQry 
                         where j.DropShipCustID.StartsWith(model.DropShipCustID)
                         select j;
            }
            if (!String.IsNullOrWhiteSpace(model.SerialNo)) {
                jobQry = from j in jobQry 
                         where j.SerialNo.StartsWith(model.SerialNo)
                         select j;
            }
            if (!String.IsNullOrWhiteSpace(model.ProdCD)) {
                jobQry = from j in jobQry 
                         where j.ProdCD.StartsWith(model.ProdCD)
                         select j;
            }
            if (!String.IsNullOrWhiteSpace(model.PrdnOrderNo)) { 
                jobQry = from j in jobQry 
                         where j.Run.PrdnOrderNo.StartsWith(model.PrdnOrderNo)
                         select j;
            }
            if (model.PrdnTypeID != null) {
                jobQry = from j in jobQry 
                         where j.Run.PrdnTypeID == model.PrdnTypeID
                         select j;
            }
            if (model.FromShipDt != null) {
                jobQry = from j in jobQry 
                         where j.Run.PrdnOrder.ShipDay >= model.FromShipDt
                         select j;
            }
            if (model.ThruShipDt != null) {

                DateTime beforeDt = (DateTime)model.ThruShipDt;
                beforeDt = beforeDt.Date.AddDays(1);
                jobQry = from j in jobQry
                         where j.Run.PrdnOrder.ShipDay < beforeDt
                         select j;
            }
            if (model.RunSeqNo != null) {
                jobQry = from j in jobQry
                         where j.RunSeqNo == model.RunSeqNo
                         select j;
            }
            if (model.JobID != null) {
                jobQry = from j in jobQry
                         where j.ID == model.JobID
                         select j;
            }
            //if (!String.IsNullOrWhiteSpace(statusStr))
            //{
            //    List<string> stati = PrdnJobStatusViewModel.UpperStatuses(statusStr).ToList();
            //    jobs = from j in jobs
            //            where stati.Contains(j.StatusStr)
            //            select j;
            //}

            var jobs = (from j in jobQry
                       orderby j.Run.PrdnOrderNo, j.Run.PrdnType.SortOrder, j.Run.PrdnType.Code, j.RunSeqNo
                       select j).Take(200);

            return jobs;
        }

        protected IEnumerable<PrdnJobListViewModel> JobLookupViewList(JobLookupModel model)
        {
            var jobs = JobsForLookup(model);
            /*var viewJobs = from j in jobs
                           select new PrdnJobListViewModel {
                               PrdnOrderNo = j.Run.PrdnOrder.OrderNo,
                               PrdnShipDay = j.Run.PrdnOrder.ShipDay,
                               RunID = j.Run.ID.ToInt(),
                               PrdnTypeCD = j.Run.PrdnType.Code,
                               JobID = j.ID.ToInt(),
                               RunSeqNo = j.RunSeqNo.ToInt(),
                               SerialNo = j.SerialNo,
                               ProdCD = j.ProdCD,
                               ProdDescr = j.Product.Description,
                               Status = j.Status,
                               StatusDt = j.StatusDt,
                               CustCode = j.Customer.Code,
                               CustLocation = j.CustLocation
                            };
            */
            List<PrdnJobListViewModel> viewJobs = Mapper.Map<List<ProductionJob>, List<PrdnJobListViewModel>>(jobs.ToList());
            return viewJobs;
        }

        protected GridModel JobGridLookup(JobLookupModel model)
        {
            IEnumerable<PrdnJobListViewModel> l = JobLookupViewList(model);
            return new GridModel(l);
        }

        protected SelectList GetCustomerList()
        {
            var custs = from c in PrdnDBContext.ProductionCustomers
                        where c.ActiveFlag == PrdnDataHelper.BoolYNTue
                        select new
                        {
                            c.ID,
                            CodeDashName = c.Code + "-" + c.Name
                        };

            return new SelectList(custs.ToList(), "ID", "CodeDashName");
        }

        protected IEnumerable<CustLocViewModel> GetCustLocations(decimal? custID)
        {
            
            if (custID == PrdnDataHelper.PrdnCustIDCST)
            {
                return IsisDbContext.CustLocs();
            }
            else if (custID == PrdnDataHelper.PrdnCustIDRW)
            {
                return new List<CustLocViewModel>{
                    new CustLocViewModel {    
                        LocID = PrdnDataHelper.RWIsisDeptID,
                        Description = PrdnDataHelper.RWIsisDeptDescr,
                        Display = PrdnDataHelper.RWIsisDeptID + "-" + PrdnDataHelper.RWIsisDeptDescr
                    }
                };
            }
            else
            {
                return new List<CustLocViewModel>();
            }
        }

        protected SelectList GetCustLocationSelList(decimal? custID)
        {
            var locs = GetCustLocations(custID);

            if (locs == null)
            {
                return new SelectList(new List<CustLocViewModel>());
            }
            else
            {
                return new SelectList(locs.ToList(), "LocID", "Display");
            }
        }

        protected InvItemViewModel LookupInvItem(string serialNo)
        {
            var item = (from i in PrdnDBContext.PrdnInvItems.Include("Product.ProductType")
                        where i.SerialNo == serialNo
                        select new InvItemViewModel
                        {
                            InvItemID = i.InvItemID,
                            SerialNo = i.SerialNo,
                            ProdTypeCD = i.Product.ProdTypeCD,
                            ProdType = i.Product.ProductType.Description,
                            ProdCD = i.Product.ProdCD,
                            Description = i.Product.Description,
                            ParentProdCD = i.Product.ParentProdCD
                        }).FirstOrDefault();

            if (item != null)
            {
                item.ItemOptions = InvItemOptDefn.ItemOptions(item.InvItemID, item.ProdCD, item.ParentProdCD);
            }

            return item;
        }

        #endregion
    }

}