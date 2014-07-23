using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CST.Prdn.Data;
using CST.Prdn.ViewModels;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using CST.Security;
using CST.Localization;

namespace CST.Prdn.ViewModels
{
    public class AppViewModel : IdCodeNameActive
    {
        [Display(Name = "App ID")]
        public decimal AppID { get { return ID; } set {ID = value ;} }
    }

    public class AppGroupsViewModel
    {
        [Display(Name = "App")]
        public string AppID { get; set; }

        public SelectList AppList { get; set; }
    }

    public class GroupViewModel : IdCodeNameActive
    {
        [Display(Name = "Group ID")]
        public decimal GroupID { get { return ID; } set { ID = value; } }

        [Display(Name = "App ID")]
        public decimal AppID { get; set; }

        [Display(Name = "App Code")]
        public string AppCode { get; set; }

        [Display(Name = "App Admin")]
        public bool AppAdmin { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

        public string AppGroupCode { get { return Group.GetAppGroupCode(AppCode, Code); } }
    }

    public class UserViewModel : ICultureModel
    {
        public UserViewModel()
        {
            Active = true;
            AlterPassword = true;
            LocalizationHelper.LoadModelCulture(this);
        }

        [Display(Name = "ID")]
        public decimal? ID { get; set; }

        [Required]
        [Display(Name = "Login")]
        public string Login { get; set; }

        public string GetLoginUpper()
        {
            return this.IfNotNull(t => t.Login).IfNotNull(l => l.ToUpper()); 
        }

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Display(Name = "Name")]
        public string FullName { get { return FirstName + " " + LastName; } }

        [RegularExpression(@"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$", ErrorMessage = "Please enter a valid e-mail adress")]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email")]
        public string EMail { get; set; }

        [DataType(DataType.PhoneNumber)]
        [Display(Name = "Phone")]
        public string Phone { get; set; }

        [Display(Name = "Language")]
        public string Culture { get; set; }

        [Display(Name = "Languages")]
        public SelectList CultureList { get; set; }

        [Display(Name = "Active")]
        public bool Active { get; set; }

        [Display(Name = "Alter Password")]
        public bool AlterPassword { get; set; }

        //[DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        //[DataType(DataType.Password)]
        //[Display(Name = "Confirm new password")]
        //[Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        //public string ConfirmPassword { get; set; }
    }

    //public class CustomPrincipalSerializeModel
    //{
    //    public int ID { get; set; }
    //    public string Login { get; set; }
    //    public string FirstName { get; set; }
    //    public string LastName { get; set; }
    //    public bool AlterPassword { get; set; }

    //    public static CustomPrincipalSerializeModel MakeFromUser(User user)
    //    {
    //        return new CustomPrincipalSerializeModel
    //        {
    //            ID = Convert.ToInt32(user.ID),
    //            Login = user.Login,
    //            FirstName = user.FirstName,
    //            LastName = user.LastName,
    //            AlterPassword = user.AlterPassword
    //        };
    //    }

    //    public static string SerializeFromUser(User user)
    //    {
    //        CustomPrincipalSerializeModel serializeModel = CustomPrincipalSerializeModel.MakeFromUser(user);

    //        JavaScriptSerializer serializer = new JavaScriptSerializer();

    //        return serializer.Serialize(serializeModel);
    //    }

    //    public static CustomPrincipalSerializeModel DeserializeFromString(string data)
    //    {
    //        JavaScriptSerializer serializer = new JavaScriptSerializer();

    //        CustomPrincipalSerializeModel serializeModel = serializer.Deserialize<CustomPrincipalSerializeModel>(data);

    //        return serializeModel;
    //    }

    //    public static CustomPrincipal DeserializedCustomPrincipal(string data)
    //    {
    //        CustomPrincipalSerializeModel serializeModel = DeserializeFromString(data);

    //        CustomPrincipal newUser = new CustomPrincipal(serializeModel.Login);
    //        newUser.ID = serializeModel.ID;
    //        newUser.FirstName = serializeModel.FirstName;
    //        newUser.LastName = serializeModel.LastName;
    //        newUser.AlterPassword = serializeModel.AlterPassword;

    //        return newUser;
    //    }
    //}

    public class GroupUsersViewModel
    {
        [Display(Name = "ID")]
        public decimal GroupID { get; set; }

        [Display(Name = "Code")]
        public string GroupCode { get; set; }

        [Display(Name = "Group")]
        public string GroupName { get; set; }

        [Display(Name = "ID")]
        public decimal AppID { get; set; }

        [Display(Name = "ID")]
        public string AppCode { get; set; }

        [Display(Name = "App")]
        public string AppName { get; set; }
    }

    public class UserLookupViewModel
    {
        public string LookupValue { get; set; }
        public string LookupLabel { get; set; }

        [Display(Name = "ID")]
        public decimal ID { get; set; }
        
        [Display(Name = "Login")]
        public string Login { get; set; }
        
        [Display(Name = "Last Name")]
        public string LastName { get; set; }
        
        [Display(Name = "First Name")]
        public string FirstName { get; set; }
        
        [Display(Name = "Name")]
        public string FullName { get { return FirstName + " " + LastName; } }
        
        [Display(Name = "Password")]
        public string Password { get; set; }
        
        [Display(Name = "Email")]
        public string EMail { get; set; }
    }

    public class UserGroupsViewModel
    {
        [Display(Name = "User")]
        public decimal UserID { get; set; }

        [Display(Name = "Login")]
        public string UserLogin { get; set; }

        [Display(Name = "Name")]
        public string FullName { get; set; }
    }

    public class GroupLookupViewModel
    {
        public string LookupValue { get; set; }
        public string LookupLabel { get; set; }

        [Display(Name = "ID")]
        public decimal? ID { get; set; }

        [Display(Name = "Code")]
        public string Code { get; set; }

        [Display(Name = "Group")]
        public string Name { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

        [Display(Name = "App ID")]
        public decimal? AppID { get; set; }

        [Display(Name = "App")]
        public string AppCode { get; set; }

        public string AppGroupCode { get { return Group.GetAppGroupCode(AppCode, Code); } }

        public decimal? UserID { get; set; }
    }
}