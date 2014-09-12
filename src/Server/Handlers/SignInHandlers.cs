using Starcounter;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;

namespace SignInApp.Server.Handlers {
    public class SignInHandlers {

        // TODO: How to remove items from this list
        internal static Dictionary<string, SignIn> signInSessions = new Dictionary<string, SignIn>();

        internal static void RegisterHandlers() {

            HandlerOptions opt = new HandlerOptions() { HandlerLevel = 0 };

            Starcounter.Handle.GET("/signinapp/signinuser", (Request request) => {

                string sessionID = Session.Current.SessionIdString;
                if (!signInSessions.ContainsKey(sessionID)) {
                    // TODO
                    return (ushort)System.Net.HttpStatusCode.InternalServerError;
                }
                SignIn masterPage = signInSessions[sessionID];

                var signInUserPage = new SignInUser() { Html = "/signinuser.html" };

                masterPage.MyPage = signInUserPage;

                if (masterPage.IsSignedIn) {
                    signInUserPage.RedirectUrl = signInUserPage.OriginUrl;
                }
                return signInUserPage;
            }, opt);

            Starcounter.Handle.GET("/signinapp/signinuser?{?}", (string query, Request request) => {

                string sessionID = Session.Current.SessionIdString;
                if (!signInSessions.ContainsKey(sessionID)) {
                    // TODO
                    return (ushort)System.Net.HttpStatusCode.InternalServerError;
                }
                SignIn masterPage = signInSessions[sessionID];

                var signInUserPage = new SignInUser() { Html = "/signinuser.html" };

                string decodedQuery = HttpUtility.UrlDecode(query);

                NameValueCollection queryCollection = HttpUtility.ParseQueryString(decodedQuery);
                signInUserPage.OriginUrl = queryCollection.Get("originurl");

                masterPage.MyPage = signInUserPage;

                if (masterPage.IsSignedIn) {
                    signInUserPage.RedirectUrl = signInUserPage.OriginUrl;
                }

                return signInUserPage;

            }, opt);

        }
    }
}
