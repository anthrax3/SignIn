using System.Text.RegularExpressions;
using Simplified.Ring3;
using Smorgasbord.PropertyMetadata;
using Starcounter;

namespace SignIn.ViewModels
{
    partial class SetPasswordPage : Json, IBound<SystemUser>
    {
        private string _oldPassword;
        private string _oldPasswordSalt;
        private bool _isInvalid;

        protected override void OnData()
        {
            base.OnData();
            _oldPassword = this.Data.Password;
            _oldPasswordSalt = this.Data.PasswordSalt;
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
            // Must contains: digit from 0-9, one lowercase, one uppercase, length: 6 to 20
            var regex = new Regex("((?=.*\\d)(?=.*[a-z])(?=.*[A-Z]).{6,20})");

            if (!regex.IsMatch(PasswordToSet))
            {
                this.Message = "Password must contains from 6 to 20 characters, with at least one uppercase letter, lowercase letter and number.";
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
