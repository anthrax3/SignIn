using Starcounter;
using Simplified.Ring3;
using Simplified.Ring5;

namespace SignIn {
    partial class SignInPage : Page {
        void Handle(Input.SignInClick Action) {
            this.Message = null;
            Action.Cancel();

            if (string.IsNullOrEmpty(this.Username)) {
                this.Message = "Username is required!";
                return;
            }

            this.Submit++;
        }

        public void SetAuthorizedState(SystemUserSession Session) {
            this.Message = string.Empty;

            if (Session.Token.User.WhoIs != null) {
                this.FullName = Session.Token.User.WhoIs.FullName;

                if (!string.IsNullOrEmpty(Session.Token.User.WhoIs.ImageURL)) {
                    this.ImageUrl = Session.Token.User.WhoIs.ImageURL;
                } else {
                    this.ImageUrl = Utils.GetGravatarUrl(string.Empty);
                }
            } else {
                this.ImageUrl = Utils.GetGravatarUrl(string.Empty);
            }

            if (string.IsNullOrEmpty(this.FullName)) {
                this.FullName = Session.Token.User.Username;
            }

            this.IsSignedIn = true;
        }

        public void SetAnonymousState() {
            this.Username = string.Empty;
            this.FullName = string.Empty;
            this.Message = Message;
            this.IsSignedIn = false;
        }

        public void RefreshSignInState() {
            SystemUserSession session = SystemUser.GetCurrentSystemUserSession();

            if (session != null) {
                this.SetAuthorizedState(session);
            } else {
                this.SetAnonymousState();
            }
        }
    }
}
