using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using CST.Localization;
using CST.Prdn.Data;
using CST.ISIS.Data;
using System.Web.Mvc;

namespace CST.Prdn.ViewModels
{
    // Jobs  ////////////////////////////
    
    // Edit Job Models  ////////////////////////////

    public class PrdnJobViewModel
    {
        public PrdnJobViewModel()
        {
            PriorityID = 1;
        }

        #region Primitive Properties

        [Display(Name = "ID", ResourceType = typeof(LocalStr))]
        public decimal? ID { get; set; }

        [Required(ErrorMessageResourceName = "RequiredError", ErrorMessageResourceType = typeof(LocalStr))]
        [Display(Name = "ID", ResourceType = typeof(LocalStr))]
        public decimal RunID { get; set; }

        [Display(Name = "ProductionOrder", ResourceType = typeof(LocalStr))]
        public string PrdnOrderNo { get; set; }

        [Display(Name = "ProductionTypeCode", ResourceType = typeof(LocalStr))]
        public string PrdnTypeCode { get; set; }

        [Display(Name = "RunCode", ResourceType = typeof(LocalStr))]
        public string RunCode { get { return PrdnOrderNo + PrdnTypeCode; } }

        //[Required]
        [Range(1, int.MaxValue)]
        [Display(Name = "SeqNo", ResourceType = typeof(LocalStr))]
        public decimal? RunSeqNo { get; set; }

        [Required(ErrorMessageResourceName = "RequiredError", ErrorMessageResourceType = typeof(LocalStr))]
        [Display(Name = "CustID", ResourceType = typeof(LocalStr))]
        public decimal? CustID { get; set; }

        [Display(Name = "Customer", ResourceType = typeof(LocalStr))]
        public string CustName { get; set; }

        [Display(Name = "Customer", ResourceType = typeof(LocalStr))]
        public string CustDisplay { get { return (CustID != null ? CustID + " - " : null) + CustName; } }

        [Display(Name = "Customer", ResourceType = typeof(LocalStr))]
        public string CustCode { get; set; }

        [Required(ErrorMessageResourceName = "RequiredError", ErrorMessageResourceType = typeof(LocalStr))]
        [Display(Name = "Priority", ResourceType = typeof(LocalStr))]
        public decimal PriorityID { get; set; }

        [Required(ErrorMessageResourceName = "RequiredError", ErrorMessageResourceType = typeof(LocalStr))]
        [Display(Name = "ProductCD", ResourceType = typeof(LocalStr))]
        public string ProdCD { get; set; }

        //[Required]
        [Display(Name = "SerialNo", ResourceType = typeof(LocalStr))]
        public string SerialNo { get; set; }

        public string InvItemID { get; set; }

        [Display(Name = "OrderNo", ResourceType = typeof(LocalStr))]
        public string OrderNo { get; set; }

        [Range(1, 1000)]
        [Display(Name = "OrderLine", ResourceType = typeof(LocalStr))]
        public decimal? OrderLine { get; set; }

        [Range(1, 1000)]
        [Display(Name = "OrderLine", ResourceType = typeof(LocalStr))]
        public int? OrderLineInt
        {
            get { return (OrderLine == null) ? (int?)null : decimal.ToInt32((decimal)OrderLine); }
            set
            {
                if (value > 0)
                {
                    OrderLine = (decimal?)value;
                }
                else
                {
                    OrderLine = null;
                }

            }
        }

        public decimal? OrderLineID { get; set; }

        [Display(Name = "PurchaseOrder", ResourceType = typeof(LocalStr))]
        public string PurchaseOrder { get; set; }

        [Required(ErrorMessageResourceName = "RequiredError", ErrorMessageResourceType = typeof(LocalStr))]
        [Display(Name = "CustLocation", ResourceType = typeof(LocalStr))]
        public String CustLocation { get; set; }

        [Display(Name = "CustLocation", ResourceType = typeof(LocalStr))]
        public String CustLocName { get; set; }

        [Display(Name = "CustLocation", ResourceType = typeof(LocalStr))]
        public string CustLocDisplay { get { return (CustLocation != null ? CustLocation + " - " : null) + CustLocName; } }

        [Display(Name = "ShipToCustID", ResourceType = typeof(LocalStr))]
        public String DropShipCustID { get; set; }

