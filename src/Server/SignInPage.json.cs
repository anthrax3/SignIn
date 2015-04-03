using Concepts.Ring8.Polyjuice;
using Simplified.Ring5;
using Starcounter;

namespace SignInApp {
    partial class SignInPage : Page {
        public string SignInAuthToken { get; set; }

        public void SignIn(string Username, string Password) {
            string message;
            SystemUserSession session = SignInOut.SignInSystemUser(Username, Password, null, out message);

            if (session == null) {
                this.SetAnonymousState(true, message);
            } else {
                this.SetAuthorizedState(session);
            }
        }

        public void SignOut() {
            SignInOut.SignOutSystemUser();
            this.SetAnonymousState();
        }

        public void FromCookie(string SignInAuthToken) {
            SystemUserTokenKey token = Db.SQL<SystemUserTokenKey>("SELECT t FROM Simplified.Ring5.SystemUserTokenKey t WHERE t.Token = ?", SignInAuthToken).First;

            if (token == null) {
                return;
            }

            SystemUserSession session = SignInOut.SignInSystemUser(token.Token);

            if (session != null) {
                this.SetAuthorizedState(session);
            }
        }

        public void SetAuthorizedState(SystemUserSession Session) {
            this.Message = string.Empty;

            if (Session.Token.User.WhoIs != null) {
                this.FullName = Session.Token.User.WhoIs.FullName;

                if (!string.IsNullOrEmpty(Session.Token.User.WhoIs.ImageURL)) {
                    this.ImageUrl = Session.Token.User.WhoIs.ImageURL;
                }
                else {
                    this.ImageUrl = Utils.GetGravatarUrl(string.Empty);
                }
            }
            else {
                this.FullName = Session.Token.User.Username;
                this.ImageUrl = Utils.GetGravatarUrl(string.Empty);
            }

            this.SignInAuthToken = Session.Token.Token;
            this.IsSignedIn = true;

            this.UpdateSignInForm();
        }

        public void SetAnonymousState() {
            this.SetAnonymousState(false);
        }

        public void SetAnonymousState(bool KeepUsernameAndPassword) {
            this.SetAnonymousState(KeepUsernameAndPassword, string.Empty);
        }

        public void SetAnonymousState(bool KeepUsernameAndPassword, string Message) {
            if (!KeepUsernameAndPassword) {
                this.Username = string.Empty;
                this.Password = string.Empty;
            }

            this.SignInAuthToken = string.Empty;
            this.FullName = string.Empty;
            this.Message = Message;
            this.IsSignedIn = false;

            this.UpdateSignInForm();
        }

        public void UpdateSignInForm() {
            SignInFormPage page = this.SignInForm as SignInFormPage;

            if (page == null) {
                return;
            }

            page.IsSignedIn = this.IsSignedIn;
            page.Message = this.Message;

            if (page.IsSignedIn) {
                page.Username = string.Empty;
                page.Password = string.Empty;
                page.RedirectUrl = page.OriginUrl;
            }
        }
    }
}
