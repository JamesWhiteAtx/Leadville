using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Globalization;
using System.Data.Entity;
using Telerik.Web.Mvc;
using AutoMapper;
using CST.Prdn.Data;
using CST.Prdn.ViewModels;
using CST.Prdn.ISIS;
using CST.ActionFilters;
using System.Net.Sockets;
using System.IO;
using CST.Localization;

namespace CST.Prdn.Controllers
{
    [CstAuthorize(Groups = "SCHED/MAINT")]
    public class MaintenanceController : CstControllerBase
    {
        ///
        // Calendar
        public ActionResult Calendar(string year, string month)
        {
            ProductionCalendar cal = ProductionCalendar.MakeProductionCalendar(year, month, PrdnDBContext);
            if (cal.DateAdjusted)
            {
                return RedirectToAction("Calendar", new { year = cal.Year.ToString(), month = cal.Month.ToString() });
            }
            cal.OrderController = "Schedule";
            cal.OrderAction = "Runs";
            cal.RunController = "Schedule";
            cal.RunAction = "Runs";
            cal.AllowEditing = true;
            return View(cal);
        }

        public ActionResult DefinePrdnCalendar(string year, string month)
        {
            ProductionCalendar cal = new ProductionCalendar();

            int yearInt = (year == null) ? 0 : Convert.ToInt32(year);
            int monthInt = (month == null) ? 0 : Convert.ToInt32(month);

            cal.PrdnDBContext = PrdnDBContext;
            DateTime undefined = cal.FirstUndefinedMonth;

            if ((undefined.Year == yearInt) && (undefined.Month == monthInt))
            {
                cal.Year = yearInt;
                cal.Month = monthInt;
                cal.MakeProductionMonthDays();
                return RedirectToAction("Calendar", new { year = year, month = month });
            }
            else { throw new Exception("Invalid month"); }

        }

        public ActionResult PrdnShipDay(string dateId, bool shipDay)
        {
            DateTime calDay = PrdnCalendarDay.FormatDate(dateId).Date;
            ProductionCalendar cal = new ProductionCalendar();

            cal.PrdnDBContext = PrdnDBContext;

            if ((cal.MaxPrdnOrderWithRun != null) && (cal.MaxPrdnOrderWithRun.ShipDay.Date >= calDay))
            { throw new Exception("The date ID: " + dateId + " is not valid for update. Production activity exists after."); }

            if ((calDay < cal.FirstPrdnDate) || (calDay > cal.LastPrdnDate))
            { throw new Exception("The production calendar has not be defined for " + calDay.ToString("d") + "."); }

            cal.Year = calDay.Year;
            cal.Month = calDay.Month;

            cal.AdjustYearMonth();

            if ((cal.Year == calDay.Year) && (cal.Month == calDay.Month))
            {
                if (shipDay)
                { cal.SetShipDay(calDay); }
                else
                { cal.UnSetShipDay(calDay); }

                return RedirectToAction("Calendar", new { year = cal.Year.ToString(), month = cal.Month.ToString() });

                //return View();// ProdCalendar(cal);
            }
            else
            { throw new Exception("The date ID: " + dateId + " is not valid for update."); }

        }

        ///
        // Reasons
        public ActionResult Reasons()
        {
            return View();
        }

        protected IEnumerable<CST.Prdn.Data.ProductionReason> ReasonList()
        {
            var reasons = from r in PrdnDBContext.ProductionReasons
                          orderby r.Code
                          select r;
            return reasons;
        }

        protected List<PrdnReasonViewModel> ReasonViewList()
        {
            List<PrdnReasonViewModel> viewReasons =
                Mapper.Map<List<CST.Prdn.Data.ProductionReason>, List<PrdnReasonViewModel>>(ReasonList().ToList());

            return viewReasons;
        }

        protected GridModel ReasonGridList()
        {
            return new GridModel(ReasonViewList());
        }

        [GridAction]
        public ActionResult _SelectReason()
        {
            return View(ReasonGridList());
        }

