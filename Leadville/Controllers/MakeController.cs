using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CST.ActionFilters;
using CST.Prdn.ViewModels;
using CST.Localization;
using CST.Prdn.Data;
using AutoMapper;
using CST.ISIS.Data;
using CST.ZebraUtils;

namespace CST.Prdn.Controllers
{
    [CstAuthorize(Groups = "MAKE/SHOP, MAKE/SUPER")]
    public class MakeController : JobControllerBase
    {
        ///
        // Orders
        public ActionResult Runs(string id)
        {
            PrdnOrdViewModel model;

            if (id == null)
            {
                UserSettingsViewModel settings = GetUserSettingsViewModel(true);
                if (settings.IsNotNull(s => s.DefaultRunOrderNo))
                {
                    id = settings.DefaultRunOrderNo;
                    return RedirectToAction(actionName: "Runs", routeValues: new { id = id });
                }
            }


            if (id != null)
            {
                model = (from o in PrdnDBContext.ProductionOrders
                         where o.OrderNo == id
                         select new PrdnOrdViewModel
                         {
                             OrderNo = o.OrderNo,
                             ShipDay = o.ShipDay,
                         }).FirstOrDefault();
                if (model == null)
                {
                    return ErrMsgView(SystemExtensions.Sentence(LocalStr.OrderNo, id, LocalStr.verbIsNot, LocalStr.valid));
                }
            }
            else {
                model = new PrdnOrdViewModel();
            }
            
            model.ShipDtStr = model.ShipDay.ToString(DateFormat.ShortDatePattern);
            return View(model);
        }

        public ActionResult FindOrder()
        {
            return View();
        }

        ///
        // Runs
        public ActionResult Jobs(string id)
        {
            PrdnRunMakeViewModel model = null;

            UserSettingsViewModel settings = GetUserSettingsViewModel(true);
            if (id == null)
            {
                if (settings.IsNotNull(s => s.DefaultRunID))
                {
                    int runID = (int)settings.DefaultRunID;
                    return RedirectToAction(actionName: "Jobs", routeValues: new { id = runID });
                }
            }
            else
            {
                int intID = Convert.ToInt32(id);

                model = Mapper.Map<PrdnRunViewModel, PrdnRunMakeViewModel>(GetRunViewModel(intID));

                if (model == null)
                {
                    return ErrMsgView(SystemExtensions.Sentence(LocalStr.RunID, id, LocalStr.verbIsNot, LocalStr.valid));
                }
            }

            if (model == null)
            {
                model = new PrdnRunMakeViewModel();
            }

            model.SettingsModel = settings;
            model.JobPageSize = model.SettingsModel.JobPageSize ?? UserSettingsModel.DefaultPageSize;

            model.FilterModel = MakeStatusModel();
            model.FilterModel.AssignFromSession(Session);

            return View(model);
        }

        protected override PrdnJobStatusViewModel MakeStatusModel()
        {
            PrdnJobStatusViewModel model = base.MakeStatusModel();
            model.PendingAllowed = false;
            model.Pending = false;
            model.CanceledAllowed = false;
            model.Canceled = false;
            return model;
        }

        public ActionResult FindRun()
        {
            return View();
        }

        ///
        // Jobs
        [HttpGet]
        public ActionResult UpdateJob(int id, string urlReturn)
        {
            EditPrdnJobViewModel editJob = GetEditJobViewModel(id, urlReturn);
            if (editJob != null)
            {
                UpdMakePrdnJobViewModel updViewJob = Mapper.Map<EditPrdnJobViewModel, UpdMakePrdnJobViewModel>(editJob);

                updViewJob.CustName = GetCustName(updViewJob.CustID);
                updViewJob.CustLocName = GetCustLocName(updViewJob.CustID);
                updViewJob.ShipMethodDescription = GetShipMethodDescr(updViewJob.ShipMethodCD);

                loadViewJobViewAttachments(updViewJob, "A,M");
                //List<ExtantFileInfo> list = ExtantAttachments(viewJob.ID);

                updViewJob.CanEditRun = false;
                updViewJob.SetAllowedToStatus(PrdnJobStatus.Canceled, AtionIsAccessible("CancelJob"));
                updViewJob.SetAllowedToStatus(PrdnJobStatus.Completed, AtionIsAccessible("CompleteJob"));

                return View(updViewJob);
            }
            else
            {
                return ErrMsgView(SystemExtensions.Sentence(LocalStr.Job, LocalStr.ID, id.ToString(), LocalStr.verbIsNot, LocalStr.valid));
            }
        }

