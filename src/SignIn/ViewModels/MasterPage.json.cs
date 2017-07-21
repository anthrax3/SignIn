using Starcounter;
using Simplified.Ring3;
using Simplified.Ring5;

namespace SignIn
{
    partial class MasterPage : Json
    {
        public SignInPage SignInPage {
            get {
                return Session.Current.Store[nameof(SignInPage)] as SignInPage;
            }
        }

        protected string url;

        public void Open(string contentUri)
        {
            this.url = contentUri;
            this.RedirectUrl = null;
            this.RefreshSignInState();
        }

        public void Redirect(string redirectUri)
        {
            this.OriginalUrl = redirectUri;
            this.Open(null);
        }

        public void RefreshSignInState()
        {
            SystemUserSession userSession = SystemUser.GetCurrentSystemUserSession();

            if (this.RequireSignIn && userSession != null)
            {
                this.Partial = Self.GET(this.url);
            }
            else if (this.RequireSignIn && userSession == null)
            {
                this.Partial = Self.GET("/signin/partial/accessdenied-form");
            }
            else if (userSession == null && !string.IsNullOrEmpty(this.url))
            {
                this.Partial = Self.GET(this.url);
            }
            else if (!string.IsNullOrEmpty(this.OriginalUrl))
            {
                this.Partial = null;
                this.RedirectUrl = this.OriginalUrl;
                this.OriginalUrl = null;
            }
            else if (userSession != null)
            {
                this.Partial = Self.GET("/signin/partial/alreadyin-form");
            }

            SignInPage sip = this.SignInPage;
            if (sip != null)
            {
                if (
                    (userSession == null && sip.Data != null) || //switching state to signed in
                    (userSession != null && !userSession.Equals(sip.Data)) //switching state to signed out
                 )
                {
                    sip.Data = userSession;
                }
            }
        }
    }
}