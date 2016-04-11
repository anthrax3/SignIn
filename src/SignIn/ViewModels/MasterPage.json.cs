using Starcounter;
using Simplified.Ring3;

namespace SignIn {
    partial class MasterPage : Page {
        protected string url;

        public void Open(string Url) {
            this.url = Url;
            this.RefreshSignInState();
        }

        public void RefreshSignInState() {
            SystemUser user = SystemUser.GetCurrentSystemUser();

            if (user == null && !string.IsNullOrEmpty(this.url)) {
                this.Partial = Self.GET(this.url);
            } else if (user != null) {
                this.Partial = Self.GET("/signin/partial/alreadyin-form");
            }
        }
    }
}
