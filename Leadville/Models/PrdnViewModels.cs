using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;
using CST.ISIS.Data;
using CST.Prdn.Data;
using CST.Localization;

namespace CST.Prdn.ViewModels
{
    public class IndexModel
    {
        public UserSettingsViewModel SettingsModel { get; set; }
    }

    public class UserSettingsViewModel
    {
        public UserSettingsViewModel()
        {
            JobPageSize = UserSettingsModel.DefaultPageSize;
            RequestPageSize = UserSettingsModel.DefaultPageSize;
        }

        [Display(Name = "ID", ResourceType = typeof(LocalStr))]
        public decimal UserID { get; set; }

        [Display(Name = "Login", ResourceType = typeof(LocalStr))]
        public string Login { get; set; }

        [Display(Name = "OrderNo", ResourceType = typeof(LocalStr))]
        public string DefaultRunOrderNo { get; set; }

        [Display(Name = "RunID", ResourceType = typeof(LocalStr))]
        public decimal? DefaultRunID { get; set; }

        [Display(Name = "DefaultRun", ResourceType = typeof(LocalStr))]
        public string DefaultRunDescr { get; set; }

        [Required(ErrorMessageResourceName = "RequiredError", ErrorMessageResourceType = typeof(LocalStr))]
        [Display(Name = "JobsPageSize", ResourceType = typeof(LocalStr))]
        public int? JobPageSize { get; set; }

        [Required(ErrorMessageResourceName = "RequiredError", ErrorMessageResourceType = typeof(LocalStr))]
        [Display(Name = "Requests Page Size")]
        public int? RequestPageSize { get; set; }

        public bool Requests { get; set; }

        [Display(Name = "LabelPrinter", ResourceType = typeof(LocalStr))]
        public int? LabelPrinterID { get; set; }

        [Display(Name = "LabelPrinter", ResourceType = typeof(LocalStr))]
        public string LabelPrinterDisplay { get; set; }

        public bool UserEditable { get; set; }

        [HiddenInput(DisplayValue = false)]
        public string urlReturn { get; set; }

    }

    public class UserSettingsEditViewModel : UserSettingsViewModel
    {
        public UserSettingsEditViewModel() : base() { }

        public bool CanCreateRun { get; set; }

        [Display(Name = "OrderNo", ResourceType = typeof(LocalStr))]
        public string NewRunOrderNo { get; set; }

        [Display(Name = "Type", ResourceType = typeof(LocalStr))]
        public decimal? NewRunPrdnTypeID { get; set; }

        [Display(Name = "Note", ResourceType = typeof(LocalStr))]
        public string NewRunNote { get; set; }

        [Display(Name = "LabelPrinter", ResourceType = typeof(LocalStr))]
        public SelectList LabelPrinters { get; set; }

    }

    ///
    // Production Orders 
    public class PrdnOrdViewModel
    {
        [Display(Name = "OrderNo", ResourceType = typeof(LocalStr))]
        public string OrderNo { get; set; }

        [Display(Name = "ShipDate", ResourceType = typeof(LocalStr))]
        [DataType(DataType.Date)]
        public DateTime ShipDay { get; set; }

        [Display(Name = "ShipDate", ResourceType = typeof(LocalStr))]
        public string ShipDtStr { get; set; }

        public int TypeCount { get; set; }
    }

    public class PrdnOrdListModel
    {
        [Display(Name = "OrderNo", ResourceType = typeof(LocalStr))]
        public string PrdnOrdNo { get; set; }

        [Display(Name = "ShipDate", ResourceType = typeof(LocalStr))]
        [DataType(DataType.Date)]
        public DateTime OrdShipDay { get; set; }

        [Display(Name = "ShipDate", ResourceType = typeof(LocalStr))]
        public string OrdShipDtStr { get; set; }

        [Display(Name = "RunCount", ResourceType = typeof(LocalStr))]
        public int RunCount { get; set; }