        [HttpPost]
        public ActionResult UpdateJob(UpdMakePrdnJobViewModel updViewJob, IEnumerable<HttpPostedFileBase> uploadedFiles)
        {
            ProcessAttacherForSave(updViewJob.Attacher, uploadedFiles); 
            bool attachChanged = (updViewJob.IfNotNull(m => m.Attacher).IfNotNull(a => a.OrigChanged) == true);

            ProductionJob editJob = (from j in PrdnDBContext.ProductionJobs
                                     where j.ID == updViewJob.ID
                                     select j).FirstOrDefault();
            if (editJob == null)
            {
                return ErrMsgView(SystemExtensions.Sentence("Error Saving Job: ", LocalStr.JobID, updViewJob.IfNotNull(j => j.ID).ToString(), LocalStr.verbIsNot, LocalStr.valid));
            }
            bool statusChanged = (editJob.Status != updViewJob.EditStatus);

            if (statusChanged && (!UpdMakePrdnJobViewModel.FromStatusTo(editJob.Status, updViewJob.EditStatus)))
            {
                return ErrMsgView(SystemExtensions.Sentence("Error Saving Job:", LocalStr.Status, LocalStr.Change, LocalStr.to, updViewJob.EditStatus.Description(), 
                    LocalStr.verbIsNot, LocalStr.valid));
            }
            if (statusChanged || attachChanged)
            {
                EnsurePrdnDBContextOpen();
                using (var transaction = PrdnDBContext.Connection.BeginTransaction())
                {
                    try
                    {
                        if (statusChanged) {
                            editJob.UpdateStatus((PrdnJobStatus)updViewJob.EditStatus, GetCurrentUserID());
                        }
                        if (attachChanged) {
                            UpdateJobAttachments(editJob, updViewJob.Attacher);
                        }
                        PrdnDBContext.SaveChanges();
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        return ErrMsgView("Error Saving Job: " + ex.Message);
                    }
                }
            }

            return RedirectIfLocal(updViewJob.UrlReturn,
                () => RedirectToAction(actionName: "Jobs", routeValues: new { id = updViewJob.RunID }));
        }

        [HttpGet]
        public ActionResult ViewJob(int id, string urlReturn)
        {
            return ViewJobBase(id, "A,M", urlReturn);
        }

        [HttpPost]
        public ActionResult ViewJob(ViewPrdnJobViewModel viewJob, IEnumerable<HttpPostedFileBase> uploadedFiles)
        {
            return ViewJobBase(viewJob, uploadedFiles);
        }

        private CompleteJobResult CompleteJob(ProductionJob editJob)
        {
            if (!UpdMakePrdnJobViewModel.FromStatusTo(editJob.Status, PrdnJobStatus.Completed))
            {
                return new CompleteJobResult(ActionUpdateType.Invalid,
                    SystemExtensions.Sentence(LocalStr.Cannot, LocalStr.Change, LocalStr.Status, LocalStr.From, editJob.Status.Description(), LocalStr.to, PrdnJobStatus.Completed.Description()));
            }

            if (editJob.CustID == PrdnDataHelper.PrdnCustIDCST)
            {
                return CompleteCstJob(editJob);
            }
            else if (editJob.CustID == PrdnDataHelper.PrdnCustIDRW)
            {
                return CompleteRWJob(editJob);
            }
            else
                return new CompleteJobResult(ActionUpdateType.Success);
        }

        private CompleteJobResult CompleteCstJob(ProductionJob editJob)
        {
            var itm = (from i in PrdnDBContext.PrdnInvItems
                       where i.SerialNo == editJob.SerialNo
                       select new { i.SerialNo }).FirstOrDefault();

            if (itm != null)
            {
                return new CompleteCstJobResult(ActionUpdateType.Invalid) { InvItemID = editJob.PrdnInvItem.InvItemID };
            }

            EnsurePrdnDBContextOpen();
            using (var transaction = PrdnDBContext.Connection.BeginTransaction())
            {
                try
                {
                    decimal invItemID = 0m;
                    PrdnDBContext.ExecuteStoreCommand(@"BEGIN FG_CREATE_ITEM_FROM_PRDN_JOB(:p0, :p1); END;", invItemID, editJob.ID);

                    if (editJob.Status != PrdnJobStatus.Completed)
                    {
                        editJob.UpdateStatus(PrdnJobStatus.Completed, GetCurrentUserID());
                        PrdnDBContext.SaveChanges();
                    }

                    transaction.Commit();
                    return new CompleteCstJobResult(ActionUpdateType.Success) { InvItemID = editJob.PrdnInvItem.InvItemID };
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return new CompleteCstJobResult(ActionUpdateType.Exception, "Error Saving Job " + ex.Message);
                }
            }
        }

