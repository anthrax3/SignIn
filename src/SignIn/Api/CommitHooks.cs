using Starcounter;
using Simplified.Ring5;

namespace SignIn
{
    internal class CommitHooks
    {
        public void Register()
        {
            Hook<SystemUserSession>.CommitInsert += (s, a) => this.RefreshSignInState();
            Hook<SystemUserSession>.CommitDelete += (s, a) => this.RefreshSignInState();
            Hook<SystemUserSession>.CommitUpdate += (s, a) => this.RefreshSignInState();
        }

        protected void RefreshSignInState()
        {
            if (Session.Current != null)
            {
                var master = Session.Current.Store[nameof(MasterPage)] as MasterPage;
                master?.RefreshSignInState();
            }
        }
    }
}