        [Display(Name = "ProductionOrder", ResourceType = typeof(LocalStr))]
        public string PrdnOrdDescr { get { 
            return PrdnOrdNo + " " + OrdShipDtStr + " " + LocalStr.RunCount + " " + RunCount.ToString();
        }}
    }

    ///
    // Production Runs
    public class PrdnRunViewModel
    {
        [Display(Name = "RunID", ResourceType = typeof(LocalStr))]
        public decimal ID { get; set; }

        [Display(Name = "OrderNo", ResourceType = typeof(LocalStr))]
        public string PrdnOrderNo { get; set; }

        [Display(Name = "Type", ResourceType = typeof(LocalStr))]
        public decimal PrdnTypeID { get; set; }

        [Display(Name = "ProductionTypeCode", ResourceType = typeof(LocalStr))]
        public string PrdnTypeCode { get; set; }

        [Display(Name = "ProductionType", ResourceType = typeof(LocalStr))]
        public string PrdnTypeDescr { get; set; }

        [Display(Name = "ProductionType", ResourceType = typeof(LocalStr))]
        public string PrdnTypeCdDescr { get { return PrdnTypeCode + " " + PrdnTypeDescr; } }

        [Display(Name = "Note", ResourceType = typeof(LocalStr))]
        public string Description { get; set; }

        [Display(Name = "HasJobs", ResourceType = typeof(LocalStr))]
        public bool HasJobs { get; set; }

        [Display(Name = "ProductTypeCode", ResourceType = typeof(LocalStr))]
        public string ProdTypeCD { get; set; }

        [Display(Name = "ProductType", ResourceType = typeof(LocalStr))]
        public string ProdTypeDescr { get; set; }

        [Display(Name = "ProductType", ResourceType = typeof(LocalStr))]
        public string ProdTypeCdDescr
        {
            get
            {
                if (ProdTypeCD == null)
                {
                    return "-";
                }
                else
                {
                    return "(" + ProdTypeCD + ") " + ProdTypeDescr;
                }
            }
        }
    }

    public class PrdnOrdRunListModel
    {
        public PrdnOrdRunListModel()
        {
            LkupPrdnRunID = String.Empty;
            LkupPrdnTypeCD = String.Empty;
        }

        [Display(Name = "OrderNo", ResourceType = typeof(LocalStr))]
        public string LkupPrdnOrdNo { get; set; }

        [Display(Name = "ShipDate", ResourceType = typeof(LocalStr))]
        public string LkupShipDtStr { get; set; }

        [Display(Name = "RunID", ResourceType = typeof(LocalStr))]
        public string LkupPrdnRunID { get; set; }

        [Display(Name = "RunCode", ResourceType = typeof(LocalStr))]
        public string LkupRunCode
        {
            get
            {
                return LkupPrdnOrdNo + LkupPrdnTypeCD;
            }
        }

        [Display(Name = "Run", ResourceType = typeof(LocalStr))]
        public string LkupRunDescr
        {
            get
            {
                return LkupRunCode + " " + LkupPrdnTypeDescr + " " + LkupShipDtStr;
            }
        }

        [Display(Name = "TypeCode", ResourceType = typeof(LocalStr))]
        public string LkupPrdnTypeCD { get; set; }

        [Display(Name = "Type", ResourceType = typeof(LocalStr))]
        public string LkupPrdnTypeDescr { get; set; }

        [Display(Name = "ProductType", ResourceType = typeof(LocalStr))]
        public string LkupProdTypeCD { get; set; }
    }

    ///
    // Id Code Name Active
    public class IdCodeNameActive
    {
        public IdCodeNameActive()
        {
            Active = true;
        }

        [Display(Name = "ID")]
        public decimal ID { get; set; }

        [Display(Name = "Code")]
        [Remote("ValidAppCode", "Admin", ErrorMessage="Reserved {0}, not allowed")]
        public string Code { get; set; }