        [HttpPost]
        [GridAction]
        public ActionResult _SaveReason(int id)
        {
            CST.Prdn.Data.ProductionReason reason = (from r in PrdnDBContext.ProductionReasons
                                       where r.ID == id
                                       select r).FirstOrDefault();

            if (TryUpdateModel(reason)) {
                PrdnDBContext.SaveChanges();
            }

            return View(ReasonGridList());
        }

        [HttpPost]
        [GridAction]
        public ActionResult _InsertReason()
        {
            CST.Prdn.Data.ProductionReason newReason = new CST.Prdn.Data.ProductionReason();

            if (TryUpdateModel(newReason)) {
                PrdnDBContext.ProductionReasons.AddObject(newReason);
                PrdnDBContext.SaveChanges();
            }

            return View(ReasonGridList());
        }

        [HttpPost]
        [GridAction]
        public ActionResult _DeleteReason(int id)
        {
            var deadReason = (from r in PrdnDBContext.ProductionReasons
                             where r.ID == id
                             select r).FirstOrDefault();

            if (deadReason != null)
            {
                PrdnDBContext.DeleteObject(deadReason);
                PrdnDBContext.SaveChanges();
            }
            
            return View(ReasonGridList());
        }

        ///
        // Manufacturers
        public ActionResult Manufacturers()
        {
            return View();
        }

        protected List<PrdnMfgrViewModel> MfgrViewList()
        {
            var mfgrs = from r in PrdnDBContext.ProductionMfgrs
                        orderby r.Code
                        select r;
                        //select new PrdnMfgrViewModel
                        //{
                        //    MfgrID = r.MfgrID,
                        //    MfgrCode = r.MfgrCode,
                        //    MfgrName = r.MfgrName,
                        //    Active = r.Active
                        //};

            List<PrdnMfgrViewModel> viewMfgrs =
                Mapper.Map<List<CST.Prdn.Data.ProductionMfgr>, List<PrdnMfgrViewModel>>(mfgrs.ToList());

            return viewMfgrs.ToList();
        }

        protected GridModel MfgrGridList()
        {
            return new GridModel(MfgrViewList());
        }

        [GridAction]
        public ActionResult _SelectMfgr()
        {
            return View(MfgrGridList());
        }

        [HttpPost]
        [GridAction]
        public ActionResult _SaveMfgr(int id)
        {
            CST.Prdn.Data.ProductionMfgr mfgr = (from r in PrdnDBContext.ProductionMfgrs
                                       where r.ID == id
                         select r).FirstOrDefault();

            if (TryUpdateModel(mfgr)) {
                PrdnDBContext.SaveChanges();
            }

            return View(MfgrGridList());
        }

        [HttpPost]
        [GridAction]
        public ActionResult _InsertMfgr()
        {
            PrdnMfgrViewModel m = new PrdnMfgrViewModel();

            CST.Prdn.Data.ProductionMfgr newMfgr = new CST.Prdn.Data.ProductionMfgr();

            if (TryUpdateModel(newMfgr)) {
                PrdnDBContext.ProductionMfgrs.AddObject(newMfgr);
                PrdnDBContext.SaveChanges();
            }

            return View(MfgrGridList());
        }

        [HttpPost]
        [GridAction]
        public ActionResult _DeleteMfgr(int id)
        {
            var deadMfgr = (from r in PrdnDBContext.ProductionMfgrs
                             where r.ID == id
                             select r).FirstOrDefault();

            if (deadMfgr != null)
            {
                PrdnDBContext.DeleteObject(deadMfgr);
                PrdnDBContext.SaveChanges();
            }
            
            return View(MfgrGridList());
        }

        ///
        // Locations
        public ActionResult Locations()
        {
            LocationViewModel model = new LocationViewModel();

            var mfgrs = from m in MfgrViewList()
                        select m;
            
            model.MfgrList = new SelectList(mfgrs.ToList(), "ID", "CodeDashName");

            var mfg1 = model.MfgrList.FirstOrDefault();
            if (mfg1 != null)
            {
                model.MfgrID = mfg1.Value;
            }

            return View(model);
        }

        protected IEnumerable<CST.Prdn.Data.ProductionLocation> LocationList()
        {
            var locations = from l in PrdnDBContext.ProductionLocations
                            orderby l.Code
                            select l;
            return locations;
        }

