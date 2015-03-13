//#define NEW_TOKEN_ON_EACH_REQUEST // NOTE: This can not be done until we can get the cookie when the Handle is called (Instead of using the authToken property we need to use the cookie value

using SignInApp.Server.Database;
using Simplified.Ring3;
using Simplified.Ring5;
using Starcounter;
using Starcounter.Internal;
using System;
using System.Security.Cryptography;
using System.Web;

namespace SignInApp.Server {
    public class SignInOut {


        /// <summary>
        /// Sign-in user
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="password"></param>
        /// <param name="signInAuthToken"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        static public SystemUserSession SignInSystemUser(string userId, string password, string signInAuthToken, out string message) {

            message = null;

            // If there is an user id then use it.
            if (!string.IsNullOrEmpty(userId)) {
                SystemUserSession userSession = SignInSystemUser(userId, password);
                if (userSession != null) {
                    return userSession;
                }

                message = "Invalid username or password";
                return null;
            }

            if (string.IsNullOrEmpty(signInAuthToken)) {

                message = "Invalid username or password";
                return null;
            }

            // Use Auth token cookie if it exist
            return SignInSystemUser(signInAuthToken);
        }

        /// <summary>
        /// Sign-in user
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="password"></param>
        static private SystemUserSession SignInSystemUser(string userId, string password) {

            string hashedPassword;
            SystemUser systemUser = null;

            if (Utils.IsValidEmail(userId)) {
                // Try signing in with email

                // Get System username
                systemUser = Db.SQL<Simplified.Ring3.SystemUser>("SELECT CAST(o.ToWhat AS Simplified.Ring3.SystemUser) FROM Concepts.Ring2.EMailAddress o WHERE o.ToWhat IS Simplified.Ring3.SystemUser AND o.EMail=?", userId).First;
                if (systemUser != null) {
                    Concepts.Ring8.Polyjuice.SystemUserPassword.GeneratePasswordHash(systemUser.Username.ToLower(), password, out hashedPassword);
                    if (systemUser.Password != hashedPassword) {
                        systemUser = null;
                    }
                }
            }
            else {
                Concepts.Ring8.Polyjuice.SystemUserPassword.GeneratePasswordHash(userId.ToLower(), password, out hashedPassword);
                // Verify username and password
                systemUser = Db.SQL<SystemUser>("SELECT o FROM Simplified.Ring3.SystemUser o WHERE o.Username=? AND o.Password=?", userId, hashedPassword).First;
            }

            if (systemUser == null) {
                return null;
            }

            // SignIn SystemUser, Username and password OK
            SystemUserSession userSession = null;
            Db.Transact(() => {

                // Create system user token
                SystemUserTokenKey token = new SystemUserTokenKey();

                token.Created = token.LastUsed = DateTime.UtcNow;
                token.Token = CreateAuthToken(systemUser.Username);
                token.User = systemUser;

                userSession = AssureSystemUserSession(token);

            });

            return userSession;
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="userid"></param>
        /// <returns></returns>
        static public String CreateAuthToken(string userid) {

            // Server has a secret key K (a sequence of, say, 128 bits, produced by a cryptographically secure PRNG).
            // A token contains the user name (U), the time of issuance (T), and a keyed integrity check computed over U and T (together),
            // keyed with K (by default, use HMAC with SHA-256 or SHA-1).
            // Auth token    Username+tokendate
            byte[] randomNumber = new byte[16];

            RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider();
            rngCsp.GetBytes(randomNumber);

            return HttpServerUtility.UrlTokenEncode(randomNumber);

            //SHA256 mySHA256 = SHA256Managed.Create();
            //byte[] hashValue = mySHA256.ComputeHash(Encoding.UTF8.GetBytes(userid));

        }

        /// <summary>
        /// Sign-in user
        /// </summary>
        /// <param name="authToken"></param>
        static private SystemUserSession SignInSystemUser(string authToken) {

            SystemUserTokenKey oldToken = Db.SQL<Simplified.Ring5.SystemUserTokenKey>("SELECT o FROM Simplified.Ring5.SystemUserTokenKey o WHERE o.Token=?", authToken).First;
            if (oldToken == null) {
                // signed-out, Invalid or expired token key
                return null;
            }

            if (oldToken.User == null) {
                // System user deleted => delete invalid token
                Db.Transact(() => {

                    // Remove token and it's assigned sessions
                    DeleteToken(oldToken);

                });
                return null;
            }

            // TODO: Check if token should expire (to old for reuse)?
            TimeSpan ts = new TimeSpan( DateTime.UtcNow.Ticks - oldToken.LastUsed.Ticks );
            if (ts.TotalDays > 7) {  // TODO: Make expire time configurable

                Db.Transact(() => {
                    // Remove token and it's assigned sessions
                    DeleteToken(oldToken);
                });

                // Session has expired
                return null;
            }
#if NEW_TOKEN_ON_EACH_REQUEST   

            // Create new token, For better security.
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
            SystemUserSession userSession = null;
            Db.Transact(() => {
                userSession = AssureSystemUserSession(oldToken);
            });

            return userSession;
#endif
        }

        /// <summary>
        /// Create system user session
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        static private SystemUserSession AssureSystemUserSession(SystemUserTokenKey token) {

            SystemUserSession userSession = null;

            //Db.Transaction(() => {
            bool bSessionCreated = false;

            userSession = Db.SQL<Simplified.Ring5.SystemUserSession>("SELECT o FROM Simplified.Ring5.SystemUserSession o WHERE o.SessionIdString=?", Session.Current.SessionIdString).First;
            if (userSession == null) {
                userSession = new SystemUserSession();
                userSession.Created = DateTime.UtcNow;
                userSession.SessionIdString = Session.Current.SessionIdString;
                bSessionCreated = true;
            }
            else {

            }

            userSession.Token = token;
            userSession.Touched = DateTime.UtcNow;
            //});

            // Simulate Commit-Hook handling
            if (bSessionCreated) {
                JSON.systemusersession userSessionJson = new JSON.systemusersession();
                userSessionJson.ObjectID = userSession.GetObjectID();
                InvokeSignInCommitHook(userSessionJson);
            }

            return userSession;
        }

        static private void DeleteToken(SystemUserTokenKey token) {

            // Remove the user sessions
            var sessions = Db.SQL<Simplified.Ring5.SystemUserSession>("SELECT o FROM Simplified.Ring5.SystemUserSession o WHERE o.Token=?", token);
            foreach (var session in sessions) {
                session.Delete();
            }

            token.Delete();
        }

#if NEW_TOKEN_ON_EACH_REQUEST   

        /// <summary>
        /// Update token on system user sessions
        /// </summary>
        /// <param name="oldToken"></param>
        /// <param name="newToken"></param>
        static private void UpdateSystemUserSessionToken(SystemUserTokenKey oldToken, SystemUserTokenKey newToken) {

            // Remove old token and update SystemUserSession instances with new token
            Db.Transaction(() => {

                var result = Db.SQL<Simplified.Ring5.SystemUserSession>("SELECT o FROM Simplified.Ring5.SystemUserSession o WHERE o.Token=?", oldToken);

                foreach (var userSession in result) {
                    userSession.Token = newToken;
                }
            });
        }
#endif

        /// <summary>
        /// Sign out user on all sessions that uses the same auth token
        /// </summary>
        /// <param name="authToken"></param>
        /// <returns>True if a user was signed out, otherwice false is user is already signed out</returns>
        static public bool SignOutSystemUser(string authToken) {

            if (authToken == null) {
                return false;
            }

            SystemUserTokenKey token = Db.SQL<Simplified.Ring5.SystemUserTokenKey>("SELECT o FROM Simplified.Ring5.SystemUserTokenKey o WHERE o.Token=?", authToken).First;
            if (token == null) {
                return false;
            }

            bool bUserWasSignedOut = false;

            Db.Transact(() => {

                var result = Db.SQL<Simplified.Ring5.SystemUserSession>("SELECT o FROM Simplified.Ring5.SystemUserSession o WHERE o.Token=?", token);
                // Sign-out user with a specified auth token in all sessions
                foreach (SystemUserSession userSession in result) {

                    // Simulate Commit-Hook handling
                    JSON.systemusersession userSessionJson = new JSON.systemusersession();
                    userSessionJson.ObjectID = userSession.GetObjectID();
                    userSessionJson.SessionIdString = userSession.SessionIdString;

                    userSession.Delete();

                    InvokeSignOutCommitHook(userSessionJson);
                    bUserWasSignedOut = true;
                }

                // Remove system user token
                token.Delete();

            });

            return bUserWasSignedOut;
        }

        #region Commit Hook replacement

        /// <summary>
        /// Temporary code until starcounter implements commit hooks
        /// </summary>
        /// <param name="user"></param>
        static void InvokeSignInCommitHook(JSON.systemusersession usersession) {

            Response r = X.POST(CommitHooks.MappedTo, usersession.ToJsonUtf8(), null);
            if (r.StatusCode < 200 || r.StatusCode >= 300) {
            }
        }

        /// <summary>
        /// Temporary code until starcounter implements commit hooks
        /// </summary>
        /// <param name="user"></param>
        static void InvokeSignOutCommitHook(JSON.systemusersession usersession) {

            Response r = X.DELETE(CommitHooks.MappedTo, usersession.ToJsonUtf8(), null);
            if (r.StatusCode < 200 || r.StatusCode >= 300) {
            }
        }
        #endregion
    }
}