        [Display(Name = "Name")]
        public string Name { get; set; }

        [Display(Name = "Sys Admin")]
        public bool SysAdmin { get; set; }

        [Display(Name = "Active")]
        public bool Active { get; set; }

        public string CodeDashName { get { return Code + "-" + Name; } }
    }

    ///
    // Reasons
    public class PrdnReasonViewModel : IdCodeNameActive
    {
        [Display(Name = "Description")]
        public String Description { get; set; }
    }

    ///
    // Manufacturers
    public class PrdnMfgrViewModel : IdCodeNameActive
    {
        [Display(Name = "CST Vendor ID")]
        public string CstVendorID { get; set; }
    }

    ///
    // Locations
    public class PrdnLocViewModel : IdCodeNameActive
    {
        [Display(Name = "Manufacturer ID")]
        public Decimal MfgrID { get; set; }
    }

    public class LocationViewModel
    {
        [Display(Name = "Manufacturer")]
        public string MfgrID { get; set; }

        public SelectList MfgrList { get; set; }
    }

    ///
    // Customers
    public class PrdnCustViewModel : IdCodeNameActive
    {
    }

    ///
    // Categories
    public class ProdCtgryViewModel
    {
        public ProdCtgryViewModel()
        {
            Active = true;
        }

        [Display(Name = "ID")]
        public Int32 CtgryID { get; set; }

        [Display(Name = "Code")]
        public String CtgryCode { get; set; }

        [Display(Name = "Name")]
        public String CtgryName { get; set; }

        [Display(Name = "Product Type")]
        public String FgProdTypeCd { get; set; }

        [Display(Name = "Active")]
        public Boolean Active { get; set; }
    }

    public class CtgryViewModel
    {
        public SelectList FgProdTypes { get; set; }
    }

    ///
    // Production Types
    public class PrdnTypeViewModel : IdCodeNameActive
    {
        [Display(Name = "Description")]
        public String Description { get; set; }

        [Display(Name = "Product Type")]
        public string ProdTypeCD { get; set; }

        [Display(Name = "Reason")]
        public decimal? ReasonID { get; set; }

        [Display(Name = "Location")]
        public decimal? LocationID { get; set; }

        [Display(Name = "Sort")]
        public decimal? SortOrder { get; set; }
    }

    public class TypesViewModel
    {
        public SelectList Reasons { get; set; }
        public SelectList ProdTypes { get; set; }
        public SelectList Locations { get; set; }
    }

    ///
    // Job Priorities
    public class PrdnPriorityViewModel : IdCodeNameActive
    {  }

    ///
    // Production Attachment Types

    public class AttachmentTypesViewModel
    {
        public AttachmentTypesViewModel()
        {
            Groups = new SelectList(new[]{
                new SelectListItem{ Text="All", Value="A"},
                new SelectListItem{ Text="Schedule", Value="S"},
                new SelectListItem{ Text="Make", Value="M"}
            }, "Value", "Text");
        }

        public SelectList Groups { get; set; }
    }

    public class PrdnAttTypeViewModel : IdCodeNameActive
    {
        [Display(Name = "Description")]
        public String Description { get; set; }

        [Display(Name = "Esp Descripción")]
        public String EspDescription { get; set; }

        [Display(Name = "Select Order")]
        public decimal? SelectOrder { get; set; }

        [Display(Name = "Display Order")]
        public decimal? DisplayOrder { get; set; }

        [Display(Name = "Group")]
        public string GroupCD { get; set; }
    }

    /// <summary>
    // Customer Locations 
    public class CustLocViewModel
    {
        public string LocID { get; set; }
        public string Description { get; set; }
        public string Display { get; set; }
    }

    public class CustLookupViewModel
    {
        public string LookupValue { get { return CustID; } }
        public string LookupLabel { get { return CustID + " " + CustName; } }
        public string CustID { get; set; }
        public string CustName { get; set; }
        public string Addr1 { get; set; }
        public string Addr2 { get; set; }
        public string Addr3 { get; set; }
        public string Addr4 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Postal { get; set; }
        public string Country { get; set; }
    }