        [Display(Name = "ShipToCustName", ResourceType = typeof(LocalStr))]
        public String DropShipCustName { get; set; }

        [Required(ErrorMessageResourceName = "RequiredError", ErrorMessageResourceType = typeof(LocalStr))]
        [Display(Name = "ShipTypeCD", ResourceType = typeof(LocalStr))]
        public String ShipMethodCD { get; set; }

        [Display(Name = "Address1", ResourceType = typeof(LocalStr))]
        public String ShipAddr1 { get; set; }

        [Display(Name = "Address2", ResourceType = typeof(LocalStr))]
        public String ShipAddr2 { get; set; }

        [Display(Name = "Address3", ResourceType = typeof(LocalStr))]
        public String ShipAddr3 { get; set; }

        [Display(Name = "Address4", ResourceType = typeof(LocalStr))]
        public String ShipAddr4 { get; set; }

        [Display(Name = "City", ResourceType = typeof(LocalStr))]
        public String ShipCity { get; set; }

        [Display(Name = "State", ResourceType = typeof(LocalStr))]
        public String ShipState { get; set; }

        [Display(Name = "Postal", ResourceType = typeof(LocalStr))]
        public String ShipPostal { get; set; }

        [Display(Name = "Country", ResourceType = typeof(LocalStr))]
        public String ShipCountry { get; set; }

        [Display(Name = "OrderAmount", ResourceType = typeof(LocalStr))]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        public decimal OrderTotal { get; set; }

        [Display(Name = "PackingListNote", ResourceType = typeof(LocalStr))]
        public String PackingListNote { get; set; }

        [Display(Name = "ScheduleNote", ResourceType = typeof(LocalStr))]
        public String ScheduleNote { get; set; }

        [Range(1, 3)]
        [Display(Name = "RowDeduction", ResourceType = typeof(LocalStr))]
        public decimal? RowDeduct { get; set; }

        [Range(1, 3)]
        [Display(Name = "RowDeduction", ResourceType = typeof(LocalStr))]
        public int? RowDeductInt
        {
            get { return (RowDeduct == null) ? (int?)null : decimal.ToInt32((decimal)RowDeduct); }
            set
            {
                if (value > 0)
                {
                    RowDeduct = (decimal?)value;
                }
                else
                {
                    RowDeduct = null;
                }

            }
        }

        [Display(Name = "Created", ResourceType = typeof(LocalStr))]
        public DateTime? CreatedDt { get; set; }

        [Display(Name = "CreatedBy", ResourceType = typeof(LocalStr))]
        public decimal? CreatedUserID { get; set; }

        [Display(Name = "CreatedBy", ResourceType = typeof(LocalStr))]
        public string CreatedUserLogin { get; set; }

        [Display(Name = "Scheduled", ResourceType = typeof(LocalStr))]
        public DateTime? ScheduledDt { get; set; }

        [Display(Name = "ScheduledBy", ResourceType = typeof(LocalStr))]
        public decimal? ScheduledUserID { get; set; }

        [Display(Name = "ScheduledBy", ResourceType = typeof(LocalStr))]
        public string ScheduledUserLogin { get; set; }

        [Display(Name = "Processing", ResourceType = typeof(LocalStr))]
        public DateTime? ProcessedDt { get; set; }

        [Display(Name = "ProcessedBy", ResourceType = typeof(LocalStr))]
        public decimal? ProcessedUserID { get; set; }

        [Display(Name = "ProcessedBy", ResourceType = typeof(LocalStr))]
        public string ProcessedUserLogin { get; set; }

        [Display(Name = "Completed", ResourceType = typeof(LocalStr))]
        public DateTime? CompletedDt { get; set; }

        [Display(Name = "CompletedBy", ResourceType = typeof(LocalStr))]
        public decimal? CompletedUserID { get; set; }

        [Display(Name = "CompletedBy", ResourceType = typeof(LocalStr))]
        public string CompletedUserLogin { get; set; }

        [Display(Name = "Canceled", ResourceType = typeof(LocalStr))]
        public DateTime? CanceledDt { get; set; }

        [Display(Name = "CanceledBy", ResourceType = typeof(LocalStr))]
        public decimal? CanceledUserID { get; set; }