        protected IEnumerable<PrdnLocViewModel> LocViewList()
        {
            //var locations = from l in LocationList()
            //                select l;
            //                select new PrdnLocViewModel
            //                {   LocationID = l.LocationID,
            //                    LocationCode = l.LocationCode,
            //                    LocationName = l.LocationName,
            //                    Active = l.Active,
            //                    MfgrID = l.MfgrID};

            List<PrdnLocViewModel> viewLlocations =
                Mapper.Map<List<CST.Prdn.Data.ProductionLocation>, List<PrdnLocViewModel>>(LocationList().ToList());

            return viewLlocations;
        }

        protected IEnumerable<PrdnLocViewModel> locViewList(decimal mfgrID)
        {
            var locations = from l in LocViewList()
                            where l.MfgrID == mfgrID
                            select l;
            return locations;
        }

        protected GridModel LocGridList(decimal mfgrID)
        {
            return new GridModel(locViewList(mfgrID));
        }

        [GridAction]
        public ActionResult _SelectLocation(string mfgrID)
        {
            return View(LocGridList(Convert.ToInt32(mfgrID)));
        }

        [HttpPost]
        [GridAction]
        public ActionResult _SaveLocation(int id)
        {
            CST.Prdn.Data.ProductionLocation location = (from r in PrdnDBContext.ProductionLocations
                                   where r.ID == id
                                   select r).FirstOrDefault();

            if (TryUpdateModel(location))
            {
                PrdnDBContext.SaveChanges();
            }

            return View(LocGridList(location.MfgrID));
        }

        [HttpPost]
        [GridAction]
        public ActionResult _InsertLocation()
        {
            CST.Prdn.Data.ProductionLocation newLocation = new CST.Prdn.Data.ProductionLocation();

            if (TryUpdateModel(newLocation))
            {
                PrdnDBContext.ProductionLocations.AddObject(newLocation);
                PrdnDBContext.SaveChanges();
            }

            return View(LocGridList(newLocation.MfgrID));
        }

        [HttpPost]
        [GridAction]
        public ActionResult _DeleteLocation(int id)
        {
            var deadLocation = (from r in PrdnDBContext.ProductionLocations
                            where r.ID == id
                            select r).FirstOrDefault();

            if (deadLocation != null)
            {
                PrdnDBContext.DeleteObject(deadLocation);
                PrdnDBContext.SaveChanges();
            }

            return View(LocGridList(deadLocation.MfgrID));
        }

        ////////////////////////////////////////////////////////////
        // Customers
        public ActionResult Customers()
        {
            return View();
        }

        protected GridModel CustViewList()
        {
            var customers = from c in PrdnDBContext.ProductionCustomers
                            orderby c.Name
                            select c;
                            //select new PrdnCustViewModel
                            //{
                            //    ID = c.ID,
                            //    Code = c.Code,
                            //    Name = c.Name,
                            //    Active = c.Active
                            //};

            List<PrdnCustViewModel> viewCustomers =
                Mapper.Map<List<CST.Prdn.Data.ProductionCustomer>, List<PrdnCustViewModel>>(customers.ToList());

            return new GridModel(viewCustomers);
        }

        [GridAction]
        public ActionResult _SelectCustomer()
        {
            return View(CustViewList());
        }

        [HttpPost]
        [GridAction]
        public ActionResult _SaveCustomer(int id)
        {
            CST.Prdn.Data.ProductionCustomer Customer = (from r in PrdnDBContext.ProductionCustomers
                                       where r.ID == id
                                       select r).FirstOrDefault();

            if (TryUpdateModel(Customer))
            {
                PrdnDBContext.SaveChanges();
            }

            return View(CustViewList());
        }

        [HttpPost]
        [GridAction]
        public ActionResult _InsertCustomer()
        {
            CST.Prdn.Data.ProductionCustomer newCustomer = new CST.Prdn.Data.ProductionCustomer();

            if (TryUpdateModel(newCustomer))
            {
                PrdnDBContext.ProductionCustomers.AddObject(newCustomer);
                PrdnDBContext.SaveChanges();
            }

            return View(CustViewList());
        }

