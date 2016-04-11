using Starcounter;
using Simplified.Ring3;

namespace SignIn {
    partial class AlreadyInPage : Page {
        protected override void OnData() {
            base.OnData();

            SystemUser user = SystemUser.GetCurrentSystemUser();

            if (user != null) {
                this.Username = user.Username;
            }
        }
    }
}
