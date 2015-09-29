using System;
using System.Web;
using System.Collections.Generic;
using System.Collections.Specialized;
using Starcounter;
using Starcounter.Internal;
using Simplified.Ring5;

namespace SignIn {
    internal class CommitHooks {
        public void Register() {
            Hook<SystemUserSession>.CommitInsert += (s, a) => {
                this.RefreshSignInState();
            };

            Hook<SystemUserSession>.CommitDelete += (s, a) => {
                this.RefreshSignInState();
            };

            Hook<SystemUserSession>.CommitUpdate += (s, a) => {
                this.RefreshSignInState();
            };
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
