using System.Web.Mvc;
using System;
using System.Linq;
using CST.Prdn.Data;

namespace CST.ActionFilters
{
    public sealed class CstAuthorizeAttribute : AuthorizeAttribute
    {
        public const string SysAdminRole = "_SYS_";

        public string Groups { 
            get {return Roles;}
            set {
                Roles = ExtractSysAppRoles(value);
            } 
        }

        public static bool Authorized(System.Security.Principal.IPrincipal user, string role)
        {
            if (user.IsInRole(SysAdminRole))
            {
                return true;
            }

            string[] appGrp = role.Trim().Split(Group.CodeSep);
            if (appGrp.Length > 0)
            {
                string app = appGrp[0];
                if (user.IsInRole(app))
                {
                    return true;
                }

                return user.IsInRole(role);
            }

            return false;
        }

        public static string ExtractSysAppRoles(string original)
        {
            return String.Join(", ", SysAdminRole, ExtractAppGroups(original));
        }

        public static string ExtractAppGroups(string original)
        {
            if (String.IsNullOrWhiteSpace(original))
            {
                return original;
            }
            string groups = original;

            var apps = (from piece in original.Split(',')
                        let trimmed = piece.Trim()
                        where !String.IsNullOrEmpty(trimmed)
                        let appGrp = trimmed.Split(Group.CodeSep)
                        where appGrp.Length > 1
                        let app = appGrp[0]
                        select app)
                        .Distinct()
                        .OrderBy(a => a)
                        .ToArray();

            if (apps.Length > 0)
            {
                string appRoles = String.Join(", ", apps);
                groups = String.Join(", ", appRoles, original);
            }
            return groups;
        }
    }
}