using Concepts.Ring3;
using Concepts.Ring5;
using Starcounter;
using System;

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
                SystemUserSession userSession = SignInSystemUser(userId, password);
                if (userSession == null) {
                    message = "Invalid username or password";
                }
                return userSession;
            }

            // Use Auth token cookie if it exist
            if (signInAuthToken != null) {
                return SignInSystemUser(signInAuthToken);
            }

            return null;
        }

        /// <summary>
        /// Sign-in user
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="password"></param>
        static private SystemUserSession SignInSystemUser(string userId, string password) {

            string hashedPassword;
            Concepts.Ring5.SystemUserPassword.GeneratePasswordHash(userId, password, out hashedPassword);

            // Verify username and password
            SystemUser systemUser = Db.SQL<SystemUser>("SELECT o FROM Concepts.Ring3.SystemUser o WHERE o.Username=? AND o.Password=?", userId, hashedPassword).First;
            if (systemUser == null) {
                return null;
            }

            // Username and password OK
            SystemUserSession userSession = null;
            Db.Transaction(() => {

                // Create system user token
                SystemUserTokenKey token = new SystemUserTokenKey(systemUser);
                userSession = AssureSystemUserSession(token);

            });

            return userSession;
        }

        /// <summary>
        /// Sign-in user
        /// </summary>
        /// <param name="authToken"></param>
        static private SystemUserSession SignInSystemUser(string authToken) {

            SystemUserTokenKey oldToken = Db.SQL<Concepts.Ring5.SystemUserTokenKey>("SELECT o FROM Concepts.Ring5.SystemUserTokenKey o WHERE o.Token=?", authToken).First;
            if (oldToken == null) {
                // signed-out, Invalid or expired token key
                return null;
            }

            if (oldToken.User == null) {
                // System user deleted => delete invalid token
                Db.Transaction(() => {
                    oldToken.Delete();
                });
                return null;
            }

#if false   // NOTE: This can not be done until we can get the cookie when the Handle is called (Instead of using the authToken property we need to use the cookie value

            // Create new token, For better sequrity.
            SystemUserTokenKey newToken = null;

            // Remove old token and update SystemUserSession instances with new token
            Db.Transaction(() => {

                // Create new token
                newToken = new SystemUserTokenKey(oldToken.User);

                // Update tokens
                UpdateSystemUserSessionToken(oldToken, newToken);

                // Delete old token
                oldToken.Delete();
            });
            return AssureSystemUserSession(newToken);
#else
            return AssureSystemUserSession(oldToken);
#endif
        }

        /// <summary>
        /// Create system user session
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        static private SystemUserSession AssureSystemUserSession(SystemUserTokenKey token) {

            SystemUserSession userSession = null;

            Db.Transaction(() => {

                userSession = Db.SQL<Concepts.Ring5.SystemUserSession>("SELECT o FROM Concepts.Ring5.SystemUserSession o WHERE o.SessionIdString=?", Session.Current.SessionIdString).First;
                if (userSession == null) {
                    userSession = new SystemUserSession();
                    userSession.Created = DateTime.UtcNow;
                    userSession.SessionIdString = Session.Current.SessionIdString;
                }

                userSession.Token = token;
                userSession.Touched = DateTime.UtcNow;
            });

            // Simulate Commit-Hook handling
            JSON.systemusersession userSessionJson = new JSON.systemusersession();
            userSessionJson.ObjectID = userSession.GetObjectID();
            InvokeSignInCommitHook(userSessionJson);

            return userSession;
        }

        /// <summary>
        /// Update token on system user sessions
        /// </summary>
        /// <param name="oldToken"></param>
        /// <param name="newToken"></param>
        static private void UpdateSystemUserSessionToken(SystemUserTokenKey oldToken, SystemUserTokenKey newToken) {

            // Remove old token and update SystemUserSession instances with new token
            Db.Transaction(() => {

                var result = Db.SQL<Concepts.Ring5.SystemUserSession>("SELECT o FROM Concepts.Ring5.SystemUserSession o WHERE o.Token=?", oldToken);

                foreach (var userSession in result) {
                    userSession.Token = newToken;
                }
            });
        }

        /// <summary>
        /// Sign out user on all sessions that uses the same auth token
        /// </summary>
        static public void SignOutSystemUser(string authToken) {

            if (authToken == null) {
                return;
            }

            SystemUserTokenKey token = Db.SQL<Concepts.Ring5.SystemUserTokenKey>("SELECT o FROM Concepts.Ring5.SystemUserTokenKey o WHERE o.Token=?", authToken).First;
            if (token == null) {
                return;
            }

            Db.Transaction(() => {

                var result = Db.SQL<Concepts.Ring5.SystemUserSession>("SELECT o FROM Concepts.Ring5.SystemUserSession o WHERE o.Token=?", token);

                // Sign-out user with a specified auth token in all sessions
                foreach (SystemUserSession userSession in result) {

                    // Simulate Commit-Hook handling
                    JSON.systemusersession userSessionJson = new JSON.systemusersession();
                    userSessionJson.ObjectID = userSession.GetObjectID();
                    InvokeSignOutCommitHook(userSessionJson);

                    userSession.Delete();
                }

                // Remove system user token
                token.Delete();
            });
        }

        #region Commit Hook replacement

        /// <summary>
        /// Temporary code until starcounter implements commit hooks
        /// </summary>
        /// <param name="user"></param>
        static void InvokeSignInCommitHook(JSON.systemusersession usersession) {

            X.POST("/__db/__default/societyobjects/systemusersession", usersession.ToJsonUtf8(), null);
        }

        /// <summary>
        /// Temporary code until starcounter implements commit hooks
        /// </summary>
        /// <param name="user"></param>
        static void InvokeSignOutCommitHook(JSON.systemusersession usersession) {

            X.DELETE("/__db/__default/societyobjects/systemusersession", usersession.ToJsonUtf8(), null);
        }
        #endregion

    }
}
