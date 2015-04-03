using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using Starcounter;
using PolyjuiceNamespace;
using Simplified.Ring3;

namespace SignIn {
    class Program {
         private static string AuthCookieName = "soauthtoken";

         static void Main() {
             CommitHooks.Register();

             Starcounter.Handle.GET("/signin/user", () => {
                 Session session = Session.Current;

                 if (session != null && session.Data != null) {
                     return session.Data;
                 }

                 List<Cookie> cookies = Handle.IncomingRequest.Cookies.Select(x => new Cookie(x)).ToList();
                 Cookie cookie = cookies.FirstOrDefault(x => x.Name == AuthCookieName);
                 SignInPage page = new SignInPage();

                 if (cookie != null) {
                     page.FromCookie(cookie.Value);
                 } else {
                     page.SetAnonymousState();
                 }

                 page.Session = session;

                 return page;
             });

             Handle.GET("/signin/signin/{?}/{?}", (string Username, string Password) => {
                 SignInPage page = Session.Current.Data as SignInPage;

                 page.SignIn(Username, Password);
                 SetAuthCookie(page);

                 return page.SignInForm;
             });

             Handle.GET("/signin/signout", () => {
                 SignInPage page = Session.Current.Data as SignInPage;

                 page.SignOut();
                 SetAuthCookie(page);

                 return page.SignInForm;
             });

             Starcounter.Handle.GET("/signin/signinuser", () => {
                 SignInPage master = X.GET<Page>("/signin/user") as SignInPage;
                 SignInFormPage page = new SignInFormPage();

                 master.SignInForm = page;
                 master.UpdateSignInForm();

                 return page;
             });

             Starcounter.Handle.GET("/signin/signinuser?{?}", (string query) => {
                 SignInPage master = X.GET<Page>("/signin/user") as SignInPage;
                 SignInFormPage page = new SignInFormPage();
                 string decodedQuery = HttpUtility.UrlDecode(query);
                 NameValueCollection queryCollection = HttpUtility.ParseQueryString(decodedQuery);

                 page.OriginUrl = queryCollection.Get("originurl");
                 master.SignInForm = page;
                 master.UpdateSignInForm();

                 return page;
             });

             Polyjuice.Map("/signin/user", "/polyjuice/user");
         }

         static Response GetNoSessionResult() {
             return new Response() {
                 StatusCode = (ushort)System.Net.HttpStatusCode.InternalServerError,
                 Body = "No Current Session"
             };
         }

         static void SetAuthCookie(SignInPage Page) {
             Cookie cookie = new Cookie(AuthCookieName, Page.SignInAuthToken);

             Handle.AddOutgoingCookie(cookie.ToString());
         }
    }
}
