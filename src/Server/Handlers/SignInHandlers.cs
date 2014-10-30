using Starcounter;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;

namespace SignInApp.Server.Handlers {
    public class SignInHandlers {

        // TODO: How to remove items from this list
        internal static Dictionary<string, SignIn> signInSessions = new Dictionary<string, SignIn>();

        internal static void Register() {

            HandlerOptions opt = new HandlerOptions() { HandlerLevel = 0 };

            Starcounter.Handle.GET("/signinapp/signinuser", (Request request) => {

                if (Session.Current == null) {
                    return new Response() { StatusCode = (ushort)System.Net.HttpStatusCode.InternalServerError, Body = "No Current Session" };
                }

                string sessionID = Session.Current.SessionIdString;
                if (!signInSessions.ContainsKey(sessionID)) {
                    return new Response() { StatusCode = (ushort)System.Net.HttpStatusCode.InternalServerError, Body = "Failed to get the signin app Session" };
                }
                SignIn masterPage = signInSessions[sessionID];

                var signInUserPage = new SignInUser() { Html = "/signinuser.html" };

                masterPage.SignInUserPage = signInUserPage;

                if (masterPage.IsSignedIn) {
                    signInUserPage.RedirectUrl = signInUserPage.OriginUrl;
                }
                return signInUserPage;
            }, opt);

            Starcounter.Handle.GET("/signinapp/signinuser?{?}", (string query, Request request) => {

                if (Session.Current == null) {
                    return new Response() { StatusCode = (ushort)System.Net.HttpStatusCode.InternalServerError, Body = "No Current Session" };
                }

                string sessionID = Session.Current.SessionIdString;
                if (!signInSessions.ContainsKey(sessionID)) {
                    return new Response() { StatusCode = (ushort)System.Net.HttpStatusCode.InternalServerError, Body = "Failed to get the signin app Session" };
                }
                SignIn masterPage = signInSessions[sessionID];

                var signInUserPage = new SignInUser() { Html = "/signinuser.html" };

                string decodedQuery = HttpUtility.UrlDecode(query);

                NameValueCollection queryCollection = HttpUtility.ParseQueryString(decodedQuery);
                signInUserPage.OriginUrl = queryCollection.Get("originurl");

                masterPage.SignInUserPage = signInUserPage;

                if (masterPage.IsSignedIn) {
                    signInUserPage.RedirectUrl = signInUserPage.OriginUrl;
                }

                return signInUserPage;
            }, opt);
        }
    }
}