    public class OrdShipToLookupViewModel : CustLookupViewModel
    {
        public string OrdNo { get; set; }
        public bool DropShip { get; set; }
        public decimal OrdTot { get; set; }
        public string FormatOrdTot { get { return string.Format("{0:c}", OrdTot); } }
    }

    /// <summary>
    // Jobs
    public class JobOrdLookupViewModel
    {
        public string LookupValue { get { return OrderNo;}  }
        public string LookupLabel { get { return OrderNo + (OrderLine != null ? " #" + OrderLine.ToString() : "") + " "+LocalStr.ProductCD+":" + ProdCD + " "+LocalStr.CustLocation+":" + CustLoc; } }
        //public string RequestID { get; set; }
        //public string SpecialWSDescr { get; set; }
        public string CustLoc { get; set; }
        //public string ShipMethodCD { get; set; }
        public string ProdCD { get; set; }
        public string ProdDescr { get; set; }
        public string ParentProdCD { get; set; }
        public string OrderNo { get; set; }
        public decimal? OrderLine { get; set; }
        public decimal? OrderLineID { get; set; }
        public string CustDeliveryFlag { get; set; }
        public bool DropShip { get { return PrdnDataHelper.BoolYN(CustDeliveryFlag); } }
        public string ShipCustID { get; set; }
    }

    /// <summary>
    // Worksheet
    public enum WSEditOptType { Characteristic, Component, Meta };

    public enum WSEditMetaType { OneTone, TwoTone, ThreeTone };

    public class WorksheetOptMeta
    {
        [Required]
        [Display(Name = "Option Code")]
        public string OptionCode2 { get; set; }

        [Required]
        [Display(Name = "Description")]
        public string OptionDescr { get; set; }
    }

    [MetadataType(typeof(WorksheetOptMeta))]
    public class WorksheetEditOpt : WorksheetOpt
    {
        public string TypeGroupCD { get; set; }

        public bool GroupStart { get; set; }

        public const string CharColorGroupCD = "meta_color_def";

        public static readonly List<string> ColorTypeCodes = new List<string>
        {
            LeatherColorTypeCD, InsertOptionTypeCD
        };

        public static readonly List<string> Color2OptCodes = new List<string>
        {
            Color2ColorOptCD, Color2OptionOptCD
        };

        public static readonly List<string> Color3OptCodes = new List<string>
        {
            Color3ColorOptCD
        };

        public bool IsColorTypeOpt
        {
            get
            {
                return ((Type == OptionType.Characteristic) && (ColorTypeCodes.Contains(TypeCD)));
            }
        }

        public bool IsColor2Opt
        {
            get
            {
                return IsColorTypeOpt && Color2OptCodes.Contains(OptionCode1);
            }
        }

        public bool IsColor3Opt
        {
            get
            {
                return IsColorTypeOpt && Color3OptCodes.Contains(OptionCode1);
            }
        }

        public string GroupID {
            get {
                string groupCd = String.Empty;
                if (Type == OptionType.Characteristic)
                {
                    if (IsColorTypeOpt)
                    {
                        groupCd = CharColorGroupCD;
                    }
                    else
                    {
                        groupCd = "char_" + ParentOptionCode2;
                    }
                }
                else
                {
                    groupCd = "comp_" + TypeCD + "_" + OptionCode2 + "_" + OptionCode3;
                }
                return groupCd; 
            }
        }

        public static void Define300Groups(List<WorksheetEditOpt> worksheetOpts)
        {
            string groupCd = String.Empty;
            string lastGroupCd = groupCd;

            foreach (var item in worksheetOpts)
            {
                if (item.IsRoot)
                {
                    groupCd = item.GroupID;
                }

                item.TypeGroupCD = groupCd;
                item.GroupStart = (groupCd != lastGroupCd);
                lastGroupCd = groupCd;
            }
        }

