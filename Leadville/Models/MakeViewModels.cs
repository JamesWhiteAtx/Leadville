using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using CST.Localization;
using CST.Prdn.Data;

namespace CST.Prdn.ViewModels
{
    ///
    // Production Runs
    public class PrdnRunMakeViewModel : PrdnRunViewModel
    {
        [Display(Name = "Size", ResourceType = typeof(LocalStr))]
        public int JobPageSize { get; set; }

        public UserSettingsViewModel SettingsModel { get; set; }

        public PrdnJobStatusViewModel FilterModel { get; set; }
    }
   
    public class UpdMakePrdnJobViewModel : EditPrdnJobViewModel
    {
        public static new bool FromStatusTo(PrdnJobStatus current, PrdnJobStatus desired)
        {
            if (desired == PrdnJobStatus.New)
            {
                return false;
            }
            else if (desired == PrdnJobStatus.Pending)
            {
                return false;
            }
            else if (desired == PrdnJobStatus.Scheduled)
            {
                return StatusInList(current, PrdnJobStatus.Scheduled, PrdnJobStatus.Processing);
            }
            else if (desired == PrdnJobStatus.Processing)
            {
                return StatusInList(current, PrdnJobStatus.Scheduled, PrdnJobStatus.Processing);
            }
            else if (desired == PrdnJobStatus.Completed)
            {
                return StatusInList(current, PrdnJobStatus.Processing, PrdnJobStatus.Completed);
            }
            else if (desired == PrdnJobStatus.Canceled)
            {
                return StatusInList(current, PrdnJobStatus.Scheduled, PrdnJobStatus.Processing, PrdnJobStatus.Canceled);
            }
            else
                return false;
        }

        public override bool CanStatusTo(PrdnJobStatus desired)
        {

            if (InvItemExists)
            {
                return false;
            }
            if (GetAllowedToStatus(desired))
            {
                bool canStatus = FromStatusTo(Status, desired);
                if (StatusInList(desired, PrdnJobStatus.Scheduled, PrdnJobStatus.Processing))
                {
                    canStatus = canStatus || ((Status == PrdnJobStatus.Canceled) && GetAllowedToStatus(PrdnJobStatus.Canceled));
                }
                return canStatus;
            }
            return false;
        }

        protected override string CaptionVerb()
        {
            return LocalStr.Update;
        }

        protected override string SaveVerb()
        {
            return LocalStr.Save;
        }

        protected override string SaveObject()
        {
            return LocalStr.Job + " " + LocalStr.Changes;
        }

    }

    public class LastScanModel
    {
        public string Value { get; set; }
        public ScanResult Result { get; set; }
        public DateTime ScanDt { get; set; }
    }

    public class ScanViewModel : IPrinterInfo
    {
        [Required]
        [StringLength(30)]
        [Display(Name = "SerialNo", ResourceType = typeof(LocalStr))]
        public string SerialNo { get; set; }

        [Display(Name = "LabelPrinter", ResourceType = typeof(LocalStr))]
        public int? LabelPrinterID { get; set; }

        [Display(Name = "LabelPrinter", ResourceType = typeof(LocalStr))]
        public string PrinterName { get; set; }

        [Display(Name = "Address")]
        public string PrinterHostName { get; set; }

        [Display(Name = "Port")]
        public int? PrinterPort { get; set; }

        public IEnumerable<ScanListViewModel> Scans { get; set; }

