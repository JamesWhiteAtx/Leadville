using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Telerik.Web.Mvc;
using AutoMapper;
using CST.Prdn.Data;
using CST.Prdn.ViewModels;
using CST.Prdn.ISIS;
using CST.ISIS.Data;
using CST.Security;
using System.IO;
using System.Data.SqlClient;
using System.Data.Common;
using System.Web.Configuration;
using CST.ActionFilters;
using CST.Localization;

namespace CST.Prdn.Controllers
{
    [CstAuthorize(Groups = "SCHED/SCHED")]
    public class JobController : JobControllerBase
    {
        [HttpPost]
        [GridAction(EnableCustomBinding = true)]
        public ActionResult _DeleteJob(GridCommand command, int id)
        {
            var deadJob = (from j in PrdnDBContext.ProductionJobs.Include("PrdnInvItem")
                           where j.ID == id
                           select j).FirstOrDefault();

            if (deadJob != null)
            {
                if (deadJob.IsNotNull(j => j.PrdnInvItem))
                {
                    ModelState.AddModelError("JobID", "This "+LocalStr.Job+" cannot be deleted because an inventory item exists for "+LocalStr.SerialNo+" "+deadJob.SerialNo);
                }
                else
                {
                    EnsurePrdnDBContextOpen();
                    using (var transaction = PrdnDBContext.Connection.BeginTransaction())
                    {
                        try {
                            // alternative is to load attachments, lots of data possibly selected just to delete, or set up cascade deletes, both in EF and DB
                            PrdnDBContext.ExecuteStoreCommand("DELETE FROM FG_PRDN_JOB_ATTACHMENT WHERE FG_PRDN_JOB_ID = :p0", deadJob.ID);

                            PrdnDBContext.DeleteObject(deadJob);
                            PrdnDBContext.SaveChanges();

                            ReSequenceRun(deadJob.RunID);
                            
                            transaction.Commit();
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            ModelState.AddModelError("JobID", ex.Message);
                        }
                    }
               
                }
            }
            //return nothing, rely on grid OnComplete to retrigger binding, thus using the current status filter list
            return View(JobGridList(command, null, null));
        }

        public ActionResult NewJob(string prodCD=null, string urlReturn=null)
        {
            if (urlReturn == null)
            {
                urlReturn = Url.Action("Index", "Home");
            }

            UserSettingsViewModel settings = GetUserSettingsViewModel(false);

            JobSelectRunViewModel model = new JobSelectRunViewModel
            {
                UrlReturn = urlReturn
            };

            Product prod = null;
            if (!String.IsNullOrWhiteSpace(prodCD))
            {
                IQueryable<string> types = GetPrdnProdTypeList().Select(t => t.Code);

                prod = (from p in PrdnDBContext.Products
                        where p.ProdCD == prodCD && types.Contains(p.ProdTypeCD)
                        select p).FirstOrDefault();
            }

            ProductionRun run = null;
            if (settings.IsNotNull(s => s.DefaultRunID))  
            {
                var runQry = from r in PrdnDBContext.ProductionRuns
                             where r.ID == settings.DefaultRunID
                             select r;

                if (prod != null)	{
                    runQry = from r in runQry
                             where r.PrdnType.ProdTypeCD == prod.ProdTypeCD
                             select r;
                }

                run = (from r in runQry select r).FirstOrDefault();
            }

            if (prod != null)
            {
                model.ForProdTypeCD = prod.ProdTypeCD;
                model.ForProdCD = prod.ProdCD;
                model.ForProdDescr = prod.Description;
            }

            if (run != null)
            {
                model.RunID = (int?)run.ID;
                model.RunDescr = run.Description;
                model.OrderNo = run.IfNotNull(r => r.PrdnOrderNo);
                model.ShipDtStr = run.PrdnOrder.ShipDay.ToString(DateFormat.ShortDatePattern);
                model.TypeCD = run.PrdnType.Code;
                model.TypeDescr = run.PrdnType.Description;
                model.ProdTypeCD = run.PrdnType.ProdTypeCD;
                model.ProdTypeDescr = run.PrdnType.ProductType.Description;
            }

            model.SettingsModel = settings;
            return View(model);
        }

        public ActionResult NewRunAddJob(string orderNo, int typeID, string note, string prodCD = null)
        {
            ProductionRun newRun = MakeProductionNewRun(orderNo, typeID, note);
            return RedirectToAction(actionName: "Add", routeValues: new { runID = newRun.ID });
        }

        [HttpGet]
        public ActionResult Add(int runID, string prodCD=null, string urlReturn=null)
        {
            var runInfo = (from r in PrdnDBContext.ProductionRuns  
                          where r.ID == runID
                          select new { 
                              run = r,
                              jobs = r.Jobs.Count()                       
                          }
                         ).FirstOrDefault();

            if (runInfo != null)
            {
                AddPrdnJobViewModel newJob = new AddPrdnJobViewModel();
                newJob.Qty = 1;
                newJob.UrlReturn = urlReturn;
                newJob.loadFromRun(runInfo.run);

                if (!String.IsNullOrWhiteSpace(prodCD))
                {
                    var prod = (from p in PrdnDBContext.Products
                               where p.ProdTypeCD == newJob.ProdTypeCD && p.ProdCD == prodCD
                               select p).FirstOrDefault();
                    if (prod != null)
                    {
                        newJob.ProdCD = prod.ProdCD;
                        newJob.ProdDescr = prod.Description;
                        newJob.ParentProdCD = prod.ParentProdCD;
                        LoadViewJobProdLists(newJob);
                    }
                }

                newJob.EditWorksheet = new WorksheetEditViewModel(newJob.ProdTypeCD) { Editable = false };

                newJob.DropShip = false;
                
                //newJob.CreatedDt = DateTime.Now;
                newJob.CreatedUserID = GetCurrentUserID();
                newJob.CreatedUserLogin = GetCurrentUserLogin();

                loadViewJobViewAttachments(newJob, "A,S");

                AssignViewJobLookupListsFuncs(newJob);

                newJob.CanEditRun = true;

                ModelState.Clear();
                return View(newJob);
            }
            else
            {
                return ErrMsgView("Sorry - Run ID " + runID.ToString() + " is not valid");
            }
        }

