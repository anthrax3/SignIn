using Starcounter;

namespace SignIn
{
    partial class MainFormPage : Json
    {
        protected override void OnData()
        {
            base.OnData();
            this.OpenSignIn();
        }

        public void OpenSignIn()
        {
            this.CurrentForm = Self.GET("/signin/partial/signin-form");
        }

        public void OpenRestorePassword()
        {
            this.CurrentForm = Self.GET("/signin/partial/restore-form");
        }
    }
}