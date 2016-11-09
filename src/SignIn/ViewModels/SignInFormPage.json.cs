using Simplified.Ring5;
using Starcounter;

namespace SignIn
{
    partial class SignInFormPage : Page
    {
        protected override void OnData()
        {
            base.OnData();
            this.Referer = Session.Current.Referer;
        }

        void Handle(Input.SignInClick Action)
        {
            this.Message = null;
            Action.Cancel();

            this.Submit++;
        }

        void Handle(Input.RestoreClick Action)
        {
            Action.Cancel();

            if (this.MainForm != null)
            {
                this.MainForm.OpenRestorePassword();
            }
        }

        protected MainFormPage MainForm
        {
            get { return this.Parent as MainFormPage; }
        }
    }
}