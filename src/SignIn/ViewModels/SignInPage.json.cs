using Starcounter;
using Simplified.Ring3;
using Simplified.Ring5;

namespace SignIn {
    partial class SignInPage : Page {
        public string SignInAuthToken { get; set; }

        public void SignIn(string Username, string Password) {
            if (string.IsNullOrEmpty(Username)) {
                this.SetAnonymousState(false, "Please input your username!");
                return;
            }

            string message;
            SystemUserSession session = SystemUser.SignInSystemUser(Username, Password, null, out message);

            if (session == null) {
                this.SetAnonymousState(true, message);
            } else {
                this.SetAuthorizedState(session);
            }
        }

        public void SignOut() {
            SystemUser.SignOutSystemUser();
            this.SetAnonymousState();
        }

        public void FromCookie(string SignInAuthToken) {
            SystemUserTokenKey token = Db.SQL<SystemUserTokenKey>("SELECT t FROM Simplified.Ring5.SystemUserTokenKey t WHERE t.Token = ?", SignInAuthToken).First;

            if (token == null) {
                return;
            }

            SystemUserSession session = SystemUser.SignInSystemUser(token.Token);

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

        public void RefreshState() {
            SystemUserSession session = SystemUser.GetCurrentSystemUserSession();

            if (session != null) {
                this.SetAuthorizedState(session);
            } else {
                this.SetAnonymousState();
            }
        }

        public void UpdateSignInForm() {
            SessionContainer container = Session.Current.Data as SessionContainer;

            if (container == null) {
                return;
            }

            SignInFormPage page = container.SignInForm;

            if (page == null) {
                return;
            }

            page.IsSignedIn = this.IsSignedIn;
            page.Message = this.Message;

            if (page.IsSignedIn) {
                SystemUser user = SystemUser.GetCurrentSystemUser();

                page.Username = user.Username;
                page.Password = string.Empty;
                page.RedirectUrl = page.OriginUrl;
            } else {
                page.Username = string.Empty;
            }
        }
    }
}
