using Concepts.Ring5;
using SignInApp.Server.Handlers;
using Starcounter;

namespace SignInApp.Server {

    [SignIn_json]
    partial class SignIn : Json {

        public object SignInUserPage;

        static void Main() {

            // Register handlers
            SignInHandlers.Register();
            Database.CommitHooks.RegisterCommitHooks();
            LauncherHooks.RegisterLauncherHooks();
        }

        #region Handler
        /// <summary>
        /// Sign-In handler
        /// </summary>
        /// <param name="action"></param>
        void Handle(Input.SignIn action) {

            string message;
            SystemUserSession userSession = SignInOut.SignInSystemUser(this.UserID, this.Password, this.SignInAuthToken, out message);
            if (userSession == null) {
                if (!string.IsNullOrEmpty(message)) {
                    this.ClearViewModelProperties(true);
                    this.Message = message;
                }
                else {
                    this.ClearViewModelProperties(true);
                }
                this.SignInEvent = !this.SignInEvent;
            }
        }

        /// <summary>
        /// Sign-Out handler
        /// </summary>
        /// <param name="action"></param>
        void Handle(Input.SignOut action) {

            bool bUserWasSignedOut = SignInOut.SignOutSystemUser(this.AuthToken);

            // Note: if user was signed out there will be a callback (commithooks) that clears the user properties.
            if (bUserWasSignedOut == false) {
                this.ClearViewModelProperties();
                this.SignInEvent = !this.SignInEvent;
            }
        }

        #endregion

        #region Update View Model

        /// <summary>
        /// Set properties
        /// </summary>
        /// <param name="userSession"></param>
        internal void SetViewModelProperties(SystemUserSession userSession) {

            this.Message = string.Empty;

            if (userSession.Token.User.WhoIs != null) {
                this.FullName = userSession.Token.User.WhoIs.FullName;
                if (!string.IsNullOrEmpty(userSession.Token.User.WhoIs.ImageURL)) {
                    this.ImageUrl = userSession.Token.User.WhoIs.ImageURL;
                }
                else {
                    this.ImageUrl = Utils.GetGravatarUrl(string.Empty);
                }
            }
            else {
                this.FullName = userSession.Token.User.Username;
                this.ImageUrl = Utils.GetGravatarUrl(string.Empty);
            }

            this.AuthToken = userSession.Token.Token;
            this.IsSignedIn = true;
        }

        /// <summary>
        /// Clear properties
        /// </summary>
        internal void ClearViewModelProperties() {

            this.ClearViewModelProperties(false);
        }

        /// <summary>
        /// Clear properties
        /// </summary>
        /// <param name="keepUserIDAndPassword"></param>
        void ClearViewModelProperties(bool keepUserIDAndPassword) {

            if (!keepUserIDAndPassword) {
                this.UserID = string.Empty;
                this.Password = string.Empty;
            }
            this.AuthToken = string.Empty;
            this.FullName = string.Empty;
            this.Message = string.Empty;
            this.IsSignedIn = false;
        }

        #endregion

        #region Base
        /// <summary>
        /// The way to get a URL for HTML partial if any.
        /// </summary>
        /// <returns></returns>
        public override string GetHtmlPartialUrl() {
            return Html;
        }

        #endregion
    }
}
