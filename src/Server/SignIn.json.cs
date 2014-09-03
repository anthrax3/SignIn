using Concepts.Ring1;
using Concepts.Ring3;
using SignInApp.Database;
using Starcounter;
using System;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Web;

namespace SignInApp.Server {

    [SignIn_json]
    partial class SignIn : Json {

        static void Main() {

            // Add some sample data
            SampleData.Init();

            // Register handlers
            Handlers.RegisterHandlers();
        }

        #region Handler

        /// <summary>
        /// Sign-In handler
        /// </summary>
        /// <param name="action"></param>
        void Handle(Input.SignIn action) {

            this.SignInUser();

            // Trigger event on client
            this.SignInEvent = !this.SignInEvent;
        }

        /// <summary>
        /// Sign-Out handler
        /// </summary>
        /// <param name="action"></param>
        void Handle(Input.SignOut action) {

            this.SignOutUser();

            // Trigger event on client
            this.SignInEvent = !this.SignInEvent;
        }

        #endregion

        #region Commit Hook replacement

        /// <summary>
        /// Temporary code until starcounter implements commit hooks
        /// </summary>
        /// <param name="user"></param>
        void InvokeSignInCommitHook(JSON.user user) {

            X.POST("/__db/__default/societyobjects/systemusersession", user.ToJsonUtf8(), null);
        }

        /// <summary>
        /// Temporary code until starcounter implements commit hooks
        /// </summary>
        /// <param name="user"></param>
        void InvokeSignOutCommitHook(JSON.user user) {

            X.DELETE("/__db/__default/societyobjects/systemusersession", user.ToJsonUtf8(), null);
        }
        #endregion

        /// <summary>
        /// Sign-in user
        /// </summary>
        public void SignInUser() {

            // If there is an user id then use it.
            if (!string.IsNullOrEmpty(this.UserID)) {
                SignInUser(this.UserID, this.Password);
                return;
            }

            // Use Auth token cookie if it exist
            if (this.SignInAuthToken != null) {
                SignInUser(this.SignInAuthToken);
            }
        }

        /// <summary>
        /// Sign-in user
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="password"></param>
        private void SignInUser(string userId, string password) {

            // Verify username and password
            SystemUser systemUser = Db.SQL<SystemUser>("SELECT o FROM Concepts.Ring3.SystemUser o WHERE o.Username=? AND o.Password=?", userId, password).First;
            if (systemUser == null) {

                // Invalid username or password
                this.ClearViewModelProperties(true);
                this.Message = "Invalid username or password";
                return;
            }

            // Username and password OK

            Db.Transaction(() => {

                // Create system user token
                SystemUserTokenKey token = new SystemUserTokenKey(systemUser);
                this.SignInUserBase(token);
            });
        }

        /// <summary>
        /// Sign-in user
        /// </summary>
        /// <param name="authToken"></param>
        private void SignInUser(string authToken) {

            SystemUserTokenKey token = Db.SQL<SignInApp.Database.SystemUserTokenKey>("SELECT o FROM SignInApp.Database.SystemUserTokenKey o WHERE o.Token=?", authToken).First;
            if (token == null) {
                // signed-out, Invalid or expired token key
                this.ClearViewModelProperties();
                return;
            }

            if (token.User == null) {
                // System user deleted => remove token
                Db.Transaction(() => {
                    token.Delete();
                });

                this.ClearViewModelProperties();
                return;
            }

            this.SignInUserBase(token);
        }

        /// <summary>
        /// Sign-in user
        /// Create system user session
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private void SignInUserBase(SystemUserTokenKey token) {

            // Create system user session
            SystemUserSession userSession = this.CreateSystemUserSession(token);

            // Set view model properties
            this.SetViewModelProperties(userSession);

            // Simulate Commit-Hook handling
            JSON.user user = Utils.SignedInUserToJson(userSession);
            this.InvokeSignInCommitHook(user);
        }

        /// <summary>
        /// Create system user session
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private SystemUserSession CreateSystemUserSession(SystemUserTokenKey token) {

            SystemUserSession userSession = null;

            Db.Transaction(() => {
                // TODO: Check for duplicated sessions
                userSession = new SystemUserSession();
                userSession.SessionIdString = Session.Current.SessionIdString;
                userSession.Token = token;
                userSession.User = token.User;
                userSession.IP = "127.0.0.0"; // TODO
                userSession.Touched = userSession.Created = DateTime.UtcNow;
            });

            return userSession;
        }

        /// <summary>
        /// Sign out user on all sessions that uses the same auth token
        /// </summary>
        public void SignOutUser() {

            string authToken = this.AuthToken;

            this.ClearViewModelProperties();

            if (authToken == null) {
                // TODO: A signed in user always has a token.. what happend here?
                return;
            }

            SystemUserTokenKey token = Db.SQL<SignInApp.Database.SystemUserTokenKey>("SELECT o FROM SignInApp.Database.SystemUserTokenKey o WHERE o.Token=?", authToken).First;
            if (token == null) {
                return;
            }

            JSON.user userJson = null;

            Db.Transaction(() => {

                var result = Db.SQL<SystemUserSession>("SELECT o FROM SignInApp.Database.SystemUserSession o WHERE o.Token=?", token);

                if (result.First != null) {
                    userJson = Utils.SignedInUserToJson(result.First);
                }

                // Sign-out user with a specified auth token in all sessions
                foreach (SystemUserSession userSession in result) {
                    userSession.Delete();
                }

                // Remove system user token
                token.Delete();
            });

            if (userJson != null) {
                InvokeSignOutCommitHook(userJson);
            }
        }

        /// <summary>
        /// Clear properties
        /// </summary>
        void ClearViewModelProperties() {

            this.ClearViewModelProperties(false);
        }

        /// <summary>
        /// Set properties
        /// </summary>
        /// <param name="userSession"></param>
        void SetViewModelProperties(SystemUserSession userSession) {

            this.Message = string.Empty;

            if (userSession.User.WhoIs != null) {
                this.FullName = userSession.User.WhoIs.FullName;
            }
            else {
                this.FullName = userSession.User.Username;
            }

            //this.FullName = signedInUserSession.User.Somebody.FullName;
            this.AuthToken = userSession.Token.Token;
            this.IsSignedIn = true;
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

        #region Base
        // Browsers will ask for "text/html" and we will give it to them
        // by loading the contents of the URI in our Html property
        public override string AsMimeType(MimeType type) {
            if (type == MimeType.Text_Html) {
                return X.GET<string>(Html);
            }
            return base.AsMimeType(type);
        }

        /// <summary>
        /// The way to get a URL for HTML partial if any.
        /// </summary>
        /// <returns></returns>
        public override string GetHtmlPartialUrl() {
            return Html;
        }

        /// <summary>
        /// Whenever we set a bound data object to this page, we update the
        /// URI property on this page.
        /// </summary>
        protected override void OnData() {
            base.OnData();
            var str = "";
            Json x = this;
            while (x != null) {
                if (x is SignIn)
                    str = (x as SignIn).UriFragment + str;
                x = x.Parent;
            }
            Uri = str;
        }

        /// <summary>
        /// Override to provide an URI fragment
        /// </summary>
        /// <returns></returns>
        protected virtual string UriFragment {
            get {
                return "";
            }
        }
        #endregion
    }
}
