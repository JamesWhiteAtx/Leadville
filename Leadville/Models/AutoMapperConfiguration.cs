using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Objects.DataClasses;
using AutoMapper;
using CST.Prdn.Data;
using CST.Prdn.ViewModels;
using CST.ISIS.Data;

namespace CST.Prdn.ViewModels
{
    public static class MappingExtensions
    {
        public static IMappingExpression<ProductionJob, TDest> MapViewJobProperties<TDest>(
             this IMappingExpression<ProductionJob, TDest> viewJobBase) where TDest : PrdnJobViewModel
        {
            return viewJobBase
                .ForMember(d => d.CustID, opt => opt.MapFrom(s => (s.CustID > 0) ? s.CustID : (decimal?)null))
                .ForMember(d => d.CustCode, opt => opt.MapFrom(s => (s.Customer != null) ? s.Customer.Code : null))
                .ForMember(d => d.InvItemExists, opt => opt.MapFrom(s => s.IsNotNull(j => j.PrdnInvItem)))
                ;
        }

        public static IMappingExpression<TSource, ProductionJob> MapJobProperties<TSource>(
            this IMappingExpression<TSource, ProductionJob> jobBase) where TSource : PrdnJobViewModel
        {
            return jobBase
                .ForMember(d => d.ID, opt => opt.Ignore())
                .ForMember(d => d.CreatedDt, opt => opt.Ignore())
                .ForMember(d => d.CreatedUserID, opt => opt.Ignore())
                .ForMember(d => d.ScheduledDt, opt => opt.Ignore())
                .ForMember(d => d.ScheduledUserID, opt => opt.Ignore())
                .ForMember(d => d.ProcessedDt, opt => opt.Ignore())
                .ForMember(d => d.ProcessedUserID, opt => opt.Ignore())
                .ForMember(d => d.CompletedDt, opt => opt.Ignore())
                .ForMember(d => d.CompletedUserID, opt => opt.Ignore())
                .ForMember(d => d.CanceledDt, opt => opt.Ignore())
                .ForMember(d => d.CanceledUserID, opt => opt.Ignore())
                ;
        }

        // map Entity objects to each other ignoring reference properties
        //public static IMappingExpression<TSource, TDestination> IgnoreEntityReferences<TSource, TDestination>(
        //    this IMappingExpression<TSource, TDestination> expression) where TSource : EntityObject where TDestination : EntityObject 
        //{
        //    var sourceType = typeof(TSource);
        //    var destinationType = typeof(TDestination);

        //    var existingMaps = Mapper.GetAllTypeMaps().First(x => x.SourceType.Equals(sourceType) && x.DestinationType.Equals(destinationType));
        //    foreach (PropertyMap map in existingMaps.GetPropertyMaps())
        //    {
        //        Type destType = map.DestinationProperty.MemberType;
        //        if ((typeof(EntityObject).IsAssignableFrom(destType))
        //            || 
        //            (destType.IsGenericType 
        //             && (typeof(EntityReference<>).IsAssignableFrom(destType.GetGenericTypeDefinition())
        //                || typeof(EntityCollection<>).IsAssignableFrom(destType.GetGenericTypeDefinition()))
        //            ))  //destType.GetGenericTypeDefinition() == typeof(EntityReference<>)
        //        {
        //            expression.ForMember(map.DestinationProperty.Name, opt => opt.Ignore());
        //        }
        //    }
        //    return expression;
        //}
    }                