        private CompleteRWJobResult CompleteRWJob(ProductionJob editJob)
        {
            return new CompleteRWJobResult(ActionUpdateType.Success);
        }

        [CstAuthorize(Groups = "MAKE/COMPLETE")]
        public ActionResult CompleteJob(int id, string urlReturn)
        {
            ProductionJob editJob = (from j in PrdnDBContext.ProductionJobs
                                     where j.ID == id
                                     select j).FirstOrDefault();
            if (editJob == null)
            {
                return ErrMsgView(SystemExtensions.Sentence(LocalStr.Job, LocalStr.Save, LocalStr.Error, "-",
                        LocalStr.JobID, id.ToString(), LocalStr.verbIsNot, LocalStr.valid));
            }

            CompleteJobResult result = CompleteJob(editJob);

            if (result.Type == ActionUpdateType.Success)
            {
                return RedirectIfLocal(urlReturn, () => RedirectToAction(actionName: "Jobs", routeValues: new { id = editJob.RunID }));
            }
            else 
            {
                return ErrMsgView(result.Message);
            }
        }

        protected ScanViewModel ScanModelPrinter(ScanViewModel model)
        {
            if (model == null)
            {
                model = new ScanViewModel();
            }
            LoadPrinterInfo(model);
            return model;
        }

        protected ActionResult LoadedScanView(ScanViewModel model)
        {
            model = ScanModelPrinter(model);
            model.Scans = ScanList(fromDt: DateTime.Today.Date, take: 30);
            return View(model);
        }

        public class CompleteJobResult : ActionUpdateResult
        {
            public CompleteJobResult()
                : base() { }
            public CompleteJobResult(ActionUpdateType type)
                : base(type) { }
            public CompleteJobResult(ActionUpdateType type, string message)
                : base(type, message) { }


            public ScanResult PrintedScanResult()
            {
                if (this is CompleteCstJobResult)
                {
                    return ScanResult.CstItemCreatedAndPrinted;
                }
                else
                {
                    return ScanResult.CompletedAndPrinted;
                }
            }

            public ScanResult NotPrintedScanResult()
            {
                if (this is CompleteCstJobResult)
                {
                    return ScanResult.CstItemCreatedAndPrinted;
                }
                else
                {
                    return ScanResult.CompletedAndPrinted;
                }
            }

        }

        public class CompleteCstJobResult : CompleteJobResult
        {
            public CompleteCstJobResult(ActionUpdateType type)
                : base(type) { }
            public CompleteCstJobResult(ActionUpdateType type, string message)
                : base(type, message) { }

            public string InvItemID { get; set; }
        }

        public class CompleteRWJobResult : CompleteJobResult
        {
            public CompleteRWJobResult(ActionUpdateType type)
                : base(type) { }
            public CompleteRWJobResult(ActionUpdateType type, string message)
                : base(type, message) { }
        }

        [CstAuthorize(Groups = "MAKE/COMPLETE")]
        [HttpGet]
        public ActionResult Scan()
        {
            ScanViewModel model = new ScanViewModel();
            return LoadedScanView(model);
        }

