using SignIn.Helpers;
using Simplified.Ring3;
using Simplified.Ring5;
using Starcounter;
using System;
using System.Collections.Specialized;
using System.Web;

namespace SignIn.Api
{
    internal class PartialHandlers
    {
        private CookieHelpers cookieHelper = new CookieHelpers();
        private MainHandlers mainHandlers = new MainHandlers();

        internal void Register()
        {

            HandlerOptions internalOption = new HandlerOptions() { SkipRequestFilters = true };

            Handle.POST("/signin/partial/signin", (Request request) =>
            {
                NameValueCollection values = HttpUtility.ParseQueryString(request.Body);
                string username = values["username"];
                string password = values["password"];
                string rememberMe = values["rememberMe"];

                HandleSignIn(username, password, rememberMe);
                return 200;
            }, internalOption);

            Handle.GET("/signin/partial/signin-form", () => new SignInFormPage() { Data = null }, internalOption);
            Handle.GET("/signin/partial/alreadyin-form", () => new AlreadyInPage() { Data = null }, internalOption);
            Handle.GET("/signin/partial/restore-form", () => new RestorePasswordFormPage(), internalOption);
            Handle.GET("/signin/partial/profile-form", () => new ProfileFormPage() { Data = null }, internalOption);
            Handle.GET("/signin/partial/accessdenied-form", () => new AccessDeniedPage(), internalOption);
            Handle.GET("/signin/partial/main-form", () => new MainFormPage() { Data = null }, internalOption);
            Handle.GET("/signin/partial/user/image", () => new UserImagePage());
            Handle.GET("/signin/partial/user/image/{?}", (string objectId) => new Json(), internalOption);
            Handle.GET("/signin/partial/signout", HandleSignOut, internalOption);
        }

        protected void HandleSignIn(string Username, string Password, string RememberMe)
        {
            Username = Uri.UnescapeDataString(Username);

            SystemUserSession session = SystemUser.SignInSystemUser(Username, Password);

            if (session == null)
            {
                MasterPage master = mainHandlers.GetMaster();
                string message = "Invalid username or password!";

                if (master.SignInPage != null)
                {
                    master.SignInPage.Message = message;
                }

                if (master.Partial is MainFormPage)
                {
                    MainFormPage page = (MainFormPage)master.Partial;
                    if (page.CurrentForm is SignInFormPage)
                    {
                        SignInFormPage form = (SignInFormPage)page.CurrentForm;
                        form.Message = message;
                    }
                }

                if (master.Partial is SignInFormPage)
                {
                    SignInFormPage page = master.Partial as SignInFormPage;
                    page.Message = message;
                }
            }
            else
            {
                if (RememberMe == "true")
                {
                    Db.Transact(() =>
                    {
                        session.Token.Expires = DateTime.UtcNow.AddDays(cookieHelper.rememberMeDays);
                        session.Token.IsPersistent = true;
                    });
                }
                cookieHelper.SetAuthCookie(session.Token);
            }
        }

        protected Response HandleSignOut()
        {
            SystemUser.SignOutSystemUser();
            cookieHelper.ClearAuthCookie();

            return 200;
        }
    }
}
