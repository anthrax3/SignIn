using Starcounter;
using Simplified.Ring5;

namespace SignIn
{
    partial class SignInPage : Json, IBound<SystemUserSession>
    {
        public bool IsSignedIn => this.Data != null;

        static SignInPage()
        {
            DefaultTemplate.FullName.Bind = "Token.User.WhoIs.FullName";
        }

        protected override void OnData()
        {
            base.OnData();
            this.SessionUri = Session.Current.SessionUri;
            this.SetAuthorizedState();
        }

        void Handle(Input.SignInClick action)
        {
            this.Message = string.Empty;
            action.Cancel();

            this.Submit++;
        }

        public void SetAuthorizedState()
        {
            this.Message = string.Empty;

            if (!this.IsSignedIn)
            {
                this.UserImage = Self.GET<Json>("/signin/partials/user/image", () => new UserImagePage());
            }
            else if (this.Data.Token.User.WhoIs != null)
            {
                this.UserImage = Self.GET<Json>("/signin/partials/user/image/" + this.Data.Token.User.WhoIs.GetObjectID(), () => new UserImagePage());
            }
        }
    }
}