        public static void Ensure310Opts(List<WorksheetEditOpt> worksheetOpts)
        {
            if (!worksheetOpts.Any(o => (o.Type == OptionType.Characteristic) && (o.TypeCD == WorksheetEditOpt.LeatherPatternTypeCD)))
            {
                WorksheetEditOpt optCol = new WorksheetEditOpt();
                optCol.Type = OptionType.Characteristic;
                optCol.TypeCD = WorksheetEditOpt.LeatherPatternTypeCD;
                optCol.TypeDescr = "Pattern";
                optCol.OptionCode1 = WorksheetEditOpt.LeatherPatternOptCD;
                worksheetOpts.Add(optCol);
            }

            if (!worksheetOpts.Any(o => (o.Type == OptionType.Characteristic) && (o.TypeCD == WorksheetEditOpt.LeatherColorTypeCD)))
            {
                WorksheetEditOpt optCol = new WorksheetEditOpt();
                optCol.Type = OptionType.Characteristic;
                optCol.TypeCD = WorksheetEditOpt.LeatherColorTypeCD;
                optCol.TypeDescr = "Color";
                optCol.OptionCode1 = WorksheetEditOpt.Color1ColorOptCD;
                worksheetOpts.Add(optCol);
            }
        }

        //public static List<WorksheetEditOpt> WorksheetWith300Groups(decimal? worksheetID)
        //{
        //    List<WorksheetEditOpt> worksheet = Worksheet<WorksheetEditOpt>(worksheetID);
        //    if (worksheet != null)
        //    {
        //        Define300Groups(worksheet);
        //    }
        //    return worksheet;
        //}
        //public static List<WorksheetEditOpt> ReqWorksheetWith300Groups(string requestID)
        //{
        //    List<WorksheetEditOpt> worksheet = ReqWorksheet<WorksheetEditOpt>(requestID);
        //    if (worksheet != null)
        //    {
        //        Define300Groups(worksheet);
        //    }
        //    return worksheet;
        //}
    }

    public class WorksheetEditViewModel
    {
        public WorksheetEditViewModel()  : this(prodTypeCD: null, worksheetID: null)  { }

        public WorksheetEditViewModel(string prodTypeCD) : this(prodTypeCD: prodTypeCD, worksheetID: null)  { }

        public WorksheetEditViewModel(string prodTypeCD, decimal? worksheetID)
        {
            Modified = false;
            ProdTypeCD = prodTypeCD;

            WorksheetID = worksheetID;

            WorksheetOpts = WorksheetEditOpt.Worksheet<WorksheetEditOpt>(worksheetID);

            //WorksheetOpts = WorksheetEditOpt.WorksheetWith300Groups(WorksheetID);
            DefineForProdType();
        }

        public WorksheetEditViewModel(string prodTypeCD, CST.Prdn.Data.Worksheet worksheet)
            : this(prodTypeCD, worksheet.IfNotNull(w => w.ID))
        {
            ProdCD = worksheet.IfNotNull(w => w.ProdCD);
            ProdSetid = worksheet.IfNotNull(w => w.ProdSetid);
        }

        public static void LoadViewJobEditWorksheet(EditPrdnJobViewModel viewJob, ProductionJob job)
        {
            if (job.IsNotNull(j => j.Worksheet))
            {
                viewJob.EditWorksheet = new WorksheetEditViewModel(job.Product.IfNotNull(j => j.ProdTypeCD), job.Worksheet);
            }
            else
            {
                if (viewJob.WorksheetID != null)
                {
                    viewJob.EditWorksheet = new WorksheetEditViewModel(job.Product.IfNotNull(j => j.ProdTypeCD), viewJob.WorksheetID);
                }
                else
                {
                    viewJob.EditWorksheet = new WorksheetEditViewModel(job.Product.IfNotNull(j => j.ProdTypeCD));
                }
            }
            viewJob.EditWorksheet.Editable = false;
        }

