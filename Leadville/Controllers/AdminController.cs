using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Telerik.Web.Mvc;
using AutoMapper;
using System.Data.Objects;
using CST.Prdn.Data;
using CST.Prdn.ViewModels;
using CST.ActionFilters;

namespace CST.Prdn.Controllers
{
    [CstAuthorize(Groups = "ADMIN/MAINT")]
    public class AdminController : CstControllerBase
    {
        ///
        /// Users

        [HttpGet]
        public ActionResult AddUser()
        {
            UserViewModel model = new UserViewModel();
            return View(model);
        }

        [HttpPost]
        public ActionResult AddUser(UserViewModel model)
        {
            ValidPasswordPolicy(model.NewPassword, model.FullPropertyName(m => m.NewPassword));
            
            if (ModelState.IsValid)
            {
                var loginUp = model.GetLoginUpper();
                var extant = (from u in PrdnDBContext.Users
                             where u.Login == loginUp
                             select new { u.Login }).FirstOrDefault();
                if (extant != null) {
                    ModelState.AddModelError(model.FullPropertyName(m => m.Login), "Already assigned to another user.");
                }
            }

            if (ModelState.IsValid)
            {
                User user = Mapper.Map<UserViewModel, User>(model);
                user.PlainPassword = model.NewPassword;

                PrdnDBContext.Users.AddObject(user);
        
                PrdnDBContext.SaveChanges();
                return RedirectToAction(actionName: "Users");
            }
            return View(model);
        }

        public ActionResult FindUser()
        {
            return View();
        }

        [HttpGet]
        public ActionResult EditUser(int id)
        {
            User user = (from x in PrdnDBContext.Users
                         where x.ID == id
                         select x).FirstOrDefault();

            if (user == null)
            {
                return ErrMsgView("Sorry - Invalid user ID "+id.ToString());                
            }

            UserViewModel model = Mapper.Map<User, UserViewModel>(user);
            return View(model);
        }

        [HttpPost]
        public ActionResult EditUser(UserViewModel model)
        {
            if (!String.IsNullOrEmpty(model.NewPassword)) 
            {
                ValidPasswordPolicy(model.NewPassword, model.FullPropertyName(m => m.NewPassword));
            }

            if (ModelState.IsValid)
            {
                User user = (from x in PrdnDBContext.Users
                             where x.ID == model.ID
                             select x).FirstOrDefault();

                if (user == null)
                {
                    return ErrMsgView("Sorry - Invalid user ID " + model.ID.ToString());
                }

                Mapper.Map(model, user);

                if (!String.IsNullOrWhiteSpace(model.NewPassword))
                {
                    user.PlainPassword = model.NewPassword;
                }

                PrdnDBContext.SaveChanges();

                return RedirectToAction(actionName: "Users");
            }
            return View(model);
        }

        [HttpPost]
        public ActionResult DeleteUser(int id)
        {
            DeleteUserByID(id);
            return RedirectToAction(actionName: "Users");
        }

        public ActionResult Users()
        {
            return View();
        }

        protected IEnumerable<User> UserList()
        {
            var users = from x in PrdnDBContext.Users
                        orderby x.Login
                        select x;
            return users;
        }

        protected GridModel UserGridList()
        {
            var users = from x in UserList()
                       select x;

            List<UserViewModel> viewUsers =
                Mapper.Map<List<User>, List<UserViewModel>>(users.ToList());

            return new GridModel(viewUsers);
        }

        [GridAction]
        public ActionResult _SelectUser()
        {
            return View(UserGridList());
        }

        //[HttpPost]
        //[GridAction]
        //public ActionResult _SaveUser(int id)
        //{
        //    User user = (from x in PrdnDBContext.Users
        //               where x.ID == id
        //               select x).FirstOrDefault();
        //    if (TryUpdateModel(user))
        //    {
        //        PrdnDBContext.SaveChanges();
        //    }
        //    return View(UserGridList());
        //}

        //[HttpPost]
        //[GridAction]
        //public ActionResult _InsertUser()
        //{
        //    User user = new User();
        //    if (TryUpdateModel(user))
        //    {
        //        user.AlterPassword = true;
        //        PrdnDBContext.Users.AddObject(user);
        //        PrdnDBContext.SaveChanges();
        //    }
        //    return View(UserGridList());
        //}

        [HttpPost]
        [GridAction]
        public ActionResult _DeleteUser(int id)
        {
            DeleteUserByID(id);
            return View(UserGridList());
        }