        [Display(Name = "CanceledBy", ResourceType = typeof(LocalStr))]
        public string CanceledUserLogin { get; set; }

        [Display(Name = "WorksheetID", ResourceType = typeof(LocalStr))]
        public decimal? WorksheetID { get; set; }

        public PrdnJobStatus Status { get; set; } 

        [Display(Name = "Status", ResourceType = typeof(LocalStr))]
        public string StatusDescr { get { return Status.Description(); } }

        [Display(Name = "StatusDate", ResourceType = typeof(LocalStr))]
        public DateTime? StatusDt
        { get {
          return ProductionJob.GetDateFromStatus(Status, CreatedDt, ScheduledDt, ProcessedDt, CompletedDt, CanceledDt);
        } }

        [Display(Name = "StatusDate", ResourceType = typeof(LocalStr))]
        public string StatusTodayDtStr { get {
            return StrTodayDtTm(StatusDt);
        } }

        private string StrTodayDtTm(DateTime? date)
        {
            if (date != null)
            {
                return ((DateTime)date).TodayDtTmStr();
            }
            else
            {
                return null;
            }
        }

        [Display(Name = "RequestID", ResourceType = typeof(LocalStr))]
        public string CstRequestID { get; set; }

        #endregion

        [Display(Name = "ProductDefinition", ResourceType = typeof(LocalStr))]
        public List<ProdOptDefn> ProdOptions { get; set; }

        [Display(Name = "WorksheetDefinition", ResourceType = typeof(LocalStr))]
        public List<WorksheetOpt> WorksheetRows { get; set; }

        public int ReqWSCount { get { return (WorksheetRows != null) ? WorksheetRows.Count : 0; } }

        public bool InvItemExists { get; set; }

        public virtual bool CanDelete { get { return !InvItemExists; } }
    }

    public class ListPrdnJobViewModel : PrdnJobViewModel
    {
        [Display(Name = "Color", ResourceType = typeof(LocalStr))]
        public string ColorStr { get; set; }

        [Display(Name = "Decor", ResourceType = typeof(LocalStr))]
        public string DecorStr { get; set; }

        [Display(Name = "Pattern", ResourceType = typeof(LocalStr))]
        public string PatternStr { get; set; }

        [Display(Name = "Descr", ResourceType = typeof(LocalStr))]
        public string ListDescr { get; set; }

        [Display(Name = "Prio", ResourceType = typeof(LocalStr))]
        public string PriorityCD { get; set; }
    }

    public class EditPrdnJobViewModel : PrdnJobViewModel, IProductViewModel, IEditWorksheetOwner, IAttachedTo
    {
        protected virtual string CaptionVerb()
        {
            return LocalStr.Edit;
        }

        protected virtual string SaveVerb()
        {
            return LocalStr.Save;
        }

        protected virtual string SaveObject()
        {
            return LocalStr.Job;
        }

        public string ViewCaption
        {
            get
            {
                if (String.IsNullOrWhiteSpace(SerialNo))
                {
                    return CaptionVerb() + " " + LocalStr.Job;
                }
                else
                {
                    return CaptionVerb() + " " + SerialNo + " " + StatusDescr;
                }
            }
        }

        public string SaveCaption { get { return SaveVerb() + " " + SaveObject(); } }

        public static bool StatusInList(PrdnJobStatus status, params PrdnJobStatus[] statusList)
        {
            return statusList.Contains(status);
        }

        public bool StatusIn(params PrdnJobStatus[] statusList) 
        {
            return StatusInList(Status, statusList);
        }

        public static bool FromStatusTo(PrdnJobStatus current, PrdnJobStatus desired)
        {
            if (desired == PrdnJobStatus.New)
            {
                return false;
            }
            else if (desired == PrdnJobStatus.Pending)
            {
                return StatusInList(current, PrdnJobStatus.New, PrdnJobStatus.Pending, PrdnJobStatus.Scheduled, PrdnJobStatus.Canceled);
            }
            else if (desired == PrdnJobStatus.Scheduled)
            {
                return StatusInList(current, PrdnJobStatus.New, PrdnJobStatus.Pending, PrdnJobStatus.Scheduled, PrdnJobStatus.Canceled);
            }
            else if (desired == PrdnJobStatus.Processing)
            {
                return false;
            }
            else if (desired == PrdnJobStatus.Completed)
            {
                return false;
            }
            else if (desired == PrdnJobStatus.Canceled)
            {
                return StatusInList(current, PrdnJobStatus.Pending, PrdnJobStatus.Scheduled, PrdnJobStatus.Canceled);
            }
            else
                return false;
        }

