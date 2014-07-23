using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using Telerik.Web.Mvc;
using AutoMapper;
using CST.Prdn.Data;
using CST.Prdn.ViewModels;
using CST.Security;
using CST.ActionFilters;
using CST.Localization;

namespace CST.Prdn.Controllers
{
    [CstAuthorize(Groups = "SCHED/SCHED")]
    public class ScheduleController : CstControllerBase
    {
        // Schedule Jobs /////////////////////////////////////////////

        public ActionResult Jobs(string id)
        {
            SchedJobRunModel model = null;
            ProductionRun run = null;

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

                run = (from r in PrdnDBContext.ProductionRuns
                           .Include("Jobs")
                           .Include("PrdnOrder")
                           .Include("PrdnType.ProductType")
                       where r.ID == intID
                       select r).FirstOrDefault();

                if (run == null)
                {
                    return ErrMsgView(SystemExtensions.Sentence(LocalStr.RunID, id, LocalStr.verbIsNot, LocalStr.valid));
                }
            }

            model = new SchedJobRunModel();

            Mapper.Map(run, model);

            model.SettingsModel = settings;
            model.PageSize = model.SettingsModel.JobPageSize ?? UserSettingsModel.DefaultPageSize;

            model.FilterModel = MakeStatusModel();
            model.FilterModel.AssignFromSession(Session);

            return View(model);
        }

        // Production Runs /////////////////////////////////////////
        public ActionResult Runs(string id)
        {
            ProductionOrder prdnOrd = null;

            if (id == null)
            {
                UserSettingsViewModel settings = GetUserSettingsViewModel(true);
                if (settings.IsNotNull(s => s.DefaultRunOrderNo))
                {
                    id = settings.DefaultRunOrderNo;
                    return RedirectToAction(actionName: "Runs", routeValues: new { id = id });
                }
            }
            else
            {
                prdnOrd = (from p in PrdnDBContext.ProductionOrders.Include("Runs")
                           where p.OrderNo == id
                           select p).FirstOrDefault();

                if (prdnOrd == null)
                {
                    return ErrMsgView(SystemExtensions.Sentence(LocalStr.OrderNo, id, LocalStr.verbIsNot, LocalStr.valid));
                }
            }

            PrdnOrdViewModel model = new PrdnOrdViewModel
            {
                TypeCount = PrdnDBContext.ProductionTypes.Count()
            };
            if (prdnOrd != null) 
            {
                model.OrderNo = prdnOrd.OrderNo;
                model.ShipDtStr = prdnOrd.ShipDay.ToString(DateFormat.ShortDatePattern);
            }

            return View(model);
        }

        public ActionResult ScheduleJobNewRun(string orderNo, int typeID, string note)
        {
            ProductionRun newRun = MakeProductionNewRun(orderNo, typeID, note);
            return RedirectToAction(actionName: "Jobs", routeValues: new { id = newRun.ID });
        }

        public ActionResult ScheduleRunNewRun(string orderNo, int typeID, string note)
        {
            ProductionRun newRun = MakeProductionNewRun(orderNo, typeID, note);
            return RedirectToAction(actionName: "Runs", routeValues: new { id = newRun.PrdnOrderNo });
        }

        [HttpPost]
        public ActionResult CreateRun(NewPrdnRunViewModel runModel)
        {
            CST.Prdn.Data.ProductionRun newRun = runModel.MakeProductionRun();

            PrdnDBContext.ProductionRuns.AddObject(newRun);
            PrdnDBContext.SaveChanges();

            CreateRunActionParm parm = runModel.RetrieveActionParm(Session, newRun);

            if (parm != null)
            {
                return RedirectToAction(actionName: parm.GotoAction, controllerName: parm.GotoController, routeValues: parm);   
            }
            else {
                return RedirectIfLocal(runModel.CalledFromUrl, () => ErrMsgView("Sorry - a new Run was created, but I got confused and don't know where to go now.") );
            }
            //if (this.IsLocalUrl(runModel.CalledFromUrl)) { return Redirect(runModel.CalledFromUrl); } else { return ErrMsgView("Sorry - a new Run was created, but I got confused and don't know where to go now."); }
        }

        [HttpPost]
        [GridAction]
        public ActionResult _SaveRun(int id, FormCollection collection)
        {
            //PrdnRunViewModel viewRun = new PrdnRunViewModel();

            var run = (from r in PrdnDBContext.ProductionRuns
                       where r.ID == id
                       select r).FirstOrDefault();

            if (TryUpdateModel(run))
            {

                //Mapper.Map(viewRun, run);
                PrdnDBContext.SaveChanges();

                return View(RunGridList(run.PrdnOrderNo));
            } else {
                return View(RunGridList(""));
            }        
        }

