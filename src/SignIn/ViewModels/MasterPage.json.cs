using Starcounter;
using Simplified.Ring3;
using Simplified.Ring5;

namespace SignIn
{
    partial class MasterPage : Json
    {
        public SignInPage SignInPage;

        protected string url;

        public void Open(string Url)
        {
            this.url = Url;
            this.RefreshSignInState();
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

            if ((userSession == null && this.SignInPage.Data != null) || !userSession.Equals(this.SignInPage.Data))
            {
                this.SignInPage.Data = userSession;
            }
        }
    }
}