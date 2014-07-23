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
using CST.ISIS.Data;
using CST.ActionFilters;

namespace CST.Prdn.Controllers
{
    [CstAuthorize(Groups = "SCHED/SCHED")]
    public class RequestController : CstControllerBase
    {
        private const string RequestLookupKey = "RequestLookupKey";
       
        public ActionResult List()
        {
            RequestLookupViewModel model = new RequestLookupViewModel();
            model.DefineListDefault();

            UserSettingsViewModel settings = GetUserSettingsViewModel(true);
            model.SettingsModel = settings;
            model.PageSize = (int)(settings.RequestPageSize ?? UserSettingsModel.DefaultPageSize);

            ViewBag.RefreshMsg = "The data refreshes every minute. To force a refresh of the data, click the refresh button on your browser";
            Response.AppendHeader("Refresh", "60");
            return View(model);
        }

        [HttpGet]
        public ActionResult Lookup()
        {
            RequestLookupViewModel model = Session[RequestLookupKey] as RequestLookupViewModel;
            if (model != null)
            {
                model.DefineCriteria = false;
            }
            else
            {
                model = new RequestLookupViewModel();
                model.DefineLookupDefault();
                model.PageSize = GetRequestPageSize();
            }

            //model.DefaultRunModel = MakeDefaultRunViewModel(true);
            UserSettingsViewModel settings = GetUserSettingsViewModel(true);
            model.SettingsModel = settings;
            model.PageSize = (int)(settings.RequestPageSize ?? UserSettingsModel.DefaultPageSize);

            return View(model);
        }

        [HttpPost]
        public ActionResult Lookup(RequestLookupViewModel lookup)
        {
            Session[RequestLookupKey] = lookup;
            return RedirectToAction("Lookup");
        }

        protected IEnumerable<RequestListViewModel> RequestViewList(RequestLookupViewModel lookup)
        {
            var requests = from r in PrdnDBContext.Requests
                           select r;

            if (!string.IsNullOrEmpty(lookup.RequestID))
	        {
                requests = from r in requests
                           where r.ID == lookup.RequestID
                           select r;
            }
            else
            {
                if ((lookup.FromDt != null) || (lookup.ThruDt != null))
                {
                    DateTime afterDt = lookup.FromDt ?? DateTime.Today.Date.AddMonths(-1);
                    afterDt = afterDt.Date;

                    DateTime beforeDt = lookup.ThruDt ?? DateTime.Today.Date;
                    beforeDt = beforeDt.Date.AddDays(1);

                    requests = from r in requests
                               where (r.RequestDt >= afterDt) && (r.RequestDt < beforeDt)
                               select r;
                }

                if (!lookup.AllStauses)
                {
		            List<string> states = new List<string>();
                    if (lookup.StatusProcessing)
                    {
                        states.Add(RequestStatus.PROCESSING.ToString().ToUpper());
                    }
                    if (lookup.StatusConfirmed)
                    {
                        states.Add(RequestStatus.CONFIRMED.ToString().ToUpper());
                    }
                    if (lookup.StatusCanceled)
                    {
                        states.Add(RequestStatus.CANCELED.ToString().ToUpper());
                    }
                    if (lookup.StatusScheduled)
                    {
                        states.Add(RequestStatus.SCHEDULED.ToString().ToUpper());
                    }
                    if ((states.Count()==0) || (lookup.StatusNew))
                    {
                        states.Add(RequestStatus.NEW.ToString().ToUpper());
                    }

                    requests = from r in requests
                               where states.Contains(r.StatusStr)
                               select r;
                }
            }

            requests = from r in requests
                    orderby r.RequestDt descending
                    select r;

            List<RequestListViewModel> viewRequests =
                Mapper.Map<List<Request>, List<RequestListViewModel>>(requests.ToList());

            return viewRequests;
        }

        protected GridModel RequestGridList(RequestLookupViewModel lookup)
        {
            GridModel g = new GridModel(RequestViewList(lookup).ToList());
            return g;
        }

        private const string UndecidedShipDept = "TBD";

