using PolyjuiceNamespace;
using Starcounter;
using Starcounter.Internal;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;
using Simplified.Ring5;

namespace SignIn {
    internal class CommitHooks {
        public void Register() {
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
            SessionContainer container = null;

            if (Session.Current != null && Session.Current.Data is SessionContainer) {
                container = Session.Current.Data as SessionContainer;
            }

            return container != null ? container.SignIn : null;
        }
    }
}
