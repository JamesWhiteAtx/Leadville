using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CST.Localization;
using CST.Prdn.ViewModels;
using CST.Prdn.Data;
using Telerik.Web.Mvc;
using CST.Prdn.ISIS;

namespace CST.Prdn.Controllers
{
    public class LookupController : CstControllerBase
    {

        public ActionResult _SettingsHtml(string serialNo)
        {
            UserSettingsViewModel model = GetUserSettingsViewModel();

            if (AtionIsAccessible(controllerName: "Schedule", actionName: "EditSettings"))
            {
                model.Requests = true;
            }
            else
            {
                model.Requests = false;
            }

            var printer = (from p in PrdnDBContext.LabelPrinters
                           where p.ID == model.LabelPrinterID
                           orderby p.Name
                           select p).FirstOrDefault();

            if (printer != null)
            {
                model.LabelPrinterDisplay = printer.ID.ToString() + " " + printer.Name + " " + printer.HostName + " " + printer.Port.ToString();
            }

            string html = RenderPartialView("ViewSettings", model);

            return Json(html, JsonRequestBehavior.AllowGet);
        }
        
        public ActionResult _PrdnOrdLookup(string term)
        {
            string ordNoUpper = term.Trim().ToUpper();

            var ordQry = (from o in PrdnDBContext.ProductionOrders
                          where o.OrderNo.StartsWith(ordNoUpper)
                          orderby o.OrderNo
                          select new
                          {
                              OrderNo = o.OrderNo,
                              ShipDay = o.ShipDay,
                              RunCount = o.Runs.Count()
                          }).Take(20).ToList();

            var orders = from o in ordQry
                         select new PrdnOrdListModel
                         {
                             PrdnOrdNo = o.OrderNo,
                             OrdShipDtStr = o.ShipDay.ToString(DateFormat.ShortDatePattern),
                             RunCount = o.RunCount
                         };

            return Json(orders, JsonRequestBehavior.AllowGet);
        }

        public ActionResult _PrdnOrdsForMonth(int? year, int? month)
        {
            DateTime fdom = new DateTime(year: (int)year, month: (int)month, day: 1);
            DateTime fdoNxtM = fdom.AddMonths(1);

            var ordQry = (from o in PrdnDBContext.ProductionOrders
                         where o.ShipDay >= fdom && o.ShipDay < fdoNxtM
                         orderby o.OrderNo
                         select new
                          {
                              OrderNo = o.OrderNo,
                              ShipDay = o.ShipDay,
                              RunCount = o.Runs.Count()
                          }).ToList();
            
            var orders = from o in ordQry
                         select new PrdnOrdListModel
                         {
                             PrdnOrdNo = o.OrderNo,
                             OrdShipDay = o.ShipDay,
                             OrdShipDtStr = o.ShipDay.ToString(DateFormat.ShortDatePattern),
                             RunCount = o.RunCount
                         };

            return Json(orders, JsonRequestBehavior.AllowGet);
        }

        public static List<PrdnOrdRunListModel> PrdnTypeRuns(IEnumerable<ProductionOrder> orders, string typeCd, string prodTypeCD, bool includePrdnOrNos = true)
        {
            List<PrdnOrdRunListModel> prdnRuns = new List<PrdnOrdRunListModel>();

            string shipDateStr;
            foreach (var order in orders)
            {
                shipDateStr = order.ShipDay.ToString(DateFormat.ShortDatePattern);

                IEnumerable<CST.Prdn.Data.ProductionRun> runs = from r in order.Runs select r;

                if (!String.IsNullOrEmpty(typeCd))
                {
                    runs = runs.Where(r => r.PrdnType.Code.ToUpper().StartsWith(typeCd));
                }

                if (!String.IsNullOrEmpty(prodTypeCD))
                {
                    runs = runs.Where(r => r.PrdnType.ProdTypeCD == prodTypeCD);
                }

                foreach (var run in runs.OrderBy(r => r.PrdnType.SortOrder))
                {
                    PrdnOrdRunListModel itemRun = new PrdnOrdRunListModel
                    {
                        LkupPrdnOrdNo = order.OrderNo,
                        LkupShipDtStr = shipDateStr,
                        LkupPrdnRunID = run.ID.ToString(),
                        LkupPrdnTypeCD = run.PrdnType.Code,
                        LkupPrdnTypeDescr = run.PrdnType.Description,
                        LkupProdTypeCD = run.PrdnType.ProdTypeCD
                    };
                    prdnRuns.Add(itemRun);
                }

                if (includePrdnOrNos)
                {
                    PrdnOrdRunListModel itemPo = new PrdnOrdRunListModel
                    {
                        LkupPrdnOrdNo = order.OrderNo,
                        LkupShipDtStr = shipDateStr,
                        LkupPrdnRunID = String.Empty,
                        LkupPrdnTypeCD = String.Empty,
                        LkupPrdnTypeDescr = String.Empty,
                        LkupProdTypeCD = String.Empty
                    };
                    prdnRuns.Add(itemPo);
                }

            }

            return prdnRuns;
        }

