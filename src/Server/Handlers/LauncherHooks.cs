using SignInApp.Server.Handlers;
using Starcounter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignInApp.Server {
    public class LauncherHooks {

        /// <summary>
        /// Register Polyjuice Launcher Hooks
        /// </summary>
        public static void RegisterLauncherHooks() {

            Starcounter.Handle.GET("/user", () => {

                var signInPage = new SignIn() { Html = "/signin.html" };

                string sessionID = Session.Current.SessionIdString;
                if (SignInHandlers.signInSessions.ContainsKey(sessionID)) {
                    SignInHandlers.signInSessions.Remove(sessionID);
                }

                SignInHandlers.signInSessions.Add(sessionID, signInPage);

                return signInPage;
            });
        }
    }
}
