using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using Starcounter;
using Simplified.Ring3;
using Simplified.Ring5;

namespace SignIn
{
    internal class MainHandlers
    {
        protected string AuthCookieName = "soauthtoken";
        protected int rememberMeDays = 30;

        public void Register()
        {
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

            Handle.GET("/signin/user", () =>
            {
                SessionContainer container = this.GetSessionContainer();

                if (container.SignIn != null)
                {
                    return container.SignIn;
                }

                Cookie cookie = GetSignInCookie();
                SignInPage page = new SignInPage();

                container.SignIn = page;

                if (cookie != null)
                {
                    SystemUser.SignInSystemUser(cookie.Value);
                    this.RefreshSignInState();
                }

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

            Handle.GET<string, string, string>("/signin/partial/signin/{?}/{?}/{?}", HandleSignIn,
                new HandlerOptions() {SkipRequestFilters = true});
            Handle.GET("/signin/partial/signin/", HandleSignIn, new HandlerOptions() {SkipRequestFilters = true});
            Handle.GET("/signin/partial/signin", HandleSignIn, new HandlerOptions() {SkipRequestFilters = true});
            Handle.GET("/signin/partial/signout", HandleSignOut, new HandlerOptions() {SkipRequestFilters = true});

            Handle.GET("/signin/signinuser", HandleSignInForm);
            Handle.GET<string>("/signin/signinuser?{?}", HandleSignInForm);

            Handle.GET("/signin/profile", () =>
            {
                MasterPage master = this.GetMaster();

                master.RequireSignIn = true;
                master.Open("/signin/partial/profile-form");

                return master;
            });

            Handle.GET("/signin/partial/signin-form", () => new SignInFormPage(), new HandlerOptions() {SelfOnly = true});
            Handle.GET("/signin/partial/alreadyin-form", () => new AlreadyInPage() {Data = null},
                new HandlerOptions() {SelfOnly = true});
            Handle.GET("/signin/partial/restore-form", () => new RestorePasswordFormPage(),
                new HandlerOptions() {SelfOnly = true});
            Handle.GET("/signin/partial/profile-form", () => new ProfileFormPage() {Data = null},
                new HandlerOptions() {SelfOnly = true});
            Handle.GET("/signin/partial/accessdenied-form", () => new AccessDeniedPage(),
                new HandlerOptions() {SelfOnly = true});

            Handle.GET("/signin/partial/main-form", () => new MainFormPage() {Data = null},
                new HandlerOptions() {SelfOnly = true});

            Handle.GET("/signin/generateadminuser", () =>
            {
                if (Db.SQL("SELECT o FROM Simplified.Ring3.SystemUser o").First != null)
                {
                    Handle.SetOutgoingStatusCode(403);
                    return "Unable to generate admin user: database is not empty!";
                }

                SignInOut.AssureAdminSystemUser();

                return "Default admin user has been successfully generated.";
            }, new HandlerOptions() {SkipRequestFilters = true});

            UriMapping.Map("/signin/user", "/sc/mapping/user"); //expandable icon; used in Launcher
            UriMapping.Map("/signin/signinuser", "/sc/mapping/userform"); //inline form; used in RSE Launcher
        }

        protected void ClearAuthCookie()
        {
            this.SetAuthCookie("", false);
        }

        protected void RefreshAuthCookie(SystemUserSession Session)
        {
            Cookie cookie = GetSignInCookie();

            if (cookie == null)
            {
                return;
            }

            Db.Transact(() => { Session.Token.Token = SystemUser.CreateAuthToken(Session.Token.User.Username); });

            cookie.Value = Session.Token.Token;

            //TODO: refreshing cookie removes the Expires date
            //store the Expires date in Simplified and use it here
            Handle.AddOutgoingCookie(cookie.Name, cookie.GetFullValueString());
        }

        protected void SetAuthCookie(string token, bool RememberMe)
        {
            Cookie cookie = new Cookie()
            {
                Name = AuthCookieName,
                Value = token
            };

            if (token == "")
            {
                //to delete a cookie, explicitly use a date in the past
                cookie.Expires = DateTime.Now.AddDays(-1).ToUniversalTime();
            }
            else if (RememberMe)
            {
                //cookie with expiration date is persistent until that date
                //cookie without expiration date expires when the browser is closed
                cookie.Expires = DateTime.Now.AddDays(rememberMeDays).ToUniversalTime();
            }

            Handle.AddOutgoingCookie(cookie.Name, cookie.GetFullValueString());
        }

        protected SessionContainer GetSessionContainer()
        {
            Session session = Session.Current;

            if (session != null && session.Data != null)
            {
                return session.Data as SessionContainer;
            }

            SessionContainer container = new SessionContainer();

            if (session == null)
            {
                session = new Session(SessionOptions.PatchVersioning);
            }

            container.Session = session;
            return container;
        }

        protected MasterPage GetMaster()
        {
            SessionContainer container = this.GetSessionContainer();

            if (container.Master == null)
            {
                container.Master = new MasterPage();
            }

            return container.Master;
        }

        protected void RefreshSignInState()
        {
            SessionContainer container = this.GetSessionContainer();

            container.RefreshSignInState();
        }

        protected Response HandleSignIn()
        {
            return HandleSignIn(null, null, null);
        }

        protected Response HandleSignIn(string Username, string Password, string RememberMe)
        {
            Username = Uri.UnescapeDataString(Username);

            SystemUserSession session = SystemUser.SignInSystemUser(Username, Password);

            if (session == null)
            {
                SessionContainer container = GetSessionContainer();
                MasterPage master = container.Master;
                string message = "Invalid username or password!";

                if (container.SignIn != null)
                {
                    container.SignIn.Message = message;
                }

                if (master != null && master.Partial is SignInFormPage)
                {
                    SignInFormPage page = master.Partial as SignInFormPage;
                    page.Message = message;
                }
            }

            SetAuthCookie(session.Token.Token, RememberMe == "true");

            return this.GetSessionContainer();
        }

        protected Response HandleSignInForm()
        {
            return this.HandleSignInForm(string.Empty);
        }

        protected Response HandleSignInForm(string OriginalUrl)
        {
            MasterPage master = this.GetMaster();

            master.RequireSignIn = false;
            master.OriginalUrl = HttpUtility.UrlDecode(OriginalUrl);
            master.Open("/signin/partial/main-form");

            return master;
        }

        protected Response HandleSignOut()
        {
            SystemUser.SignOutSystemUser();
            ClearAuthCookie();

            return this.GetSessionContainer();
        }

        protected Cookie GetSignInCookie()
        {
            List<Cookie> cookies = Handle.IncomingRequest.Cookies.Select(x => new Cookie(x)).ToList();
            Cookie cookie = cookies.FirstOrDefault(x => x.Name == AuthCookieName);

            return cookie;
        }
    }
}