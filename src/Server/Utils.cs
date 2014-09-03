using SignInApp.Database;
using Starcounter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace SignInApp.Server {
    public class Utils {

        /// <summary>
        /// Convert SignedInUser to Json
        /// </summary>
        /// <param name="signedInUser"></param>
        /// <returns></returns>
        static public JSON.user SignedInUserToJson(SystemUserSession userSession) {

            Concepts.Ring3.SystemUser user = userSession.User;

            // Create JSON response body
            JSON.user userJson = new JSON.user();
            userJson.userId = user.Username;

            if (user.WhoIs != null) {
                userJson.fullName = user.WhoIs.FullName;
            }
            else {
                userJson.fullName = user.Username;
            }

            userJson.authToken = userSession.Token.Token;
            userJson.sessionid = userSession.SessionIdString;
            return userJson;
        }


    }
}
