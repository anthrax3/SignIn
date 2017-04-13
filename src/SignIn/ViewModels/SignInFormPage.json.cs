using Simplified.Ring5;
using Starcounter;

namespace SignIn
{
    partial class SignInFormPage : Json
    {
        protected override void OnData()
        {
            base.OnData();
            this.SessionUri = Session.Current.SessionUri;
        }

        void Handle(Input.SignInClick action)
        {
            this.Message = null;
            action.Cancel();

            this.Submit++;
        }

        void Handle(Input.RestoreClick action)
        {
            action.Cancel();

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