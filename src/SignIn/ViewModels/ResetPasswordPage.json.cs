using System;
using Simplified.Ring3;
using Simplified.Ring6;
using Starcounter;

namespace SignIn
{
    [ResetPasswordPage_json]
    partial class ResetPasswordPage : Json
    {
        internal ResetPassword ResetPassword { get; set; }

        void Handle(Input.ConfirmPassword action)
        {
            CheckPasswordMatch(this.Password, action.Value);
        }

        void Handle(Input.Save action)
        {
            if (!CheckPasswordMatch(this.Password, this.ConfirmPassword))
            {
                return;
            }

            if (this.ResetPassword == null)
            {
                this.Message = "Reset token already used";
                return;
            }

            if (this.ResetPassword.Expire < DateTime.UtcNow)
            {
                this.Message = "Reset token expired";
                return;
            }

            if (this.ResetPassword.User == null)
            {
                this.Message = "Failed to get the user"; // TODO: Better message
                return;
            }

            Db.Transact(() =>
            {
                var user = this.ResetPassword.User;

                UserHelper.SetPassword(user, this.Password);
                ResetPassword.Delete();

                if (SystemUser.GetCurrentSystemUser() != user)
                {
                    SystemUser.SignOutSystemUser(user);
                }
            });

            this.RedirectUrl = "/signin/signinuser";
        }

        private bool CheckPasswordMatch(string pw1, string pw2)
        {
            if (pw1 != pw2)
            {
                this.Message = "Password missmatch";
                return false;
            }
            this.Message = string.Empty;
            return true;
        }
    }
}