        [HttpPost]
        [GridAction]
        public ActionResult _DeleteRun(int id)
        {
            var deadRun = (from r in PrdnDBContext.ProductionRuns
                           where r.ID == id
                           select r).FirstOrDefault();

            if (deadRun != null)
            {
                PrdnDBContext.DeleteObject(deadRun);
                PrdnDBContext.SaveChanges();
            }
            return View(RunGridList(deadRun.PrdnOrderNo));
        }

        //public ActionResult _PrdnXXXOrdLookup(string poTerm)
        //{
        //    string prdnNo = (poTerm == null) ? String.Empty : poTerm.Trim();
        //    if (prdnNo.Length < 3)
        //    {
        //        return Json(null, JsonRequestBehavior.AllowGet);
        //    }

        //    var orders =
        //        from i in
        //            (from o in PrdnDBContext.ProductionOrders
        //             where o.OrderNo.StartsWith(prdnNo)
        //             orderby o.OrderNo
        //             select new { PrdnOrdNo = o.OrderNo, o.ShipDay }
        //             ).ToList()
        //        let shipStr = i.ShipDay.ToString(DateFormat.ShortDatePattern)
        //        select new
        //        {
        //            LookupLabel = i.PrdnOrdNo + " " + shipStr,
        //            PrdnOrdNo = i.PrdnOrdNo,
        //            ShipDay = shipStr
        //        }
        //    ;

        //    return Json(orders, JsonRequestBehavior.AllowGet);
        //}

        [HttpGet]
        public ActionResult EditDefaultRun(decimal? runID, string runDescr, string urlReturn, string message)
        {
            DefaultRunEditViewModel model;
            if (runID != null)
            {
                model = new DefaultRunEditViewModel();
                model.UserID = GetCurrentUserID();
                model.UserLogin = GetCurrentUserLogin();
                model.DefaultRunID = runID;
                model.DefaultRunDescr = runDescr;
            }
            else
            {
                model = Mapper.Map<DefaultRunEditViewModel>(MakeUserDefaultPrdnRun());
                model.UserLogin = GetCurrentUserLogin();
            }
            model.UrlReturn = urlReturn;
            if (message != null)
            {
                model.Message = message;
            }

            model.LookupRunModel = new PrdnRunLookupModel();
            model.NewRunModel = new NewPrdnRunViewModel(Session, RouteData.Values,
                new RequestCreateRunActionParm { UrlReturn = model.UrlReturn });

            return View(model);
        }

        [HttpPost]
        public ActionResult EditDefaultRun(DefaultRunEditViewModel defaultRun)
        {
            if (TryValidateModel(defaultRun))
            {
                SaveDefaultUserPrdnRun(defaultRun.UserID, (decimal)defaultRun.DefaultRunID);

                return RedirectIfLocal(defaultRun.UrlReturn, () => View(defaultRun) );
                //if (this.IsLocalUrl(defaultRun.UrlReturn)) {return Redirect(defaultRun.UrlReturn);} else { return View(defaultRun); }
            }

            return View(defaultRun);
        }

        public ActionResult ClearDefaultRun(string urlReturn)
        {
            return ClearDefaultRunBase(urlReturn);
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

        /// 
        // Settings
        [HttpGet]
        public override ActionResult EditSettings(string urlReturn)
        {
            return EditSettingsBase(urlReturn);
        }

        public ActionResult FindJob()
        {
            JobLookupModel model = new JobLookupModel();

            var types = from t in PrdnDBContext.ProductionTypes
                        orderby t.SortOrder, t.Code
                        select new { 
                            t.ID,
                            Text = t.Code+" - "+t.Description
                        };

            model.PrdnTypes = new SelectList(types.ToList(), "ID", "Text");
            model.CustList = GetCustomerList();

            return View(model);
        }

        public ActionResult Product(string id)
        {
            ProdScheduleLookupModel model = new ProdScheduleLookupModel
            {
                ProdTypeList = GetProdTypeList(),
                PrdnProdTypeList = GetPrdnProdTypeList()
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

        public ActionResult PrdnList(string year, string month)
        {
            if (year == null)
            {
                year = DateTime.Today.Year.ToString();
            }
            if (month == null)
            {
                month = DateTime.Today.Month.ToString();
            }

            PrdnOrderMonth prdnMonth = new PrdnOrderMonth(year, month, PrdnDBContext);
            if (prdnMonth.Cal.DateAdjusted)
            {
                return RedirectToAction(RouteData.Values["action"].ToString(), new { year = prdnMonth.Cal.Year.ToString(), month = prdnMonth.Cal.Month.ToString() });
            }

            prdnMonth.Cal.AllowEditing = false;

            UserSettingsViewModel settings = GetUserSettingsViewModel(false);
            prdnMonth.JobPageSize = settings.JobPageSize ?? UserSettingsModel.DefaultPageSize;

            return View(prdnMonth);
        }


    }
}
