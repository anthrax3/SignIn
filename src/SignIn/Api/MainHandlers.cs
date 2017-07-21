using System;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using Starcounter;
using Simplified.Ring3;
using Simplified.Ring2;
using Simplified.Ring6;
using SignIn.Helpers;

namespace SignIn
{
    internal class MainHandlers
    {
        private CookieHelpers cookieHelpers = new CookieHelpers();

        public void Register()
        {
            Handle.GET("/signin/app-name", () => new AppName());

            Handle.GET("/signin", () =>
            {
                return Self.GET("/signin/signinuser");
            });

            Handle.GET("/signin/user", () =>
            {
                MasterPage master = this.GetMaster();

                if (master.SignInPage != null)
                {
                    return master.SignInPage;
                }

                Cookie cookie = cookieHelpers.GetSignInCookie();
                SignInPage page = new SignInPage() { Data = null };

                Session.Current.Store[nameof(SignInPage)] = page;
                
                if (cookie != null)
                {
                    SystemUser.SignInSystemUser(cookie.Value);
                    master.RefreshSignInState();
                }

                return page;
            });

            Handle.GET("/signin/signinuser", HandleSignInForm);
            Handle.GET<string>("/signin/signinuser?{?}", HandleSignInForm);

            Handle.GET("/signin/profile", () =>
            {
                MasterPage master = this.GetMaster();

                master.RequireSignIn = true;
                master.Open("/signin/partial/profile-form");

                return master;
            });

            Handle.GET("/signin/generateadminuser", (Request request) =>
            {
                if (Db.SQL("SELECT o FROM Simplified.Ring3.SystemUser o").First != null)
                {
                    Handle.SetOutgoingStatusCode(403);
                    return "Unable to generate admin user: database is not empty!";
                }

                string ip = request.ClientIpAddress.ToString();

                if (ip == "127.0.0.1" || ip == "localhost")
                {
                    SignInOut.AssureAdminSystemUser();

                    return "Default admin user has been successfully generated.";
                }

                Handle.SetOutgoingStatusCode(403);
                return "Access denied.";

            }, new HandlerOptions() { SkipRequestFilters = true });

            Handle.GET("/signin/admin/settings", (Request request) =>
            {
                Json page;
                if (!AuthorizationHelper.TryNavigateTo("/signin/admin/settings", request, out page))
                {
                    return page;
                }

                return Db.Scope(() =>
                {
                    var settingsPage = new SettingsPage
                    {
                        Html = "/SignIn/viewmodels/SettingsPage.html",
                        Uri = request.Uri,
                        Data = MailSettingsHelper.GetSettings()
                    };
                    return settingsPage;
                });
            });

            // Reset password
            Handle.GET("/signin/user/resetpassword?{?}", (string query, Request request) =>
            {
                NameValueCollection queryCollection = HttpUtility.ParseQueryString(query);
                string token = queryCollection.Get("token");

                MasterPage master = this.GetMaster();

                if (token == null)
                {
                    // TODO:
                    master.Partial = null; // (ushort)System.Net.HttpStatusCode.NotFound;
                    return master;
                }

                // Retrive the resetPassword instance
                ResetPassword resetPassword = Db.SQL<ResetPassword>("SELECT o FROM Simplified.Ring6.ResetPassword o WHERE o.Token=? AND o.Expire>?", token, DateTime.UtcNow).First;

                if (resetPassword == null)
                {
                    // TODO: Show message "Reset token already used or expired"
                    master.Partial = null; // (ushort)System.Net.HttpStatusCode.NotFound;
                    return master;
                }

                if (resetPassword.User == null)
                {
                    // TODO: Show message "User deleted"
                    master.Partial = null; // (ushort)System.Net.HttpStatusCode.NotFound;
                    return master;
                }

                SystemUser systemUser = resetPassword.User;

                ResetPasswordPage page = new ResetPasswordPage()
                {
                    Html = "/SignIn/viewmodels/ResetPasswordPage.html",
                    Uri = "/signin/user/resetpassword"
                    //Uri = request.Uri // TODO:
                };

                page.ResetPassword = resetPassword;

                if (systemUser.WhoIs != null)
                {
                    page.FullName = systemUser.WhoIs.FullName;
                }
                else
                {
                    page.FullName = systemUser.Username;
                }

                master.Partial = page;

                return master;
            });

            Handle.GET("/signin/user/authentication/settings/{?}", (string userid, Request request) =>
            {
                Json page;
                if (!AuthorizationHelper.TryNavigateTo("/signin/user/authentication/settings/{?}", request, out page))
                {
                    return new Json();
                }

                // Get system user
                SystemUser user = Db.SQL<SystemUser>("SELECT o FROM Simplified.Ring3.SystemUser o WHERE o.ObjectID = ?", userid).FirstOrDefault();

                if (user == null)
                {
                    // TODO: Return a "User not found" page
                    return new Json();
                    //return (ushort)System.Net.HttpStatusCode.NotFound;
                }

                SystemUser systemUser = SystemUser.GetCurrentSystemUser();
                SystemUserGroup adminGroup = Db.SQL<SystemUserGroup>("SELECT o FROM Simplified.Ring3.SystemUserGroup o WHERE o.Name = ?",
                        AuthorizationHelper.AdminGroupName).FirstOrDefault();

                // Check if current user has permission to get this user instance
                if (AuthorizationHelper.IsMemberOfGroup(systemUser, adminGroup))
                {
                    if (user.WhoIs is Person)
                    {
                        page = Db.Scope(() => new SystemUserAuthenticationSettings
                        {
                            Html = "/SignIn/viewmodels/SystemUserAuthenticationSettings.html",
                            Uri = request.Uri,
                            Data = user,
                            UserPassword = Self.GET("/signin/user/authentication/password/" + user.GetObjectID())
                        });

                        return page;
                    }
                }

                return new Json();
            }, new HandlerOptions { SelfOnly = true });

            Handle.GET("/signin/user/authentication/password/{?}", (string userid, Request request) =>
            {
                // Get system user
                SystemUser user = Db.SQL<SystemUser>("SELECT o FROM Simplified.Ring3.SystemUser o WHERE o.ObjectID = ?", userid).FirstOrDefault();

                if (user == null)
                {
                    return new Json();
                }

                Json page = Db.Scope(() => new SetPasswordPage
                {
                    Html = "/SignIn/viewmodels/SetPasswordPage.html",
                    Data = user
                });

                return page;
            }, new HandlerOptions { SelfOnly = true });
        }

        internal MasterPage GetMaster()
        {
            MasterPage master = Session.Ensure().Store[nameof(MasterPage)] as MasterPage;
            if (master == null) 
            {
                master = new MasterPage();
                Session.Current.Store[nameof(MasterPage)] = master;
            }
            return master;
        }

        protected Response HandleSignInForm()
        {
            return this.HandleSignInForm(string.Empty);
        }

        protected Response HandleSignInForm(string OriginalUrl)
        {
            var settings = DataHelper.GetSettings();

            MasterPage master = this.GetMaster();
            master.RequireSignIn = false;

            if (settings.SignInFormAsFullPage && Handle.CallLevel > 0 && !string.IsNullOrEmpty(OriginalUrl))
            {
                master.Redirect("/signin/signinuser?" + OriginalUrl);
            }
            else
            {
                master.OriginalUrl = HttpUtility.UrlDecode(OriginalUrl);
                master.Open("/signin/partial/main-form");
            }

            return master;
        }
    }
}