        public static string ResultMessage(ScanResult result, string value, string scanJobStatusStr, string message)
        {
            string msg = String.Empty;
            if (result == ScanResult.CompletedAndPrinted)
            {
                msg = SystemExtensions.Sentence(LocalStr.Job, LocalStr.Completed, LocalStr.and, LocalStr.Printed, LocalStr.For, LocalStr.SerialNo, value);
            }
            else if (result == ScanResult.CompletedNotPrinted)
            {
                msg = SystemExtensions.Sentence(LocalStr.Job, LocalStr.Completed, LocalStr.but, LocalStr.not, LocalStr.Printed, LocalStr.For, LocalStr.SerialNo, value);
            }
            else if (result == ScanResult.CstItemCreatedAndPrinted)
            {
                msg = SystemExtensions.Sentence(LocalStr.InventoryItem, LocalStr.Created, LocalStr.and, LocalStr.Printed, LocalStr.For, LocalStr.SerialNo, value);
            }
            else if (result == ScanResult.CstItemCreatedNotPrinted)
            {
                msg = SystemExtensions.Sentence(LocalStr.InventoryItem, LocalStr.Created, LocalStr.but, LocalStr.not, LocalStr.Printed, LocalStr.For, LocalStr.SerialNo, value);
            }
            else if (result == ScanResult.InvalidScanValue)
            {
                msg = SystemExtensions.Sentence(LocalStr.SerialNo, value, LocalStr.verbIsNot, LocalStr.valid);
            }
            else if (result == ScanResult.ItemExists)
            {
                msg = SystemExtensions.Sentence(LocalStr.SerialNo, value, LocalStr.alreadyExistsInInventory);
            }
            else if (result == ScanResult.InvalidStatus)
            {
                string sts = scanJobStatusStr.ConverToEnum<PrdnJobStatus>().Description();
                msg = SystemExtensions.Sentence(LocalStr.Cannot, LocalStr.Change, LocalStr.Status, LocalStr.From, sts, LocalStr.to, PrdnJobStatus.Completed.Description());
            }
            else if (result == ScanResult.ScanException)
            {
                msg = LocalStr.Error + ":";
            }
            if (message == null)
            {
                return msg;
            } else {
                return msg + " " + message;
            }
        }

        public static string ResultMessage(ProductionScan scan)
        {
            return ResultMessage(scan.Result, scan.Value, scan.JobStatusStr, scan.Message);
        }

        public static string ResultMessage(ScanListViewModel scan)
        {
            return ResultMessage(scan.Result, scan.Value, scan.ScanJobStatusStr, scan.Message);
        }
    }

    public class LookupScanViewModel
    {
        [DataType(DataType.Date)]
        [Display(Name = "Date", ResourceType = typeof(LocalStr))]
        public DateTime? LookupDt { get; set; }

        [Display(Name = "SerialNo", ResourceType = typeof(LocalStr))]
        public string SerialNo { get; set; }

        public IEnumerable<ScanListViewModel> Scans { get; set; }
    }

    public class ScanListViewModel
    {
        [Display(Name = "ID", ResourceType = typeof(LocalStr))]
        public decimal ID { get; set; }

        [Display(Name = "ScannedSerialNo", ResourceType = typeof(LocalStr))]
        public string Value { get; set; }

        [Display(Name = "Date", ResourceType = typeof(LocalStr))]
        public DateTime ScanDt { get; set; }

        [Display(Name = "Date", ResourceType = typeof(LocalStr))]
        public String ScanDtTmStr { get { return ScanDt.TodayDtTmStr(); } }

        [Display(Name = "SerialNo", ResourceType = typeof(LocalStr))]
        public int UserID { get; set; }

        [Display(Name = "User", ResourceType = typeof(LocalStr))]
        public string UserLogin { get; set; }

        [Display(Name = "Result", ResourceType = typeof(LocalStr))]
        public ScanResult Result { get; set; }

        [Display(Name = "Result", ResourceType = typeof(LocalStr))]
        public decimal ResultNum
        {
            get { return ResultNum = Result.ConvertToInt(); }
            set { Result = value.ToInt().ConverToEnum<ScanResult>(); }
        }

        [Display(Name = "Message", ResourceType = typeof(LocalStr))]
        public string Message { get; set; }
       
        [Display(Name = "Message", ResourceType = typeof(LocalStr))]
        public string ResultMessage { get { 
            return ScanViewModel.ResultMessage(this); 
        } }

        [Display(Name = "JobID", ResourceType = typeof(LocalStr))]
        public int? JobID { get; set; }

        [Display(Name = "Status", ResourceType = typeof(LocalStr))]
        public string ScanJobStatusStr { get; set; }

        [Display(Name = "Status", ResourceType = typeof(LocalStr))]
        public string CurrentJobStatusStr { get; set; }

        [Display(Name = "SerialNo")]
        public string SerialNo { get; set; }
        
        [Display(Name = "Item ID")]
        public string InvItemID { get; set; }
    }

}