        [HttpPost]
        [GridAction]
        public ActionResult _DeleteCustomer(int id)
        {
            var deadCustomer = (from r in PrdnDBContext.ProductionCustomers
                              where r.ID == id
                              select r).FirstOrDefault();

            if (deadCustomer != null)
            {
                PrdnDBContext.DeleteObject(deadCustomer);
                PrdnDBContext.SaveChanges();
            }

            return View(CustViewList());
        }

        /// Categories /////////////////////////////////////////////////////////
        // Categories
        //public ActionResult Categories()
        //{
        //    IsisEntities isis= IsisRepository.CreateIsisEntities();

        //    var types = from t in isis.FG_PROD_TYPE
        //                orderby t.FG_PROD_TYPE_CD
        //                select new
        //                {
        //                    Value = t.FG_PROD_TYPE_CD,
        //                    Text = "(" + t.FG_PROD_TYPE_CD + ") " + t.FG_DESCRIPTION
        //                };

        //    SelectList typeList = new SelectList(types, "Value", "Text"); // isis.Entitiesx<ProductType>().Select(t => new { Value = t.ProdTypeCd, Text = "("+t.ProdTypeCd+") "+t.Description }), 
            
        //    CtgryViewModel model = new CtgryViewModel {
        //        FgProdTypes = typeList
        //    };
        //    return View(model);
        //}

        //protected IEnumerable<ProdCtgryViewModel> CtgryViewList()
        //{
        //    var ctgrys = from c in DbContext.ProductCtgries
        //                 orderby c.CtgryName
        //                 select new ProdCtgryViewModel
        //                 {
        //                     CtgryID = c.CtgryID,
        //                     CtgryCode = c.CtgryCode,
        //                     CtgryName = c.CtgryName,
        //                     FgProdTypeCd = c.FgProdTypeCd,
        //                     Active = c.Active
        //                 };

        //    return ctgrys;
        //}

        //protected GridModel CtgryGridList()
        //{
        //    return new GridModel(CtgryViewList());
        //}

        //[GridAction]
        //public ActionResult _SelectCtgry()
        //{
        //    return View(CtgryGridList());
        //}

        //[HttpPost]
        //[GridAction]
        //public ActionResult _SaveCtgry(int id)
        //{
        //    ProductCtgry ctgry = (from r in DbContext.ProductCtgries
        //                                   where r.CtgryID == id
        //                                   select r).FirstOrDefault();

        //    if (TryUpdateModel(ctgry))
        //    {
        //        DbContext.SaveChanges();
        //    }
        //    return View(CtgryGridList());
        //}

        //[HttpPost]
        //[GridAction]
        //public ActionResult _InsertCtgry()
        //{
        //    ProductCtgry newCtgry = new ProductCtgry();

        //    if (TryUpdateModel(newCtgry))
        //    {
        //        DbContext.ProductCtgries.AddObject(newCtgry);
        //        DbContext.SaveChanges();
        //    }
        //    return View(CtgryGridList());
        //}

        //[HttpPost]
        //[GridAction]
        //public ActionResult _DeleteCtgry(int id)
        //{
        //    var deadCtgry = (from r in DbContext.ProductCtgries
        //                        where r.CtgryID == id
        //                        select r).FirstOrDefault();

        //    if (deadCtgry != null)
        //    {
        //        DbContext.DeleteObject(deadCtgry);
        //        DbContext.SaveChanges();
        //    }
        //    return View(CtgryGridList());
        //}

        ////////////////////////////////////////////////////////////
        // Production Types

        public ActionResult ProductionTypes()
        {

            var locs = from loc in LocationList()
                       orderby loc.Mfgr.Code, loc.Code
                       select new {
                            loc.ID,
                            LocMfgrDescr = loc.Mfgr.Code+" - "+loc.Code+" - "+loc.Name
                       };


            TypesViewModel model = new TypesViewModel
            {
                Reasons = new SelectList(ReasonViewList(), "ID", "CodeDashName"),
                ProdTypes = new SelectList(GetDIProdTypeList(), "Code", "CodeDashName"),
                Locations = new SelectList(locs, "ID", "LocMfgrDescr")
            };

            return View(model);
        }

