using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace CST.Prdn
{
    public class BulkSaleImpItem
    {
        public string RunName { get; set; }
        [Display(Name = "Serial Number")]
        public string SerialNum { get; set; }
        [Display(Name = "PO")]
        public string CustPO { get; set; }
        public const char KeyDelimiter = '%';
        public string ItemKey { get { return RunName + KeyDelimiter + SerialNum; } }
        public bool ParseItemKey(string itemKey)
        {
            string[] vals;
            vals = itemKey.Split(KeyDelimiter);
            if (vals.Length == 2)
            {
                RunName = vals[0];
                SerialNum = vals[1];
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public class BulkSaleImpInfo : BulkSaleImpItem
    {
        [Display(Name = "Vendor ID")]
        public string VendorID { get; set; }
        [Display(Name = "Product CD")]
        public string ProdCD { get; set; }
        [Display(Name = "Descr")]
        public string ProdDescr { get; set; }
        [Display(Name = "HV")]
        public string HV { get; set; }
        [Display(Name = "RW Prod CD")]
        public string RWProdCD { get { return ProdCD + (HV == "HV" ? "-HV" : String.Empty); } }
        [Display(Name = "Status")]
        public string Status { get; set; }
        [Display(Name = "Branch")]
        public string DeptID { get; set; }
        [Display(Name = "Cost")]
        public decimal? Cost { get; set; }
        [Display(Name = "Cost")]
        public string CostStr { get { return string.Format("{0:c}", Cost); } }
    }

    public class BulkSaleImpIModel
    {
        public BulkSaleImpIModel()
        {
            SkipFirstRow = true;
        }

        [Display(Name = "Warehouse")]
        [Required]
        public string RunName { get; set; }

        [Display(Name = "Skip First(Title) Row")]
        public bool SkipFirstRow { get; set; }

        [Display(Name = "Insert")]
        public bool Insert { get; set; }

        [Display(Name = "Current Count")]
        public int Count { get; set; }
    }

    public class BulkSaleImpSumModel
    {
        [Display(Name = "Warehouse")]
        [Required]
        public string RunName { get; set; }

        [Display(Name = "Locked")]
        public string LockedStr { get; set; }

        [Display(Name = "Locked")]
        public bool Locked { get { return LockedStr.Equals("T"); } }

        [Display(Name = "Count")]
        public int Count { get; set; }
    }

    public partial class BulkSaleResult
    {
        [Display(Name = "RW Prod CD")]
        public string RWProdCD { get { return ProdCD + (HV == "HV" ? "-HV" : String.Empty); } }
    }

    public class UpldNsPartsImpIModel
    {
        public UpldNsPartsImpIModel()
        {
            SkipFirstRow = true;
            QtyLoaded = 0;
        }

        [Display(Name = "Skip First(Title) Row")]
        public bool SkipFirstRow { get; set; }

        [Display(Name = "File Name")]
        public string FileName { get; set; }

        [Display(Name = "Qty Loaded")]
        public int QtyLoaded { get; set; }
    }

}