        [HttpPost]
        public ActionResult Add(AddPrdnJobViewModel newViewJob, IEnumerable<HttpPostedFileBase> uploadedFiles)
        {
            ProductionJob addedJob;

            //ModelState.AddModelError("", "Test Add Error");

            ActionUpdateResult result = AddNewJobFromViewModel(newViewJob, uploadedFiles, out addedJob, AddExtraJobsForQty);

            if (result.Type == ActionUpdateType.Success)
            {
                return RedirectToAction(actionName: "Jobs", controllerName: "Schedule",
                        routeValues: new {
                            id = addedJob.RunID,
                            page = LastPageForRun(addedJob.RunID)
                        });
            }
            else if (result.Type == ActionUpdateType.Invalid)
            {
                return View(newViewJob);
            } else { //if (result ==  ActionSaveResult.Exception {
                return ErrMsgView("Error Saving Job: "+result.Message);
            }
        }

        protected void AddExtraJobsForQty(EditPrdnJobViewModel newViewJob, ProductionJob job)
        {
            AddPrdnJobViewModel addedViewJob = newViewJob as AddPrdnJobViewModel;
            if (addedViewJob.IfNotNull(j => j.Qty) > 1)
            {
                CopyJob(job, addedViewJob.Qty-1);
                
                PrdnDBContext.SaveChanges();
            }
        }

        protected bool JobModelValidate(EditPrdnJobViewModel viewJob) 
        {
            bool isValid = (ModelState.IsValid);
            if (isValid)
            {
                if (!String.IsNullOrEmpty(viewJob.ProdCD))
                {
                    var prodObj = (from p in PrdnDBContext.Products
                                   where (p.ProdTypeCD == viewJob.ProdTypeCD && p.ProdCD == viewJob.ProdCD)
                                   select new { p.ProdCD })
                                   .FirstOrDefault();
                    if (prodObj == null)
                    {
                        ModelState.AddModelError(viewJob.FullPropertyName(j => j.ProdCD), SystemExtensions.Sentence(viewJob.ProdCD, LocalStr.verbIsNot, LocalStr.valid));
                        isValid = false;
                    }
                }
            }
            if (isValid)
            {
                if ((viewJob.CustID == @PrdnDataHelper.PrdnCustIDCST) && (!String.IsNullOrEmpty(viewJob.OrderNo)))
                {
                    isValid = JobModelValidateCstOrder(viewJob);
                }
            }
            if (isValid)
            {
                isValid = JobModelValidateOtherJobs(viewJob);
            }

            return isValid;
        }

        protected bool JobModelValidateCstOrder(EditPrdnJobViewModel viewJob)
        {
            var lnQry = from l in PrdnDBContext.CstOrderLines
                        where l.OrderLine == viewJob.OrderLine
                        select new
                        {
                            l.OrderNo,
                            l.OrderLine,
                            l.OrderLineID,
                            l.ProdCD
                        };

            var ordMatch = (from o in PrdnDBContext.CstOrders
                            where o.OrderNo == viewJob.OrderNo
                            join l in lnQry on o.OrderNo equals l.OrderNo into temp
                            from ln in temp.DefaultIfEmpty()
                            select new
                            {
                                o.OrderNo,
                                line = ln
                            }).FirstOrDefault();

            bool isValid = false;
            if (ordMatch == null)
            {
                ModelState.AddModelError(viewJob.FullPropertyName(j => j.OrderNo), SystemExtensions.Sentence(LocalStr.OrderNo, viewJob.OrderNo, LocalStr.verbIsNot, LocalStr.valid));
            }
            else if (ordMatch.line == null)
            {
                ModelState.AddModelError(viewJob.FullPropertyName(j => j.OrderLineInt), SystemExtensions.Sentence(LocalStr.OrderNo, viewJob.OrderNo, LocalStr.OrderLine, viewJob.OrderLineInt.ToString(), LocalStr.verbIsNot, LocalStr.valid));
            }
            else if (ordMatch.line.ProdCD != viewJob.ProdCD)
            {
                ModelState.AddModelError(viewJob.FullPropertyName(j => j.ProdCD), SystemExtensions.Sentence(LocalStr.OrderNo, viewJob.OrderNo, LocalStr.OrderLine, viewJob.OrderLineInt.ToString(), LocalStr.ProductCD, viewJob.ProdCD, LocalStr.doesNotMatch, ordMatch.line.ProdCD));
            }
            else 
            {
                isValid = true;
                viewJob.OrderLineID = ordMatch.line.OrderLineID;
            }
            return isValid;
        }

