using System;
using System.Web;
using Simplified.Ring3;
using Simplified.Ring6;
using Smorgasbord.PropertyMetadata;
using Starcounter;

// FORGOT PASSWORD:
// http://www.asp.net/identity/overview/features-api/account-confirmation-and-password-recovery-with-aspnet-identity

namespace SignIn
{
    partial class SystemUserAuthenticationSettings : PropertyMetadataPage, IBound<SystemUser>
    {
        public bool ResetPassword_Enabled_
        {
            get
            {
                var emailAddress = Utils.GetUserEmailAddress(this.Data);
                return MailSettingsHelper.GetSettings().Enabled && Utils.IsValidEmail(emailAddress.EMail);
            }
        }

        private void Handle(Input.SaveNewPassword action)
        {
            // Validate new data
            if (NewPasswordClick <= 0)
            {
                return;
            }

            AssureNewPasswordPropertyFeedback();

            if (this.IsInvalid)
            {
                return;
            }

            Db.Transact(() =>
            {
                // Set the new password
                SystemUser user = this.Data;
                UserHelper.SetPassword(user, NewPassword);

                // Sign out the changed user if required
                if (SystemUser.GetCurrentSystemUser() != user)
                {
                    SystemUser.SignOutSystemUser(user);
                }
            });

            // TODO: Set message with success status
        }

        private void Handle(Input.CancelNewPassword action)
        {
            // Reset all fields
            Message = null;
            NewPassword = string.Empty;
            NewPasswordRepeat = string.Empty;
            NewPasswordClick = 0;
            SaveNewPassword = 0;
            CancelNewPassword = 0;
        }

        void Handle(Input.ResetPassword action)
        {
            // Go to "Reset password" form
            this.Message = null;
            this.ResetUserPassword();
        }

        protected void ResetUserPassword()
        {
            string link = null;
            string fullName = string.Empty;
            var mailSettings = MailSettingsHelper.GetSettings();

            if (mailSettings.Enabled == false)
            {
                this.Message = "Mail Server not enabled in the settings.";
                return;
            }

            if (string.IsNullOrEmpty(mailSettings.SiteHost))
            {
                this.Message = "Invalid settings, check site host name / port";
                return;
            }

            var emailAddress = Utils.GetUserEmailAddress(this.Data);
            var email = emailAddress.EMail;
            if (!Utils.IsValidEmail(email))
            {
                this.Message = "Username is not an email address";
                return;
            }

            var transaction = this.Transaction;
            transaction.Scope(() =>
            {
                SystemUser systemUser = this.Data;
                // Generate Password Reset token
                ResetPassword resetPassword = new ResetPassword()
                {
                    User = systemUser,
                    Token = HttpUtility.UrlEncode(Guid.NewGuid().ToString()),
                    Expire = DateTime.UtcNow.AddMinutes(1440)
                };

                // Get FullName
                if (systemUser.WhoIs != null)
                {
                    fullName = systemUser.WhoIs.FullName;
                }
                else
                {
                    fullName = systemUser.Username;
                }

                // Build reset password link
                UriBuilder uri = new UriBuilder();

                uri.Host = mailSettings.SiteHost;
                uri.Port = (int)mailSettings.SitePort;

                uri.Path = "signin/user/resetpassword";
                uri.Query = "token=" + resetPassword.Token;

                link = uri.ToString();
            });

            transaction.Commit();

            try
            {
                this.Message = string.Format("Sending mail sent to {0}...", email);
                Utils.SendResetPasswordMail(fullName, email, link);
                this.Message = "Mail sent.";
            }
            catch (Exception e)
            {
                this.Message = e.Message;
            }
        }

        #region Validate properties
        protected void AssureNewPasswordPropertyFeedback()
        {
            if (string.IsNullOrEmpty(NewPassword))
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
            else if (NewPassword != NewPasswordRepeat)
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
