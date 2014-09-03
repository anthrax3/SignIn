using Concepts.Ring1;
using Starcounter;
using System;

namespace SignInApp.Database {
    /// <summary>
    /// Table with Signed in users sessions
    /// </summary>
    [Database]
    public class SystemUserSession {

        /// <summary>
        /// Token
        /// </summary>
        public SystemUserTokenKey Token;

        /// <summary>
        /// Session ID
        /// </summary>
        public String SessionIdString;

        /// <summary>
        /// Signed in user
        /// </summary>
        public Concepts.Ring3.SystemUser User;

        /// <summary>
        /// Time when user signed in
        /// </summary>
        public DateTime Created;

        /// <summary>
        /// Time when user lasted touched the session access
        /// TODO:
        /// </summary>
        public DateTime Touched;

        /// <summary>
        /// IP Address of the signed in user
        /// TODO:
        /// </summary>
        public string IP;
    }
}