        protected bool JobModelValidateOtherJobs(EditPrdnJobViewModel viewJob)
        {
            bool isValid = true;
            string dbCancStr = PrdnJobStatus.Canceled.DbValStr();

            using (PrdnEntities PrdnDBContext = new PrdnEntities())
            {
                if (!String.IsNullOrEmpty(viewJob.CstRequestID))
                {
                    var jobReqQry = from j in PrdnDBContext.ProductionJobs
                                    where j.CstRequestID == viewJob.CstRequestID
                                    && j.CustID == viewJob.CustID
                                    && j.StatusStr != dbCancStr
                                    select j;

                    if (viewJob.ID != null)
                    {
                        jobReqQry = from j in jobReqQry
                                    where j.ID != viewJob.ID
                                    select j;
                    }

                    var reqJob = (from j in jobReqQry
                                       select new
                                       {
                                           j.ID,
                                           j.SerialNo,
                                           j.Run.PrdnOrderNo,
                                           j.Run.PrdnType.Code,
                                           j.StatusStr,
                                       }).FirstOrDefault();

                    if (reqJob != null)
                    {
                        ModelState.AddModelError(viewJob.FullPropertyName(j => j.CstRequestID),
                            SystemExtensions.Sentence(LocalStr.Job, reqJob.PrdnOrderNo + reqJob.Code, LocalStr.JobID, reqJob.ID.ToString(), LocalStr.SerialNo, reqJob.SerialNo, LocalStr.isAlreadyAssignedTo, LocalStr.RequestID, viewJob.CstRequestID));
                        isValid = false;
                    }
                }

                if (isValid && !String.IsNullOrEmpty(viewJob.OrderNo) && (viewJob.OrderLine != null) && !String.IsNullOrEmpty(viewJob.ProdCD))
                {
                    var jobOrdQry = from j in PrdnDBContext.ProductionJobs
                                    where j.OrderNo == viewJob.OrderNo
                                    && j.CustID == viewJob.CustID
                                    && j.OrderLine == viewJob.OrderLine
                                    && j.ProdCD == viewJob.ProdCD
                                    && j.StatusStr != dbCancStr
                                    select j;

                    if (viewJob.ID != null)
                    {
                        jobOrdQry = from j in jobOrdQry
                                    where j.ID != viewJob.ID
                                    select j;
                    }

                    var ordJob = (from j in jobOrdQry
                                       select new
                                       {
                                           j.ID,
                                           j.SerialNo,
                                           j.Run.PrdnOrderNo,
                                           j.Run.PrdnType.Code,
                                           j.StatusStr,
                                       }).FirstOrDefault();

                    if (ordJob != null)
                    {
                        ModelState.AddModelError(viewJob.FullPropertyName(j => j.CstRequestID),
                            SystemExtensions.Sentence(LocalStr.Job, ordJob.PrdnOrderNo + ordJob.Code, LocalStr.JobID, ordJob.ID.ToString(), LocalStr.SerialNo, ordJob.SerialNo,
                                LocalStr.isAlreadyAssignedTo, LocalStr.OrderNo, viewJob.OrderNo, LocalStr.OrderLine, viewJob.OrderLineInt.ToString(), LocalStr.ProductCD, viewJob.ProdCD));
                        isValid = false;
                    }
                }
            }
            return isValid;
        }

        protected void AssignCustSpecific(EditPrdnJobViewModel viewJob)
        { 
            if (viewJob.CustID == PrdnDataHelper.PrdnCustIDCST)
            {
                AssignCstpecific(viewJob);
            }
        }
        protected void AssignCstpecific(EditPrdnJobViewModel viewJob)
        {
            viewJob.OrderTotal = 0;
            if (!String.IsNullOrEmpty(viewJob.OrderNo))
            {
                if (viewJob.OrderLineID == null)
                {
                    viewJob.OrderTotal = PrdnDBContext.GetCstOrdTotal(viewJob.OrderNo) ?? 0;
                }
                else
                {
                    var lnQry = from l in PrdnDBContext.CstOrderLines
                                where l.OrderLineID == viewJob.OrderLineID
                                select new
                                {
                                    l.OrderNo,
                                    l.OrderLine,
                                    l.OrderLineID,
                                    l.ProdCD
                                };

                    var ordInfo = (from o in PrdnDBContext.CstOrders
                                   where o.OrderNo == viewJob.OrderNo
                                   join l in lnQry on o.OrderNo equals l.OrderNo into temp
                                   from ln in temp.DefaultIfEmpty()
                                   select new
                                   {
                                       o.OrderNo,
                                       OrderTot = o.OrderLines.Sum(l => l.LineTotal) + (o.Tax ?? 0) + (o.Transport ?? 0) + (o.Shipping ?? 0),
                                       line = ln
                                   }).FirstOrDefault();

                    if (ordInfo != null)
                    {
                        viewJob.OrderTotal = ordInfo.OrderTot;
                        viewJob.OrderLine = ordInfo.line.IfNotNull(l => l.OrderLine);
                    }
                }
            }
        }

        private void ReSequenceRun(decimal runID) 
        {
            PrdnDBContext.ExecuteStoreCommand(@"BEGIN FG_SEQ_PRDN_JOB(:p0); END;", runID);
        }

