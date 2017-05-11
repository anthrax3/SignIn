using Simplified.Ring3;
using Smorgasbord.PropertyMetadata;
using Starcounter;

namespace SignIn.ViewModels
{
    partial class SetPasswordPage : PropertyMetadataPage, IBound<SystemUser>
    {
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
