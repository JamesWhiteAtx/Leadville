using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;
using CST.Localization;
using CST.ISIS.Data;
using CST.ZebraUtils;

namespace CST.Prdn.ViewModels
{
    public interface IProductViewModel
    {
        string ProdTypeCD { get; set; }
        string ProdTypeDescr { get; set; }
        string ProdCD { get; set; }
        string ProdDescr { get; set; }
        string ParentProdCD { get; set; }
        
    }

    public interface IProductCostViewModel : IProductViewModel
    {
        decimal? Cost { get; set; }
    }

    public class ProdTypeListModel
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string CodeDashName { get { return Code + "-" + Name; } }
    }

    public class ProductListModel : IProductCostViewModel
    {
        [Display(Name = "ProductTypeCode", ResourceType = typeof(LocalStr))]
        public string ProdTypeCD { get; set; }

        [Display(Name = "ProductType", ResourceType = typeof(LocalStr))]
        public string ProdTypeDescr { get; set; }

        [Display(Name = "ProductCD", ResourceType = typeof(LocalStr))]
        public string ProdCD { get; set; }

        [Display(Name = "Description", ResourceType = typeof(LocalStr))]
        public string ProdDescr { get; set; }

        public string ParentProdCD { get; set; }

        [Display(Name = "Description", ResourceType = typeof(LocalStr))]
        public string CodeDescr { get {
            return "(" + ProdCD + ") " + ProdDescr;
        } }

        [DataType(DataType.Currency)]
        [Display(Name = "Cost", ResourceType = typeof(LocalStr))]
        public decimal? Cost { get; set; }
    }

    public class LeatherListModel : ProductListModel
    {
        [Display(Name = "Color", ResourceType = typeof(LocalStr))]
        public string ColorCDStr { get; set; }

        [Display(Name = "Color", ResourceType = typeof(LocalStr))]
        public string ColorDescrStr { get; set; }

        [Display(Name = "Decor", ResourceType = typeof(LocalStr))]
        public string DecorStr { get; set; }

        [Display(Name = "Pattern", ResourceType = typeof(LocalStr))]
        public string PatternStr { get { return ParentProdCD; } }
    }

    public class ProductLookupModel : IProductCostViewModel
    {
        [Display(Name = "ProductTypeCode", ResourceType = typeof(LocalStr))]
        public string ProdTypeCD { get; set; }

        [Display(Name = "ProductType", ResourceType = typeof(LocalStr))]
        public string ProdTypeDescr { get; set; }

        [Display(Name = "ProductCD", ResourceType = typeof(LocalStr))]
        public string ProdCD { get; set; }

        public string ProdCdDisplayTerm { get; set; }

        public string ProdCdDisplay { get {
            if (ProdCdDisplayTerm == null)
            {
                return this.GetDisplayName(t => t.ProdCD);
            }
            else
            {
                return ProdCdDisplayTerm;
            }
        } }

        [Display(Name = "Description", ResourceType = typeof(LocalStr))]
        public string ProdDescr { get; set; }

        [Display(Name = "Parent")]
        public string ParentProdCD { get; set; }

        public IEnumerable<ProdTypeListModel> ProdTypeList { get; set; }

        public string OptsPartial { get; set; }
        public string ImgSetPartial { get; set; }

        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        [Display(Name = "Cost", ResourceType = typeof(LocalStr))]
        public decimal? Cost { get; set; }
    }

    public class PatternLookupModel : ProductLookupModel
    {
        public IEnumerable<LeatherListModel> LeatherList { get; set; }
    }

    public class ProdScheduleLookupModel : ProductLookupModel
    {
        public IEnumerable<ProdTypeListModel> PrdnProdTypeList { get; set; }
    }

    /// Inventory Item
    ///////////////////////////

    public class InvItemViewModel
    {
        [Display(Name = "InvItemID", ResourceType = typeof(LocalStr))]
        public string InvItemID { get; set; }

        [Display(Name = "SerialNo", ResourceType = typeof(LocalStr))]
        public string SerialNo { get; set; }
        
        [Display(Name = "ProductTypeCode", ResourceType = typeof(LocalStr))]
        public string ProdTypeCD { get; set; }

        [Display(Name = "ProductType", ResourceType = typeof(LocalStr))]
        public string ProdType { get; set; }

        [Display(Name = "ProductCD", ResourceType = typeof(LocalStr))]
        public string ProdCD { get; set; }
        
        [Display(Name = "Description", ResourceType = typeof(LocalStr))]
        public string Description { get; set; }
        
        public string ParentProdCD { get; set; }

        [Display(Name = "InvItemDefinition", ResourceType = typeof(LocalStr))]
        public List<ProdOptDefn> ItemOptions { get; set; }

        //public ZplMultiParam ZplMultiParam() 
        //{
        //    ZplMultiParam parm = new ZplMultiParam
        //    {
        //        SerialNo = SerialNo,
        //        ProdCD = ProdCD,
        //        Descr = Description,
        //    };

        //    if (ItemOptions.IsAny())
        //    {
        //        foreach (var opt in ItemOptions)
        //        {
        //            if (opt.Type == OptionType.Characteristic)
        //            {
        //                if (opt.TypeCD == CharCompOpt.LeatherColorTypeCD) 
        //                {
        //                    if (opt.OptionCode1 == CharCompOpt.Color1ColorOptCD) 
        //                    {
        //                        parm.Col1 = opt.OptionCode2;
        //                        parm.ColDescr1 = opt.OptionDescr;
        //                    }
        //                    else if (opt.OptionCode1 == CharCompOpt.Color2ColorOptCD) 
        //                    {
        //                        parm.Col2 = opt.OptionCode2;
        //                        parm.ColDescr2 = opt.OptionDescr;
        //                    }
        //                    else if (opt.OptionCode1 == CharCompOpt.Color3ColorOptCD)
        //                    {
        //                        parm.Col3 = opt.OptionCode2;
        //                        parm.ColDescr3 = opt.OptionDescr;
        //                    }
        //                }
        //                else if (opt.TypeCD == CharCompOpt.LeatherPatternTypeCD)
        //                {
        //                    parm.Pattern = opt.OptionCode2;
        //                }
        //            }
        //            else if (opt.Type == OptionType.Component)
        //            { 
        //                if (opt.TypeCD == CharCompOpt.ProdCDEmbroidery)
        //                {
        //                    parm.Emb = opt.OptionCode2;
        //                    parm.EmbNote = opt.OptionDescr;

        //                    parm.EmbThrds = String.Join("/", ItemOptions
        //                        .Where(i => i.ParentOptionCode2 == parm.Emb && i.TypeCD == CharCompOpt.ProdCDEmbrThread)
        //                        .Select(i => i.OptionCode2)
        //                        .ToArray());
        //                }
        //                else if (opt.TypeCD == CharCompOpt.ProdCDHeatSeal)
        //                {
        //                    parm.HS = opt.OptionCode2;
        //                    parm.HSNote = opt.OptionDescr;
        //                }
        //                else if (opt.TypeCD == CharCompOpt.ProdCDPerforatedInsert)
        //                {
        //                    parm.PI = opt.OptionCode2;
        //                    parm.PINote = opt.OptionDescr;
        //                }
        //            }
        //        }
        //    }

        //    return parm;
        //}

    }

    public class InvLookupItemViewModel : InvItemViewModel, IPrinterInfo
    {
        [Display(Name = "LabelPrinter", ResourceType = typeof(LocalStr))]
        public int? LabelPrinterID { get; set; }

        [Display(Name = "LabelPrinter", ResourceType = typeof(LocalStr))]
        public string PrinterName { get; set; }

        [Display(Name = "Address")]
        public string PrinterHostName { get; set; }

        [Display(Name = "Port")]
        public int? PrinterPort { get; set; }
    }

    public class PrdnZplMultiParam : ZplMultiParam
    {
        public PrdnZplMultiParam(InvItemViewModel item)
        {
            SerialNo = item.SerialNo;
            ProdCD = item.ProdCD;
            Descr = item.Description;
            LoadFromCharCompOpts(item.ItemOptions);
        }

        public PrdnZplMultiParam(EditPrdnJobViewModel job)
        {
            SerialNo = job.SerialNo;
            ProdCD = job.ProdCD;
            Descr = job.ProdDescr;

            LoadFromCharCompOpts(job.IfNotNull(j => j.EditWorksheet).IfNotNull(w => w.WorksheetOpts));

            Priority = job.PriorityDescription;
            SetPrdnOrderDisp(job.PrdnOrderNo, job.PrdnTypeCode, job.RunSeqNo);
            ShipCD = job.IfNotNull(j => j.ShipMethodCD);
        }

        public void SetPrdnOrderDisp(string prdnOrderNo, string prdnTypeCode, decimal? runSeqNo)
        {
            PrdnOrder = prdnOrderNo + prdnTypeCode;
            if (runSeqNo != null)
            {
                PrdnOrder = PrdnOrder + " Seq#" + runSeqNo.ToInt().ToString();
            }
        }

        public void LoadFromCharCompOpts(IEnumerable<CharCompOpt> opts)
        {
            if (opts.IsAny())
            {
                foreach (var opt in opts)
                {
                    if (opt.Type == OptionType.Characteristic)
                    {
                        if (opt.TypeCD == CharCompOpt.LeatherColorTypeCD)
                        {
                            if (opt.OptionCode1 == CharCompOpt.Color1ColorOptCD)
                            {
                                Col1 = opt.OptionCode2;
                                ColDescr1 = opt.OptionDescr;
                            }
                            else if (opt.OptionCode1 == CharCompOpt.Color2ColorOptCD)
                            {
                                Col2 = opt.OptionCode2;
                                ColDescr2 = opt.OptionDescr;
                            }
                            else if (opt.OptionCode1 == CharCompOpt.Color3ColorOptCD)
                            {
                                Col3 = opt.OptionCode2;
                                ColDescr3 = opt.OptionDescr;
                            }
                        }
                        else if (opt.TypeCD == CharCompOpt.LeatherPatternTypeCD)
                        {
                            Pattern = opt.OptionCode2;
                        }
                    }
                    else if (opt.Type == OptionType.Component)
                    {
                        if (opt.TypeCD == CharCompOpt.ProdCDEmbroidery)
                        {
                            Emb = opt.OptionCode2;
                            EmbNote = opt.OptionDescr;

                            EmbThrds = String.Join("/", opts
                                .Where(i => i.ParentOptionCode2 == Emb && i.TypeCD == CharCompOpt.ProdCDEmbrThread)
                                .Select(i => i.OptionCode2)
                                .ToArray());
                        }
                        else if (opt.TypeCD == CharCompOpt.ProdCDHeatSeal)
                        {
                            HS = opt.OptionCode2;
                            HSNote = opt.OptionDescr;
                        }
                        else if (opt.TypeCD == CharCompOpt.ProdCDPerforatedInsert)
                        {
                            PI = opt.OptionCode2;
                            PINote = opt.OptionDescr;
                        }
                    }
                }
            }
        }

    }
}