        protected bool DeleteUserByID(int id)
        {
            User user = (from x in PrdnDBContext.Users
                         where x.ID == id
                         select x).FirstOrDefault();

            if (user != null)
            {
                PrdnDBContext.DeleteObject(user);
                PrdnDBContext.SaveChanges();
                return true;
            }
            return false;
        }

        ///
        /// Apps

        public ActionResult Apps()
        {
            return View();
        }

        protected IEnumerable<App> AppList()
        {
            var apps = from x in PrdnDBContext.Apps
                       orderby x.Code
                       select x;
            return apps;
        }

        protected List<AppViewModel> AppViewModelList()
        {
            var apps = from x in AppList()
                       select x;

            List<AppViewModel> viewApps =
                Mapper.Map<List<App>, List<AppViewModel>>(apps.ToList());

            return viewApps.ToList();
        }

        protected GridModel AppGridList()
        {
            return new GridModel(AppViewModelList());
        }

        [GridAction]
        public ActionResult _SelectApp()
        {
            return View(AppGridList());
        }

        [HttpPost]
        [GridAction]
        public ActionResult _SaveApp(int id)
        {
            App app = (from x in PrdnDBContext.Apps
                       where x.ID == id
                       select x).FirstOrDefault();
            if (TryUpdateModel(app))
            {
                PrdnDBContext.SaveChanges();
            }
            return View(AppGridList());
        }

        [HttpPost]
        [GridAction]
        public ActionResult _InsertApp()
        {
            App app = new App();
            if (TryUpdateModel(app))
            {
                PrdnDBContext.Apps.AddObject(app);
                PrdnDBContext.SaveChanges();
            }
            return View(AppGridList());
        }

        [HttpPost]
        [GridAction]
        public ActionResult _DeleteApp(int id)
        {
            App app = (from x in PrdnDBContext.Apps
                       where x.ID == id
                       select x).FirstOrDefault();

            if (app != null)
            {
                PrdnDBContext.DeleteObject(app);
                PrdnDBContext.SaveChanges();
            }
            return View(AppGridList());
        }

        [HttpPost]
        public ActionResult _AppsSelectList(int? userID)
        {
            SelectList appsList;
            if (userID == null)
            {
                appsList = new SelectList(AppViewModelList(), "ID", "CodeDashName");
            }
            else 
            {
                var apps = from a in AppsNotAssgnToUser((int)userID)
                           orderby a.Code
                           select a;

                appsList = new SelectList(apps.ToList(), "ID", "CodeDashName");
            }

            return Json(appsList, JsonRequestBehavior.AllowGet);
        }

        public IEnumerable<Group> GroupsNotAssgnToUser(int userID, int? appID=null)
        {
            var userGroups = from u in PrdnDBContext.Users
                             from g in u.Groups
                             where u.ID == userID
                             select g;
            
            if (appID != null)
            {
                userGroups = from g in userGroups
                             where g.AppID == (int)appID
                             select g;
            }

            var exclGroups = from g in PrdnDBContext.Groups
                             where !userGroups.Any(ug => ug.ID == g.ID)
                             select g;

            if (appID != null)
            {
                exclGroups = from g in exclGroups
                             where g.AppID == (int)appID
                             select g;
            }

            return exclGroups;
        }

        public JsonResult ValidAppCode(string code)
        {
            return Json((code.Trim().ToUpper() != CstAuthorizeAttribute.SysAdminRole), JsonRequestBehavior.AllowGet);
        }

        ///
        /// Groups

        public ActionResult Groups(string appID)
        {
            AppGroupsViewModel model = new AppGroupsViewModel();

            var apps = from x in AppViewModelList()
                       select x;

            model.AppList = new SelectList(apps.ToList(), "ID", "CodeDashName", appID);

            model.AppID = model.AppList.SelectedValue as string; 

            if (model.AppID == null)
            {
                var selItem = model.AppList.FirstOrDefault();
                if (selItem != null)
                {
                    model.AppID = selItem.Value;
                }
            }

            return View(model);
        }

        protected IEnumerable<Group> AppGroupList(decimal appID)
        {
            var groups = from x in PrdnDBContext.Groups
                         where x.AppID == appID
                         orderby x.Code
                         select x;
            return groups;
        }