        public virtual bool CanStatusTo(PrdnJobStatus desired)
        {
            if (InvItemExists)
            {
                return false;
            }
            if (GetAllowedToStatus(desired))
            {
                return FromStatusTo(Status, desired);
            }
            return false;
        }


        private HashSet<PrdnJobStatus> allowedStatuses = new HashSet<PrdnJobStatus>((PrdnJobStatus[])Enum.GetValues(typeof(PrdnJobStatus)) );

        public bool GetAllowedToStatus(PrdnJobStatus status)
        {
            return allowedStatuses.Contains(status);
        }

        public void SetAllowedToStatus(PrdnJobStatus status, bool allowed)
        {
            if (allowed)
            {
                allowedStatuses.Add(status);
            }
            else
            { 
                allowedStatuses.Remove(status); 
            }
        }

        #region Primitive Properties

        public bool CanEditRun { get; set; }

        [Range(1, int.MaxValue)]
        [Display(Name = "SeqNo", ResourceType = typeof(LocalStr))]
        public int? EditRunSeqNo { get; set; }

        [Required]
        [Display(Name = "Status", ResourceType = typeof(LocalStr))]
        public PrdnJobStatus EditStatus { get; set; }

        [Display(Name = "Run", ResourceType = typeof(LocalStr))]
        public String RunDescription { get; set; }

        [Display(Name = "ProductTypeCode", ResourceType = typeof(LocalStr))]
        public String ProdTypeCD { get; set; }

        [Display(Name = "ProductType", ResourceType = typeof(LocalStr))]
        public string ProdTypeDescr { get; set; }

        [Display(Name = "Parent")]
        public String ParentProdCD { get; set; }

        [Display(Name = "Description", ResourceType = typeof(LocalStr))]
        public String ProdDescr { get; set; }

        [Display(Name = "ShipTypeCD", ResourceType = typeof(LocalStr))]
        public String ShipMethodDescription { get; set; }

        [Display(Name = "Priority", ResourceType = typeof(LocalStr))]
        public String PriorityDescription { get; set; }

        public List<ProdImageInfo> ProdImageInfoSet { get; set; }

        [Display(Name = "DropShip", ResourceType = typeof(LocalStr))]
        public bool DropShip { get; set; }

        public string UrlReturn { get; set; }

        #endregion

        public Func<SelectList> CustListFunc { get; set; }

        public Func<decimal?, SelectList> CustLocsFunc { get; set; }

        public Func<SelectList> ShipCodesFunc { get; set; }

        public Func<SelectList> JobPrioritiesFunc { get; set; }

        //public PrdnRunLookupModel LookupRunModel { get; set; }

        public void loadFromRun(ProductionRun run)
        {
            RunID = run.ID;
            PrdnOrderNo = run.PrdnOrderNo;
            PrdnTypeCode = run.PrdnType.Code;
            RunDescription = run.RunDescr;
            ProdTypeCD = run.PrdnType.ProdTypeCD;
            ProdTypeDescr = run.PrdnType.ProductType.Description;
        }

        [Display(Name = "InvItemDefinition", ResourceType = typeof(LocalStr))]
        public WorksheetEditViewModel EditWorksheet { get; set; }

        public FileAttacher Attacher { get; set; }
    }

    public class ViewPrdnJobViewModel : EditPrdnJobViewModel
    {
        protected override string CaptionVerb()
        {
            return LocalStr.View;
        }

        protected override string SaveObject()
        {
            return LocalStr.Attachment+" "+LocalStr.Changes;
        }
    }

    public class SchedulePrdnJobViewModel : EditPrdnJobViewModel
    {
        protected override string CaptionVerb()
        {
            return LocalStr.Schedule;
        }

        protected override string SaveVerb()
        {
            return CaptionVerb();
        }

        [Display(Name = "Worksheet Required")]
        public string SpecialWSDescr { get; set; }

        [Display(Name = "Attachment")]
        public bool HasAttachment { get; set; }

    }