    public static class AutoMapperConfiguration
    {
        public static void Configure(System.Globalization.DateTimeFormatInfo dateFormat)
        {
            Mapper.CreateMap<ProductionRun, PrdnRunViewModel>();
            Mapper.CreateMap<PrdnRunViewModel, ProductionRun>()
                .ForMember(d => d.ID, opt => opt.Ignore());

            Mapper.CreateMap<PrdnRunViewModel, PrdnRunMakeViewModel>();
            Mapper.CreateMap<PrdnRunMakeViewModel, PrdnRunViewModel>();

            Mapper.CreateMap<ProductionJob, PrdnJobViewModel>()
                .MapViewJobProperties()
                ;
            Mapper.CreateMap<ProductionJob, ListPrdnJobViewModel>()
                .MapViewJobProperties()
                .ForMember(d => d.PriorityCD, opt => opt.MapFrom(s => (s.Priority != null) ? s.Priority.Code : null))
                .ForMember(d => d.ColorStr, opt => opt.MapFrom(s => s.ColorCodesStr))
                .ForMember(d => d.DecorStr, opt => opt.MapFrom(s => s.DecorAbrevStr))
                .ForMember(d => d.PatternStr, opt => opt.MapFrom(s => s.PatternCDStr))
                .ForMember(d => d.ListDescr, opt => opt.MapFrom(s => (s.Product != null) ? s.Product.Description : null))
                ;
            Mapper.CreateMap<ProductionJob, EditPrdnJobViewModel>()
                .MapViewJobProperties()
                .ForMember(d => d.ProdDescr, opt => opt.MapFrom(s => (s.Product != null) ? s.Product.Description : null))
                .ForMember(d => d.ParentProdCD, opt => opt.MapFrom(s => (s.Product != null) ? s.Product.ParentProdCD : null))
                .ForMember(d => d.PriorityDescription, opt => opt.MapFrom(s => (s.Priority != null) ? s.Priority.Name : null))
                .ForMember(d => d.EditRunSeqNo, opt => opt.MapFrom(s => Convert.ToInt32(s.RunSeqNo)))
                .ForMember(d => d.EditStatus, opt => opt.MapFrom(s => s.Status))
                .ForMember(d => d.CreatedUserLogin, opt => opt.MapFrom(s => (s.CreatedUser != null) ? s.CreatedUser.Login : null))
                .ForMember(d => d.ScheduledUserLogin, opt => opt.MapFrom(s => (s.ScheduledUser != null) ? s.ScheduledUser.Login : null))
                .ForMember(d => d.ProcessedUserLogin, opt => opt.MapFrom(s => (s.ProcessedUser != null) ? s.ProcessedUser.Login : null))
                .ForMember(d => d.CompletedUserLogin, opt => opt.MapFrom(s => (s.CompletedUser != null) ? s.CompletedUser.Login : null))
                .ForMember(d => d.CanceledUserLogin, opt => opt.MapFrom(s => (s.CanceledUser != null) ? s.CanceledUser.Login : null))
                .ForMember(d => d.InvItemID, opt => opt.MapFrom(s => (s.IsNotNull(j => j.PrdnInvItem)) ? s.PrdnInvItem.InvItemID : null))
                ;
            Mapper.CreateMap<ProductionJob, PrdnJobListViewModel>()
                .ForMember(d => d.PrdnOrderNo, opt => opt.MapFrom(s => s.Run.IfNotNull(r => r.PrdnOrder.OrderNo) ))
                .ForMember(d => d.PrdnShipDay, opt => opt.MapFrom(s => s.Run.IfNotNull(r => r.PrdnOrder.ShipDay) ))
                .ForMember(d => d.RunID, opt => opt.MapFrom(s => s.Run.IfNotNull(r => r.ID.ToInt()) ))
                .ForMember(d => d.PrdnTypeCD, opt => opt.MapFrom(s => s.Run.IfNotNull(r => r.PrdnType.Code) ))
                .ForMember(d => d.JobID, opt => opt.MapFrom(s => s.ID.ToInt() ))
                .ForMember(d => d.RunSeqNo, opt => opt.MapFrom(s => s.RunSeqNo.ToInt() ))
                .ForMember(d => d.ProdDescr, opt => opt.MapFrom(s => s.Product.IfNotNull(p => p.Description) ))
                .ForMember(d => d.PatternStr, opt => opt.MapFrom(s => s.PatternCDStr))
                .ForMember(d => d.ColorStr, opt => opt.MapFrom(s => s.ColorCodesStr))
                .ForMember(d => d.CustCode, opt => opt.MapFrom(s => s.Customer.IfNotNull(c => c.Code) ))
                ;

            //Mapper.CreateMap<ProductionJob, ProductionJob>().IgnoreEntityReferences()
            //    .ForMember(d => d.ID, opt => opt.Ignore());

            Mapper.CreateMap<EditPrdnJobViewModel, UpdMakePrdnJobViewModel>();

            Mapper.CreateMap<PrdnJobViewModel, ProductionJob>()
                .MapJobProperties()
                //.ForMember(d => d.ID, opt => opt.Ignore())
                .ForMember(d => d.CustID, opt => opt.MapFrom(s => (s.CustID != null) ? (decimal)s.CustID : 0))
                ;
            Mapper.CreateMap<EditPrdnJobViewModel, ProductionJob>()
                .MapJobProperties()
                //.ForMember(d => d.ID, opt => opt.Ignore())
                .ForMember(d => d.DropShipCustID, opt => opt.MapFrom(s => s.DropShip ? s.DropShipCustID : null))
                .ForMember(d => d.DropShipCustName, opt => opt.MapFrom(s => s.DropShip ? s.DropShipCustName : null))
                .ForMember(d => d.ShipAddr1, opt => opt.MapFrom(s => s.DropShip ? s.ShipAddr1 : null))
                .ForMember(d => d.ShipAddr2, opt => opt.MapFrom(s => s.DropShip ? s.ShipAddr2 : null))
                .ForMember(d => d.ShipAddr3, opt => opt.MapFrom(s => s.DropShip ? s.ShipAddr3 : null))
                .ForMember(d => d.ShipAddr4, opt => opt.MapFrom(s => s.DropShip ? s.ShipAddr4 : null))
                .ForMember(d => d.ShipCity, opt => opt.MapFrom(s => s.DropShip ? s. ShipCity : null))
                .ForMember(d => d.ShipState , opt => opt.MapFrom(s => s.DropShip ? s.ShipState : null))
                .ForMember(d => d.ShipPostal , opt => opt.MapFrom(s => s.DropShip ? s.ShipPostal : null))
                .ForMember(d => d.ShipCountry, opt => opt.MapFrom(s => s.DropShip ? s.ShipCountry : null))
                ;

            Mapper.CreateMap<ProductionPriority, PrdnPriorityViewModel>();
            Mapper.CreateMap<PrdnPriorityViewModel, ProductionPriority>()
                .ForMember(d => d.ID, opt => opt.Ignore());

            Mapper.CreateMap<PrdnAttachmentType, PrdnAttTypeViewModel>();
            Mapper.CreateMap<PrdnAttTypeViewModel, PrdnAttachmentType>()
                .ForMember(d => d.ID, opt => opt.Ignore());

            Mapper.CreateMap<ProductionReason, PrdnReasonViewModel>();
            Mapper.CreateMap<PrdnReasonViewModel, ProductionReason>()
                .ForMember(d => d.ID, opt => opt.Ignore());

            Mapper.CreateMap<ProductionCustomer, PrdnCustViewModel>();
            Mapper.CreateMap<PrdnCustViewModel, ProductionCustomer>()
                .ForMember(d => d.ID, opt => opt.Ignore());

            Mapper.CreateMap<ProductionLocation, PrdnLocViewModel>();
            Mapper.CreateMap<PrdnLocViewModel, ProductionLocation>()
                .ForMember(d => d.ID, opt => opt.Ignore());

            Mapper.CreateMap<ProductionMfgr, PrdnMfgrViewModel>();
            Mapper.CreateMap<PrdnMfgrViewModel, ProductionMfgr>()
                .ForMember(d => d.ID, opt => opt.Ignore());

            Mapper.CreateMap<ProductionType, PrdnTypeViewModel>();
            Mapper.CreateMap<PrdnTypeViewModel, ProductionType>()
                .ForMember(d => d.ID, opt => opt.Ignore())
                ;
            Mapper.CreateMap<ProductionRun, SchedJobRunModel>()
                .ForMember(d => d.RunID, opt => opt.MapFrom(s => s.ID))
                .ForMember(d => d.PrdnTypeCD, opt => opt.MapFrom(s => s.PrdnType.Code))
                .ForMember(d => d.PrdnTypeDescr, opt => opt.MapFrom(s => s.PrdnType.Description))
                .ForMember(d => d.ShipDtStr, opt => opt.MapFrom(
                    s => s.PrdnOrder.ShipDay.ToString(dateFormat.ShortDatePattern)))
                .ForMember(d => d.ProdTypeCD, opt => opt.MapFrom(
                    s => s.PrdnType.ProdTypeCD));

            Mapper.CreateMap<Request, RequestViewModel>()
                .ForMember(d => d.ParentProdCD, opt => opt.MapFrom(s => (s.Product != null) ? s.Product.ParentProdCD : null))
                ;
            Mapper.CreateMap<RequestViewModel, Request>();

            Mapper.CreateMap<Request, RequestListViewModel>();
            Mapper.CreateMap<RequestListViewModel, Request>();

            Mapper.CreateMap<DefaultRunViewModel, UserDefaultPrdnRun>();
            Mapper.CreateMap<UserDefaultPrdnRun, DefaultRunViewModel>();

            Mapper.CreateMap<DefaultRunEditViewModel, UserDefaultPrdnRun>();
            Mapper.CreateMap<UserDefaultPrdnRun, DefaultRunEditViewModel>();

            Mapper.CreateMap<UserSettingsViewModel, UserSettingsModel>()
                .ForMember(d => d.JobPageSize, opt => opt.MapFrom(s => s.JobPageSize ?? 0))
                .ForMember(d => d.RequestPageSize, opt => opt.MapFrom(s => s.RequestPageSize ?? 0))
                ;
            Mapper.CreateMap<UserSettingsModel, UserSettingsViewModel>()
                .ForMember(d => d.JobPageSize, opt => opt.MapFrom(s => (s.JobPageSize > 0) ? s.JobPageSize : (int?)null))
                .ForMember(d => d.RequestPageSize, opt => opt.MapFrom(s => (s.RequestPageSize > 0) ? s.RequestPageSize : (int?)null))
                ;
            Mapper.CreateMap<UserSettingsViewModel, UserSettingsEditViewModel>();
            Mapper.CreateMap<UserSettingsEditViewModel, UserSettingsViewModel>();

            Mapper.CreateMap<UserSettingsEditViewModel, PrdnUserSetting>()
                .ForMember(d => d.JobPageSize, opt => opt.MapFrom(s => (decimal?)s.JobPageSize))
                .ForMember(d => d.RequestPageSize, opt => opt.MapFrom(s => (decimal?)s.RequestPageSize))
                .ForMember(d => d.LabelPrinterID, opt => opt.MapFrom(s => (decimal?)s.LabelPrinterID))
                ;
            Mapper.CreateMap<WorksheetOpt, WorksheetComp>()
                .ForMember(d => d.WorksheetID, opt => opt.Condition(s => s.WorksheetID != null))
                .ForMember(d => d.WorksheetID, opt => opt.MapFrom(s => s.WorksheetID))
                //.ForMember(d => d.CompProdCD, opt => opt.MapFrom(s => s.CompProdCD))
                //.ForMember(d => d.CompProdSetid, opt => opt.MapFrom(s => s.CompProdSetid))
                .ForMember(d => d.Sequence, opt => opt.MapFrom(s => s.CompSeq))
                .ForMember(d => d.SortOrder, opt => opt.MapFrom(s => s.CompSeq == null ? (decimal?)null : (decimal?)s.CompSeq))
                //.ForMember(d => d.ParentCompProdCD, opt => opt.MapFrom(s => s.ParentCompProdCD))
                //.ForMember(d => d.ParentCompProdSetid, opt => opt.MapFrom(s => s.ParentCompProdSetid))
                .ForMember(d => d.ParentSequence, opt => opt.MapFrom(s => s.ParentCompSeq == null ? (decimal?)null : (decimal?)s.ParentCompSeq))
                .ForMember(d => d.UserDefinition, opt => opt.MapFrom(s => s.UserDefined ? s.OptionDescr : null))
                ;
            Mapper.CreateMap<WorksheetOpt, WorksheetChar>()
                .ForMember(d => d.WorksheetID, opt => opt.Condition(s => s.WorksheetID != null))
                .ForMember(d => d.WorksheetID, opt => opt.MapFrom(s => s.WorksheetID))
                .ForMember(d => d.ProdCharCD, opt => opt.MapFrom(s => s.CharCD))
                .ForMember(d => d.InternalName, opt => opt.MapFrom(s => s.CharInternalName))
                //.ForMember(d => d.ParentCompProdCD, opt => opt.MapFrom(s => s.ParentCompProdCD))
                //.ForMember(d => d.ParentCompProdSetid, opt => opt.MapFrom(s => s.ParentCompProdSetid))
                .ForMember(d => d.ParentSequence, opt => opt.MapFrom(s => s.ParentCompSeq == null ? (decimal?)null : (decimal?)s.ParentCompSeq))
                .ForMember(d => d.UserDefinition, opt => opt.MapFrom(s => s.UserDefined ? s.OptionDescr : null))
                ;

            Mapper.CreateMap<App, AppViewModel>();
            Mapper.CreateMap<AppViewModel, App>()
                .ForMember(d => d.ID, opt => opt.Ignore());

            Mapper.CreateMap<Group, GroupViewModel>();
            Mapper.CreateMap<GroupViewModel, Group>()
                .ForMember(d => d.ID, opt => opt.Ignore());

            Mapper.CreateMap<Group, GroupLookupViewModel>()
                .ForMember(d => d.AppCode, opt => opt.MapFrom(s => s.IfNotNull(g => g.App).IfNotNull(a => a.Code)));

            Mapper.CreateMap<User, UserViewModel>();
            Mapper.CreateMap<UserViewModel, User>()
                .ForMember(d => d.ID, opt => opt.Ignore())
                .ForMember(d => d.Login, opt => opt.MapFrom(s => s.GetLoginUpper()))
                ;
            Mapper.CreateMap<User, UserLookupViewModel>();
            Mapper.CreateMap<UserLookupViewModel, User>()
                .ForMember(d => d.ID, opt => opt.Ignore());

            Mapper.CreateMap<LabelPrinter, PrintLabelModel>()
                .ForMember(d => d.Port, opt => opt.MapFrom(s => s.Port == null ? (int)0 : Convert.ToInt32(s.Port)))
                ;
            Mapper.CreateMap<PrintLabelModel, LabelPrinter>()
                .ForMember(d => d.Port, opt => opt.MapFrom(s => (decimal?)s.Port))
                .ForMember(d => d.ID, opt => opt.Ignore())
                ;

            Mapper.CreateMap<InvItemViewModel, InvLookupItemViewModel>()
                ;

        }
    }
}