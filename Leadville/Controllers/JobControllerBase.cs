using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AutoMapper;
using CST.ISIS.Data;
using CST.Prdn.ISIS;
using CST.Prdn.Data;
using CST.Prdn.ViewModels;
using CST.Localization;

namespace CST.Prdn.Controllers
{
    public class JobControllerBase : CstControllerBase
    {
        protected void loadJobViewForOrder(EditPrdnJobViewModel viewJob)
        {
            if (viewJob == null)
            {
                return;
            }

            if (!String.IsNullOrEmpty(viewJob.OrderNo))
            {
                OrderShipToInfo info = PrdnDBContext.GetOrderShipToInfo(viewJob.OrderNo);

                if (info != null)
                {
                    if (info.DropShip)
                    {
                        viewJob.DropShip = true;
                        viewJob.DropShipCustID = info.ShipToCustID;
                        viewJob.DropShipCustName = info.ShipToName;
                        viewJob.ShipAddr1 = info.ShipToAddr1;
                        viewJob.ShipAddr2 = info.ShipToAddr2;
                        viewJob.ShipAddr3 = info.ShipToAddr3;
                        viewJob.ShipAddr4 = info.ShipToAddr4;
                        viewJob.ShipCity = info.ShipToCity;
                        viewJob.ShipState = info.ShipToState;
                        viewJob.ShipPostal = info.ShipToPostal;
                        viewJob.ShipCountry = info.ShipToCountry;
                    }
                    else
                    {
                        viewJob.DropShip = false;
                    }
                    viewJob.OrderTotal = info.OrderTot;
                }
            }
        }

        protected SelectList GetShipCodes()
        {
            var methods = from m in PrdnDBContext.ShipMethods
                          select new
                          {
                              ShipID = m.ShipMethodCD,
                              Description = m.ShipMethodCD + "-" + m.Description
                          };
            return new SelectList(methods.ToList(), "ShipID", "Description");
        }

        protected SelectList GetJobPriorities()
        {
            var pris = (from p in PrdnDBContext.ProductionPriorities
                        where p.ActiveFlag == PrdnDataHelper.BoolYNTue
                        select new
                        {
                            p.ID,
                            p.Code,
                            p.Name
                        })
                        .AsEnumerable().Select(i =>
                            new
                            {
                                ID = i.ID.ToString(),
                                Description = i.Code + "-" + i.Name
                            }).ToList()
                        ;
            return new SelectList(pris, "ID", "Description");
        }

        protected void AssignViewJobLookupListsFuncs(EditPrdnJobViewModel viewJob)
        {
            viewJob.CustListFunc = GetCustomerList;
            viewJob.CustLocsFunc = GetCustLocationSelList;
            viewJob.ShipCodesFunc = GetShipCodes;
            viewJob.JobPrioritiesFunc = GetJobPriorities;
        }

        protected void LoadViewJobProdLists(EditPrdnJobViewModel viewJob, ProductionJob job = null)
        {
            viewJob.ProdOptions = ProdOptDefn.ProdOptions(viewJob.ProdCD, viewJob.ParentProdCD);
            viewJob.ProdImageInfoSet = IsisDbContext.ProdImageInfoSet(viewJob.ProdCD);
        }

        protected void overrideViewJobProdOpts(EditPrdnJobViewModel viewJob)
        {
            if ((viewJob.NotNullAny(j => j.ProdOptions)) && (viewJob.NotNullAny(j => j.IfNotNull(x => x.EditWorksheet).IfNotNull(e => e.WorksheetOpts))))
            {
                foreach (var wOpt in viewJob.EditWorksheet.WorksheetOpts)
                {
                    foreach (var pOpt in viewJob.ProdOptions.Where(o => !o.Overriden && o.OverrideMatch(wOpt)))
                    {
                        pOpt.Overriden = true;
                    }
                }
            };
        }

