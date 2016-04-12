using Simplified.Ring5;
using Starcounter;

namespace SignIn {
    partial class SignInFormPage : Page {
        void Handle(Input.SignInClick Action) {
            string message = null;

            this.Message = null;
            Action.Cancel();

            if (string.IsNullOrEmpty(this.Username)) {
                message = "Username is required!";
            }

            if (!string.IsNullOrEmpty(message)) {
                this.Message = message;
                return;
            }

            this.Submit++;
        }
    }
}