        protected List<GroupViewModel> AppGroupViewModelList(decimal appID)
        {
            var groups = from x in AppGroupList(appID)
                         select x;

            List<GroupViewModel> viewGroups =
                Mapper.Map<List<Group>, List<GroupViewModel>>(groups.ToList());

            return viewGroups;
        }

        protected GridModel AppGroupGridList(decimal appID)
        {
            return new GridModel(AppGroupViewModelList(appID));
        }

        [GridAction]
        public ActionResult _SelectAppGroup(string appID)
        {
            return View(AppGroupGridList(Convert.ToInt32(appID)));
        }

        [HttpPost]
        [GridAction]
        public ActionResult _SaveAppGroup(int id)
        {
            Group group = (from x in PrdnDBContext.Groups
                       where x.ID == id
                       select x).FirstOrDefault();
            if (TryUpdateModel(group))
            {
                PrdnDBContext.SaveChanges();
                //PrdnDBContext.Refresh(RefreshMode.StoreWins, PrdnDBContext.Groups);
            }
            return View(AppGroupGridList(group.AppID));
        }

        [HttpPost]
        [GridAction]
        public ActionResult _InsertAppGroup()
        {
            Group group = new Group();
            if (TryUpdateModel(group))
            {
                PrdnDBContext.Groups.AddObject(group);
                PrdnDBContext.SaveChanges();
            }
            return View(AppGroupGridList(group.AppID));
        }

        [HttpPost]
        [GridAction]
        public ActionResult _DeleteAppGroup(int id)
        {
            Group group = (from x in PrdnDBContext.Groups
                       where x.ID == id
                       select x).FirstOrDefault();

            if (group != null)
            {
                PrdnDBContext.DeleteObject(group);
                PrdnDBContext.SaveChanges();
            }
            return View(AppGroupGridList(group.AppID));
        }

        protected List<UserLookupViewModel> UserPropLookup(IQueryable<User> userQry, string term, string property)
        {
            string termUpper = term.ToUpper();
            string propUpper = property.ToUpper();

            var modelQry = (from u in userQry
                            select new UserLookupViewModel
                            {
                                ID = u.ID,
                                Login = u.Login,
                                LastName = u.LastName,
                                FirstName = u.FirstName,
                                EMail = u.EMail
                            }).Take(20);

            List<UserLookupViewModel> users = null;
            User user = null;
            if (propUpper == user.FullPropertyName(u => u.Login).ToUpper())
            {
                users = (from u in modelQry
                         where u.Login.ToUpper().StartsWith(termUpper)
                         orderby u.Login
                         select u).ToList();

                users.ForEach(u =>
                {
                    u.LookupValue = u.Login;
                    u.LookupLabel = u.Login + " " + u.FirstName + " " + u.LastName;
                });
            }

            else if (propUpper == user.FullPropertyName(u => u.FirstName).ToUpper())
            {
                users = (from u in modelQry
                         where u.FirstName.ToUpper().StartsWith(termUpper)
                         orderby u.FirstName
                         select u).ToList();

                users.ForEach(u =>
                {
                    u.LookupValue = u.FirstName;
                    u.LookupLabel = u.FirstName + " " + u.LastName + " (" + u.Login + ")";
                });
            }

            else if (propUpper == user.FullPropertyName(u => u.LastName).ToUpper())
            {
                users = (from u in modelQry
                         where u.LastName.ToUpper().StartsWith(termUpper)
                         orderby u.LastName
                         select u).ToList();

                users.ForEach(u =>
                {
                    u.LookupValue = u.LastName;
                    u.LookupLabel = u.LastName + ", " + u.FirstName + " (" + u.Login + ")";
                });
            }

            else if (propUpper == user.FullPropertyName(u => u.EMail).ToUpper())
            {
                users = (from u in modelQry
                         where u.EMail.ToUpper().StartsWith(termUpper)
                         orderby u.EMail
                         select u).ToList();

                users.ForEach(u =>
                {
                    u.LookupValue = u.EMail;
                    u.LookupLabel = u.EMail + " " + u.FirstName + " " + u.LastName + " (" + u.Login + ")";
                });
            }

            return users;
        }

