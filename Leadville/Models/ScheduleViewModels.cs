using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;
using CST.Prdn.Data;
using CST.Localization;

namespace CST.Prdn.ViewModels
{
    public class DefaultRunViewModel
    {
        public DefaultRunViewModel() 
        {
            UserEditable = true;
        }

        public DefaultRunViewModel(bool editable): this()
        {
            UserEditable = editable;
        }

        [Required]
        [Display(Name = "User ID")]
        public decimal UserID { get; set; }

        [Display(Name = "User")]
        public string UserLogin { get; set; }

        [Required]
        [Display(Name = "Default Run ID")]
        public decimal? DefaultRunID { get; set; }

        [Display(Name = "Default Run")]
        public string DefaultRunDescr { get; set; }

        public string UrlReturn { get; set; }

        public bool UserEditable { get; set; }

        [Display(Name = "Message")]
        public string Message { get; set; }
    }

    public class DefaultRunEditViewModel : DefaultRunViewModel
    {
        public PrdnRunLookupModel LookupRunModel { get; set; }

        public NewPrdnRunViewModel NewRunModel { get; set; }
    }

    // Production Runs /////////////////

    public class PrdnOrdLookupModel
    {
        [Display(Name = "Lookup Order")]
        public string LookupTerm { get; set; }

        [Display(Name = "Order #")]
        public string LookupPrdnOrdNo { get; set; }

        [Display(Name = "Ship Dt")]
        public string LookupShipDt { get; set; }
    }

    public class NewPrdnRunViewModel 
    {
        private string gotoController;
        private string gotoAction;

        public NewPrdnRunViewModel() {
            EditOrdNo = true;
        }

        public NewPrdnRunViewModel(HttpSessionStateBase session, IDictionary<string, object> routeValues, CreateRunActionParm parm = null)
            : this()
        {
            AssignRouteValues(routeValues);
            StoreActionParm(session, parm);
        }

        public NewPrdnRunViewModel(HttpSessionStateBase session, string routeController, string routeAction, CreateRunActionParm parm = null)
            : this()
        {
            gotoController = routeController;
            gotoAction = routeAction;
            StoreActionParm(session, parm);
        }

        [Display(Name = "Order #")]
        public string NewPrdnOrderNo { get; set; }

        [Display(Name = "Ship Dt")]
        public string PrdnOrderShipDt { get; set; }

        [Display(Name = "Run Type")]
        public int NewPrdnTypeID { get; set; }

        [Display(Name = "Run Note")]
        public string NewDescr { get; set; }

        public string CalledFromUrl { get; set; }

        public bool EditOrdNo { get; set; }

        public void AssignRouteValues(IDictionary<string, object> routeValues)
        {
            gotoController = routeValues["controller"].ToString();
            gotoAction = routeValues["action"].ToString();
        }

        virtual public object NewRunActionParm(CST.Prdn.Data.ProductionRun newRun)
        {
            return new { prdnOrdNo = newRun.PrdnOrderNo, runID = newRun.ID };
        }

        public CST.Prdn.Data.ProductionRun MakeProductionRun()
        {
            CST.Prdn.Data.ProductionRun newRun = new CST.Prdn.Data.ProductionRun();
            newRun.PrdnOrderNo = this.NewPrdnOrderNo;
            newRun.PrdnTypeID = this.NewPrdnTypeID;
            newRun.Description = this.NewDescr;
            return newRun;
        }

        public const string ActionParmSessionKey = "_actionParmSessionKey";

        public void StoreActionParm(HttpSessionStateBase session, CreateRunActionParm parm)
        {
            if (parm == null)
            {
                parm = new CreateRunActionParm();
            }
            parm.GotoController = gotoController;
            parm.GotoAction = gotoAction;
            session[ActionParmSessionKey] = parm;
        }

        public CreateRunActionParm RetrieveActionParm(HttpSessionStateBase session, ProductionRun newRun)
        {
            CreateRunActionParm parm = session[ActionParmSessionKey] as CreateRunActionParm;
            if (parm != null)
            {
                parm.RunID = newRun.ID;
                parm.RunDescr = newRun.RunDescr;
            }
            return parm;
        }

    }

    public class CreateRunActionParm
    {
        public decimal? RunID { get; set; }

        public string RunDescr { get; set; }

        public string GotoController { get; set; }

        public string GotoAction { get; set; }
    }

    public class PrdnOrdCreateRunActionParm : CreateRunActionParm
    {
        public string PrdnOrdNo { get; set; }
    }

    public class RequestCreateRunActionParm : CreateRunActionParm
    {
        public string RequestID { get; set; }
        public string UrlReturn { get; set; }
    }

    public class PrdnRunLookupModel
    {
        public PrdnRunLookupModel()
        {
            LookupRunID = String.Empty;
            LookupTypeCD = String.Empty;
        }
        
        [Display(Name = "Lookup Run")]
        public string LookupTerm { get; set; }
        
        [Display(Name = "Order Term")]
        public string PrdnOrdTerm { get; set; }

        [Display(Name = "Type Term")]
        public string PrdnTypeTerm { get; set; }

        [Display(Name = "Order #")]
        public string LookupPrdnOrdNo { get; set; }

        [Display(Name = "Ship Dt")]
        public string LookupShipDt { get; set; }

        [Display(Name = "Run ID")]
        public string LookupRunID { get; set; }

        [Display(Name = "Type Code")]
        public string LookupTypeCD { get; set; }

        [Display(Name = "Type")]
        public string LookupTypeDescr { get; set; }
    }

    public class SchedJobRunModel
    {
        public SchedJobRunModel()
        {
            PageSize = UserSettingsModel.DefaultPageSize;
        }

        [Display(Name = "Run ID")]
        public decimal? RunID { get; set; }

        public decimal NotNullRunID { get { return RunID ?? 0; } }

        [Display(Name = "Order #")]
        public string PrdnOrderNo { get; set; }

        [Display(Name = "Ship Date")]
        public string ShipDtStr { get; set; }

        [Display(Name = "Type Code")]
        public string PrdnTypeCD { get; set; }

        [Display(Name = "Prod Type Cd")]
        public string ProdTypeCD { get; set; }

        [Display(Name = "Run Type")]
        public string PrdnTypeDescr { get; set; }

        [Display(Name = "Production Run")]
        public string RunDescr { get { return PrdnOrderNo + PrdnTypeCD + " " + PrdnTypeDescr + " " + ShipDtStr; } }

        public int PageSize { get; set; }

        public UserSettingsViewModel SettingsModel { get; set; }

        public PrdnJobStatusViewModel FilterModel { get; set; }

        //public PrdnRunLookupModel LookupModel { get; set; }
        //public NewPrdnRunViewModel NewRunModel { get; set; }
        //public DefaultRunViewModel DefaultRunModel { get; set; }
    }
}