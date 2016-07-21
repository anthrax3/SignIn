using Simplified.Ring5;
using Starcounter;

namespace SignIn {
    partial class SignInFormPage : Page {
        void Handle(Input.SignInClick Action) {
            this.Message = null;
            Action.Cancel();

            if (string.IsNullOrEmpty(this.Username)) {
                this.Message = "Username is required!";
                return;
            }

            this.Submit++;
        }

        void Handle(Input.RestoreClick Action) {
            Action.Cancel();

            if (this.MainForm != null) {
                this.MainForm.OpenRestorePassword();
            }
        }

        protected MainFormPage MainForm {
            get {
                return this.Parent as MainFormPage;
            }
        }
    }
}
