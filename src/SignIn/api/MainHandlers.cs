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
                SignInPage page = Session.Current.Data as SignInPage;

                page.SignIn(Username, Password);
                SetAuthCookie(page);

                return page.SignInForm;
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
                SignInPage page = Session.Current.Data as SignInPage;

                page.SignOut();
                SetAuthCookie(page);

                return page.SignInForm;
            });

            Handle.GET("/signin/signinuser", () => {
                SignInPage master = Self.GET<Page>("/signin/user") as SignInPage;
                SignInFormPage page = new SignInFormPage();

                master.SignInForm = page;
                master.UpdateSignInForm();

                return page;
            });

            Handle.GET("/signin/signinuser?{?}", (string query) => {
                SignInPage master = Self.GET<Page>("/signin/user") as SignInPage;
                SignInFormPage page = new SignInFormPage();
                string decodedQuery = HttpUtility.UrlDecode(query);
                NameValueCollection queryCollection = HttpUtility.ParseQueryString(decodedQuery);

                page.OriginUrl = queryCollection.Get("originurl");
                master.SignInForm = page;
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
    }
}