        protected IEnumerable<CST.Prdn.Data.ProductionType> PrdnTypeList()
        {
            var types = from type in PrdnDBContext.ProductionTypes
                        orderby type.SortOrder, type.Code
                        select type;
            return types;
        }

        protected List<PrdnTypeViewModel> PrdnTypeViewList()
        {
            List<PrdnTypeViewModel> viewTypes =
                Mapper.Map<List<CST.Prdn.Data.ProductionType>, List<PrdnTypeViewModel>>(PrdnTypeList().ToList());

            return viewTypes;
        }

        protected GridModel PrdnTypeGridList()
        {
            return new GridModel(PrdnTypeViewList());
        }

        [GridAction]
        public ActionResult _SelectPrdnType()
        {
            return View(PrdnTypeGridList());
        }

        [HttpPost]
        [GridAction]
        public ActionResult _SavePrdnType(int id)
        {
            CST.Prdn.Data.ProductionType type = (from r in PrdnDBContext.ProductionTypes
                                  where r.ID == id
                                  select r).FirstOrDefault();

            if (TryUpdateModel(type))
            {
                PrdnDBContext.SaveChanges();
            }
            return View(PrdnTypeGridList());
        }

        [HttpPost]
        [GridAction]
        public ActionResult _InsertPrdnType()
        {
            CST.Prdn.Data.ProductionType newType = new CST.Prdn.Data.ProductionType();

            if (TryUpdateModel(newType))
            {
                PrdnDBContext.ProductionTypes.AddObject(newType);
                PrdnDBContext.SaveChanges();
            }
            return View(PrdnTypeGridList());
        }

        [HttpPost]
        [GridAction]
        public ActionResult _DeletePrdnType(int id)
        {
            var deadCtgry = (from r in PrdnDBContext.ProductionTypes
                             where r.ID == id
                             select r).FirstOrDefault();

            if (deadCtgry != null)
            {
                PrdnDBContext.DeleteObject(deadCtgry);
                PrdnDBContext.SaveChanges();
            }
            return View(PrdnTypeGridList());
        }

        ////////////////////////////////////////////////////////////
        // Production Priorities

        public ActionResult Priorities()
        {
            return View();
        }

        protected IEnumerable<ProductionPriority> PriorityList()
        {
            var priorities = from p in PrdnDBContext.ProductionPriorities
                          orderby p.Code
                          select p;

            return priorities;
        }

        protected GridModel PriorityGridList()
        {
            var priorities = from r in PriorityList()
                              select r;

            List<PrdnPriorityViewModel> viewPriorities =
                Mapper.Map<List<ProductionPriority>, List<PrdnPriorityViewModel>>(priorities.ToList());
            
            return new GridModel(viewPriorities);
        }

        [GridAction]
        public ActionResult _SelectPriority()
        {
            return View(PriorityGridList());
        }

        [HttpPost]
        [GridAction]
        public ActionResult _SavePriority(int id)
        {
            ProductionPriority priority = (from r in PrdnDBContext.ProductionPriorities
                                       where r.ID == id
                                       select r).FirstOrDefault();

            if (TryUpdateModel(priority))
            {
                PrdnDBContext.SaveChanges();
            }

            return View(PriorityGridList());
        }

        [HttpPost]
        [GridAction]
        public ActionResult _InsertPriority()
        {
            ProductionPriority newPriority = new ProductionPriority();

            if (TryUpdateModel(newPriority))
            {
                PrdnDBContext.ProductionPriorities.AddObject(newPriority);
                PrdnDBContext.SaveChanges();
            }

            return View(PriorityGridList());
        }

        [HttpPost]
        [GridAction]
        public ActionResult _DeletePriority(int id)
        {
            ProductionPriority deadPriority = (from r in PrdnDBContext.ProductionPriorities
                                    where r.ID == id
                                    select r).FirstOrDefault();

            if (deadPriority != null)
            {
                PrdnDBContext.DeleteObject(deadPriority);
                PrdnDBContext.SaveChanges();
            }

            return View(PriorityGridList());
        }

        ////////////////////////////////////////////////////////////
        // Production Attachment Types

