using System;
using System.Web;
using System.Collections.Generic;
using System.Collections.Specialized;
using Starcounter;
using Starcounter.Internal;
using Simplified.Ring5;

namespace SignIn
{
    internal class CommitHooks
    {
        public void Register()
        {
            Hook<SystemUserSession>.CommitInsert += (s, a) => { this.RefreshSignInState(); };

            Hook<SystemUserSession>.CommitDelete += (s, a) => { this.RefreshSignInState(); };

            Hook<SystemUserSession>.CommitUpdate += (s, a) => { this.RefreshSignInState(); };
        }

        protected void RefreshSignInState()
        {
            if (Session.Current != null)
            {
                var master = Session.Current.Data as MasterPage;
                master.RefreshSignInState();
            }
        }
    }
}