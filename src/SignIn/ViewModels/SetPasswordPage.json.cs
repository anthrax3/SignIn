using Simplified.Ring3;
using Starcounter;

namespace SignIn
{
    partial class SetPasswordPage : Json, IBound<SystemUser>
    {
        private string OldPassword;
        private string OldPasswordSalt;
        private bool IsInvalid;

        protected override void OnData()
        {
            base.OnData();
            this.OldPassword = this.Data.Password;
            this.OldPasswordSalt = this.Data.PasswordSalt;
        }

        private void Handle(Input.PasswordToSet action)
        {
            this.PasswordToSet = action.Value;
            this.ValidatePassword();
        }

        private void Handle(Input.PasswordRepeat action)
        {
            this.PasswordRepeat = action.Value;
            this.ValidatePassword();
        }

        private void ValidatePassword()
        {
            this.AssureNewPasswordPropertyFeedback();

            if (this.IsInvalid)
            {
                this.Data.Password = this.OldPassword;
                this.Data.PasswordSalt = this.OldPasswordSalt;
                return;
            }

            UserHelper.SetPassword(this.Data, PasswordToSet);
        }

        #region Validate properties
        protected void AssureNewPasswordPropertyFeedback()
        {
            if (string.IsNullOrEmpty(PasswordToSet))
            {
                this.Message = "Password must not be empty!";
                this.IsInvalid = true;
            }
            else if (PasswordToSet != PasswordRepeat)
            {
                this.Message = "Passwords do not match!";
                this.IsInvalid = true;
            }
            else
            {
                this.Message = null;
                this.IsInvalid = false;
            }
        }
        #endregion
    }
}