        public ActionResult AttachmentTypes()
        {
            AttachmentTypesViewModel model = new AttachmentTypesViewModel();
            return View(model);
        }

        protected IEnumerable<PrdnAttachmentType> AttTypeList()
        {
            var types = from t in PrdnDBContext.PrdnAttachmentTypes
                        orderby t.DisplayOrder
                        select t;
            return types;
        }

        protected GridModel AttTypeGridList()
        {
            var types = from r in AttTypeList()
                             select r;

            List<PrdnAttTypeViewModel> viewPriorities =
                Mapper.Map<List<PrdnAttachmentType>, List<PrdnAttTypeViewModel>>(types.ToList());

            return new GridModel(viewPriorities);
        }

        [GridAction]
        public ActionResult _SelectAttType()
        {
            return View(AttTypeGridList());
        }

        [HttpPost]
        [GridAction]
        public ActionResult _SaveAttType(int id)
        {
            PrdnAttachmentType type = (from t in PrdnDBContext.PrdnAttachmentTypes
                                       where t.ID == id
                                       orderby t.DisplayOrder
                                       select t).FirstOrDefault();
            if (TryUpdateModel(type))
            {
                PrdnDBContext.SaveChanges();
            }
            return View(AttTypeGridList());
        }

        [HttpPost]
        [GridAction]
        public ActionResult _InsertAttType()
        {
            PrdnAttachmentType type = new PrdnAttachmentType();
            if (TryUpdateModel(type))
            {
                PrdnDBContext.PrdnAttachmentTypes.AddObject(type);
                PrdnDBContext.SaveChanges();
            }
            return View(AttTypeGridList());
        }

        [HttpPost]
        [GridAction]
        public ActionResult _DeleteAttType(int id)
        {
            PrdnAttachmentType type = (from t in PrdnDBContext.PrdnAttachmentTypes
                                       where t.ID == id
                                       select t).FirstOrDefault();
            if (type != null)
            {
                PrdnDBContext.DeleteObject(type);
                PrdnDBContext.SaveChanges();
            }
            return View(AttTypeGridList());
        }

        /////////////////////////////////////////////////
        // Label Printers

        public ActionResult LabelPrinters()
        {
            return View();
        }

        protected GridModel LabelPrinterViewList()
        {
            var printers = from p in PrdnDBContext.LabelPrinters
                           orderby p.Name
                           select p;

            List<PrintLabelModel> models =
                Mapper.Map<List<CST.Prdn.Data.LabelPrinter>, List<PrintLabelModel>>(printers.ToList());

            return new GridModel(models);
        }

        [GridAction]
        public ActionResult _SelectLabelPrinter()
        {
            return View(LabelPrinterViewList());
        }

        //[HttpPost]
        //[GridAction]
        //public ActionResult _SaveLabelPrinter(int id)
        //{
        //    CST.Prdn.Data.LabelPrinter printer = (from r in PrdnDBContext.LabelPrinters
        //                                          where r.ID == id
        //                                          select r).FirstOrDefault();

        //    if (TryUpdateModel(printer))
        //    {
        //        PrdnDBContext.SaveChanges();
        //    }

        //    return View(LabelPrinterViewList());
        //}

        //[HttpPost]
        //[GridAction]
        //public ActionResult _InsertLabelPrinter()
        //{
        //    CST.Prdn.Data.LabelPrinter newPrinter = new CST.Prdn.Data.LabelPrinter();

        //    if (TryUpdateModel(newPrinter))
        //    {
        //        PrdnDBContext.LabelPrinters.AddObject(newPrinter);
        //        PrdnDBContext.SaveChanges();
        //    }

        //    return View(LabelPrinterViewList());
        //}

        [HttpPost]
        [GridAction]
        public ActionResult _DeleteLabelPrinter(int id)
        {
            var deadCustomer = (from r in PrdnDBContext.LabelPrinters
                                where r.ID == id
                                select r).FirstOrDefault();

            if (deadCustomer != null)
            {
                PrdnDBContext.DeleteObject(deadCustomer);
                PrdnDBContext.SaveChanges();
            }

            return View(LabelPrinterViewList());
        }

