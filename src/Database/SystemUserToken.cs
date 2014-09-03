using Starcounter;
using System;
using System.Security.Cryptography;
using System.Web;

namespace SignInApp.Database {
    [Database]
    public class SystemUserTokenKey {

        public SystemUserTokenKey(Concepts.Ring3.SystemUser user) {
            this._User = user;
            this._Created = this.LastUsed = DateTime.UtcNow;
            this._Token = CreateAuthToken(user.Username);
        }

        /// <summary>
        /// Token
        /// </summary>
        private String _Token;

        /// <summary>
        /// Token
        /// </summary>
        public String Token { get { return _Token; } }

        /// <summary>
        /// System User
        /// </summary>
        private Concepts.Ring3.SystemUser _User;

        /// <summary>
        /// System User
        /// </summary>
        public Concepts.Ring3.SystemUser User { get { return _User; } }

        /// <summary>
        /// Created
        /// </summary>
        private DateTime _Created;

        /// <summary>
        /// Created
        /// </summary>
        public DateTime Created { get { return _Created; } }

        /// <summary>
        /// Last used
        /// </summary>
        public DateTime LastUsed;

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
    }
}
