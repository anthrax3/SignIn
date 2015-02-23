using Concepts.Ring1;
using Concepts.Ring2;
using Concepts.Ring3;
using Starcounter;

namespace Concepts.Ring8.Polyjuice.Permissions {

    [Database]
    public class UriPermission : Something {
        public string Uri;
        public bool CanGet;


        static public bool CanGetUri(SystemUser user, string uri, Request request) {

            // Check if there is any permission set for a url
            UriPermission per = Db.SQL<UriPermission>("SELECT o FROM  Concepts.Ring8.Polyjuice.Permissions.UriPermission o WHERE o.Uri=?", uri).First;
            if (per == null) {

                // TODO: Check if user is part of Admin group, then allow acces?

                // No permission configuration for this url = DENY ACCESS
                return false;
            }

            UriPermission permission = UriPermission.GetPermission(user, uri);
            if (permission != null) {
                return permission.CanGet;
            }

            return false;
        }

        static private UriPermission GetPermission(SystemUser user, string uri) {

            if (user == null || string.IsNullOrEmpty(uri)) {
                return null;
            }

            UriPermission permission = Db.SQL<UriPermission>("SELECT o.Permission FROM Concepts.Ring8.Polyjuice.Permissions.SystemUserUriPermission o WHERE o.Permission.Uri=? AND o.SystemUser=?", uri, user).First;
            if (permission != null) {
                return permission;
            }

            // Check user group
            var groups = Db.SQL<Concepts.Ring3.SystemUserGroupMember>("SELECT o FROM Concepts.Ring3.SystemUserGroupMember o WHERE o.SystemUser=?", user);
            foreach (var group in groups) {

                permission = GetPermissionFromGroup(group.SystemUserGroup, uri);
                if (permission != null) {
                    return permission;
                }
            }
            return null;
        }

        static private UriPermission GetPermissionFromGroup(SystemUserGroup group, string url) {

            if (group == null) return null;

            UriPermission permission = Db.SQL<UriPermission>("SELECT o.Permission FROM Concepts.Ring8.Polyjuice.Permissions.SystemUserGroupUriPermission o WHERE o.Permission.Uri=? AND o.SystemUserGroup=?", url, group).First;
            if (permission != null) {
                return permission;
            }

            permission = GetPermissionFromGroup(group.Parent, url);
            if (permission != null) {
                return permission;
            }

            return null;
        }

        //static public bool IsMemberOfAdminGroup(Concepts.Ring3.SystemUser user) {

        //    if (user == null) return false;
        //    Concepts.Ring3.SystemUserGroup adminGroup = Db.SQL<Concepts.Ring3.SystemUserGroup>("SELECT o FROM Concepts.Ring3.SystemUserGroup o WHERE o.Name=?", Program.AdminGroupName).First;

        //    return IsMemberOfGroup(user, adminGroup);
        //}

        static public bool IsMemberOfGroup(SystemUser user, SystemUserGroup basedOnGroup) {

            if (user == null) return false;
            if (basedOnGroup == null) return false;

            var groups = Db.SQL<Concepts.Ring3.SystemUserGroup>("SELECT o.SystemUserGroup FROM Concepts.Ring3.SystemUserGroupMember o WHERE o.SystemUser=?", user);
            foreach (var groupItem in groups) {

                bool flag = IsBasedOnGroup(groupItem, basedOnGroup);
                if (flag) {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// TODO: Avoid circular references!!
        /// </summary>
        /// <param name="group"></param>
        /// <param name="basedOnGroup"></param>
        /// <returns></returns>
        static private bool IsBasedOnGroup(SystemUserGroup group, SystemUserGroup basedOnGroup) {
            if (group == null) return false;

            // NOTE: To compare to objects queried from database we need to use .Equals(),  "==" wont work!!.
            if (group.Equals(basedOnGroup)) {
                return true;
            }

            if (IsBasedOnGroup(group.Parent, basedOnGroup)) {
                return true;
            }

            return false;
        }


        static public void AssureUriPermission(string uri, SystemUserGroup group) {

            UriPermission permission = Db.SQL<UriPermission>("SELECT o.Permission FROM Concepts.Ring8.Polyjuice.Permissions.SystemUserGroupUriPermission o WHERE o.Permission.Uri=? AND o.SystemUserGroup=?", uri, group).First;

            if (permission == null) {

                Db.Transact(() => {
                    UriPermission p1 = new UriPermission() { Uri = uri, CanGet = true };
                    new SystemUserGroupUriPermission() { ToWhat = p1, WhatIs = group };
                });
            }
        }


        /// <summary>
        /// Assure that there is at least one system user beloning to the admin group 
        /// </summary>
        static public void AssureOneAdminSystemUser(string adminGroupName, string description) {

            Concepts.Ring3.SystemUserGroup adminGroup = Db.SQL<SystemUserGroup>("SELECT o FROM Concepts.Ring3.SystemUserGroup o WHERE o.Name=?", adminGroupName).First;

            // Assure that there is at least one system user with admin rights
            var result = Db.SQL<SystemUser>("SELECT o FROM Concepts.Ring3.SystemUser o");
            foreach (var user in result) {

                if (UriPermission.IsMemberOfGroup(user, adminGroup)) {
                    return;
                }
            }

            // There is no system user beloning to the admin group

            Db.Transact(() => {

                // Assure that there is a Admin group

                if (adminGroup == null) {
                    adminGroup = new Concepts.Ring3.SystemUserGroup();
                    adminGroup.Name = adminGroupName;
                    adminGroup.Description = description;
                }

                // Check if there is an "admin" system user
                Concepts.Ring3.SystemUser systemUser = Db.SQL<Concepts.Ring3.SystemUser>("SELECT o FROM Concepts.Ring3.SystemUser o WHERE o.Username=?", "admin").First;
                if (systemUser == null) {

                    string username = "admin";
                    string password = "admin";

                    // Create Person and system user;
                    Person person = new Person() { FirstName = username, Surname = username };
                    systemUser = new Concepts.Ring3.SystemUser(person);
                    systemUser.Username = username;

                    // Set password
                    string hashedPassword;
                    Concepts.Ring8.Polyjuice.SystemUserPassword.GeneratePasswordHash(systemUser.Username.ToLower(), password, out hashedPassword);
                    systemUser.Password = hashedPassword;


                    // Add ability to also sign in with email
                    EMailAddress emailRel = new EMailAddress();
                    emailRel.SetToWhat(systemUser);
                    emailRel.EMail = "change@this.email".ToLowerInvariant();
                    //person.ImageURL = Utils.GetGravatarUrl(emailRel.EMail);


                    //systemUser = SystemUserAdmin.AddPerson("admin", "admin", "admin", "change@this.email", "admin");
                }

                // Add the admin group to the system admin user
                SystemUserGroupMember systemUserGroupMember = new Concepts.Ring3.SystemUserGroupMember();
                systemUserGroupMember.SetSystemUser(systemUser);
                systemUserGroupMember.SetToWhat(adminGroup);

                //SystemUserAdmin.AddSystemUserToSystemUserGroup(systemUser, adminGroup);
            });

        }

    }
}
