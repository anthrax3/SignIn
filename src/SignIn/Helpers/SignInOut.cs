using System;
using System.Web;
using Starcounter;
using Starcounter.Internal;
using Simplified.Ring2;
using Simplified.Ring3;
using Simplified.Ring5;

namespace SignIn
{
    public class SignInOut
    {
        internal static string AdminGroupName = "Admin (System Users)";
        internal static string AdminGroupDescription = "System User Administrator Group";
        internal static string AdminUsername = "admin";
        internal static string AdminPassword = "admin";
        internal static string AdminEmail = "admin@starcounter.com";

        /// <summary>
        /// Assure that there is at least one system user beloning to the admin group 
        /// </summary>
        internal static void AssureAdminSystemUser()
        {
            SystemUserGroup group =
                Db.SQL<SystemUserGroup>("SELECT o FROM Simplified.Ring3.SystemUserGroup o WHERE o.Name = ?",
                    AdminGroupName).First;
            SystemUser user =
                Db.SQL<SystemUser>("SELECT o FROM Simplified.Ring3.SystemUser o WHERE o.Username = ?", AdminUsername)
                    .First;

            if (group != null && user != null && SystemUser.IsMemberOfGroup(user, group))
            {
                return;
            }

            // There is no system user beloning to the admin group
            Db.Transact(() =>
            {
                if (group == null)
                {
                    group = new SystemUserGroup();
                    group.Name = AdminGroupName;
                    group.Description = AdminGroupDescription;
                }

                if (user == null)
                {
                    Person person = new Person()
                    {
                        FirstName = AdminUsername,
                        LastName = AdminUsername
                    };

                    user = SystemUser.RegisterSystemUser(AdminUsername, AdminEmail, AdminPassword);
                    user.WhatIs = person;
                }

                // Add the admin group to the system admin user
                SystemUserGroupMember member = new Simplified.Ring3.SystemUserGroupMember();

                member.WhatIs = user;
                member.ToWhat = group;
            });
        }
    }
}