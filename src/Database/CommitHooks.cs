using PolyjuiceNamespace;
using SignInApp.Server.Handlers;
using Starcounter;
using Starcounter.Internal;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;

namespace SignInApp.Server.Database {
    public class CommitHooks {

        internal static string LocalAppUrl;
        internal static string MappedTo;


        internal static void RegisterCommitHooks() {

            CommitHooks.LocalAppUrl = "/SignInApp/__db/__" + StarcounterEnvironment.DatabaseNameLower + "/societyobjects/systemusersession";
            CommitHooks.MappedTo = "/polyjuice/signin";


            //Starcounter.Handle.GET(CommitHooks.LocalAppUrl, (Request request) => {
            //    return (ushort)System.Net.HttpStatusCode.OK;
            //});


            // User signed in event
            Starcounter.Handle.POST(CommitHooks.LocalAppUrl, (Request request) => {

                JSON.systemusersession systemUserSessionJson = new JSON.systemusersession();
                systemUserSessionJson.PopulateFromJson(request.Body);
                Concepts.Ring8.Polyjuice.SystemUserSession userSession = Db.SQL<Concepts.Ring8.Polyjuice.SystemUserSession>("SELECT o FROM Concepts.Ring8.Polyjuice.SystemUserSession o WHERE o.ObjectID=?", systemUserSessionJson.ObjectID).First;

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
            });

            // User signed out event
            Starcounter.Handle.DELETE(CommitHooks.LocalAppUrl, (Request request) => {

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
            });

            Polyjuice.Map(CommitHooks.LocalAppUrl, CommitHooks.MappedTo, "POST");
            Polyjuice.Map(CommitHooks.LocalAppUrl, CommitHooks.MappedTo, "DELETE");

        }
    }
}
