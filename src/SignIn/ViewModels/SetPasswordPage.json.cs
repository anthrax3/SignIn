using Simplified.Ring3;
using Smorgasbord.PropertyMetadata;
using Starcounter;

namespace SignIn.ViewModels
{
    partial class SetPasswordPage : PropertyMetadataPage, IBound<SystemUser>
    {
        private string _oldPassword;
        private string _oldPasswordSalt;

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

            if (this.IsInvalid)
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
                var message = "Password must not be empty!";
                this.AddPropertyFeedback(new PropertyMetadataItem
                {
                    Message = "Password must not be empty!",
                    ErrorLevel = 1,
                    PropertyName = "PasswordToSet"
                });
                this.IsInvalid = true;
                this.Message = message;
            }
            else if (PasswordToSet != PasswordRepeat)
            {
                var message = "Password repeat does not match the Password!";
                this.AddPropertyFeedback(new PropertyMetadataItem
                {
                    Message = message,
                    ErrorLevel = 1,
                    PropertyName = "PasswordToSet"
                });
                this.AddPropertyFeedback(new PropertyMetadataItem
                {
                    Message = message,
                    ErrorLevel = 1,
                    PropertyName = "PasswordRepeat"
                });
                this.IsInvalid = true;
                this.Message = message;
            }
            else
            {
                this.RemovePropertyFeedback("PasswordToSet");
                this.RemovePropertyFeedback("PasswordRepeat");
                this.Message = null;
            }
        }
        #endregion
    }
}