        public void DefineForProdType()
        {
            if (ProdTypeCD == PrdnDataHelper.LeatherProdTypeCd) 
            {
                if (WorksheetOpts.IsAny())
                {
                    WorksheetEditOpt.Define300Groups(WorksheetOpts);
                }
                else
                {
                    WorksheetOpts = null;
                }
            }
            else
            if (ProdTypeCD == PrdnDataHelper.WarrantyProdTypeCd)
            {
                WorksheetEditOpt.Ensure310Opts(WorksheetOpts);
            }
        }

        public void LoadFromRequest(string requestID)
        {
            WorksheetOpts = WorksheetEditOpt.ReqWorksheet<WorksheetEditOpt>(requestID);

            //WorksheetOpts = WorksheetEditOpt.ReqWorksheetWith300Groups(requestID);
            DefineForProdType();

            if (WorksheetOpts.IsAny())
            {
                WorksheetOpts.ForEach(o => o.WorksheetID = WorksheetID);
            }
        }

        public decimal? WorksheetID { get; set; }

        [Display(Name = "EditItem", ResourceType = typeof(LocalStr))]
        public bool Editable { get; set; }

        public bool Modified { get; set; }

        public string ProdTypeCD { get; set; }
        public string ProdCD  { get; set; }
        public bool ProdCD1 { get; set; }
        public string ProdCD2 { get; set; }

        public string ProdSetid { get; set; }

        public List<WorksheetEditOpt> WorksheetOpts { get; set; }

        public string ActionTypeCD { get; set; }

        public string ActionTypeDescr { get; set; }

        public string ActionOptCD2 { get; set; }

        public string ActionOptDescr { get; set; }

        public bool ActionOptUDF { get; set; }

        public WSEditMetaType MetaType { get; set; }

        private void EnsureWorksheet()
        {
            if (WorksheetOpts == null)
            {
                WorksheetOpts = new List<WorksheetEditOpt>();
            }
        }

        //public static void WorksheetRemoveColorOpts(List<WorksheetEditOpt> worksheetOpts) { worksheetOpts.RemoveAll(item => item.IsColorTypeOpt);}

        public void WorksheetRemove2rdColorOpts()
        {
            WorksheetOpts.RemoveAll(item => item.IsColor2Opt);
        }

        public void WorksheetRemove3rdColorOpts()
        {
            WorksheetOpts.RemoveAll(item => item.IsColor3Opt);
        }

        public void Swap(int oldIdx, int newIdx)
        {
            var item = WorksheetOpts[oldIdx];
            WorksheetOpts.RemoveAt(oldIdx);
            if (newIdx > oldIdx) newIdx--;         // the actual index could have shifted due to the removal
            WorksheetOpts.Insert(newIdx, item);
        }

        public int Ensure1ColorOpts()
        {
            EnsureWorksheet();
            int col1Idx = WorksheetOpts.FindIndex(item =>
                (item.Type == OptionType.Characteristic)
                && (item.TypeCD == WorksheetEditOpt.LeatherColorTypeCD)
                && (item.OptionCode1 == WorksheetEditOpt.Color1ColorOptCD));

            if (col1Idx == -1)
            {
                col1Idx = 0;
                WorksheetOpts.Insert(col1Idx,
                    new WorksheetEditOpt
                    {
                        TypeGroupCD = WorksheetEditOpt.CharColorGroupCD,
                        Type = OptionType.Characteristic,
                        TypeCD = WorksheetEditOpt.LeatherColorTypeCD,
                        TypeDescr = "Color",
                        OptionCode1 = WorksheetEditOpt.Color1ColorOptCD,
                        GroupStart = true
                    });
            }
            else if (col1Idx != 0)
            {
                Swap(col1Idx, 0);
                col1Idx = 0;
            }

            return col1Idx;
        }