        private ActionUpdateResult AddNewJobFromViewModel(EditPrdnJobViewModel newViewJob, IEnumerable<HttpPostedFileBase> uploadedFiles, out ProductionJob addedJob,
            Action<EditPrdnJobViewModel, ProductionJob> extraSaveProc=null)
        {
            addedJob = null;

            ProcessAttacherForSave(newViewJob.Attacher, uploadedFiles);

            JobModelValidate(newViewJob);
            if (ModelState.IsValid)
            {
                EnsurePrdnDBContextOpen();
                using (var transaction = PrdnDBContext.Connection.BeginTransaction())
                {
                    try
                    {
                        newViewJob.RunSeqNo = PrdnDBContext.NextRunSequence(newViewJob.RunID);
                        newViewJob.SerialNo = PrdnDBContext.NextSerialStr();

                        ProductionJob newJob = new ProductionJob(DateTime.Now, GetCurrentUserID());
                        
                        Mapper.Map<EditPrdnJobViewModel, ProductionJob>(newViewJob, newJob);

                        newJob.UpdateStatus((PrdnJobStatus)newViewJob.EditStatus, GetCurrentUserID());

                        PrdnDBContext.ProductionJobs.AddObject(newJob);
                        
                        UpdateJobWorksheet(newJob, newViewJob);

                        UpdateJobAttachments(newJob, newViewJob.Attacher);

                        PrdnDBContext.SaveChanges();
                        if (extraSaveProc != null) {
                            extraSaveProc(newViewJob, newJob);
                        }

                        if ((newViewJob.EditRunSeqNo != null) && (newJob.RunSeqNo != newViewJob.EditRunSeqNo))
                        {
                            InsertRunSeqs(PrdnDBContext, newJob, (int)newViewJob.EditRunSeqNo);    
                        }
                        ReSequenceRun(newJob.RunID);

                        transaction.Commit();

                        addedJob = newJob;

                        return new ActionUpdateResult(ActionUpdateType.Success);
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        return new ActionUpdateResult(ActionUpdateType.Exception, ex.Message);
                    }
                }
            }
            else
            {
                loadViewJobForPost(newViewJob);
                return new ActionUpdateResult(ActionUpdateType.Invalid);
            }
        }

        protected void CopyJob(ProductionJob sourceJob, int qty)
        {
            if (qty > 0)
            {
                ProductionJob copyJob = null;
                for (int i = 0; i < qty; i++)
                {
                    copyJob = MakeJobCopy(sourceJob);
                    PrdnDBContext.ProductionJobs.AddObject(copyJob);
                    PrdnDBContext.SaveChanges();
                }
            }
        }

        protected ProductionJob MakeJobCopy(ProductionJob sourceJob)
        {
            ProductionJob copyJob = (ProductionJob)sourceJob.Clone();
            copyJob.SetCreatedDt(DateTime.Now);
            copyJob.RunSeqNo = PrdnDBContext.NextRunSequence(copyJob.RunID);
            copyJob.SerialNo = PrdnDBContext.NextSerialStr();

            Worksheet copyWS = PrdnDBContext.CloneWorksheet(sourceJob.Worksheet, PrdnIsisEntities.GetNextWorksheetID());
            copyJob.AssignWorksheet(copyWS);

            foreach (ProductionJobAttachment att in sourceJob.Attachments)
            {
                ProductionJobAttachment attClone = (ProductionJobAttachment)att.Clone();
                attClone.ID = 0;
                attClone.JobID = 0;
                copyJob.Attachments.Add(attClone);
            }
            return copyJob;
        }

        [HttpGet]
        public ActionResult Edit(int id, string urlReturn)
        {
            EditPrdnJobViewModel viewJob = GetEditJobViewModel(id, urlReturn);
            if (viewJob != null)
            {
                AssignViewJobLookupListsFuncs(viewJob);

                AssignCustSpecific(viewJob);

                loadViewJobViewAttachments(viewJob, "A,S");

                viewJob.CanEditRun = false;

                return View(viewJob);
            } else {
                return ErrMsgView(SystemExtensions.Sentence(LocalStr.Job, LocalStr.ID, id.ToString(), LocalStr.verbIsNot, LocalStr.valid));
            }
        }

        [HttpPost]
        public ActionResult Edit(EditPrdnJobViewModel editViewJob, IEnumerable<HttpPostedFileBase> uploadedFiles)
        {
            ProcessAttacherForSave(editViewJob.Attacher, uploadedFiles);

            ProductionJob editJob = (from j in PrdnDBContext.ProductionJobs.Include("PrdnInvItem")
                                     where j.ID == editViewJob.ID
                                     select j).FirstOrDefault();

            if (editJob == null)
            {
                return ErrMsgView(SystemExtensions.Sentence("Error Saving Job: ", LocalStr.JobID, editViewJob.IfNotNull(j => j.ID).ToString(), LocalStr.verbIsNot, LocalStr.valid));
            }

            if (editViewJob.EditStatus == PrdnJobStatus.New)
            {
                editViewJob.EditStatus = editJob.Status;
            }
            else 
            if ((editJob.Status != editViewJob.EditStatus) && (!EditPrdnJobViewModel.FromStatusTo(editJob.Status, editViewJob.EditStatus)))
            {
                ModelState.AddModelError(editViewJob.FullPropertyName(j => j.EditStatus), 
                    SystemExtensions.Sentence(LocalStr.Status, LocalStr.Change, LocalStr.to, editViewJob.EditStatus.Description(),
                    LocalStr.verbIsNot, LocalStr.valid));
            }

            JobModelValidate(editViewJob);

            if (ModelState.IsValid)
            {
                if (editJob.IsNotNull(j => j.PrdnInvItem))
                {
                    return ErrMsgView(SystemExtensions.Sentence("Error Saving Job: ", editJob.PrdnInvItem.SerialNo, " exists in inventory."));
                }
                EnsurePrdnDBContextOpen();
                using (var transaction = PrdnDBContext.Connection.BeginTransaction())
                {
                    try
                    {
                        Mapper.Map<EditPrdnJobViewModel, ProductionJob>(editViewJob, editJob);

                        if (editJob.Status != editViewJob.EditStatus)
                        {
                            editJob.UpdateStatus((PrdnJobStatus)editViewJob.EditStatus, GetCurrentUserID());
                        }

                        UpdateJobWorksheet(editJob, editViewJob); 
                        UpdateJobAttachments(editJob, editViewJob.Attacher);
                        PrdnDBContext.SaveChanges();

                        if ((editViewJob.EditRunSeqNo != null) && (editJob.RunSeqNo != editViewJob.EditRunSeqNo))
                        {
                            InsertRunSeqs(PrdnDBContext, editJob, (int)editViewJob.EditRunSeqNo);
                            ReSequenceRun(editJob.RunID);
                            PrdnDBContext.SaveChanges();
                        }

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        return ErrMsgView("Error Saving Job: " + ex.Message);
                    }
                }

                return RedirectIfLocal(editViewJob.UrlReturn, 
                    () => RedirectToAction(actionName: "Jobs", controllerName: "Schedule", routeValues: new { id = editViewJob.RunID }));
                //if (this.IsLocalUrl(editViewJob.UrlReturn)) { return Redirect(editViewJob.UrlReturn);} else {return RedirectToAction(actionName: "Jobs", controllerName: "Schedule", routeValues: new { RunID = editViewJob.RunID });}
            }
            else
            {
                //assignViewJobLookupListsFuncs(editViewJob);    //ProcessAttacherForPost(editViewJob.Attacher);
                loadViewJobForPost(editViewJob);
                return View(editViewJob);
            }
        }

