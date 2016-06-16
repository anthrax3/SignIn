using Starcounter;

namespace SignIn {
    partial class SessionContainer : Json {
        public SignInPage SignIn {
            get {
                return this.SignInPage as SignInPage;
            }
            set {
                this.SignInPage = value;
            }
        } 

        public MasterPage Master {
            get {
                return this.MasterPage as MasterPage;
            }
            set {
                this.MasterPage = value;
            }
        }

        public void RefreshSignInState() {
            if (this.SignIn != null) {
                this.SignIn.RefreshSignInState();
            }

            if (this.Master != null) {
                this.Master.RefreshSignInState();
            }
        }
    }
}
