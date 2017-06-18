using Simplified.Ring3;
using Simplified.Ring5;
using Starcounter;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SignIn.Api
{
    internal class Middleware
    {
        protected string AuthCookieName = "soauthtoken";
        protected int rememberMeDays = 30;

        internal void Register()
        {
            Application.Current.Use(new HtmlFromJsonProvider());
            Application.Current.Use(new PartialToStandaloneHtmlProvider());

            Application.Current.Use((Request req) =>
            {
                Cookie cookie = GetSignInCookie();

                if (cookie != null)
                {
                    if (Session.Current == null)
                    {
                        Session.Current = new Session(SessionOptions.PatchVersioning);
                    }

                    SystemUserSession session = SystemUser.SignInSystemUser(cookie.Value);

                    if (session != null)
                    {
                        RefreshAuthCookie(session);
                    }
                }

                return null;
            });
        }


        protected Cookie GetSignInCookie()
        {
            List<Cookie> cookies = Handle.IncomingRequest.Cookies.Where(val => !string.IsNullOrEmpty(val)).Select(x => new Cookie(x)).ToList();
            Cookie cookie = cookies.FirstOrDefault(x => x.Name == AuthCookieName);

            return cookie;
        }

        protected void RefreshAuthCookie(SystemUserSession Session)
        {
            Cookie cookie = GetSignInCookie();

            if (cookie == null)
            {
                return;
            }

            Db.Transact(() =>
            {
                Session.Token = SystemUser.RenewAuthToken(Session.Token);
                if (Session.Token.IsPersistent)
                {
                    Session.Token.Expires = DateTime.UtcNow.AddDays(rememberMeDays);
                }
            });

            cookie.Value = Session.Token.Token;
            if (Session.Token.IsPersistent)
            {
                cookie.Expires = Session.Token.Expires;
            }

            Handle.AddOutgoingCookie(cookie.Name, cookie.GetFullValueString());
        }
    }
}