        protected void loadViewJobForPost(EditPrdnJobViewModel viewJob)
        {
            LoadViewJobProdLists(viewJob);
            AssignViewJobLookupListsFuncs(viewJob);
            if (viewJob.IsNotNull(j => j.EditWorksheet))
            {
                string modKey = viewJob.FullPropertyName(m => m.EditWorksheet.Editable);
                ModelState.Remove(modKey);
                viewJob.EditWorksheet.Editable = false;
            }
            ProcessAttacherForPost(viewJob.Attacher);
        }

        [HttpGet]
        public ActionResult ViewJob(int id, string urlReturn)
        {
            return ViewJobBase(id, "A,S", urlReturn);
        }

        [HttpPost]
        public ActionResult ViewJob(ViewPrdnJobViewModel viewJob, IEnumerable<HttpPostedFileBase> uploadedFiles)
        {
            return ViewJobBase(viewJob, uploadedFiles);
        }

        [HttpGet]
        public ActionResult ScheduleRequest(string reqID, string urlReturn, int? runID)
        {
            // get Request
            var request = (from r in PrdnDBContext.Requests.Include("CstOrderLine").Include("Product").Include("Product.ProductType")
                           where r.ID == reqID
                           select r).FirstOrDefault();

            if (request == null)
            {
                return ErrMsgView("request ID '" + reqID + "' is not valid.");
            }
            else if ((request.Status != RequestStatus.NEW) && (request.Status != RequestStatus.PROCESSING))
            {
                return ErrMsgView("request ID '" + reqID + "' is not new or processing.");
            }
            
            if (runID == null)
            {
                UserSettingsViewModel settings = GetUserSettingsViewModel(true);
                runID = (int?)settings.IfNotNull(s => s.DefaultRunID);
            }

            CST.Prdn.Data.ProductionRun run = null;
            if (runID != null)  // is this run ID the correct product type?
            {
                run = (from r in PrdnDBContext.ProductionRuns
                       where r.ID == runID
                       select r)
                       .FirstOrDefault();
            }
        
            if ((run == null) || (run.PrdnType.ProdTypeCD != request.Product.ProdTypeCD)) 
            {
                RequestScheduleViewModel model = new RequestScheduleViewModel
                {
                    RequestID = reqID,
                    OrderNo = run.IfNotNull(r => r.PrdnOrderNo),
                    ProdTypeCD = request.Product.ProdTypeCD,
                    ProdTypeDescr = request.Product.ProductType.Description,
                    ProdCD = request.ProdCD,
                    ProdDescr = request.PartDescr,
                    UrlReturn = urlReturn
                };

                return RedirectToAction(actionName: "RequestRun", routeValues: model);
            }

            // job view model
            SchedulePrdnJobViewModel viewJob = new SchedulePrdnJobViewModel();

            viewJob.UrlReturn = urlReturn;
            //viewJob.LookupRunModel = new PrdnRunLookupModel();

            viewJob.loadFromRun(run);

            viewJob.CustID = PrdnDataHelper.PrdnCustIDCST;
            viewJob.PriorityID = PrdnDataHelper.PrdnPriorityIDDefault;

            if (viewJob.ProdTypeCD != request.Product.ProdTypeCD)
            {
                return ErrMsgView("Sorry - The Request product type (" + request.Product.ProdTypeCD +
                    ") does not match the target Runs's product type (" + viewJob.ProdTypeCD + ")");
            }

            viewJob.ProdCD = request.ProdCD;
            viewJob.ProdDescr = request.Product.Description;
            viewJob.ParentProdCD = request.Product.ParentProdCD;

            viewJob.CustLocation = request.RequestDeptID;
            viewJob.ShipMethodCD = request.ShipBranchVia;
            viewJob.PackingListNote = request.RequestComment;

            viewJob.CreatedUserLogin = GetCurrentUserLogin();

            viewJob.OrderNo = request.OrderNo;
            viewJob.OrderLine = request.OrderLine;
            viewJob.OrderLineID = request.CstOrderLine.IfNotNull(l => l.OrderLineID);

            viewJob.CstRequestID = request.ID;
            viewJob.SpecialWSDescr = request.SpecialWSDescr;

            loadJobViewForOrder(viewJob);
            LoadViewJobProdLists(viewJob);
            AssignViewJobLookupListsFuncs(viewJob);
            loadViewJobViewAttachments(viewJob, "A,S");

            viewJob.CustName = GetCustName(viewJob.CustID);
            viewJob.CustLocName = GetCustLocName(viewJob.CustID);

            if (viewJob.CstRequestID != null)
            {
                viewJob.EditWorksheet = new WorksheetEditViewModel(viewJob.ProdTypeCD) { Editable = false };
                viewJob.EditWorksheet.LoadFromRequest(viewJob.CstRequestID);
                overrideViewJobProdOpts(viewJob);

                var fileName = (from a in PrdnDBContext.RequestAttachments
                                where a.ID == viewJob.CstRequestID
                                select a.FileName).FirstOrDefault();

                viewJob.HasAttachment = (!String.IsNullOrEmpty(fileName));
            }

            viewJob.CanEditRun = true;
            return View(viewJob);
        }