    public class AddPrdnJobViewModel : EditPrdnJobViewModel
    {
        [Display(Name = "Qty")]
        [Range(1, 200)]
        public int Qty { get; set; }

        protected override string CaptionVerb()
        {
            return LocalStr.Add; 
        }

        protected override string SaveVerb()
        {
            return CaptionVerb();
        }
    }

    // Make new Job utility model
    public class JobSelectRunViewModel
    {
        [Display(Name = "RunCode", ResourceType = typeof(LocalStr))]
        public string RunCode { get; set; }

        [Display(Name = "RunID", ResourceType = typeof(LocalStr))]
        public int? RunID { get; set; }

        [Display(Name = "Descr", ResourceType = typeof(LocalStr))]
        public string RunDescr { get; set; }

        [Display(Name = "ProductType", ResourceType = typeof(LocalStr))]
        public string ForProdTypeCD { get; set; }

        [Display(Name = "ProductCD", ResourceType = typeof(LocalStr))]
        public string ForProdCD { get; set; }

        [Display(Name = "Product", ResourceType = typeof(LocalStr))]
        public string ForProdDescr { get; set; }

        [Display(Name = "OrderNo", ResourceType = typeof(LocalStr))]
        public string OrderNo { get; set; }

        [Display(Name = "ShipDate", ResourceType = typeof(LocalStr))]
        public string ShipDtStr { get; set; }

        [Display(Name = "TypeCode", ResourceType = typeof(LocalStr))]
        public string TypeCD { get; set; }

        [Display(Name = "Type", ResourceType = typeof(LocalStr))]
        public string TypeDescr { get; set; }

        [Display(Name = "ProductTypeCode", ResourceType = typeof(LocalStr))]
        public string ProdTypeCD { get; set; }

        [Display(Name = "ProductType", ResourceType = typeof(LocalStr))]
        public string ProdTypeDescr { get; set; }

        [Display(Name = "New Run Type")]
        public int? NewRunTypeID { get; set; }

        [Display(Name = "New Run Note")]
        public string NewRunNote { get; set; }

        public string UrlReturn { get; set; }

        public UserSettingsViewModel SettingsModel { get; set; }
    }

    // Lookup Job Models  ////////////////////////////

    public class JobLookupModel
    {
        [Display(Name = "Customer", ResourceType = typeof(LocalStr))]
        public decimal? CustID { get; set; }

        public IEnumerable<SelectListItem> CustList { get; set; }

        [Display(Name = "CustLocation", ResourceType = typeof(LocalStr))]
        public String CustLocation { get; set; }

        [Display(Name = "OrderNo", ResourceType = typeof(LocalStr))]
        public string OrderNo { get; set; }

        [Display(Name = "PurchaseOrder", ResourceType = typeof(LocalStr))]
        public string PurchaseOrder { get; set; }

        [Display(Name = "RequestID", ResourceType = typeof(LocalStr))]
        public string CstRequestID { get; set; }

        [Display(Name = "ShipToCustID", ResourceType = typeof(LocalStr))]
        public string DropShipCustID { get; set; }

        [Display(Name = "SerialNo", ResourceType = typeof(LocalStr))]
        public string SerialNo { get; set; }

        [Display(Name = "ProductCD", ResourceType = typeof(LocalStr))]
        public string ProdCD { get; set; }

        [Display(Name = "ProductionOrder", ResourceType = typeof(LocalStr))]
        public string PrdnOrderNo { get; set; }

        [Display(Name = "ProductionType", ResourceType = typeof(LocalStr))]
        public int? PrdnTypeID { get; set; }

        public IEnumerable<SelectListItem> PrdnTypes { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "From", ResourceType = typeof(LocalStr))]
        public DateTime? FromShipDt { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Thru", ResourceType = typeof(LocalStr))]
        public DateTime? ThruShipDt { get; set; }

        [Display(Name = "Sequence", ResourceType = typeof(LocalStr))]
        public int? RunSeqNo { get; set; }

        [Display(Name = "JobID", ResourceType = typeof(LocalStr))]
        public int? JobID { get; set; }

        public bool AnyCriteria { get { 
            return 
                (CustID != null)
                || !String.IsNullOrWhiteSpace(CustLocation)
                || !String.IsNullOrWhiteSpace(OrderNo)
                || !String.IsNullOrWhiteSpace(PurchaseOrder)
                || !String.IsNullOrWhiteSpace(CstRequestID)
                || !String.IsNullOrWhiteSpace(DropShipCustID)
                || !String.IsNullOrWhiteSpace(SerialNo)
                || !String.IsNullOrWhiteSpace(ProdCD)
                || !String.IsNullOrWhiteSpace(PrdnOrderNo)
                || (PrdnTypeID != null)
                || (FromShipDt != null)
                || (ThruShipDt != null)
                || (RunSeqNo != null)
                || (JobID != null)
                ; 
        } }
    }