        public int Ensure2ColorOpts()
        {
            EnsureWorksheet();
            int col1Idx = Ensure1ColorOpts();
            int afterCol1Idx = col1Idx + 1;

            int col2Idx = WorksheetOpts.FindIndex(item =>
                (item.Type == OptionType.Characteristic)
                && (item.TypeCD == WorksheetEditOpt.LeatherColorTypeCD)
                && (item.OptionCode1 == WorksheetEditOpt.Color2ColorOptCD));

            if (col2Idx == -1)
            {
                col2Idx = afterCol1Idx;
                WorksheetOpts.Insert(col2Idx,
                    new WorksheetEditOpt
                    {
                        TypeGroupCD = WorksheetEditOpt.CharColorGroupCD,
                        Type = OptionType.Characteristic,
                        TypeCD = WorksheetEditOpt.LeatherColorTypeCD,
                        TypeDescr = "1st Insert",
                        OptionCode1 = WorksheetEditOpt.Color2ColorOptCD,
                        GroupStart = false
                    });
            }
            else if (col2Idx != afterCol1Idx)
            {
                Swap(col2Idx, afterCol1Idx);
                col2Idx = afterCol1Idx;
            }

            int afterCol2Idx = col2Idx + 1;

            int insert2Idx = WorksheetOpts.FindIndex(item =>
                (item.Type == OptionType.Characteristic)
                && (item.TypeCD == WorksheetEditOpt.InsertOptionTypeCD)
                && (item.OptionCode1 == WorksheetEditOpt.Color2OptionOptCD));

            if (insert2Idx == -1)
            {
                insert2Idx = afterCol2Idx;
                WorksheetOpts.Insert(insert2Idx,
                    new WorksheetEditOpt
                    {
                        TypeGroupCD = WorksheetEditOpt.CharColorGroupCD,
                        Type = OptionType.Characteristic,
                        TypeCD = WorksheetEditOpt.InsertOptionTypeCD,
                        TypeDescr = "Insert Option",
                        OptionCode1 = WorksheetEditOpt.Color2OptionOptCD,
                        GroupStart = false
                    });
            }
            else if (insert2Idx != afterCol2Idx)
            {
                Swap(insert2Idx, afterCol2Idx);
                insert2Idx = afterCol2Idx;
            }

            return insert2Idx;
        }

        public int Ensure3ColorOpts()
        {
            EnsureWorksheet();
            int col2Idx = Ensure2ColorOpts();
            int afterCol2Idx = col2Idx + 1;

            int col3Idx = WorksheetOpts.FindIndex(item =>
                (item.Type == OptionType.Characteristic)
                && (item.TypeCD == WorksheetEditOpt.LeatherColorTypeCD)
                && (item.OptionCode1 == WorksheetEditOpt.Color3ColorOptCD));

            if (col3Idx == -1)
            {
                col3Idx = afterCol2Idx;
                WorksheetOpts.Insert(col3Idx,
                    new WorksheetEditOpt
                    {
                        TypeGroupCD = WorksheetEditOpt.CharColorGroupCD,
                        Type = OptionType.Characteristic,
                        TypeCD = WorksheetEditOpt.LeatherColorTypeCD,
                        TypeDescr = "2nd Insert",
                        OptionCode1 = WorksheetEditOpt.Color3ColorOptCD,
                        GroupStart = false
                    });
            }
            else if (col3Idx != afterCol2Idx)
            {
                Swap(col3Idx, afterCol2Idx);
                col3Idx = afterCol2Idx;
            }

            return col3Idx;
        }

        private void OneTone()
        {
            if (WorksheetOpts != null)
            {
                WorksheetRemove2rdColorOpts();
                WorksheetRemove3rdColorOpts();
            }
            Ensure1ColorOpts();
            Modified = true;
        }

        private void TwoTone()
        {
            if (WorksheetOpts != null)
            {
                WorksheetRemove3rdColorOpts();
            }
            Ensure2ColorOpts();
            Modified = true;
        }

