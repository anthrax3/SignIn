using PolyjuiceNamespace;
using Starcounter;
using Starcounter.Internal;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;
using Simplified.Ring5;

namespace SignIn {
    internal class CommitHooks {
        protected string appNameOnStart = "SignIn";

        public void Register() {
            appNameOnStart = StarcounterEnvironment.AppName;

            Hook<SystemUserSession>.OnInsert(s => {
                this.RefreshSignInState();
            });

            Hook<SystemUserSession>.OnDelete(s => {
                this.RefreshSignInState();
            });

            Hook<SystemUserSession>.OnUpdate(s => {
                this.RefreshSignInState();
            });
        }

        protected void RefreshSignInState() {
            SignInPage page = GetSignInPage();

            if (page != null) {
                page.RefreshState();
            }
        }

        protected SignInPage GetSignInPage() {
            string appName = StarcounterEnvironment.AppName;
            SessionContainer container = null;

            StarcounterEnvironment.AppName = this.appNameOnStart;

            if (Session.Current != null && Session.Current.Data is SessionContainer) {
                container = Session.Current.Data as SessionContainer;
            }

            StarcounterEnvironment.AppName = appName;

            return container != null ? container.SignIn : null;
        }
    }
}