    // Lookup job result list model
    public class PrdnJobListViewModel
    {
        [Display(Name = "PONo", ResourceType = typeof(LocalStr))]
        public string PrdnOrderNo { get; set; }

        [Display(Name = "ShipDate", ResourceType = typeof(LocalStr))]
        public DateTime? PrdnShipDay { get; set; }

        [Display(Name = "ShipDate", ResourceType = typeof(LocalStr))]
        public string PrdnShipShortDtStr
        {
            get
            {
                if (PrdnShipDay == null)
                {
                    return null; //var x = LocalStr.
                }
                return ((DateTime)PrdnShipDay).ToString(System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern);
            }
        }

        [Display(Name = "RunID", ResourceType = typeof(LocalStr))]
        public int? RunID { get; set; }

        [Display(Name = "Type", ResourceType = typeof(LocalStr))]
        public string PrdnTypeCD { get; set; }

        [Display(Name = "ID", ResourceType = typeof(LocalStr))]
        public int? JobID { get; set; }

        [Display(Name = "SeqNo", ResourceType = typeof(LocalStr))]
        public int? RunSeqNo { get; set; }

        [Display(Name = "SerialNo", ResourceType = typeof(LocalStr))]
        public string SerialNo { get; set; }

        [Display(Name = "Product", ResourceType = typeof(LocalStr))]
        public string ProdCD { get; set; }

        [Display(Name = "Description", ResourceType = typeof(LocalStr))]
        public String ProdDescr { get; set; }

        [Display(Name = "Pattern", ResourceType = typeof(LocalStr))]
        public string PatternStr { get; set; }

        [Display(Name = "Color", ResourceType = typeof(LocalStr))]
        public string ColorStr { get; set; }

        public PrdnJobStatus Status { get; set; }

        [Display(Name = "Status", ResourceType = typeof(LocalStr))]
        public string StatusDescr { get { return Status.Description(); } }

        [Display(Name = "StatusDate", ResourceType = typeof(LocalStr))]
        public DateTime? StatusDt  { get; set; }

        [Display(Name = "StatusDate", ResourceType = typeof(LocalStr))]
        public string StatusShortDtStr
        {
            get
            {
                if (StatusDt == null)
                {
                    return null;
                }
                return ((DateTime)StatusDt).ToString(System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern);
            }
        }

        [Display(Name = "Customer", ResourceType = typeof(LocalStr))]
        public string CustCode { get; set; }

        [Display(Name = "CustLocation", ResourceType = typeof(LocalStr))]
        public String CustLocation { get; set; }
    }

    // Job Status Filter Model /////////////////////
    public class PrdnJobStatusViewModel
    {
        public PrdnJobStatusViewModel()
        {
            PendingAllowed = true;
            Pending = true;
            ScheduledAllowed = true;
            Scheduled = true;
            ProcessingAllowed = true;
            Processing = true;
            CompletedAllowed = true;
            Completed = true;
            CanceledAllowed = true;
            Canceled = true;
        }

        //public PrdnJobStatusViewModel(HttpSessionStateBase session)
        //    : this()
        //{
        //    AssignFromSession(session);
        //}

        public void AssignFromSession(HttpSessionStateBase session)
        {
            Dictionary<string, bool> dict = RetrieveStatusDict(session);
            if (dict.Count() == 0)
            {
                return;
            }
            Pending = GetStatusProdVal(PendingAllowed, dict, this.FullPropertyName(x => x.Pending));
            Scheduled = GetStatusProdVal(ScheduledAllowed, dict, this.FullPropertyName(x => x.Scheduled));
            Processing = GetStatusProdVal(ProcessingAllowed, dict, this.FullPropertyName(x => x.Processing));
            Completed = GetStatusProdVal(CompletedAllowed, dict, this.FullPropertyName(x => x.Completed));
            Canceled = GetStatusProdVal(CanceledAllowed, dict, this.FullPropertyName(x => x.Canceled));
            //if (PendingAllowed) { Pending = (Array.IndexOf(statuses, this.FullPropertyName(x => x.Pending).ToUpper()) != -1);}
        }

