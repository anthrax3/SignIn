using Starcounter;
using Simplified.Ring3;
using Simplified.Ring5;

namespace SignIn
{
    partial class SignInPage : Page
    {
        void Handle(Input.SignInClick Action)
        {
            this.Message = null;
            Action.Cancel();

            this.Submit++;
        }

        public void SetAuthorizedState(SystemUserSession Session)
        {
            this.Message = string.Empty;

            if (Session.Token.User.WhoIs != null)
            {
                this.FullName = Session.Token.User.WhoIs.FullName;
                this.ImageUrl = Session.Token.User.WhoIs.Illustration?.Content?.URL;
            }

            if (string.IsNullOrEmpty(this.ImageUrl))
            {
                this.ImageUrl = Utils.GetGravatarUrl(string.Empty);
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