        [HttpPost]
        public ActionResult ScheduleRequest(SchedulePrdnJobViewModel newViewJob, IEnumerable<HttpPostedFileBase> uploadedFiles)
        {
            ProductionJob addedJob;

            //ModelState.AddModelError("", "Test Schedule Error");

            ActionUpdateResult result = AddNewJobFromViewModel(newViewJob, uploadedFiles, out addedJob, UpdateScheduledRequest);

            if (result.Type == ActionUpdateType.Success)
            {
                return RedirectToAction(actionName: "Jobs", controllerName: "Schedule",
                        routeValues: new
                        {
                            id = addedJob.RunID,
                            page = LastPageForRun(addedJob.RunID)
                        });
            }
            else if (result.Type == ActionUpdateType.Invalid)
            {
                return View(newViewJob);
            }
            else
            { //if (result ==  ActionSaveResult.Exception {
                return ErrMsgView("Error Saving Job");
            }
        }

        public ActionResult RequestRun(RequestScheduleViewModel model)
        {
            if (model.OrderNo != null)
            {
                var runs = (from r in PrdnDBContext.ProductionRuns
                            where r.PrdnOrderNo == model.OrderNo && r.PrdnType.ProdTypeCD == model.ProdTypeCD
                            select r).ToList();

                model.PossibleRuns = new SelectList(runs.ToList(), "ID", "RunDescr");
            }

            model.SettingsModel = GetUserSettingsViewModel(true);

            return View(model);
        }

        public ActionResult RequestNewRun(string id, string urlReturn, string orderNo, int typeID, string note)
        {
            ProductionRun newRun = MakeProductionNewRun(orderNo, typeID, note);
            return RedirectToAction(actionName: "ScheduleRequest", routeValues: new { reqID = id, urlReturn = urlReturn, runID = newRun.ID });
        }


        //[HttpPost]
        //public ActionResult RequestRun()
        //{
        //    RequestScheduleViewModel model = null;
        //    if (TryValidateModel(model)) {
        //        return RedirectToAction(actionName: model.RouteAction, controllerName: model.RouteController, 
        //                routeValues: new
        //                {
        //                    requestID = model.RequestID,
        //                    urlReturn = model.UrlReturn,
        //                    runID = model.RunID
        //                });
        //    } 
        //    else {
        //        model.LookupRunModel = new PrdnRunLookupModel();
        //        model.NewRunModel = new NewPrdnRunViewModel(Session, model.RouteController, model.RouteAction, 
        //            new RequestCreateRunActionParm { RequestID = model.RequestID, UrlReturn = model.UrlReturn });
        //        return View(model);
        //    }
        //}

        protected void UpdateScheduledRequest(EditPrdnJobViewModel newViewJob, ProductionJob job)
        {
            if (job.CstRequestID == null) { 
                return; 
            }

            var request = (from r in PrdnDBContext.Requests
                            where r.ID == job.CstRequestID
                            select r).FirstOrDefault();

            if (request != null)
            {
                CST.Prdn.Data.ProductionJobAttachment newAttachment = null;

                request.Status = RequestStatus.SCHEDULED;
                request.ScheduledJobID = job.ID;
                request.ScheduledDt = DateTime.Now;
                request.ScheduledUserID = GetCurrentUserID();

                var reqAtt = (from a in PrdnDBContext.RequestAttachments
                                where a.ID == job.CstRequestID && a.Attachment != null
                                select new
                                {   a.ID,
                                    a.FileName,
                                    MimeContentType = a.MimeType.ContentType,
                                    MimeSubType = a.MimeTypeCD,
                                    a.MimeType,
                                }).FirstOrDefault();

                if (reqAtt != null)
                {
                    string requestAttachmentTypeID = WebConfigurationManager.AppSettings["RequestAttachmentTypeID"];
                    decimal attTypeID = Convert.ToDecimal(requestAttachmentTypeID);

                    newAttachment = new CST.Prdn.Data.ProductionJobAttachment
                    {   //JobID = editJob.ID,
                        FileName = reqAtt.FileName,
                        MimeType = CST.ISIS.Data.Attachment.ConcatMimeType(reqAtt.MimeContentType, reqAtt.MimeSubType),
                        Description = "Original Request Attachment",
                        Attachment = null,
                        AttachmentTypeID = attTypeID
                    };
                    job.Attachments.Add(newAttachment);
                }

                PrdnDBContext.SaveChanges();

                if (newAttachment != null) {
                    PrdnDBContext.ExecuteStoreCommand(
@"UPDATE FG_PRDN_JOB_ATTACHMENT J SET J.FG_ATTACH_DATA = (SELECT R.FG_ATTACHMENT FROM FG_REQ_PART R WHERE R.FG_REQUEST_ID = :p0) WHERE J.FG_PRDN_JOB_ATT_ID = :p1"
                    , reqAtt.ID, newAttachment.ID);
                }

            }
        }