        public List<string> DBStatusList
        {
            get
            {
                List<string> list = new List<string>();

                if (PendingAllowed && Pending)
                {
                    list.Add(this.FullPropertyName(x => x.Pending).ToUpper());
                }
                if (ScheduledAllowed && Scheduled)
                {
                    list.Add(this.FullPropertyName(x => x.Scheduled).ToUpper());
                }
                if (ProcessingAllowed && Processing)
                {
                    list.Add(this.FullPropertyName(x => x.Processing).ToUpper());
                }
                if (CompletedAllowed && Completed)
                {
                    list.Add(this.FullPropertyName(x => x.Completed).ToUpper());
                }
                if (CanceledAllowed && Canceled)
                {
                    list.Add(this.FullPropertyName(x => x.Canceled).ToUpper());
                }
                return list;
            }
        }

        protected static bool GetStatusProdVal(bool allowed, Dictionary<string, bool> dict, string key)
        {
            if (!allowed)
            {
                return false;
            }
            bool include;
            if (dict.TryGetValue(key, out include))
            {
                return include;
            }
            else return true;
        }

        public static string[] UpperStatuses(string statusStr)
        {
            return statusStr.ToUpper().Split(new Char[] { ',' });
        }

        public bool PendingAllowed { get; set; }
        [Display(Name = "Pending", ResourceType = typeof(LocalStr))]
        public bool Pending { get; set; }

        public bool ScheduledAllowed { get; set; }
        [Display(Name = "Scheduled", ResourceType = typeof(LocalStr))]
        public bool Scheduled { get; set; }

        public bool ProcessingAllowed { get; set; }
        [Display(Name = "Processing", ResourceType = typeof(LocalStr))]
        public bool Processing { get; set; }

        public bool CompletedAllowed { get; set; }
        [Display(Name = "Completed", ResourceType = typeof(LocalStr))]
        public bool Completed { get; set; }

        public bool CanceledAllowed { get; set; }
        [Display(Name = "Canceled", ResourceType = typeof(LocalStr))]
        public bool Canceled { get; set; }

        private const string StsListKey = "_StsListKey";

        public static List<string> StoreStatusListStr(HttpSessionStateBase session, string statusVals)
        {
            List<string> statusList = new List<string>();
            Dictionary<string, bool> statusDict = null;
            if (!String.IsNullOrWhiteSpace(statusVals))
            {
                var col = HttpUtility.ParseQueryString(statusVals);
                statusDict = new Dictionary<string, bool>();
                string statusStr;
                bool include;
                foreach (string key in col)
                {
                    statusStr = key.Trim(); 
                    try {
                        include = bool.Parse(col[key]);
	                } catch (Exception) {
                        include = false;
                    }
                    statusDict.Add(statusStr, include);
                    if (include)
                    {
                        statusList.Add(statusStr.ToUpper());
                    }
                }
            }
            session[StsListKey] = statusDict;
            return statusList;
        }

        public static Dictionary<string, bool> RetrieveStatusDict(HttpSessionStateBase session)
        {
            Dictionary<string, bool> statusDict = session[StsListKey] as Dictionary<string, bool>;
            if (statusDict != null)
            {
                return statusDict;
            }
            else
            {
                return new Dictionary<string, bool>();
            }
        }

        public List<string> RetrieveDBStatusList(HttpSessionStateBase session)
        {
            AssignFromSession(session);
            return DBStatusList;
        }
    }

    public class PrdnOrderMonth 
    {
        public PrdnOrderMonth(string year, string month, PrdnEntities prdnDBContext)
        {
            Cal = ProductionCalendar.MakeProductionCalendar(year, month, prdnDBContext); 
        }

        public ProductionCalendar Cal {get; set;}

        public int Year
        {
            get { return Cal.Year; }
        }

        public int Month
        {
            get { return Cal.Month; }
        }

        public int JobPageSize { get; set; }
    }
}