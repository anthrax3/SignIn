using Starcounter;
using System.Collections.Specialized;
using System.Web;

namespace SignInApp.Server {
    public class Handlers {

        internal static void RegisterHandlers() {

            HandlerOptions opt = new HandlerOptions() { HandlerLevel = 0 };

            Starcounter.Handle.GET("/user", () => {

                var signInPage = new SignIn() {
                    Html = "/signin.html",
                };

                Session.Current.Data = signInPage;

                return signInPage;
            });

            // TODO: maybe alias this (tomalec) 
            // Starcounter.Handle.GET("/signinuser", (Request request) =>
            Starcounter.Handle.GET("/signinapp/signinuser", (Request request) => {

                SignIn masterPage = Session.Current.Data as SignIn;

                var signInUserPage = new SignInUser() {
                    Html = "/signinuser.html",
                };

                masterPage.MyPage = signInUserPage;

                return signInUserPage;
            }, opt);

            // Starcounter.Handle.GET("/signinuser", (Request request) =>
            Starcounter.Handle.GET("/signinapp/signinuser?{?}", (string query, Request request) => {

                SignIn masterPage = Session.Current.Data as SignIn;

                var signInUserPage = new SignInUser() {
                    Html = "/signinuser.html",
                };

                string decodedQuery = HttpUtility.UrlDecode(query);

                NameValueCollection queryCollection = HttpUtility.ParseQueryString(decodedQuery);
                signInUserPage.OriginUrl = queryCollection.Get("originurl");

                masterPage.MyPage = signInUserPage;

                return signInUserPage;

            }, opt);

            #region Sign in/out event

            // User signed in event
            Starcounter.Handle.POST("/__db/__default/societyobjects/systemusersession", (Request request) => {

                Response response = new Response();

                if (Session.Current.Data is SignIn) {
                    SignIn page = (SignIn)Session.Current.Data;
                    JSON.user user = new JSON.user();
                    user.PopulateFromJson(request.Body);


                    Concepts.Ring5.SystemUserSession userSession = Db.SQL<Concepts.Ring5.SystemUserSession>("SELECT o FROM Concepts.Ring5.SystemUserSession o WHERE o.SessionIdString=?", user.sessionid).First;
                    if (userSession != null) {
                        page.SetViewModelProperties(userSession);
                        page.SignInEvent = !page.SignInEvent;

                        SignInUser signInUserPage = page.MyPage as SignInUser;
                        if (signInUserPage != null) {
                            signInUserPage.SetViewModelProperties(userSession);
                            signInUserPage.SignInEvent = !signInUserPage.SignInEvent;
                            signInUserPage.RedirectUrl = signInUserPage.OriginUrl;
                        }
                    }
                }


                response.StatusCode = (ushort)System.Net.HttpStatusCode.OK;
                return response;
            }, opt);

            // User signed out event
            Starcounter.Handle.DELETE("/__db/__default/societyobjects/systemusersession", (Request request) => {

                Response response = new Response();

                if (Session.Current.Data is SignIn) {
                    SignIn page = (SignIn)Session.Current.Data;
                    page.ClearViewModelProperties();
                    page.SignInEvent = !page.SignInEvent;

                    SignInUser signInUserPage = page.MyPage as SignInUser;
                    if (signInUserPage != null) {
                        signInUserPage.ClearViewModelProperties();
                        signInUserPage.SignInEvent = !page.SignInEvent;
                        signInUserPage.RedirectUrl = "/";
                    }
                }

                response.StatusCode = (ushort)System.Net.HttpStatusCode.OK;
                return response;

            }, opt);
            #endregion
        }
    }
}
