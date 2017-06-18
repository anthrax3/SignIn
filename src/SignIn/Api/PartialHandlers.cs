using Simplified.Ring3;
using Simplified.Ring5;
using Starcounter;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace SignIn.Api
{
    internal class PartialHandlers
    {
        protected string AuthCookieName = "soauthtoken";
        protected int rememberMeDays = 30;

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
                Session.Current.CalculatePatchAndPushOnWebSocket();

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
                MasterPage master = GetMaster();
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
                        session.Token.Expires = DateTime.UtcNow.AddDays(rememberMeDays);
                        session.Token.IsPersistent = true;
                    });
                }
                SetAuthCookie(session.Token);
            }
        }

        protected MasterPage GetMaster()
        {
            Session session = Session.Current;

            if (session != null && session.Data != null)
            {
                return session.Data as MasterPage;
            }

            MasterPage master = new MasterPage();

            if (session == null)
            {
                session = new Session(SessionOptions.PatchVersioning);
            }

            master.Session = session;
            return master;
        }

        protected void SetAuthCookie(SystemUserTokenKey token)
        {
            Cookie cookie = new Cookie()
            {
                Name = AuthCookieName
            };

            if (token == null)
            {
                //to delete a cookie, explicitly use a date in the past
                cookie.Expires = DateTime.Now.AddDays(-1).ToUniversalTime();
            }
            else
            {
                cookie.Value = token.Token;
                if (token.IsPersistent)
                {
                    cookie.Expires = token.Expires;
                }
            }

            Handle.AddOutgoingCookie(cookie.Name, cookie.GetFullValueString());
        }

        protected Response HandleSignOut()
        {
            SystemUser.SignOutSystemUser();
            ClearAuthCookie();

            return this.GetMaster();
        }

        protected void ClearAuthCookie()
        {
            this.SetAuthCookie(null);
        }
    }
}