        public ActionResult _UserLookup(string term, string property)
        {
            var userQry = from u in PrdnDBContext.Users
                          select u;

            List<UserLookupViewModel> users = UserPropLookup(userQry, term, property);
            return Json(users, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult _GroupsSelectList(int? grpLookAppsDownList, int? userID)
        {
            SelectList groupsList;
            if (userID == null)
            {
                groupsList = new SelectList(AppGroupViewModelList((int)grpLookAppsDownList), "ID", "CodeDashName");
            }
            else
            {
                var apps = from a in GroupsNotAssgnToUser((int)userID, grpLookAppsDownList)
                           orderby a.Code
                           select a;

                groupsList = new SelectList(apps.ToList(), "ID", "CodeDashName");
            }

            return Json(groupsList, JsonRequestBehavior.AllowGet);
        }

        public IEnumerable<App> AppsNotAssgnToUser(int userID)
        {
            var groups = GroupsNotAssgnToUser(userID);

            return from a in PrdnDBContext.Apps
                   where groups.Any(gq => gq.AppID == a.ID)
                   select a;
        }

        ///
        /// Group Users

        public ActionResult GroupUsers(int? groupID)
        {
            if (groupID == null)
            {
                return ErrMsgView("Sorry - A Group ID is required for group users.");
            }

            var group = (from g in PrdnDBContext.Groups.Include("App")
                        where g.ID == (int)groupID
                        select g).FirstOrDefault();
            
            if (group == null)
            {
                return ErrMsgView("Sorry - Group ID " + groupID + " is not valid");
            }

            GroupUsersViewModel model = new GroupUsersViewModel { 
                GroupID = group.ID,
                GroupCode = group.Code,
                GroupName = group.Name,
                AppID = group.App.ID,
                AppCode = group.App.Code,
                AppName = group.App.Name
            };

            return View(model);
        }

        protected IEnumerable<User> GroupUserList(int groupID)
        {
            var users = from g in PrdnDBContext.Groups
                        from u in g.Users
                        where g.ID == groupID
                        orderby u.Login
                        select u;

            return users;
        }

        protected GridModel GroupUserGridList(int groupID)
        {
            var users = from u in GroupUserList(groupID)
                        select u;

            List<UserLookupViewModel> viewUsers = Mapper.Map<List<User>, List<UserLookupViewModel>>(users.ToList());

            return new GridModel(viewUsers);
        }

        [GridAction]
        public ActionResult _SelectGroupUser(string groupID)
        {
            return View(GroupUserGridList(Convert.ToInt32(groupID)));
        }

        [HttpPost]
        [GridAction]
        public ActionResult _InsertGroupUser(string groupID)
        {
            int groupIntID;
            try
            {
                groupIntID = Convert.ToInt32(groupID);
            }
            catch (Exception)
            {
                return ErrMsgView("Sorry - An invalid Group ID was used to insert a user.");
            }

            UserLookupViewModel viewUser = new UserLookupViewModel();
            if (TryUpdateModel(viewUser))
            {
                var addUser = (from u in PrdnDBContext.Users
                              where u.ID == viewUser.ID
                              select u).FirstOrDefault();

                if (addUser != null)
                {
                    var group = (from g in PrdnDBContext.Groups
                                 where g.ID == groupIntID
                                 select g).FirstOrDefault();

                    if (group != null)
                    {
                        group.Users.Add(addUser);
                        PrdnDBContext.SaveChanges();
                    }
                    else
                    {
                        ModelState.AddModelError("ID", "invalid group ID");
                    }
                }
                else
                {
                    ModelState.AddModelError("ID", "invalid user ID");
                }
            }

            return View(GroupUserGridList(groupIntID));
        }

        [HttpPost]
        [GridAction]
        public ActionResult _DeleteGroupUser(string groupID)
        {
            int groupIntID;
            try
            {
                groupIntID = Convert.ToInt32(groupID);
            }
            catch (Exception)
            {
                return ErrMsgView("Sorry - An invalid Group ID was used to insert a user.");
            }

            UserLookupViewModel viewUser = new UserLookupViewModel();
            if (TryUpdateModel(viewUser))
            {
                Group grp = (from g in PrdnDBContext.Groups
                               where g.ID == groupIntID
                               select g).FirstOrDefault();

                if (grp != null)
                {
                    User user = (from u in grp.Users
                                 where u.ID == viewUser.ID
                                 select u).FirstOrDefault();

                    if (user != null)
                    {
                        grp.Users.Remove(user);
                        PrdnDBContext.SaveChanges();
                    }
                    else
                    {
                        ModelState.AddModelError("ID", "User ID "+viewUser.ID.ToString()+" was not a member of group "+grp.Name+".");
                    }
                }
                else
                {
                    ModelState.AddModelError("ID", "invalid group ID");
                }
            }
            return View(GroupUserGridList(groupIntID));
        }

        public ActionResult _UserNotGroupLookup(int groupID, string term, string property)
        {
            var groupUsers = from g in PrdnDBContext.Groups
                             from u in g.Users
                             where g.ID == groupID
                             select u;

            var userQry = from u in PrdnDBContext.Users
                          where !groupUsers.Any(gu => gu.ID == u.ID)
                          select u;

            List<UserLookupViewModel> users = UserPropLookup(userQry, term, property);
            return Json(users, JsonRequestBehavior.AllowGet);
        }

        ///
        /// User Groups

        public ActionResult UserGroups(int? userID)
        {
            if (userID == null)
            {
                return ErrMsgView("Sorry - A User ID is required for user groups.");
            }

            var user = (from u in PrdnDBContext.Users
                         where u.ID == (int)userID
                         select u).FirstOrDefault();

            if (user == null)
            {
                return ErrMsgView("Sorry - User ID " + userID + " is not valid");
            }

            UserGroupsViewModel model = new UserGroupsViewModel
            {
                UserID = user.ID,
                UserLogin = user.Login,
                FullName = user.FullName
            };

            return View(model);
        }

        protected IEnumerable<Group> UserGroupList(int userID)
        {
            var groups = from u in PrdnDBContext.Users //.Include("Groups").Include("App")
                         from g in u.Groups
                         where u.ID == userID
                         orderby g.App.Code, g.Code
                         select g;

            return groups;
        }

        protected GridModel UserGroupGridList(int userID)
        {
            var groups = from u in UserGroupList(userID)
                         select u;

            List<GroupLookupViewModel> viewGroups = Mapper.Map<List<Group>, List<GroupLookupViewModel>>(groups.ToList());

            viewGroups.ForEach(m => m.UserID = userID);

            return new GridModel(viewGroups);
        }

        [GridAction]
        public ActionResult _SelectUserGroup(string userID)
        {
            return View(UserGroupGridList(Convert.ToInt32(userID)));
        }

        [HttpPost]
        [GridAction]
        public ActionResult _InsertUserGroup(string userID,  GroupLookupViewModel viewGroup)
        {
            int userIntID;
            try
            {
                userIntID = Convert.ToInt32(userID);
            }
            catch (Exception)
            {
                return ErrMsgView("Sorry - An invalid User ID was used to insert a group.");
            }
            if (ModelState.IsValid)
            {
                var addGroup = (from g in PrdnDBContext.Groups
                                where g.ID == viewGroup.ID
                                select g).FirstOrDefault();

                if (addGroup != null)
                {
                    var user = (from u in PrdnDBContext.Users
                                where u.ID == userIntID
                                select u).FirstOrDefault();

                    if (user != null)
                    {
                        user.Groups.Add(addGroup);
                        PrdnDBContext.SaveChanges();
                    }
                    else
                    {
                        ModelState.AddModelError(viewGroup.FullPropertyName(m => m.AppID), "invalid user ID");
                    }
                }
                else
                {
                    ModelState.AddModelError(viewGroup.FullPropertyName(m => m.ID), "invalid group ID");
                }
            }
            return View(UserGroupGridList(userIntID));
        }

        [HttpPost]
        [GridAction]
        public ActionResult _DeleteUserGroup(string userID, GroupLookupViewModel viewGroup)
        {
            int userIntID;
            try
            {
                userIntID = Convert.ToInt32(userID);
            }
            catch (Exception)
            {
                return ErrMsgView("Sorry - An invalid User ID was used to insert a group.");
            }
            if (ModelState.IsValid)
            {
                User usr = (from u in PrdnDBContext.Users
                            where u.ID == userIntID
                            select u).FirstOrDefault();

                if (usr != null)
                {
                    Group grp = (from u in usr.Groups
                                 where u.ID == viewGroup.ID
                                 select u).FirstOrDefault();

                    if (grp != null)
                    {
                        usr.Groups.Remove(grp);
                        PrdnDBContext.SaveChanges();
                    }
                    else
                    {
                        ModelState.AddModelError("ID", "Group ID " + viewGroup.ID.ToString() + " was not a member of group " + grp.Code + ".");
                    }
                }
                else
                {
                    ModelState.AddModelError("ID", "invalid user ID");
                }
            }
            return View(UserGroupGridList(userIntID));
        }

    }
}