        [CstAuthorize(Groups = "MAKE/COMPLETE")]
        [HttpPost]
        public ActionResult Scan(ScanViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                ProductionScan newScan;

                ProductionJob scanJob = (from j in PrdnDBContext.ProductionJobs.Include("Product").Include("Priority").Include("Run.PrdnType").Include("Run.PrdnOrder")
                                         where j.SerialNo == model.SerialNo
                                         select j).FirstOrDefault();

                if (scanJob == null)
                {
                    //string msg = SystemExtensions.Sentence(LocalStr.SerialNo, model.SerialNo, LocalStr.verbIsNot, LocalStr.valid);
                    newScan = AddScan(model.SerialNo, ScanResult.InvalidScanValue);
                    if (newScan != null)
                    {
                        ModelState.AddModelError(model.FullPropertyName(s => s.SerialNo), ScanViewModel.ResultMessage(newScan).SafeSub(0, 200));
                    }
                    return LoadedScanView(model);
                }

                CompleteJobResult result = CompleteJob(scanJob);

                if (result.Type == ActionUpdateType.Success)
                {
                    model = ScanModelPrinter(model);
                    if (model.IsNotNull(m => m.LabelPrinterID))
                    {
                        try
                        {
                            PrintJobLabel(model, scanJob);

                            newScan = AddScan(model.SerialNo, result.PrintedScanResult(), scanJob.Status);
                        }
                        catch (Exception ex)
                        {
                            string msg = (ex.GetType().Name + " - " + ex.Message).SafeSub(0, 450);
                            if (AddScan(model.SerialNo, result.NotPrintedScanResult(), scanJob.Status, msg) != null)
                            {
                                ModelState.AddModelError(model.FullPropertyName(s => s.SerialNo), msg.SafeSub(0, 200));
                            }
                        }
                    }
                    else
                    {
                        newScan = AddScan(model.SerialNo, result.NotPrintedScanResult(), scanJob.Status, SystemExtensions.Sentence(LocalStr.No, LocalStr.ScanPrinter, LocalStr.Assigned));
                    }
                }
                else if (result.Type == ActionUpdateType.Invalid)
                {
                    CompleteCstJobResult cstResult = result as CompleteCstJobResult;
                    if (cstResult.IfNotNull(r => r.InvItemID) != null)      // cst job result with an item id means the item already exists
                    {
                        newScan = AddScan(model.SerialNo, ScanResult.ItemExists, scanJob.Status);
                        if (newScan != null)
                        {
                            ModelState.AddModelError(model.FullPropertyName(s => s.SerialNo), ScanViewModel.ResultMessage(newScan).SafeSub(0, 200));
                        }
                        return LoadedScanView(model);
                    }
                    else
                    {
                        newScan = AddScan(model.SerialNo, ScanResult.InvalidStatus, scanJob.Status);
                        if (newScan != null)
                        {
                            ModelState.AddModelError(model.FullPropertyName(s => s.SerialNo), ScanViewModel.ResultMessage(newScan).SafeSub(0, 200));
                        }
                        return LoadedScanView(model);
                    }
                }
                else if (result.Type == ActionUpdateType.Exception)
                {
                    //string msg = SystemExtensions.Sentence(result.Message.SafeSub(0, 450));
                    newScan = AddScan(model.SerialNo, ScanResult.ScanException, scanJob.Status);
                    if (newScan != null)
                    {
                        ModelState.AddModelError(model.FullPropertyName(s => s.SerialNo), ScanViewModel.ResultMessage(newScan).SafeSub(0, 200));
                    }
                    return LoadedScanView(model);
                }
            }
            catch (Exception ex)
            {
                string msg = (ex.GetType().Name +" - "+ ex.Message).SafeSub(0, 450);
                if (AddScan(model.SerialNo, ScanResult.ScanException, null, msg) != null)
                {
                    ModelState.AddModelError(model.FullPropertyName(s => s.SerialNo), msg.SafeSub(0, 200));
                }
                return LoadedScanView(model);
            }

            model.SerialNo = null;
            ModelState.Remove(model.FullPropertyName(m => m.SerialNo));

            return LoadedScanView(model);
        }

        private const string LastScanKey = "_LastScanKey";

        protected ProductionScan AddScan(string value, ScanResult result, PrdnJobStatus? status = null, string msg = null)
        {
            DateTime nowTime = DateTime.Now;
            
            ProductionScan newScan = new ProductionScan(nowTime, GetCurrentUserID(), value, result, status, msg);

            if ((result != ScanResult.CstItemCreatedAndPrinted) &&  (result != ScanResult.CstItemCreatedNotPrinted)
                && (result != ScanResult.CompletedAndPrinted) && (result != ScanResult.CompletedNotPrinted))
            {
                LastScanModel lastScan = Session[LastScanKey] as LastScanModel;
                if ((lastScan != null) && (lastScan.Value == value) && (lastScan.Result == result))
                {
                    TimeSpan ts = lastScan.ScanDt.Subtract(nowTime);
                    if (ts.Minutes < 2)
                    {
                        lastScan.ScanDt = nowTime;
                        Session[LastScanKey] = lastScan;
                        return newScan;
                    }
                }
            }
            
            PrdnDBContext.ProductionScans.AddObject(newScan);
            PrdnDBContext.SaveChanges();

            Session[LastScanKey] = new LastScanModel { Value = newScan.Value, Result = newScan.Result, ScanDt = newScan.ScanDt };

            return newScan;
        }

