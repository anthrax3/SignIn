using Concepts.Ring3;
using SignInApp.Database;
using Starcounter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignInApp.Server {
    public class SignInOut {

        static public SystemUserSession Test() {
            return null;
        }

        /// <summary>
        /// Sign-in user
        /// </summary>
        static public SystemUserSession SignInSystemUser(string userId, string password, string signInAuthToken, out string message) {

            message = null;
            // If there is an user id then use it.
            if (!string.IsNullOrEmpty(userId)) {
                SystemUserSession userSession = SignInUser(userId, password);
                if (userSession == null) {
                    message = "Invalid username or password";
                }
                return userSession;
            }

            // Use Auth token cookie if it exist
            if (signInAuthToken != null) {
                return SignInUser(signInAuthToken);
            }

            return null;
        }

        /// <summary>
        /// Sign-in user
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="password"></param>
        static private SystemUserSession SignInUser(string userId, string password) {

            // Verify username and password
            SystemUser systemUser = Db.SQL<SystemUser>("SELECT o FROM Concepts.Ring3.SystemUser o WHERE o.Username=? AND o.Password=?", userId, password).First;
            if (systemUser == null) {
                return null;
            }

            // Username and password OK
            SystemUserSession userSession = null;
            Db.Transaction(() => {

                // Create system user token
                SystemUserTokenKey token = new SystemUserTokenKey(systemUser);
                userSession = CreateSystemUserSession(token);

            });

            return userSession;
        }

        /// <summary>
        /// Sign-in user
        /// </summary>
        /// <param name="authToken"></param>
        static private SystemUserSession SignInUser(string authToken) {

            SystemUserTokenKey token = Db.SQL<SignInApp.Database.SystemUserTokenKey>("SELECT o FROM SignInApp.Database.SystemUserTokenKey o WHERE o.Token=?", authToken).First;
            if (token == null) {
                // signed-out, Invalid or expired token key
                return null;
            }

            if (token.User == null) {
                // System user deleted => remove token
                Db.Transaction(() => {
                    token.Delete();
                });
                return null;
            }

            return CreateSystemUserSession(token);
        }

        /// <summary>
        /// Create system user session
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        static private SystemUserSession CreateSystemUserSession(SystemUserTokenKey token) {

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

            // Simulate Commit-Hook handling
            JSON.user user = Utils.SignedInUserToJson(userSession);
            InvokeSignInCommitHook(user);

            return userSession;
        }

        /// <summary>
        /// Sign out user on all sessions that uses the same auth token
        /// </summary>
        static public void SignOutSystemUser(string authToken) {

            if (authToken == null) {
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

        #region Commit Hook replacement

        /// <summary>
        /// Temporary code until starcounter implements commit hooks
        /// </summary>
        /// <param name="user"></param>
        static void InvokeSignInCommitHook(JSON.user user) {

            X.POST("/__db/__default/societyobjects/systemusersession", user.ToJsonUtf8(), null);
        }

        /// <summary>
        /// Temporary code until starcounter implements commit hooks
        /// </summary>
        /// <param name="user"></param>
        static void InvokeSignOutCommitHook(JSON.user user) {

            X.DELETE("/__db/__default/societyobjects/systemusersession", user.ToJsonUtf8(), null);
        }
        #endregion

    }
}
