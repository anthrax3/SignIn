using Starcounter;
using Simplified.Ring3;
using Simplified.Ring5;

namespace SignIn
{
    partial class SignInPage : Page
    {
        protected override void OnData()
        {
            base.OnData();
            this.SessionUri = Session.Current.SessionUri;
        }

        void Handle(Input.SignInClick action)
        {
            this.Message = null;
            action.Cancel();

            this.Submit++;
        }

        public void SetAuthorizedState(SystemUserSession Session)
        {
            this.Message = string.Empty;

            if (Session.Token.User.WhoIs != null)
            {
                this.FullName = Session.Token.User.WhoIs.FullName;
                this.UserImage = Self.GET<Json>("/signin/partials/user/image/" + Session.Token.User.WhoIs.GetObjectID(), () => new UserImagePage());
            }

            if (string.IsNullOrEmpty(this.FullName))
            {
                this.FullName = Session.Token.User.Username;
            }

            this.IsSignedIn = true;
        }

        public void SetAnonymousState()
        {
            this.FullName = string.Empty;
            this.Message = Message;
            this.IsSignedIn = false;
        }

        public void RefreshSignInState()
        {
            SystemUserSession session = SystemUser.GetCurrentSystemUserSession();

            if (session != null)
            {
                this.SetAuthorizedState(session);
            }
            else
            {
                this.SetAnonymousState();
            }
        }
    }
}