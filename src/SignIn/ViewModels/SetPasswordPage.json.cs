using Simplified.Ring3;
using Starcounter;

namespace SignIn
{
    partial class SetPasswordPage : Json, IBound<SystemUser>
    {
        private string _oldPassword;
        private string _oldPasswordSalt;
        private bool _isInvalid;

        protected override void OnData()
        {
            base.OnData();
            this._oldPassword = this.Data.Password;
            this._oldPasswordSalt = this.Data.PasswordSalt;
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

            if (this._isInvalid)
            {
                this.Data.Password = this._oldPassword;
                this.Data.PasswordSalt = this._oldPasswordSalt;
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
                this._isInvalid = true;
            }
            else if (PasswordToSet != PasswordRepeat)
            {
                this.Message = "Passwords do not match!";
                this._isInvalid = true;
            }
            else
            {
                this.Message = null;
                this._isInvalid = false;
            }
        }
        #endregion
    }
}