        private List<PrdnOrdRunListModel> PrdnOrdTypRuns(string prdnOrderNo, string typeCd, string prodTypeCD, bool includePrdnOrNos = true)
        {
            var orders = from o in PrdnDBContext.ProductionOrders
                         .Include("Runs")
                         .Include("Runs.PrdnType")
                         where o.OrderNo.StartsWith(prdnOrderNo)
                         orderby o.OrderNo descending
                         select o
            ;

            List<PrdnOrdRunListModel> prdnRuns = PrdnTypeRuns(orders, typeCd, prodTypeCD, includePrdnOrNos);

            return prdnRuns;
        }

        private List<PrdnOrdRunListModel> PrdnRunsType(string prdnOrderNo, string typeCd, string prodTypeCD)
        {
            var runs = from r in PrdnDBContext.ProductionRuns
                       .Include("PrdnOrder").Include("PrdnType")
                       where r.PrdnOrder.OrderNo.StartsWith(prdnOrderNo)
                       select r;

            if (typeCd != null)
            {
                runs = from r in runs
                       where r.PrdnType.Code.StartsWith(typeCd)
                       select r;
            }

            if (prodTypeCD != null)
            {
                runs = from r in runs
                       where r.PrdnType.ProdTypeCD == prodTypeCD
                       select r;
            }

            runs = from r in runs
                   orderby r.PrdnOrder.OrderNo, r.PrdnType.Code
                   select r;

            var result = from run in runs.ToList()
                       let shipDateStr = run.PrdnOrder.ShipDay.ToString(DateFormat.ShortDatePattern)
                       select new PrdnOrdRunListModel
                       {
                           LkupPrdnOrdNo = run.PrdnOrder.OrderNo,
                           LkupShipDtStr = shipDateStr,
                           LkupPrdnRunID = run.ID.ToString(),
                           LkupPrdnTypeCD = run.PrdnType.Code,
                           LkupPrdnTypeDescr = run.PrdnType.Description,
                           LkupProdTypeCD = run.PrdnType.ProdTypeCD
                       };
            
            return result.ToList();
        }

        public ActionResult _PrdnRunLookup(string poTerm, string typeTerm, string prodTypeCD, bool includePrdnOrNos = true)
        {
            string prdnNo = (poTerm == null) ? String.Empty : poTerm.Trim();
            if (prdnNo.Length < 3)
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }

            string typeCd = String.IsNullOrEmpty(typeTerm) ? null : typeTerm.Trim().ToUpper();

            List<PrdnOrdRunListModel> prdnRuns;
            if (includePrdnOrNos)
            {
                prdnRuns = PrdnOrdTypRuns(prdnNo, typeCd, prodTypeCD, includePrdnOrNos);
            }
            else 
            {
                prdnRuns = PrdnRunsType(prdnNo, typeCd, prodTypeCD);
            }

            return Json(prdnRuns, JsonRequestBehavior.AllowGet);
        }

