using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using Starcounter;
using PolyjuiceNamespace;
using Simplified.Ring3;
using Simplified.Ring5;

namespace SignIn {
    internal class MainHandlers {
        protected string AuthCookieName = "soauthtoken";

        public void Register() {
            Starcounter.Handle.GET("/signin/user", () => {
                SessionContainer container = this.GetSessionContainer();
                //Session session = Session.Current;

                if (container.SignIn != null) {
                    return container.SignIn;
                }

                List<Cookie> cookies = Handle.IncomingRequest.Cookies.Select(x => new Cookie(x)).ToList();
                Cookie cookie = cookies.FirstOrDefault(x => x.Name == AuthCookieName);
                SignInPage page = new SignInPage();

                if (cookie != null) {
                    page.FromCookie(cookie.Value);
                } else {
                    page.SetAnonymousState();
                }

                //page.Session = session;
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
            });

            Handle.GET("/signin/signin/{?}/{?}", (string Username, string Password) => {
                SessionContainer container = this.GetSessionContainer();
                //SignInPage page = Session.Current.Data as SignInPage;

                //page.SignIn(Username, Password);
                container.SignIn.SignIn(Username, Password);
                SetAuthCookie(container.SignIn);

                //return page.SignInForm;
                return container.SignInForm;
            });

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

            Handle.GET("/signin/signout", () => {
                SessionContainer container = this.GetSessionContainer();
                //SignInPage page = Session.Current.Data as SignInPage;

                //page.SignOut();
                container.SignIn.SignOut();
                SetAuthCookie(container.SignIn);

                //return page.SignInForm;
                return container.SignInForm;
            });

            Handle.GET("/signin/signinuser", () => {
                SignInPage master = Self.GET<Page>("/signin/user") as SignInPage;
                SignInFormPage page = new SignInFormPage();
                SessionContainer container = this.GetSessionContainer();

                container.SignInForm = page;
                master.UpdateSignInForm();

                return page;
            });

            Handle.GET("/signin/signinuser?{?}", (string query) => {
                SignInPage master = Self.GET<Page>("/signin/user") as SignInPage;
                SignInFormPage page = new SignInFormPage();
                string decodedQuery = HttpUtility.UrlDecode(query);
                NameValueCollection queryCollection = HttpUtility.ParseQueryString(decodedQuery);
                SessionContainer container = this.GetSessionContainer();

                page.OriginUrl = queryCollection.Get("originurl");
                container.SignInForm = page;
                master.UpdateSignInForm();

                return page;
            });

            //Test handler
            Handle.GET("/signin/deleteadminuser", () => {
                Db.Transact(() => {
                    Db.SlowSQL("DELETE FROM Simplified.Ring3.SystemUserGroupMember WHERE SystemUser.Username = ?", SignInOut.AdminUsername);
                    Db.SlowSQL("DELETE FROM Simplified.Ring3.SystemUser WHERE Username = ?", SignInOut.AdminUsername);
                });

                return 200;
            });

            Polyjuice.Map("/signin/user", "/polyjuice/user");
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
            SessionContainer container = Session.Current.Data as SessionContainer;

            if (container == null && Session.Current.Data != null) {
                throw new Exception("Invalid object in session!");
            }

            if (container == null) {
                container = new SessionContainer();
                Session.Current.Data = container;
            }

            return container;
        }
    }
}