        protected List<ExtantFileInfo> ExtantAttachments(decimal? jobID) 
        {
            if (jobID == null)
            {
                return null;
            }
            var esEsp = CultureEsEspanol();
            return (from a in PrdnDBContext.ProductionJobAttachments
                    where a.JobID == jobID
                    orderby a.AttachmentType.DisplayOrder
                    select new ExtantFileInfo
                    {
                        DecimalID = a.ID,
                        Description = a.Description,
                        FileName = a.FileName,
                        MimeType = a.MimeType,
                        AttTypeID = (int?)a.AttachmentTypeID,
                        AttTypeDescr = esEsp ? a.AttachmentType.EspDescription : a.AttachmentType.Description,
                        GroupCD = a.AttachmentType.GroupCD
                    }).ToList();
        }

        protected void loadViewJobViewAttachments(EditPrdnJobViewModel viewJob, string groups)
        {
            viewJob.Attacher = new FileAttacher();
            viewJob.Attacher.Groups = groups;
            if (viewJob.ID != null)
            {
                List<ExtantFileInfo> edit = new List<ExtantFileInfo>();
                List<ExtantFileInfo> view = new List<ExtantFileInfo>();

                List<ExtantFileInfo> allExtant = ExtantAttachments(viewJob.ID);
                string[] groupArr = groups.Split(',');
                foreach (var item in allExtant)
                {
                    if (groupArr.Contains(item.GroupCD)) { edit.Add(item); } else { view.Add(item); }
                }
                if (edit.Count() > 0)
                {
                    viewJob.Attacher.ExtantFiles = edit;
                }
                if (view.Count() > 0)
                {
                    viewJob.Attacher.ViewFiles = view;
                }
            }
            viewJob.Attacher.AttTypesFunc = GetAttTypes;
            viewJob.Attacher.RecordCounts();
        }

        protected void UpdateJobAttachments(ProductionJob editJob, FileAttacher attacher)
        {
            if (attacher != null)
            {
                List<string> delIDS = attacher.DelFileIDs();
                if (delIDS.IsAny())
                {
                    string sql = "DELETE FROM FG_PRDN_JOB_ATTACHMENT WHERE FG_PRDN_JOB_ATT_ID IN (" +
                        PrdnDataHelper.ParmFormatList(delIDS.Count()) + ")";
                    PrdnDBContext.ExecuteStoreCommand(sql, delIDS.Cast<object>().ToArray());
                }

                if (attacher.NotNullAny(a => a.ExtantFiles))
                {
                    foreach (var dbFile in attacher.ExtantFiles)
                    {
                        var mods = editJob.Attachments.Where(a =>
                            (a.ID == dbFile.DecimalID)
                            && ((a.AttachmentTypeID != dbFile.AttTypeID) && (dbFile.AttTypeID != null)
                                || a.Description != dbFile.Description)
                        );
                        foreach (var modFile in mods)
                        {
                            modFile.AttachmentTypeID = (decimal)dbFile.AttTypeID;
                            modFile.Description = dbFile.Description;
                        }
                    }
                };
                if (attacher.NotNullAny(a => a.CachedFiles))
                {
                    foreach (var newFile in attacher.CachedFiles)
                    {
                        CST.Prdn.Data.ProductionJobAttachment newAttachment = new CST.Prdn.Data.ProductionJobAttachment
                        {   //JobID = editJob.ID,
                            FileName = newFile.FileName,
                            MimeType = newFile.MimeType,
                            Description = newFile.Description,
                            Attachment = newFile.FileData
                        };
                        if (newFile.AttTypeID != null)
                        {
                            newAttachment.AttachmentTypeID = (decimal)newFile.AttTypeID;
                        }
                        editJob.Attachments.Add(newAttachment);
                    }
                }
            }
        }

        protected string GetCustName(decimal? custID)
        {
            return (from c in PrdnDBContext.ProductionCustomers
                    where c.ID == custID
                    select c.Name).FirstOrDefault();
        }

