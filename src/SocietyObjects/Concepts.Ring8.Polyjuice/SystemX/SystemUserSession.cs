using Starcounter;
using System;

namespace Concepts.Ring8.Polyjuice {
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
        /// Time when user signed in
        /// </summary>
        public DateTime Created;

        /// <summary>
        /// Time when user lasted touched the session access
        /// TODO:
        /// </summary>
        public DateTime Touched;
    }
}
