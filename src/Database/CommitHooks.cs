using SignInApp.Server.Handlers;
using Starcounter;
using Starcounter.Internal;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;

namespace SignInApp.Server.Database {
    public class CommitHooks {

        internal static void RegisterCommitHooks() {

            HandlerOptions opt = new HandlerOptions() { HandlerLevel = 0 };

            // User signed in event
            Starcounter.Handle.POST("/__db/__" + StarcounterEnvironment.DatabaseNameLower + "/societyobjects/systemusersession", (Request request) => {

                JSON.systemusersession systemUserSessionJson = new JSON.systemusersession();
                systemUserSessionJson.PopulateFromJson(request.Body);
                Concepts.Ring5.SystemUserSession userSession = Db.SQL<Concepts.Ring5.SystemUserSession>("SELECT o FROM Concepts.Ring5.SystemUserSession o WHERE o.ObjectID=?", systemUserSessionJson.ObjectID).First;

                if (userSession != null && SignInHandlers.signInSessions.ContainsKey(userSession.SessionIdString)) {

                    SignIn page = SignInHandlers.signInSessions[userSession.SessionIdString];

                    page.SetViewModelProperties(userSession);
                    page.SignInEvent = !page.SignInEvent;

                    SignInUser signInUserPage = page.SignInUserPage as SignInUser;
                    if (signInUserPage != null) {
                        signInUserPage.SetViewModelProperties(userSession);
                        signInUserPage.SignInEvent = !signInUserPage.SignInEvent;
                        signInUserPage.RedirectUrl = signInUserPage.OriginUrl;
                    }
                }

                return (ushort)System.Net.HttpStatusCode.OK;
            }, opt);

            // User signed out event
            Starcounter.Handle.DELETE("/__db/__" + StarcounterEnvironment.DatabaseNameLower + "/societyobjects/systemusersession", (Request request) => {

                JSON.systemusersession systemUserSessionJson = new JSON.systemusersession();
                systemUserSessionJson.PopulateFromJson(request.Body);

                if (SignInHandlers.signInSessions.ContainsKey(systemUserSessionJson.SessionIdString)) {

                    SignIn page = SignInHandlers.signInSessions[systemUserSessionJson.SessionIdString];

                    page.ClearViewModelProperties();
                    page.SignInEvent = !page.SignInEvent;

                    SignInUser signInUserPage = page.SignInUserPage as SignInUser;
                    if (signInUserPage != null) {
                        signInUserPage.ClearViewModelProperties();
                        signInUserPage.SignInEvent = !page.SignInEvent;
                        signInUserPage.RedirectUrl = "/";       // TODO: Where to go when signing out
                    }
                }

                return (ushort)System.Net.HttpStatusCode.OK;
            }, opt);
        }
    }
}
