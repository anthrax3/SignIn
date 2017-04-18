using Starcounter;
using Simplified.Ring2;
using Simplified.Ring3;
using Simplified.Ring5;

namespace SignIn
{
    partial class ProfileFormPage : Json
    {
        protected override void OnData()
        {
            base.OnData();

            SystemUser user = SystemUser.GetCurrentSystemUser();
            EmailAddress email = Utils.GetUserEmailAddress(user);

            this.Username = user.Username;

            if (email != null)
            {
                this.Email = email.Name;
            }
        }

        void Handle(Input.UpdateClick action)
        {
            action.Cancel();
            this.Message = null;
            this.MessageCss = "alert alert-danger";

            if (string.IsNullOrEmpty(this.Email))
            {
                this.Message = "E-mail address is required!";
                return;
            }

            if (!Utils.IsValidEmail(this.Email))
            {
                this.Message = "This is not a valid e-mail address!";
                return;
            }

            Db.Transact(() =>
            {
                SystemUser user = SystemUser.GetCurrentSystemUser();
                EmailAddress email = Utils.GetUserEmailAddress(user);

                if (email == null)
                {
                    email = new EmailAddress();

                    EmailAddressRelation relation = new EmailAddressRelation()
                    {
                        EmailAddress = email,
                        Somebody = user.WhoIs as Person
                    };
                }

                email.Name = this.Email;
            });

            this.Message = "Profile changes has been updated";
            this.MessageCss = "alert alert-success";
        }

        void Handle(Input.ChangePasswordClick action)
        {
            action.Cancel();
            this.Message = null;
            this.MessageCss = "alert alert-danger";

            SystemUser user = SystemUser.GetCurrentSystemUser();
            bool validOldPassword = SystemUser.ValidatePasswordHash(user.Username, this.OldPassword, user.PasswordSalt, user.Password);

            if (!validOldPassword)
            {
                this.Message = "Invalid old password!";
                return;
            }

            if (string.IsNullOrEmpty(this.NewPassword))
            {
                this.Message = "New password is required!";
                return;
            }

            if (this.NewPassword != this.RepeatPassword)
            {
                this.Message = "Passwords do not match!";
                return;
            }

            string password = SystemUser.GeneratePasswordHash(user.Username, this.NewPassword, user.PasswordSalt);

            Db.Transact(() => { user.Password = password; });

            this.Message = "Your password has been successfully changed";
            this.MessageCss = "alert alert-success";
            this.OldPassword = null;
            this.NewPassword = null;
            this.RepeatPassword = null;
        }
    }
}