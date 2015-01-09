using Concepts.Ring8.Polyjuice.Permissions;
using SocietyObjects.Concepts.Ring8.Polyjuice.App;
using Starcounter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Concepts.Ring8.Polyjuice.App {
    public class UriPermissionHelper {

        static public string LauncherWorkSpacePath = "/launcher/workspace"; // NOTE: If you change this you also need to change the links in the HTML files.

        static public bool TryNavigateTo(string url, Request request, string html, out Json returnPage) {

            returnPage = null;

            //string html = "/signinapp/redirect.html";

            Concepts.Ring3.SystemUser systemUser = UriPermissionHelper.GetCurrentSystemUser();
            if (systemUser == null) {
                // Ask user to sign in.
                returnPage = UriPermissionHelper.GetSignInPage(UriPermissionHelper.LauncherWorkSpacePath + request.Uri, html);
                return false;
            }

            // Check user permission
            if (!UriPermission.CanGetUri(systemUser, url, request)) {
                // User has no permission, redirect to app's root page
                returnPage = UriPermissionHelper.GetRedirectPage("/", html);
                return false;
            }

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        static public Json GetSignInPage(string referer, string html) {
            return GetRedirectPage(UriPermissionHelper.LauncherWorkSpacePath + "/signinapp/signinuser?" + HttpUtility.UrlEncode("originurl" + "=" + referer), html);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        static public Json GetRedirectPage(string redirectUrl, string html) {
            return new RedirectPage() { Html = html, RedirectUrl = redirectUrl };
        }


        static public Concepts.Ring3.SystemUser GetCurrentSystemUser() {

            SystemUserSession userSession = Db.SQL<SystemUserSession>("SELECT o FROM Concepts.Ring8.Polyjuice.SystemUserSession o WHERE o.SessionIdString=?", Session.Current.SessionIdString).First;
            if (userSession == null) {
                return null;
            }

            if (userSession.Token == null) {
                return null;
            }

            return userSession.Token.User;
        }
    }
}
