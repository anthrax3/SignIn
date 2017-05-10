using Simplified.Ring3;
using Smorgasbord.PropertyMetadata;
using Starcounter;

namespace SignIn.ViewModels
{
    partial class SetPasswordPage : PropertyMetadataPage, IBound<SystemUser>
    {
        protected override void OnData()
        {
            base.OnData();
            SystemUser user = this.Data;
            //UserHelper.SetPassword(user, NewPassword);
            //this.SessionUri = Session.Current.SessionUri;
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
            if (PasswordRepeat == PasswordToSet)
            {
                UserHelper.SetPassword(this.Data, PasswordToSet);
            }
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
                    PropertyName = "NewPassword"
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
                    PropertyName = "NewPassword"
                });
                this.AddPropertyFeedback(new PropertyMetadataItem
                {
                    Message = message,
                    ErrorLevel = 1,
                    PropertyName = "NewPasswordRepeat"
                });
                this.Message = message;
            }
            else
            {
                this.RemovePropertyFeedback("NewPassword");
                this.RemovePropertyFeedback("NewPasswordRepeat");
                this.Message = null;
            }

        }
        #endregion
    }
}
