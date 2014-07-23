using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using CST.Prdn.Data;
using CST.ActionFilters;

namespace CST.Security
{
    public class AdminRoleProvider : RoleProvider
    {
        #region Unimplemented RoleProvider Methods

        //public override void Initialize(string name, System.Collections.Specialized.NameValueCollection config)
        //{
        //    base.Initialize(name, config);
        //}

        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }

        public override string ApplicationName
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override void CreateRole(string roleName)
        {
            throw new NotImplementedException();
        }

        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            throw new NotImplementedException();
        }

        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            throw new NotImplementedException();
        }

        public override string[] GetAllRoles()
        {
            throw new NotImplementedException();
        }

        public override string[] GetUsersInRole(string roleName)
        {
            throw new NotImplementedException();
        }

        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }

        public override bool RoleExists(string roleName)
        {
            throw new NotImplementedException();
        }

        #endregion

        public override bool IsUserInRole(string username, string roleName)
        {
            if (String.IsNullOrWhiteSpace(username) || String.IsNullOrWhiteSpace(roleName))
                return false;

            string appCode;
            string groupCode;
            
            if (Group.SplitAppGroupCode(roleName, out appCode, out groupCode)) 
            {
                using (PrdnEntities PrdnDBContext = new PrdnEntities())
                {
                    var code = (from u in PrdnDBContext.Users
                                where u.Login == username
                                from g in u.Groups
                                where g.App.Code == appCode && g.Code == groupCode
                                select g.Code).FirstOrDefault();

                    return (code != null);
                }
            }
            return false;
        }

        public override string[] GetRolesForUser(string username)
        {
            using (PrdnEntities PrdnDBContext = new PrdnEntities())
            {
                string[] roles = new string[0];

                var groups = (from u in PrdnDBContext.Users
                              where u.Login == username
                              from g in u.Groups
                              where g.ActiveFlag == PrdnDataHelper.BoolYNTue
                              where g.App.ActiveFlag == PrdnDataHelper.BoolYNTue
                              orderby g.App.Code, g.Code
                              select g).ToList();

                if (groups.IsAny())
	            {
                    var groupRoles = groups.Select(g => g.AppGroupCode);

                    var adminRoles = groups.Where(g => g.AppAdmin).Select(g => g.App.Code).ToList();
                    if (adminRoles.IsAny())
                    {
                        if (groups.Any(g => g.AppAdmin && g.App.SysAdmin))
                        {
                            adminRoles.Insert(0, CstAuthorizeAttribute.SysAdminRole);
                        }
                        adminRoles.AddRange(groupRoles);

                        roles = adminRoles.ToArray();
                    }
                    else 
                    {
                        roles = groupRoles.ToArray();
                    }
                }

                string s = String.Join(", ", roles);
                return roles;
            }
        }

    }
}