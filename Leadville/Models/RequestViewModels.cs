using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using CST.Prdn.Data;
using CST.ISIS.Data;
using CST.Localization;

namespace CST.Prdn.ViewModels
{
    public class RequestLookupViewModel
    {
        public RequestLookupViewModel()
        {
            PageSize = UserSettingsModel.DefaultPageSize;
        }

        [Display(Name = "Define Lookup")]
        public bool DefineCriteria { get; set; }

        [Display(Name = "ID")]
        public string RequestID { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "From Date")]
        public DateTime? FromDt { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Thru Date")]
        public DateTime? ThruDt { get; set; }

        [Display(Name = "All")]
        public bool AllStauses { get; set; }

        [Display(Name = "New")]
        public bool StatusNew { get; set; }

        [Display(Name = "Processing")]
        public bool StatusProcessing { get; set; }

        [Display(Name = "Confirmed")]
        public bool StatusConfirmed { get; set; }

        [Display(Name = "Canceled")]
        public bool StatusCanceled { get; set; }

        [Display(Name = "Scheduled")]
        public bool StatusScheduled { get; set; }

        public string CriteriaDescription
        {
            get
            {
                List<string> criteria = new List<string>();

                criteria.Add("Lookup Criteria:");
                if (string.IsNullOrEmpty(RequestID))
                {
                    if (AllStauses)
                    {
                        criteria.Add("All Statuses");
                    }
                    else
                    {
                        List<string> statuses = new List<string>();
                        if (StatusNew)
                        {
                            statuses.Add(RequestStatus.NEW.ToString());
                        }
                        if (StatusProcessing)
                        {
                            statuses.Add(RequestStatus.PROCESSING.ToString());
                        }
                        if (StatusConfirmed)
                        {
                            statuses.Add(RequestStatus.CONFIRMED.ToString());
                        }
                        if (StatusCanceled)
                        {
                            statuses.Add(RequestStatus.CANCELED.ToString());
                        }
                        if (StatusScheduled)
                        {
                            statuses.Add(RequestStatus.SCHEDULED.ToString());
                        }
                        criteria.Add("Statuses=" + string.Join(",", statuses));
                    }

                    if (FromDt != null)
                    {
                        criteria.Add(FromDt.HasValue
                            ? "From Date=" + FromDt.Value.ToString("d", CultureInfo.CurrentUICulture)
                            : string.Empty);
                    }
                    if (ThruDt != null)
                    {
                        criteria.Add(ThruDt.HasValue
                            ? "Thru Date=" + ThruDt.Value.ToString("d", CultureInfo.CurrentUICulture)
                            : string.Empty);
                    }
                }
                else
                {
                    criteria.Add("Request ID=" + RequestID);
                }

                return string.Join(" ", criteria);
            }
        }

        public void DefineListDefault()
        {
            DefineCriteria = true;

            FromDt = null;
            ThruDt = null;

            AllStauses = false;
            StatusNew = true;
            StatusProcessing = true;
            StatusConfirmed = false;
            StatusCanceled = false;
        }

        public void DefineLookupDefault()
        {
            DefineCriteria = true;

            FromDt = DateTime.Today.Date.AddMonths(-1);
            ThruDt = DateTime.Today.Date;

            AllStauses = false;
            StatusNew = true;
            StatusProcessing = true;
            StatusConfirmed = false;
            StatusCanceled = false;
        }

        public int PageSize { get; set; }

        public UserSettingsViewModel SettingsModel { get; set; }
    }
    
    public class RequestViewModel
    {
        [Display(Name = "Request ID")]
        public string ID { get; set; }

        [Display(Name = "Date Requested")]
        public DateTime RequestDt { get; set; }

        [Display(Name = "Requested By")]
        public string RequestCstUserID { get; set; }

        public string SoldItemFlag { get; set; }
        [Display(Name = "Sold/Stock")]
        public string SoldStockStr { get; set; }

