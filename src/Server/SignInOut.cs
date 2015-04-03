using System;
using System.Security.Cryptography;
using System.Web;
using Starcounter;
using Starcounter.Internal;
using Simplified.Ring3;
using Simplified.Ring5;
using Concepts.Ring8.Polyjuice;

namespace SignIn {
    public class SignInOut {
        static public SystemUserSession GetCurrentSystemUserSession() {
            return Db.SQL<SystemUserSession>("SELECT o FROM Simplified.Ring5.SystemUserSession o WHERE o.SessionIdString = ?", Session.Current.SessionIdString).First;
        }

        /// <summary>
        /// Sign-in user
        /// </summary>
        /// <param name="UserId"></param>
        /// <param name="Password"></param>
        /// <param name="SignInAuthToken"></param>
        /// <param name="Message"></param>
        /// <returns></returns>
        static public SystemUserSession SignInSystemUser(string UserId, string Password, string SignInAuthToken, out string Message) {
            Message = null;

            if (!string.IsNullOrEmpty(UserId)) {
                SystemUserSession userSession = SignInSystemUser(UserId, Password);
                
                if (userSession != null) {
                    return userSession;
                }

                Message = "Invalid username or password";
                return null;
            }

            if (string.IsNullOrEmpty(SignInAuthToken)) {

                Message = "Invalid username or password";
                return null;
            }

            // Use Auth token cookie if it exist
            return SignInSystemUser(SignInAuthToken);
        }

        /// <summary>
        /// Sign-in user
        /// </summary>
        /// <param name="UserId"></param>
        /// <param name="Password"></param>
        static private SystemUserSession SignInSystemUser(string UserId, string Password) {

            string hashedPassword;
            SystemUser systemUser = null;

            SystemUserPassword.GeneratePasswordHash(UserId.ToLower(), Password, out hashedPassword);
            systemUser = Db.SQL<SystemUser>("SELECT o FROM Simplified.Ring3.SystemUser o WHERE o.Username=? AND o.Password=?", UserId, hashedPassword).First;

            if (systemUser == null) {
                return null;
            }

            SystemUserSession userSession = null;
            
            Db.Transact(() => {
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
        /// <param name="UserId"></param>
        /// <returns></returns>
        static public String CreateAuthToken(string UserId) {
            // Server has a secret key K (a sequence of, say, 128 bits, produced by a cryptographically secure PRNG).
            // A token contains the user name (U), the time of issuance (T), and a keyed integrity check computed over U and T (together),
            // keyed with K (by default, use HMAC with SHA-256 or SHA-1).
            // Auth token    Username+tokendate
            byte[] randomNumber = new byte[16];

            RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider();
            rngCsp.GetBytes(randomNumber);

            return HttpServerUtility.UrlTokenEncode(randomNumber);
        }

        /// <summary>
        /// Sign-in user
        /// </summary>
        /// <param name="AuthToken"></param>
        static public SystemUserSession SignInSystemUser(string AuthToken) {
            SystemUserTokenKey oldToken = Db.SQL<SystemUserTokenKey>("SELECT o FROM Simplified.Ring5.SystemUserTokenKey o WHERE o.Token=?", AuthToken).First;
            
            if (oldToken == null) {
                return null;
            }

            if (oldToken.User == null) {
                Db.Transact(() => {
                    DeleteToken(oldToken);
                });

                return null;
            }

            // TODO: Check if token should expire (to old for reuse)?
            TimeSpan ts = new TimeSpan(DateTime.UtcNow.Ticks - oldToken.LastUsed.Ticks);
            
            if (ts.TotalDays > 7) {

                Db.Transact(() => {
                    DeleteToken(oldToken);
                });

                return null;
            }

            SystemUserSession session = null;

            Db.Transact(() => {
                session = AssureSystemUserSession(oldToken);
            });

            return session;
        }

        /// <summary>
        /// Create system user session
        /// </summary>
        /// <param name="Token"></param>
        /// <returns></returns>
        static private SystemUserSession AssureSystemUserSession(SystemUserTokenKey Token) {
            SystemUserSession userSession = null;

            bool bSessionCreated = false;

            Db.Transact(() => {
                userSession = Db.SQL<SystemUserSession>("SELECT o FROM Simplified.Ring5.SystemUserSession o WHERE o.SessionIdString=?", Session.Current.SessionIdString).First;

                if (userSession == null) {
                    userSession = new SystemUserSession();
                    userSession.Created = DateTime.UtcNow;
                    userSession.SessionIdString = Session.Current.SessionIdString;
                    bSessionCreated = true;
                }

                userSession.Token = Token;
                userSession.Touched = DateTime.UtcNow;
            });

            if (bSessionCreated) {
                InvokeSignInCommitHook(userSession.SessionIdString);
            }

            return userSession;
        }

        static private void DeleteToken(SystemUserTokenKey Token) {
            QueryResultRows<SystemUserSession> sessions = Db.SQL<SystemUserSession>("SELECT o FROM Simplified.Ring5.SystemUserSession o WHERE o.Token=?", Token);

            foreach (var session in sessions) {
                session.Delete();
            }

            Token.Delete();
        }

        static public bool SignOutSystemUser() {
            SystemUserSession session = GetCurrentSystemUserSession();

            if (session != null) {
                return SignOutSystemUser(session.Token.Token);
            }

            return false;
        }

        /// <summary>
        /// Sign out user on all sessions that uses the same auth token
        /// </summary>
        /// <param name="AuthToken"></param>
        /// <returns>True if a user was signed out, otherwice false is user is already signed out</returns>
        static public bool SignOutSystemUser(string AuthToken) {
            if (AuthToken == null) {
                return false;
            }

            SystemUserTokenKey token = Db.SQL<Simplified.Ring5.SystemUserTokenKey>("SELECT o FROM Simplified.Ring5.SystemUserTokenKey o WHERE o.Token=?", AuthToken).First;

            if (token == null) {
                return false;
            }

            bool bUserWasSignedOut = false;

            Db.Transact(() => {
                QueryResultRows<SystemUserSession> result = Db.SQL<SystemUserSession>("SELECT o FROM Simplified.Ring5.SystemUserSession o WHERE o.Token=?", token);

                foreach (SystemUserSession userSession in result) {
                    string sessoinId = userSession.SessionIdString;

                    userSession.Delete();
                    InvokeSignOutCommitHook(sessoinId);
                    bUserWasSignedOut = true;
                }

                token.Delete();
            });

            return bUserWasSignedOut;
        }

        #region Commit Hook replacement

        /// <summary>
        /// Temporary code until starcounter implements commit hooks
        /// </summary>
        /// <param name="user"></param>
        static void InvokeSignInCommitHook(string SessionIdString) {
            Response r = X.POST(CommitHooks.MappedTo, SessionIdString, null);

            if (r.StatusCode < 200 || r.StatusCode >= 300) {
            }
        }

        /// <summary>
        /// Temporary code until starcounter implements commit hooks
        /// </summary>
        /// <param name="user"></param>
        static void InvokeSignOutCommitHook(string SessionIdString) {
            Response r = X.DELETE(CommitHooks.MappedTo, SessionIdString, null);

            if (r.StatusCode < 200 || r.StatusCode >= 300) {
            }
        }
        #endregion
    }
}