        private void ThreeTone()
        {
            Ensure3ColorOpts();
            Modified = true;
        }

        public void WorksheetMeta()
        {
            if (MetaType == WSEditMetaType.OneTone)
            { OneTone(); }
            else if (MetaType == WSEditMetaType.TwoTone)
            { TwoTone(); }
            else if (MetaType == WSEditMetaType.ThreeTone)
            { ThreeTone(); }
        }

        public WorksheetEditOpt AddComp()
        {
            Modified = true;

            int nextSeq = SortSeqComps();

            WorksheetEditOpt parentOpt = new WorksheetEditOpt
            {
                Level = 1,
                Type = OptionType.Component,
                TypeCD = ActionTypeCD,
                TypeDescr = ActionTypeDescr,
                OptionCode1 = IsisEntities.SalesSetid,
                OptionCode2 = ActionOptCD2,
                CompOptCd3Seq = nextSeq,
                OptionDescr = ActionOptDescr,
                ImgCount = 0,
                WorksheetID = this.WorksheetID,
                UserDefined = ActionOptUDF,
                GroupStart = true
            };
            parentOpt.TypeGroupCD = parentOpt.GroupID;

            List<WorksheetEditOpt> compGrpOpts = new List<WorksheetEditOpt> { parentOpt };

            WorksheetOpt.CompSubChars<WorksheetEditOpt>(parentOpt, compGrpOpts);
            WorksheetOpt.CompSubComps<WorksheetEditOpt>(parentOpt, compGrpOpts);

            compGrpOpts.ForEach(o => o.TypeGroupCD = parentOpt.GroupID );

            EnsureWorksheet();
            WorksheetOpts.AddRange(compGrpOpts);

            return parentOpt;
        }

        public int SortSeqComps()
        {
            int seqOrd = 1;
            if (WorksheetOpts != null)
            {
                foreach (var opt in WorksheetOpts.Where(opt => (opt.Type == OptionType.Component)))
                {
                    opt.CompOptCd3Seq = seqOrd;
                    seqOrd++;
                }
            }
            return seqOrd;
        }
    
    }

    public interface IEditWorksheetOwner
    {
        WorksheetEditViewModel EditWorksheet { get; set; }
    }

    public class EditWorksheetOwner : IEditWorksheetOwner
    {
        public WorksheetEditViewModel EditWorksheet { get; set; }
    }

    public class PrintLabelModel : IPEndPointModel
    {
        public PrintLabelModel()
        {
            Port = 9100;
            Active = true;
        }

        [Display(Name = "ID")]
        [Required]
        public decimal ID { get; set; }

        [Display(Name = "Printer Name")]
        [Required]
        public string Name { get; set; }

        [Display(Name = "Active")]
        [Required]
        public bool Active { get; set; }

        [HiddenInput(DisplayValue = false)]
        public string UrlReturn { get; set; }

        public string Zpl { get; set; }
    }

    public class PrintLabelTestModel : PrintLabelModel
    {
        public string Message { get; set; }
    }

    public interface IPrinterInfo
    {
        int? LabelPrinterID { get; set; }

        //[Display(Name = "LabelPrinter", ResourceType = typeof(LocalStr))]
        string PrinterName { get; set; }

        //[Display(Name = "Address")]
        string PrinterHostName { get; set; }

        //[Display(Name = "Port")]
        int? PrinterPort { get; set; }
    }

    public class PrinterInfo : IPrinterInfo
    {
        public int? LabelPrinterID { get; set; }

        [Display(Name = "LabelPrinter", ResourceType = typeof(LocalStr))]
        public string PrinterName { get; set; }

        [Display(Name = "Address")]
        public string PrinterHostName { get; set; }

        [Display(Name = "Port")]
        public int? PrinterPort { get; set; }

        [Display(Name = "Message", ResourceType = typeof(LocalStr))]
        public string Message { get; set; }
    }

}
