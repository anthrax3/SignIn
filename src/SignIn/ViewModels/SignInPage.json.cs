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
            if (this.IsSignedIn)
            {
                this.SetAuthorizedState();
            }
            else
            {
                this.SetAnonymousState();
            }
        }

        void Handle(Input.SignInClick action)
        {
            this.Message = string.Empty;
            action.Cancel();

            this.Submit++;
        }

        public void SetAnonymousState()
        {
            this.UserImage = Self.GET<Json>("/signin/partials/user/image", () => new UserImagePage());
        }

        public void SetAuthorizedState()
        {
            this.Message = string.Empty;

            if (this.Data.Token.User.WhoIs != null)
            {
                this.UserImage = Self.GET<Json>("/signin/partials/user/image/" + this.Data.Token.User.WhoIs.GetObjectID(), () => new UserImagePage());
            }
        }
    }
}