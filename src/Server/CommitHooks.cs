using PolyjuiceNamespace;
using Starcounter;
using Starcounter.Internal;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;
using Simplified.Ring5;

namespace SignIn {
    public class CommitHooks {
        internal static string LocalAppUrl = "/SignIn/__db/__" + StarcounterEnvironment.DatabaseNameLower + "/societyobjects/systemusersession";
        internal static string MappedTo = "/polyjuice/signin";

        internal static void Register() {
            // User signed in event
            Starcounter.Handle.POST(CommitHooks.LocalAppUrl, (Request request) => {
                string sessionId = request.Body;
                SystemUserSession userSession = Db.SQL<SystemUserSession>("SELECT o FROM Simplified.Ring5.SystemUserSession o WHERE o.SessionIdString = ?", sessionId).First;
                SignInPage page = GetSignInPage();

                if (userSession != null && page != null) {
                    page.SetAuthorizedState(userSession);
                }

                return (ushort)System.Net.HttpStatusCode.OK;
            });

            // User signed out event
            Starcounter.Handle.DELETE(CommitHooks.LocalAppUrl, () => {
                SignInPage page = GetSignInPage();

                if (page != null) {
                    page.SetAnonymousState();
                }

                return (ushort)System.Net.HttpStatusCode.OK;
            });

            Polyjuice.Map(CommitHooks.LocalAppUrl, CommitHooks.MappedTo, "POST");
            Polyjuice.Map(CommitHooks.LocalAppUrl, CommitHooks.MappedTo, "DELETE");
        }

        private static SignInPage GetSignInPage() {
            if (Session.Current != null && Session.Current.Data is SignInPage) {
                return Session.Current.Data as SignInPage;
            }

            return null;
        }
    }
}
