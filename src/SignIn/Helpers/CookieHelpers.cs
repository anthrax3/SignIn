using Simplified.Ring3;
using Simplified.Ring5;
using Starcounter;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SignIn.Helpers
{
    internal class CookieHelpers
    {
        internal string AuthCookieName = "soauthtoken";
        internal int rememberMeDays = 30;

        internal void SetAuthCookie(SystemUserTokenKey token)
        {
            Cookie cookie = new Cookie()
            {
                Name = AuthCookieName
            };

            if (token == null)
            {
                DeleteCookie(cookie);
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

        internal void ClearAuthCookie()
        {
            this.SetAuthCookie(null);
        }

        internal void DeleteCookie(Cookie cookie)
        {
            cookie.Expires = DateTime.Now.AddDays(-1).ToUniversalTime();
        }


        internal Cookie GetSignInCookie()
        {
            List<Cookie> cookies = Handle.IncomingRequest.Cookies.Where(val => !string.IsNullOrEmpty(val)).Select(x => new Cookie(x)).ToList();
            Cookie cookie = cookies.FirstOrDefault(x => x.Name == AuthCookieName);

            return cookie;
        }

        internal void RefreshAuthCookie(SystemUserSession Session)
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