        protected void UpdateJobWorksheet(ProductionJob editJob, EditPrdnJobViewModel viewJob)
        {
            bool anyWSRows = viewJob.NotNullAny(n => n.IfNotNull(x => x.EditWorksheet).IfNotNull(x => x.WorksheetOpts));

            Worksheet worksheet = editJob.Worksheet;
            if (worksheet != null) // clear the existing worksheet chars and comps
            {
                PrdnDBContext.ExecuteStoreCommand("DELETE FROM FG_WORKSHEET_CHAR WHERE FG_WORKSHEET_ID = :p0", worksheet.ID);
                PrdnDBContext.ExecuteStoreCommand("DELETE FROM FG_WORKSHEET_COMP WHERE FG_PARENT_COMP_PROD_CD IS NOT NULL AND FG_WORKSHEET_ID = :p0", worksheet.ID);
                PrdnDBContext.ExecuteStoreCommand("DELETE FROM FG_WORKSHEET_COMP WHERE FG_WORKSHEET_ID = :p0", worksheet.ID);

                if (anyWSRows)
                {
                    editJob.AssignWorksheet(worksheet);
                }
                else
                {
                    PrdnDBContext.DeleteObject(worksheet);
                }

            }
            else if (anyWSRows)
            {
                worksheet = new Worksheet { ID = PrdnIsisEntities.GetNextWorksheetID() };
                editJob.AssignWorksheet(worksheet);
            }

            if (anyWSRows)
            {
                viewJob.EditWorksheet.WorksheetOpts.ForEach(o => o.AssignTypeProps());

                foreach (var opt in viewJob.EditWorksheet.WorksheetOpts.Where(o => (o.Type == OptionType.Component) && o.IsRoot))
                {
                    WorksheetComp comp = Mapper.Map<WorksheetOpt, WorksheetComp>(opt);
                    worksheet.WorksheetComps.Add(comp);
                }
                foreach (var opt in viewJob.EditWorksheet.WorksheetOpts.Where(o => (o.Type == OptionType.Component) && !o.IsRoot))
                {
                    WorksheetComp comp = Mapper.Map<WorksheetOpt, WorksheetComp>(opt);
                    worksheet.WorksheetComps.Add(comp);
                }
                foreach (var opt in viewJob.EditWorksheet.WorksheetOpts.Where(o => o.Type == OptionType.Characteristic))
                {
                    WorksheetChar chr = Mapper.Map<WorksheetOpt, WorksheetChar>(opt);
                    worksheet.WorksheetChars.Add(chr);
                }
            }
        }

        private CST.Prdn.Data.ProductionRun NextRunForType(string prodTypeCD)
        {
            DateTime today = DateTime.Today;

            var nextOrdNo = from o in PrdnDBContext.ProductionOrders
                            where o.ShipDay > DateTime.Today
                            group o by 1 into g
                            select g.Min(x => x.OrderNo);

            var nextRun = (from r in PrdnDBContext.ProductionRuns
                           where nextOrdNo.Contains(r.PrdnOrderNo)
                             && r.PrdnType.ProdTypeCD == prodTypeCD
                           select r)
                           .FirstOrDefault();
            return nextRun;
        }

        public ActionResult _JobSeqSwitch(int id, int seqFrom, int seqTo)
        {
            if ((seqFrom == seqTo) || ((seqTo) < 1))
            {
                return Json(new { result = false }, JsonRequestBehavior.AllowGet);
            }

            var jobFrom = (from j in PrdnDBContext.ProductionJobs
                           where j.ID == id
                           select j).FirstOrDefault();

            if (jobFrom == null)
            {
                return Json(new { result = false, message = SystemExtensions.Sentence(LocalStr.JobID, id.ToString(), LocalStr.verbIsNot, LocalStr.valid)}, JsonRequestBehavior.AllowGet);
            }

            if (jobFrom.RunSeqNo != seqFrom)
            {
                return Json(new { result = false, message = SystemExtensions.Sentence(LocalStr.Job, LocalStr.SeqNo, LocalStr.verbIsNot, seqFrom.ToString()) }, JsonRequestBehavior.AllowGet);
            }

            var maxSeq = (from j in PrdnDBContext.ProductionJobs
                          where j.RunID == jobFrom.RunID 
                          select j.RunSeqNo).Max();

            if ((seqTo) > maxSeq)
            {
                return Json(new { result = false }, JsonRequestBehavior.AllowGet);
            }

            var jobTo = (from j in PrdnDBContext.ProductionJobs
                         where j.RunID == jobFrom.RunID && j.RunSeqNo == seqTo
                         select j).FirstOrDefault();

            if (jobTo == null)
            {
                return Json(new { result = false, message = SystemExtensions.Sentence(LocalStr.Job, LocalStr.SeqNo, seqTo.ToString(), LocalStr.verbIsNot, LocalStr.valid) }, JsonRequestBehavior.AllowGet);
            }

            EnsurePrdnDBContextOpen();
            using (var transaction = PrdnDBContext.Connection.BeginTransaction())
            {
                try
                {
                    SwitchRunSeqs(PrdnDBContext, jobFrom, jobTo);

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return Json(new { result = false, message = ex.Message}, JsonRequestBehavior.AllowGet);
                }
            }

            var jsonObj = new { result = true };
            return Json(jsonObj, JsonRequestBehavior.AllowGet);
        }