        private void LoadRequestModelForOrder(RequestViewModel requestModel)
        {
            if (requestModel == null)
            {
                return;
            }

            if (!String.IsNullOrEmpty(requestModel.OrderNo))
            {
                OrderShipToInfo info = PrdnDBContext.GetOrderShipToInfo(requestModel.OrderNo);

                if ((info != null) && (info.DropShip))
                {
                    requestModel.DropShipOrder = true;
                    requestModel.ShipCustID = info.ShipToCustID;
                    requestModel.ShipToName = info.ShipToName;
                    requestModel.ShipToAddr1 = info.ShipToAddr1;
                    requestModel.ShipToAddr2 = info.ShipToAddr2;
                    requestModel.ShipToAddr3 = info.ShipToAddr3;
                    requestModel.ShipToAddr4 = info.ShipToAddr4;
                    requestModel.ShipToCity = info.ShipToCity;
                    requestModel.ShipToState = info.ShipToState;
                    requestModel.ShipToPostal = info.ShipToPostal;
                    requestModel.ShipToCountry = info.ShipToCountry;
                    requestModel.OrderTotal = info.OrderTot;
                }
                else { requestModel.DropShipOrder = false; }
            }
        }

        protected void LoadRequestData(RequestViewModel requestModel)
        {
            requestModel.ProdImageInfoSet = IsisDbContext.ProdImageInfoSet(requestModel.ProdCD);

            requestModel.ProdOptions = ProdOptDefn.ProdOptions(requestModel.ProdCD, requestModel.ParentProdCD);

            requestModel.WorksheetOptions = WorksheetOpt.Worksheet(requestModel.WorksheetID);


            var fileName = (from a in PrdnDBContext.RequestAttachments
                            where a.ID == requestModel.ID
                            select a.FileName).FirstOrDefault();

            requestModel.HasAttachment = (!String.IsNullOrEmpty(fileName));


            List<SelectListItem> depts = (from d in IsisDbContext.SalesDepts
                                          orderby d.DeptID
                                          select new SelectListItem
                                          {
                                              Value = d.DeptID,
                                              Text = d.DeptID + " " + d.Description
                                          }).ToList();
            depts.Add(new SelectListItem() { Text = "To Be Determined", Value = UndecidedShipDept });

            requestModel.DepartmentList = new SelectList(depts, "Value", "Text");

            if (string.IsNullOrEmpty(requestModel.ShipDeptID))
            {
                requestModel.ShipDeptID = UndecidedShipDept;
            }


            var types = from t in IsisDbContext.SalesShipTypes
                        orderby t.Description
                        select new
                        {
                            Value = t.TypeID,
                            Text = "(" + t.TypeID + ") " + t.Description
                        };

            requestModel.ShipMethodList = new SelectList(types.ToList(), "Value", "Text");

            LoadRequestModelForOrder(requestModel);

            if (String.IsNullOrEmpty(requestModel.ProcessedCstUserID))
            {
                requestModel.ProcessedCstUserID = GetCurrentUserLogin();
            }
        }