        public ActionResult _PrdnRunLookup2(string term, bool includePrdnOrNos = true, string prodTypeCD= null)
        {
            string poTerm;
            string typeTerm;
            string[] terms = term.ToUpper().Split(new Char[] { ' ', '-' });
            if (terms.Length == 1)
            {
                poTerm = terms[0].SafeSub(0, 4);
                typeTerm = terms[0].SafeSub(4, term.Length);
            }
            else if (terms.Length > 1)
            {
                poTerm = terms[0].Trim();
                typeTerm = terms[1].Trim();
            }
            else
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }

            if (poTerm.Length < 3)
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }

            string typeCd = String.IsNullOrEmpty(typeTerm) ? null : typeTerm.Trim().ToUpper();

            List<PrdnOrdRunListModel> prdnRuns;
            if (includePrdnOrNos)
            {
                prdnRuns = PrdnOrdTypRuns(poTerm, typeCd, prodTypeCD, includePrdnOrNos);
            }
            else
            {
                prdnRuns = PrdnRunsType(poTerm, typeCd, prodTypeCD);
            }

            return Json(prdnRuns, JsonRequestBehavior.AllowGet);
        }

        public ActionResult _JobSerialLookup(string term, int? take = null)
        {
            string serialUpper = term.Trim().ToUpper();

            var serials = from j in PrdnDBContext.ProductionJobs
                          where j.SerialNo.StartsWith(serialUpper)
                          orderby j.SerialNo
                          select new
                          {
                              j.SerialNo
                          };
            if (take != null)
            {
                serials = serials.Take((int)take);
            }

            return Json(serials, JsonRequestBehavior.AllowGet);
        }
        

        public ActionResult _JobRunSeqlist(int id)
        {
            var sequences = from j in PrdnDBContext.ProductionJobs
                            where j.RunID == id
                            orderby j.RunSeqNo
                            select j.RunSeqNo;

            return Json(sequences.ToList(), JsonRequestBehavior.AllowGet);
        }

        public ActionResult _InvSerialLookup(string term, int? take = null)
        {
            string serialUpper = term.Trim().ToUpper();

            var serials = from j in PrdnDBContext.PrdnInvItems
                          where j.SerialNo.StartsWith(serialUpper)
                          orderby j.SerialNo
                          select new
                          {
                              j.SerialNo
                          };
            if (take != null)
            {
                serials = serials.Take((int)take);
            }

            return Json(serials, JsonRequestBehavior.AllowGet);
        }


        public ActionResult _SerialLookupSubView(string serialNo)
        {
            InvItemViewModel model = LookupInvItem(serialNo);
           
            if (model == null)
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }

            var itemInfo = new
            {
                SerialNo = model.SerialNo,
                DetailPartial = RenderPartialView("InvItemDetails", model)
            };

            return Json(itemInfo, JsonRequestBehavior.AllowGet);
        }

        public ActionResult _CustLocations(int custID)
        {
            var locs = GetCustLocations(custID); // IsisDbContext.CustLocs();
            return Json(locs, JsonRequestBehavior.AllowGet);
        }

        public ActionResult _CstOrdLineLookup(string orderNo, string prodTypeCD)
        {
            var linesQry = from l in PrdnDBContext.CstOrderLines
                           where l.OrderNo.StartsWith(orderNo)
                           select l;

            if (!String.IsNullOrWhiteSpace(prodTypeCD))
            {
                linesQry = from l in linesQry
                           where l.Product.ProdTypeCD == prodTypeCD
                           select l;
            }

            var lines = (from l in linesQry
                         orderby l.OrderNo, l.OrderLine
                         select new
                         {
                             l.OrderNo,
                             l.OrderLine,
                             l.OrderLineID,
                             DeptID = l.Order.DeptID,
                             l.ProdCD,
                             l.PartDescr,
                             l.Product.ParentProdCD,
                             l.Order.CustDeliveryFlag,
                             l.Order.ShipToCustID
                         }).Take(20)
                        .AsEnumerable().Select(i =>
                            new JobOrdLookupViewModel
                            {   
                                CustLoc = i.DeptID,
                                ProdCD = i.ProdCD,
                                ProdDescr = i.PartDescr,
                                ParentProdCD = i.ParentProdCD,
                                OrderNo = i.OrderNo,
                                OrderLine = i.OrderLine,
                                OrderLineID = i.OrderLineID,
                                CustDeliveryFlag = i.CustDeliveryFlag,
                                ShipCustID = i.ShipToCustID
                            }).ToList()
                        ;

            return Json(lines, JsonRequestBehavior.AllowGet);
        }

        public ActionResult _CstOrdNoLookup(string orderNo, string prodTypeCD)
        {
            var ordObj = (from l in PrdnDBContext.CstOrderLines
                          where ((l.OrderNo == orderNo) && (l.Product.ProdTypeCD == prodTypeCD))
                          select new JobOrdLookupViewModel
                          {
                              CustLoc = l.Order.DeptID,
                              ProdCD = l.ProdCD,
                              ProdDescr = l.PartDescr,
                              ParentProdCD = l.Product.ParentProdCD,
                              OrderNo = l.OrderNo,
                              OrderLine = l.OrderLine,
                              CustDeliveryFlag = l.Order.CustDeliveryFlag,
                              ShipCustID = l.Order.ShipToCustID
                          }).FirstOrDefault();

            return Json(ordObj, JsonRequestBehavior.AllowGet);
        }

        public ActionResult _CstOrdTotalLookup(string orderNo)
        {
            decimal? ordTot = PrdnDBContext.GetCstOrdTotal(orderNo) ?? 0;

            var jsonObj = new { value = ordTot, formatted = string.Format("{0:c}", ordTot) };

            return Json(jsonObj, JsonRequestBehavior.AllowGet);
        }

        public ActionResult _OrderShipToLookup(string orderNo)
        {
            OrderShipToInfo info = PrdnDBContext.GetOrderShipToInfo(orderNo);

            if (info != null)
            {
                var jsnInfo = new OrdShipToLookupViewModel
                {
                    OrdNo = info.OrderNo,
                    DropShip = info.DropShip,
                    OrdTot = info.OrderTot,

                    CustID = info.ShipToCustID,
                    CustName = info.ShipToName,
                    Addr1 = info.ShipToAddr1,
                    Addr2 = info.ShipToAddr2,
                    Addr3 = info.ShipToAddr3,
                    Addr4 = info.ShipToAddr4,
                    City = info.ShipToCity,
                    State = info.ShipToState,
                    Postal = info.ShipToPostal,
                    Country = info.ShipToCountry,
                };
                return Json(jsnInfo, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        // Production Runs
        [GridAction]
        public ActionResult _SelectRun(string prdnNo)
        {
            return View(RunGridList(prdnNo));
        }

        /// <summary>
        // Production Jobs
        [GridAction(EnableCustomBinding = true)]
        public ActionResult _SelectStatusJobs(GridCommand command, decimal? id, string statusVals)
        {
            List<string> statusList = PrdnJobStatusViewModel.StoreStatusListStr(Session, statusVals);
            return View(JobGridList(command, id, statusList));
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult _SelectRunJobs(GridCommand command, decimal? id)
        {
            return View(JobGridList(command, id, null));
        }

        [GridAction]
        public ActionResult _JobLookup(JobLookupModel model)
        {
            return View(JobGridLookup(model));
        }

        public ActionResult _UnusedTypes(string prdnOrdNo, string prodTypeCD)
        {
            var unusedTypes = from t in PrdnDBContext.ProductionTypes
                              where !(from r in PrdnDBContext.ProductionRuns
                                      where r.PrdnOrderNo == prdnOrdNo
                                      select r.PrdnTypeID)
                              .Contains(t.ID)
                              select t;

            if (!String.IsNullOrEmpty(prodTypeCD))
            {
                unusedTypes = from t in unusedTypes
                              where t.ProdTypeCD == prodTypeCD
                              select t;
            }

            var typesList = from t in unusedTypes
                            orderby t.SortOrder
                            select new
                            {
                                Id = t.ID,
                                Name = t.Code + " " + t.Description + " (" + t.ProdTypeCD + ")",
                                Code = t.Code,
                                ProdTypeCD = t.ProdTypeCD,
                                UpperTypeCD = t.Code.ToUpper()
                            };

            return Json(typesList, JsonRequestBehavior.AllowGet);
        }
    }
}