        [HttpGet]
        public ActionResult AddLabelPrinter(string urlReturn)
        {
            PrintLabelModel model = new PrintLabelModel { 
                UrlReturn = urlReturn
            };
            
            ViewBag.Title = "Add Label Printer";
            return View("LabelPrinterEdit", model);
        }
        
        [HttpPost]
        public ActionResult AddLabelPrinter(PrintLabelModel model)
        {
            if (ModelState.IsValid)
            {
                LabelPrinter printer = Mapper.Map<PrintLabelModel, LabelPrinter>(model);

                PrdnDBContext.LabelPrinters.AddObject(printer);

                PrdnDBContext.SaveChanges();
                return RedirectToAction(actionName: "LabelPrinters");
            }

            ViewBag.Title = "Add Label Printer";
            return View("LabelPrinterEdit", model);
        }

        [HttpGet]
        public ActionResult EditLabelPrinter(int id, string urlReturn)
        {
            LabelPrinter printer = (from x in PrdnDBContext.LabelPrinters
                                    where x.ID == id
                                    select x).FirstOrDefault();

            if (printer == null)
            {
                return ErrMsgView("Sorry - Invalid printer ID " + id.ToString());
            }

            PrintLabelModel model = Mapper.Map<LabelPrinter, PrintLabelModel>(printer);
            model.UrlReturn = urlReturn;
            
            ViewBag.Title = "Edit Label Printer";
            return View("LabelPrinterEdit", model);
        }

        [HttpPost]
        public ActionResult EditLabelPrinter(PrintLabelModel model)
        {
            if (ModelState.IsValid)
            {
                LabelPrinter printer = (from x in PrdnDBContext.LabelPrinters
                                        where x.ID == model.ID
                                        select x).FirstOrDefault();

                if (printer == null)
                {
                    return ErrMsgView("Sorry - Invalid printer ID " + model.ID.ToString());
                }

                Mapper.Map(model, printer);

                PrdnDBContext.SaveChanges();

                return RedirectToAction(actionName: "LabelPrinters");
            }

            ViewBag.Title = "Edit Label Printer";
            return View("LabelPrinterEdit", model);
        }

        public ActionResult _TestZebraConnection(string hostName, int port)
        {
            bool success = false;
            string msg = null;
            TcpClient Zebraclient = new TcpClient();
            try
            {
                Zebraclient.SendTimeout = 500;
                Zebraclient.ReceiveTimeout = 500;
                Zebraclient.Connect(hostName, port);
                if (Zebraclient.Connected)
                {
                    //send and receive illustrated below
                    NetworkStream mynetworkstream;
                    StreamReader mystreamreader;
                    StreamWriter mystreamwriter;
                    mynetworkstream = Zebraclient.GetStream();
                    mystreamreader = new StreamReader(mynetworkstream);
                    mystreamwriter = new StreamWriter(mynetworkstream);
                    string commandtosend = "~HS";
                    mystreamwriter.WriteLine(commandtosend);
                    mystreamwriter.Flush();
                    char[] mk = null;
                    mk = new char[100];
                    mystreamreader.Read(mk, 0, mk.Length);
                    string data1 = new string(mk);
                    msg = LocalStr.Success + "!  Response: " + data1;
                    Zebraclient.Close();
                    success = true;
                }
                else
                {
                    success = false;
                    msg = LocalStr.Error + " - Not Connected";
                }
            }
            catch (Exception e)
            {
                success = false;
                msg = LocalStr.Error + " - " + e.Message;
            }
            return Json(new { Success = success, Msg = msg }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult _TestLabelPrinter(int id)
        {
            var printer = (from p in PrdnDBContext.LabelPrinters
                           where p.ID == id
                           select new { p.HostName, p.Port }).FirstOrDefault();
            if (printer != null)
            {
                return _TestZebraConnection(printer.HostName, printer.IfNotNull(p => (int)p.Port.ToInt()));
            }
            else
                return Json(new { Success = false, Msg = SystemExtensions.Sentence(LocalStr.Error+"-" + LocalStr.Printer, LocalStr.ID, LocalStr.verbIsNot, LocalStr.valid)}, 
                    JsonRequestBehavior.AllowGet);
        }

    }
}