        protected string GetCustLocName(decimal? custID)
        {
            if (custID == PrdnDataHelper.PrdnCustIDCST)
            {
                return (from d in IsisDbContext.CustDepts() select d.Description).FirstOrDefault();
            }
            else if (custID == PrdnDataHelper.PrdnCustIDRW)
            {
                return PrdnDataHelper.RWIsisDeptDescr;
            }
            else { return null; }            
        }

        protected string GetShipMethodDescr(string code)
        {
            return (from m in PrdnDBContext.ShipMethods
                    where m.ShipMethodCD == code
                    select m.Description).FirstOrDefault();
        }

        protected EditPrdnJobViewModel GetEditJobViewModel(int id, string urlReturn)
        {
            EditPrdnJobViewModel model = new EditPrdnJobViewModel();
            LoadEditJobViewModel(model, id, urlReturn);
            return model;
        }

        protected void LoadEditJobViewModel(EditPrdnJobViewModel model, int id, string urlReturn)
        {
            var job = (from j in PrdnDBContext.ProductionJobs
                       .Include("Run").Include("Product").Include("Request").Include("Priority").Include("PrdnInvItem")
                       .Include("CreatedUser").Include("ScheduledUser").Include("ProcessedUser").Include("CompletedUser").Include("CanceledUser")
                       where j.ID == id
                       select j).FirstOrDefault();

            if (job != null)
            {
                Mapper.Map<ProductionJob, EditPrdnJobViewModel>(job, model);

                if (job.IsNotNull(j => j.PrdnInvItem)) { }

                model.UrlReturn = urlReturn;

                model.loadFromRun(job.Run);

                LoadViewJobProdLists(model, job);

                WorksheetEditViewModel.LoadViewJobEditWorksheet(model, job);

                overrideViewJobProdOpts(model);
            }
            else
            {
                model = null;
            }
        }

        protected ActionResult ViewJobBase(int id, string attGroups, string urlReturn)
        {
            ViewPrdnJobViewModel viewJob = new ViewPrdnJobViewModel();
            LoadEditJobViewModel(viewJob, id, urlReturn);
            if (viewJob != null)
            {
                viewJob.CustName = GetCustName(viewJob.CustID);
                viewJob.CustLocName = GetCustLocName(viewJob.CustID);
                viewJob.ShipMethodDescription = GetShipMethodDescr(viewJob.ShipMethodCD);

                loadViewJobViewAttachments(viewJob, attGroups);

                viewJob.CanEditRun = false;

                return View(viewJob);
            }
            else
            {
                return ErrMsgView(SystemExtensions.Sentence(LocalStr.Job, LocalStr.ID, id.ToString(), LocalStr.verbIsNot, LocalStr.valid));
            }
        }

        protected ActionResult ViewJobBase(ViewPrdnJobViewModel viewJob, IEnumerable<HttpPostedFileBase> uploadedFiles)
        {
            ProcessAttacherForSave(viewJob.Attacher, uploadedFiles);
            bool attachChanged = (viewJob.IfNotNull(m => m.Attacher).IfNotNull(a => a.OrigChanged) == true);

            ProductionJob editJob = (from j in PrdnDBContext.ProductionJobs
                                     where j.ID == viewJob.ID
                                     select j).FirstOrDefault();
            if (editJob == null)
            {
                return ErrMsgView(SystemExtensions.Sentence("Error Saving Job: ", LocalStr.JobID, viewJob.IfNotNull(j => j.ID).ToString(), LocalStr.verbIsNot, LocalStr.valid));
            }
            if (attachChanged)
            {
                EnsurePrdnDBContextOpen();
                using (var transaction = PrdnDBContext.Connection.BeginTransaction())
                {
                    try
                    {
                        UpdateJobAttachments(editJob, viewJob.Attacher);
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

            return RedirectIfLocal(viewJob.UrlReturn,
                () => RedirectToAction(actionName: "Jobs", routeValues: new { id = viewJob.RunID }));
        }

    }
}