        [HttpGet]
        public ActionResult Edit(string id, string urlReturn)
        {
            // get Request
            var request = (from r in PrdnDBContext.Requests.Include("Product")  //.Include("Product.ProductType")
                           where r.ID == id
                           select r).FirstOrDefault();

            if (request == null)
            {
                return ErrMsgView("Request ID '" + id + "' is not valid.");
            }

            RequestViewModel model = Mapper.Map<RequestViewModel>(request);

            LoadRequestData(model);

            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(RequestViewModel model, string urlReturn)
        {
            string statusConf = RequestStatus.CONFIRMED.ToString();
            if (model.StatusStr.Equals(statusConf))
            {
                string confOrdReq = statusConf + " requests require a ";

                if (model.InProduction.Equals("N"))
                { // not shipped from DI
                    if (string.IsNullOrEmpty(model.ShipContact))
                    {
                        ModelState.AddModelError("ShipContact", confOrdReq + "Branch Contact");
                    }
                    if (string.IsNullOrEmpty(model.ShipDeptID) || (model.ShipDeptID.Equals(UndecidedShipDept)))
                    {
                        ModelState.AddModelError("ShipDeptId", confOrdReq + "Shipped From Branch");
                    }
                }
                if (string.IsNullOrEmpty(model.ShipVia))
                {
                    ModelState.AddModelError("ShipVia", confOrdReq + "Shipping Method");
                }

                if (model.ExpArrivalDt == null)
                {
                    ModelState.AddModelError("ExpArrivalDt", confOrdReq + "Expected Arrival Date");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // get Request
                    var request = (from r in PrdnDBContext.Requests
                                   where r.ID == model.ID
                                   select r).FirstOrDefault();

                    if (request == null)
                    {
                        return ErrMsgView("Request ID '" + model.ID + "' is not valid.");
                    }

                    request.StatusStr = model.StatusStr;
                    request.UpdateStatusDtUsr(GetCurrentUserLogin());
                    request.InProduction = model.InProduction;
                    request.SerialNo = model.SerialNo;
                    request.ExpArrivalDt = model.ExpArrivalDt;
                    request.ShipDeptID = model.ShipDeptID.Equals(UndecidedShipDept) ? null : model.ShipDeptID;
                    request.ShipContact = model.ShipContact;
                    request.ShipVia = model.ShipVia;
                    request.TrackingNo = model.TrackingNo;
                    request.ProcComment = model.ProcComment;

                    PrdnDBContext.SaveChanges();
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error updating request: " + ex.Message);
                }
            }

            if (ModelState.IsValid)
            {
                return RedirectIfLocal(urlReturn, () => RedirectToAction("List") );
                //if (urlReturn != null) {return Redirect(urlReturn);} else { return RedirectToAction("List");}
            }
            else
            {
                LoadRequestData(model);
                return View(model);
            }
        }

        public ActionResult Attachment(string id)
        {
            decimal deciID = decimal.Parse(id);

            var att = (from a in PrdnDBContext.RequestAttachments
                       where a.ID == id
                       select new ImageFile
                       {   ID = deciID,
                           Description = a.FileName,
                           FileName = a.FileName,
                           MimeContentType = a.MimeType.ContentType,
                           MimeSubType = a.MimeTypeCD,
                           Data = a.Attachment
                       }).FirstOrDefault();

            return ViewAttachment(att);
        }

        [GridAction]
        public ActionResult _SelectRequest(string id, DateTime? fromDt, DateTime? thruDt, 
            bool allStauses, bool statusNew, bool statusProcessing, bool statusConfirmed, bool statusScheduled, bool statusCanceled)
        {
            RequestLookupViewModel lookup = new RequestLookupViewModel()
            {
                RequestID = id,
                FromDt = fromDt,
                ThruDt = thruDt,
                AllStauses = allStauses,
                StatusNew = statusNew,
                StatusProcessing = statusProcessing,
                StatusConfirmed = statusConfirmed,
                StatusScheduled = statusScheduled,
                StatusCanceled = statusCanceled
            };

            return View(RequestGridList(lookup));
        }

        //public ActionResult _JobRequestLookup(string request)
        //{
        //    string cancSts = RequestState.CANCELED.ToString().ToUpper();
        //    string schedSts = RequestState.SCHEDULED.ToString().ToUpper();

        //    var reqs = (from r in PrdnDBContext.Requests
        //                where (r.ID.StartsWith(request) && r.StatusStr != cancSts && r.StatusStr != schedSts)
        //                select new {   
        //                    r.ID,
        //                    r.SpecialWSStr,
        //                    r.OrderNo,
        //                    r.OrderLine,
        //                    r.RequestDeptID,
        //                    r.ProdCD,
        //                    r.ReqPattern,
        //                    r.ColorDescr,
        //                    r.PartDescr,
        //                    r.ShipBranchVia,
        //                    r.Product.ParentProdCD,
        //                    r.CstOrderLine.Order.CustDeliveryFlag,
        //                    r.CstOrderLine.Order.ShipToCustID
        //                }).Take(20)
        //                .AsEnumerable().Select(i => 
        //                    new JobOrdReqViewModel {
        //                        LookupValue = i.ID,
        //                        LookupLabel = i.ID + " part#:" + i.ProdCD + " branch:" + i.RequestDeptID,
        //                        RequestID = i.ID,
        //                        //SpecialWSDescr = CST.Prdn.Data.Request.CalcSpecialWSDescr(CST.Prdn.Data.Request.CalcSpecWS(i.SpecialWSStr)),
        //                        CustLoc = i.RequestDeptID,
        //                        ShipMethodCD = i.ShipBranchVia,
        //                        ProdCD = i.ProdCD,
        //                        ProdDescr = i.PartDescr,
        //                        ParentProdCD = i.ParentProdCD,
        //                        OrderNo = i.OrderNo,
        //                        OrderLine = i.OrderLine,
        //                        DropShip = (PrdnDataHelper.BoolYN(i.CustDeliveryFlag)),
        //                        ShipCustID = i.ShipToCustID
        //                    }).ToList()
        //               ;

        //    return Json(reqs, JsonRequestBehavior.AllowGet);
        //}

        public ActionResult _WorksheetOptions(string requestID)
        {
            List<WorksheetOpt> wsOptions = WorksheetOpt.ReqWorksheet(requestID);
            return Json(wsOptions, JsonRequestBehavior.AllowGet);
        }
    }
}
