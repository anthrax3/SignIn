using Starcounter;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;

namespace SignInApp.Server {
    public class Handlers {

        // TODO: How to remove items from this list
        static Dictionary<string, SignIn> signInSessions = new Dictionary<string, SignIn>();

        internal static void RegisterHandlers() {

            HandlerOptions opt = new HandlerOptions() { HandlerLevel = 0 };

            Starcounter.Handle.GET("/user", () => {

                var signInPage = new SignIn() { Html = "/signin.html" };

                string sessionID = Session.Current.SessionIdString;
                if (signInSessions.ContainsKey(sessionID)) {
                    signInSessions.Remove(sessionID);
                }

                signInSessions.Add(sessionID, signInPage);

                return signInPage;
            });

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

            #region Sign in/out commit hook

            // User signed in event
            Starcounter.Handle.POST("/__db/__default/societyobjects/systemusersession", (Request request) => {

                JSON.systemusersession systemUserSessionJson = new JSON.systemusersession();
                systemUserSessionJson.PopulateFromJson(request.Body);
                Concepts.Ring5.SystemUserSession userSession = Db.SQL<Concepts.Ring5.SystemUserSession>("SELECT o FROM Concepts.Ring5.SystemUserSession o WHERE o.ObjectID=?", systemUserSessionJson.ObjectID).First;

                if (userSession != null && signInSessions.ContainsKey(userSession.SessionIdString)) {

                    SignIn page = signInSessions[userSession.SessionIdString];

                    page.SetViewModelProperties(userSession);
                    page.SignInEvent = !page.SignInEvent;

                    SignInUser signInUserPage = page.MyPage as SignInUser;
                    if (signInUserPage != null) {
                        signInUserPage.SetViewModelProperties(userSession);
                        signInUserPage.SignInEvent = !signInUserPage.SignInEvent;
                        signInUserPage.RedirectUrl = signInUserPage.OriginUrl;
                    }
                }

                return (ushort)System.Net.HttpStatusCode.OK;
            }, opt);

            // User signed out event
            Starcounter.Handle.DELETE("/__db/__default/societyobjects/systemusersession", (Request request) => {

                JSON.systemusersession systemUserSessionJson = new JSON.systemusersession();
                systemUserSessionJson.PopulateFromJson(request.Body);
                Concepts.Ring5.SystemUserSession userSession = Db.SQL<Concepts.Ring5.SystemUserSession>("SELECT o FROM Concepts.Ring5.SystemUserSession o WHERE o.ObjectID=?", systemUserSessionJson.ObjectID).First;

                if (userSession != null && signInSessions.ContainsKey(userSession.SessionIdString)) {

                    SignIn page = signInSessions[userSession.SessionIdString];

                    page.ClearViewModelProperties();
                    page.SignInEvent = !page.SignInEvent;

                    SignInUser signInUserPage = page.MyPage as SignInUser;
                    if (signInUserPage != null) {
                        signInUserPage.ClearViewModelProperties();
                        signInUserPage.SignInEvent = !page.SignInEvent;
                        signInUserPage.RedirectUrl = "/";
                    }
                }

                return (ushort)System.Net.HttpStatusCode.OK;
            }, opt);
            #endregion
        }
    }
}