        protected IEnumerable<ScanListViewModel> ScanList(DateTime? fromDt=null, DateTime? upToDt=null, string value=null, int? take=null)
        {
            var scans = from s in PrdnDBContext.ProductionScans.Include("Users")
                        join j in PrdnDBContext.ProductionJobs on s.Value equals j.SerialNo into scanJobs
                        from job in scanJobs.DefaultIfEmpty()
                        join i in PrdnDBContext.PrdnInvItems on s.Value equals i.SerialNo into scanItems
                        from item in scanItems.DefaultIfEmpty()
                        orderby s.ScanDt descending
                        select new ScanListViewModel
                        {
                            ID = s.ID,
                            ScanDt = s.ScanDt,
                            Value = s.Value,
                            ResultNum = s.ResultNum,
                            Message = s.Message,
                            UserID = (int)s.User.ID,
                            UserLogin = s.User.Login,
                            JobID = job == null ? (int?)null : (int?)job.ID,
                            CurrentJobStatusStr = job == null ? null : job.StatusStr,
                            ScanJobStatusStr = s.JobStatusStr,
                            SerialNo = item == null ? null : item.SerialNo,
                            InvItemID = item == null ? null : item.InvItemID
                        };

            if (fromDt != null)
            {
                scans = from s in scans
                        where s.ScanDt >= (DateTime)fromDt
                        select s;
            }
            if (upToDt != null)
            {
                scans = from s in scans
                        where s.ScanDt < (DateTime)upToDt
                        select s;
            }
            if (value != null)
            {
                scans = from s in scans
                        where s.Value.StartsWith(value)
                        select s;
            }
            if (take > 0)
            {
                scans = scans.Take((int)take);
            }
            
            return scans.ToList();
        }


        public ActionResult LookupScans(LookupScanViewModel model)
        {
            if (model.IsNotNull(m => m.LookupDt) || model.IsNotNull(m => m.SerialNo))
            {
                DateTime? from = null;
                DateTime? upTo = null;

                if (model.IsNotNull(m => m.LookupDt))
                {
                    DateTime dt = (DateTime)model.LookupDt;
                    from = dt.Date;
                    upTo = dt.Date.AddDays(1);
                }

                model.Scans = ScanList(fromDt: from, upToDt: upTo, value: model.SerialNo);
            }
            else
            {
                model.Scans = null;
            }
            return View(model);
        }

        [CstAuthorize(Groups = "MAKE/CANCEL")]
        public ActionResult CancelJob(int id, string urlReturn)
        {
            return View();
        }

        ///
        // Calendar
        public ActionResult Calendar(string year, string month)
        {
            ProductionCalendar cal = ProductionCalendar.MakeProductionCalendar(year, month, PrdnDBContext);
            if (cal.DateAdjusted)
            {
                return RedirectToAction("Calendar", new { year = cal.Year.ToString(), month = cal.Month.ToString() });
            }

            cal.OrderAction = "Runs";
            cal.RunAction = "Runs";
            cal.AllowEditing = false;
            return View(cal);
        }

        public ActionResult Product(string id)
        {
            ProductLookupModel model = new ProductLookupModel
            {
                ProdTypeList = GetProdTypeList()
            };
            return ProductBase(model, id);
        }

        public ActionResult Pattern(string id)
        {
            return PatternBase(new PatternLookupModel(), id);
        }

        public ActionResult InvItem(string id)
        {
            return InvItemBase(id);
        }

        public ActionResult FindItemSerial(string id)
        {
            return FindItemSerialBase(id);
        }

        public ActionResult ItemLabel(string id, PrinterInfo printerInfo)
        {
            return ItemLabelBase(id, printerInfo);
        }

    }
}