        [Display(Name = "Date Required")]
        public DateTime RequiredDt { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Expected Arrival Date")]
        public DateTime? ExpArrivalDt { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Processed")]
        public DateTime? ProcessedDt { get; set; }

        public string ProcessedCstUserID { get; set; }
        public string ProcComment { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Confirmed")]
        public DateTime? ConfirmDt { get; set; }
        public string ConfirmCstUserID { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Canceled")]
        public DateTime? CancelDt { get; set; }
        public string CancelCstUserID { get; set; }

        public TimeSpan Elapsed { get; set; }

        [Display(Name = "Order #")]
        public string OrderNo { get; set; }
        
        public string OrderBU { get; set; }

        [Display(Name = "Order Line #")]
        public decimal? OrderLine { get; set; }

        [Display(Name = "Part #")]
        public string ProdCD { get; set; }

        [Display(Name = "Description")]
        public string PartDescr { get; set; }
        
        [Display(Name = "Pattern")]
        public string ReqPattern { get; set; }
        public string ColorDescr { get; set; }

        [Display(Name = "C#")]
        public string SerialNo { get; set; }

        [Display(Name = "Request Comments")]
        public string RequestComment { get; set; }
        
        [Display(Name = "Branch")]
        public string RequestDeptID { get; set; }
        public string RequestDeptSetid { get; set; }
        
        public string SpecialWSStr { get; set; }
        public RequestSpecWS SpecialWS { get; set; }

        [Display(Name = "Worksheet Required")]
        public string SpecialWSDescr { get; set; }

        [Display(Name = "Status")]
        public string StatusStr { get; set; }

        [Display(Name = "Status")]
        public RequestStatus Status { get; set; }

        public SelectList StatusList()
        {
            var statuses = from RequestStatus s in Enum.GetValues(typeof(RequestStatus))
                           select new { Id = s.ToString().ToUpper(), Name = s.ToString().ToUpper() };
            return new SelectList(statuses, "Id", "Name", this.Status);
        }

        public SelectList DepartmentList { get; set; }
        
        public SelectList ShipMethodList { get; set; }

        [Display(Name = "Branch Contact")]
        public string ShipContact { get; set; }
        
        [Display(Name = "Shipped From Branch")]
        public string ShipDeptID { get; set; }

        public string ShipDeptSetid { get; set; }

        [Display(Name = "Shipping Method")]
        public string ShipVia { get; set; }

        public string ShipViaSetid { get; set; }

        [Display(Name = "Tracking Number")]
        public string TrackingNo { get; set; }

        [Display(Name = "Produced at DIa")]
        public string InProduction { get; set; }

        [Display(Name = "Requested Ship Via")]
        public string ShipBranchVia { get; set; }
        public string ShipBranchViaSetid { get; set; }
        
        public string ModifiedKitFlag { get; set; }
        public bool ModifiedKit { get; set; }
        
        [Display(Name = "Modified Kit")]
        public string ModifiedKitStr
        {
            get
            {
                if (ModifiedKit)
                {
                    return "Yes";
                }
                else 
                {
                    return "No";
                }
            }
        }

        [Display(Name = "Interior Color")]
        public string InteriorColor { get; set; }
        
        public decimal? WorksheetID { get; set; }

        [Display(Name = "Drop Ship")]
        public bool DropShipOrder { get; set; }

        [Display(Name = "Customer ID")]
        public string ShipCustID { get; set; }

        [Display(Name = "Drop Ship Customer")]
        public string ShipToName { get; set; }

        [Display(Name = "Addr1")]
        public string ShipToAddr1 { get; set; }

        [Display(Name = "Addr2")]
        public string ShipToAddr2 { get; set; }

        [Display(Name = "Addr3")]
        public string ShipToAddr3 { get; set; }

        [Display(Name = "Addr4")]
        public string ShipToAddr4 { get; set; }

        [Display(Name = "City")]
        public string ShipToCity { get; set; }

        [Display(Name = "State")]
        public string ShipToState { get; set; }

        [Display(Name = "Postal")]
        public string ShipToPostal { get; set; }

        [Display(Name = "Country")]
        public string ShipToCountry { get; set; }

        [Display(Name = "Order Amount")]
        [DisplayFormat(DataFormatString="{0:C}")]
        public decimal OrderTotal { get; set; }

        [Display(Name = "Attachment")]
        public bool HasAttachment { get; set; }

        [Display(Name = "Parent")]
        public string ParentProdCD { get; set; }

        public string UrlReturn { get; set; }

        public List<ProdImageInfo> ProdImageInfoSet { get; set; }

        [Display(Name = "Product Definition")]
        public List<ProdOptDefn> ProdOptions { get; set; }

        [Display(Name = "Worksheet Details")]
        public List<WorksheetOpt> WorksheetOptions { get; set; }
    }

    public class RequestListViewModel
    {
        [Display(Name = "Sold/Stock")]
        public string SoldStockStr { get; set; }

        [Display(Name = "Request ID")]
        public string ID { get; set; }

        [Display(Name = "Order #")]
        public string OrderNo { get; set; }

        [Display(Name = "Elapsed Time")]
        public TimeSpan Elapsed { get; set; }

        [Display(Name = "Elapsed Time")]
        public string ElapsedStr
        { 
            get {
                if (Elapsed.Days > 0)
                {
                    return string.Format("{0:D2} day(s), {1:D2}:{2:D2}:{3:D2}", Elapsed.Days, Elapsed.Hours, Elapsed.Minutes, Elapsed.Seconds);
                }
                else
                {
                    return string.Format("{0:D2}:{1:D2}:{2:D2}", Elapsed.Hours, Elapsed.Minutes, Elapsed.Seconds);
                }
            } 
        }

        [Display(Name = "Date Requested")]
        public DateTime RequestDt { get; set; }

        [Display(Name = "Date Required")]
        public DateTime RequiredDt { get; set; }

        [Display(Name = "Expected Arrival Date")]
        public DateTime? ExpArrivalDt { get; set; }

        [Display(Name = "Branch")]
        public string RequestDeptID { get; set; }

        [Display(Name = "Part#")]
        public string ProdCD { get; set; }

        [Display(Name = "Pattern")]
        public string ReqPattern { get; set; }

        [Display(Name = "Part Description")]
        public string PartDescr { get; set; }

        [Display(Name = "Color")]
        public string ColorDescr { get; set; }

        [Display(Name = "Description")]
        public string ListDescr
        {
            get
            {
                return PartDescr + " " + ColorDescr;
            }
        }

        [Display(Name = "Requested By")]
        public string RequestCstUserID { get; set; }

        [Display(Name = "Status")]
        public string StatusStr { get; set; }

        [Display(Name = "Processed Date")]
        public DateTime? ProcessedDt { get; set; }

        [Display(Name = "Processed User")]
        public string ProessedCstUserID { get; set; }

        [Display(Name = "Processed")]
        public string ProcDtUser
        {
            get {
                return ((ProcessedDt != null) ? ((DateTime)ProcessedDt).ToString(CultureInfo.CurrentUICulture) : String.Empty)
                       + " / " +
                       ProessedCstUserID;
            }
        }

        [Display(Name = "Confirmed Date")]
        public DateTime? ConfirmDt { get; set; }

        [Display(Name = "Confirmed User")]
        public string ConfirmCstUserID { get; set; }

        [Display(Name = "Confirmed")]
        public string ConfDtUser
        {
            get
            {
                return ((ConfirmDt != null) ? ((DateTime)ConfirmDt).ToString(CultureInfo.CurrentUICulture) : String.Empty)
                       + " / " +
                       ConfirmCstUserID;
            }
        }
       
    }

    public class RequestScheduleViewModel
    {
        [Display(Name = "RequestID", ResourceType = typeof(LocalStr))]
        public string RequestID { get; set; }

        [Display(Name = "OrderNo", ResourceType = typeof(LocalStr))]
        public string OrderNo { get; set; }

        public SelectList PossibleRuns { get; set; }

        [Display(Name = "RunID", ResourceType = typeof(LocalStr))]
        public int? RunID { get; set; }

        [Display(Name = "Descr", ResourceType = typeof(LocalStr))]
        public string RunDescr { get; set; }

        [Display(Name = "ProductTypeCode", ResourceType = typeof(LocalStr))]
        public string ProdTypeCD { get; set; }

        [Display(Name = "ProductTypeCode", ResourceType = typeof(LocalStr))]
        public string ProdTypeDescr { get; set; }

        [Display(Name = "ProductCD", ResourceType = typeof(LocalStr))]
        public string ProdCD { get; set; }

        [Display(Name = "Part Description")]
        public string ProdDescr { get; set; }

        [Display(Name = "Requested Part")]
        public string ReqPartDescr { get {
            return "(" + ProdCD + ") " + ProdDescr + " (" + ProdTypeCD + ") " + ProdTypeDescr;
        }  }

        public string UrlReturn { get; set; }

        public UserSettingsViewModel SettingsModel { get; set; }
    }
}