        protected void SwitchRunSeqs(PrdnEntities prdnDBContext, ProductionJob jobFrom, ProductionJob jobTo)
        {
            decimal seqFrom = jobFrom.RunSeqNo;
            decimal seqTo = jobTo.RunSeqNo;

            jobFrom.RunSeqNo = -99;
            prdnDBContext.SaveChanges();

            jobTo.RunSeqNo = seqFrom;
            prdnDBContext.SaveChanges();

            jobFrom.RunSeqNo = seqTo;
            prdnDBContext.SaveChanges();
        }

        protected void InsertRunSeqs(PrdnEntities prdnDBContext, ProductionJob jobFrom, int targSeqTo)
        {
            if (jobFrom.RunSeqNo > targSeqTo)
            {
                InsertRunSeqBefore(prdnDBContext, jobFrom, targSeqTo);
            }
            else if (jobFrom.RunSeqNo < targSeqTo)
            {
                InsertRunSeqAfter(prdnDBContext, jobFrom, targSeqTo);
            }
        }

        protected void InsertRunSeqBefore(PrdnEntities prdnDBContext, ProductionJob jobFrom, int targSeqTo)
        {
            var targJobs = (from j in PrdnDBContext.ProductionJobs
                            where j.RunID == jobFrom.RunID
                            where j.RunSeqNo > targSeqTo - 1 && j.RunSeqNo <= targSeqTo
                            orderby j.RunSeqNo
                            select new { j.RunSeqNo }).ToList();

            if (targJobs.Count < 1)
            {
                throw new Exception(SystemExtensions.Sentence(LocalStr.Job, LocalStr.SeqNo, targSeqTo.ToString(), LocalStr.verbIsNot, LocalStr.valid));
            }

            var targJob = targJobs.OrderBy(j => j.RunSeqNo).Last();

            if (targJobs.Count == 1)
            {
                jobFrom.RunSeqNo = targJob.RunSeqNo - 0.5M;
            }
            else
            {
                var nextLowJob = targJobs.Where(j => j.RunSeqNo < targJob.RunSeqNo).OrderBy(j => j.RunSeqNo).Last();
                decimal gap = targJob.RunSeqNo - nextLowJob.RunSeqNo;
                jobFrom.RunSeqNo = nextLowJob.RunSeqNo + gap + (gap/2);
            }
            prdnDBContext.SaveChanges();
        }

        protected void InsertRunSeqAfter(PrdnEntities prdnDBContext, ProductionJob jobFrom, int targSeqTo)
        {
            var targJobs = (from j in PrdnDBContext.ProductionJobs
                            where j.RunID == jobFrom.RunID
                            where j.RunSeqNo < targSeqTo + 1 && j.RunSeqNo >= targSeqTo
                            orderby j.RunSeqNo
                            select new { j.RunSeqNo }).ToList();

            if (targJobs.Count < 1)
            {
                throw new Exception(SystemExtensions.Sentence(LocalStr.Job, LocalStr.SeqNo, targSeqTo.ToString(), LocalStr.verbIsNot, LocalStr.valid));
            }

            var targJob = targJobs.OrderBy(j => j.RunSeqNo).First();

            if (targJobs.Count == 1)
            {
                jobFrom.RunSeqNo = targJob.RunSeqNo + 0.5M;
            }
            else
            {
                var nextHiJob = targJobs.Where(j => j.RunSeqNo > targJob.RunSeqNo).OrderBy(j => j.RunSeqNo).First();
                decimal gap = nextHiJob.RunSeqNo - targJob.RunSeqNo;
                jobFrom.RunSeqNo = nextHiJob.RunSeqNo + gap + (gap / 2);
            }
            prdnDBContext.SaveChanges();
        }

        public ActionResult _JobSeqMove(int id, int seq, int move)
        {
            return _JobSeqSwitch(id, seq, seq + move);
        }

        public ActionResult _JobSeqDn(int id, int seq)
        {
            return _JobSeqMove(id, seq, 1);
        }

        public ActionResult _JobSeqUp(int id, int seq)
        {
            return _JobSeqMove(id, seq, -1);
        }

        public ActionResult Attachment(string id)
        {
            decimal deciID = decimal.Parse(id);

            var att = (from a in PrdnDBContext.ProductionJobAttachments
                       where a.ID == deciID
                       select new ImageFile
                       {
                           ID = deciID,
                           Description = a.FileName,
                           FileName = a.FileName,
                           MimeType = a.MimeType,
                           Data = a.Attachment
                       }).FirstOrDefault();

            if (att != null)
            {
                if (!att.IsNotNull(a => a.Data))
                {
                    return ErrMsgView("Error Displaying Job Attachment: No attachment data.");
                }
                else
                {
                    return ViewAttachment(att);
                }
            }
            else
            {
                return ErrMsgView("Error Displaying Job Attachment "+ id +". Attachment was no found.");
            }
        }

        public ActionResult FindItemSerial(string id)
        {
            return FindItemSerialBase(id);
        }

    }
}
