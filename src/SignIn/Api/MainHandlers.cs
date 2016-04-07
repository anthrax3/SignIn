using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using Starcounter;
using Simplified.Ring3;
using Simplified.Ring5;

namespace SignIn {
    internal class MainHandlers {
        protected string AuthCookieName = "soauthtoken";

        public void Register() {
            //Testing JWT
            /*Handle.GET("/signin/jwt/{?}/{?}", (string Username, string Password) => {
                string message;
                SystemUserSession session = SignInOut.SignInSystemUser(Username, Password, null, out message);

                if (session != null) {
                    string jwt = JWT.JsonWebToken.Encode(new { Username = Username, Issuer = "Polyjuice.SignIn" }, session.Token.User.Password, JWT.JwtHashAlgorithm.HS256);
                    Handle.AddOutgoingHeader("x-jwt", jwt);
                }

                return 200;
            });*/

            Application.Current.Use((Request req) => {
                Cookie cookie = GetSignInCookie();

                if (cookie != null) {
                    Session session = Session.Current;
                    if (session == null) {
                        session = new Session(SessionOptions.PatchVersioning);
                    }
                    Session.Current = session;
                    SystemUser.SignInSystemUser(cookie.Value);
                }

                return null;
            });

            Handle.GET("/signin/user", HandleUser);
            Handle.GET<string, string>("/signin/partial/signin/{?}/{?}", HandleSignIn, new HandlerOptions() { SkipRequestFilters = true });
            Handle.GET("/signin/partial/signin/", HandleSignIn, new HandlerOptions() { SkipRequestFilters = true });
            Handle.GET("/signin/partial/signin", HandleSignIn, new HandlerOptions() { SkipRequestFilters = true });
            Handle.GET("/signin/partial/signout", HandleSignOut, new HandlerOptions() { SkipRequestFilters = true });
            Handle.GET("/signin/signinuser", HandleSignInUser);
            Handle.GET<string>("/signin/signinuser?{?}", HandleSignIn);

            //Test handler
            /*Handle.GET("/signin/deleteadminuser", () => {
                Db.Transact(() => {
                    Db.SlowSQL("DELETE FROM Simplified.Ring3.SystemUserGroupMember WHERE SystemUser.Username = ?", SignInOut.AdminUsername);
                    Db.SlowSQL("DELETE FROM Simplified.Ring3.SystemUser WHERE Username = ?", SignInOut.AdminUsername);
                });

                return 200;
            });*/

            UriMapping.Map("/signin/user", "/sc/mapping/user"); //expandable icon; used in Launcher
			UriMapping.Map("/signin/signinuser", "/sc/mapping/userform"); //inline form; used in RSE Launcher
        }

        protected void SetAuthCookie(SignInPage Page) {
            Cookie cookie = new Cookie(AuthCookieName, Page.SignInAuthToken);

            Handle.AddOutgoingCookie(cookie.Name, cookie.GetFullValueString());
        }

        protected Response GetNoSessionResult() {
            return new Response() {
                StatusCode = (ushort)System.Net.HttpStatusCode.InternalServerError,
                Body = "No Current Session"
            };
        }

        protected SessionContainer GetSessionContainer() {
            Session session = Session.Current;

            if (session != null && session.Data != null) {
                return session.Data as SessionContainer;
            }

            SessionContainer container = new SessionContainer();

            if (session == null) {
                session = new Session(SessionOptions.PatchVersioning);
            }

            container.Session = session;
            return container;
        }

        protected Response HandleSignIn() {
            return HandleSignIn(null, null);
        }

        protected Response HandleSignIn(string Username, string Password) {
            SessionContainer container = this.GetSessionContainer();

            var page = container.SignIn as SignInPage;
            if (page == null) {
                //as long as Registration calls /signin/partial/signin/{?}/{?} directly
                //this handler needs to make sure SignIn page is created before it proceeds
                page = Self.GET<Page>("/signin/user") as SignInPage;
            }

            page.SignIn(Username, Password);
            SetAuthCookie(page);

            return container;
        }

        protected Response HandleSignIn(string Query) {
            SignInPage master = Self.GET<Page>("/signin/user") as SignInPage;
            SignInFormPage page = new SignInFormPage();
            string decodedQuery = HttpUtility.UrlDecode(Query);
            NameValueCollection queryCollection = HttpUtility.ParseQueryString(decodedQuery);
            SessionContainer container = this.GetSessionContainer();

            page.OriginUrl = queryCollection.Get("originurl");
            container.SignInForm = page;
            master.UpdateSignInForm();

            return page;
        }

        protected Response HandleSignInUser() {
            SignInPage master = Self.GET<Page>("/signin/user") as SignInPage;
            SignInFormPage page = new SignInFormPage();
            SessionContainer container = this.GetSessionContainer();

            container.SignInForm = page;
            master.UpdateSignInForm();

            return page;
        }

        protected Response HandleSignOut() {
            SessionContainer container = this.GetSessionContainer();

            if (container.SignIn == null) {
                throw new SignInException("Trying to use SignIn app before if was initialized");
            }

            container.SignIn.SignOut();
            SetAuthCookie(container.SignIn);

            return container;
        }

        protected Response HandleUser() {
            if (Session.Current == null) {
                return null;
            }

            SessionContainer container = this.GetSessionContainer();

            if (container.SignIn != null) {
                return container.SignIn;
            }

            Cookie cookie = GetSignInCookie();
            SignInPage page = new SignInPage();

            if (cookie != null) {
                page.FromCookie(cookie.Value);
            } else {
                page.SetAnonymousState();
            }

            container.SignIn = page;

            //Testing JWT
            /*if (Handle.IncomingRequest.HeadersDictionary.ContainsKey("x-jwt")) {
                System.Web.Script.Serialization.JavaScriptSerializer serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                string jwt = Handle.IncomingRequest.HeadersDictionary["x-jwt"];
                Dictionary<string, string> payload = JWT.JsonWebToken.DecodeToObject<Dictionary<string, string>>(jwt, string.Empty, false);
                string username = payload["Username"];
                SystemUser user = Db.SQL<SystemUser>("SELECT su FROM Simplified.Ring3.SystemUser su WHERE su.Username = ?", username).First;

                try {
                    JWT.JsonWebToken.DecodeToObject<Dictionary<string, string>>(jwt, user.Password, true);
                    page.SetAuthorizedState(SignInOut.SignInSystemUser(user));
                } catch (JWT.SignatureVerificationException) { 
                }
            }*/

            return page;
        }

        protected Cookie GetSignInCookie() {
            List<Cookie> cookies = Handle.IncomingRequest.Cookies.Select(x => new Cookie(x)).ToList();
            Cookie cookie = cookies.FirstOrDefault(x => x.Name == AuthCookieName);

            return cookie;
        }
    }
}
