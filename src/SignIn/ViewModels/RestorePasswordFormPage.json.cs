using System;
using System.Net;
using System.Net.Mail;
using Starcounter;
using Simplified.Ring2;
using Simplified.Ring3;
using Simplified.Ring6;

namespace SignIn
{
    partial class RestorePasswordFormPage : Json
    {
        void Handle(Input.SignInClick action)
        {
            action.Cancel();

            if (this.MainForm != null)
            {
                this.MainForm.OpenSignIn();
            }
        }

        void Handle(Input.Username action) // Makes the Reset Password clickable again.
        {
            this.DisableRestoreClick = 0;
        }

        void Handle(Input.RestoreClick action)
        {
            this.DisableRestoreClick = 1;
            this.MessageCss = "alert alert-danger";

            if (string.IsNullOrEmpty(this.Username))
            {
                this.Message = "Username is required!";
                return;
            }

            SystemUser user = SystemUser.GetSystemUser(this.Username);

            if (user == null)
            {
                this.Message = "Invalid username!";
                return;
            }

            Person person = user.WhoIs as Person;
            EmailAddress email = Utils.GetUserEmailAddress(user);

            if (person == null || email == null || string.IsNullOrEmpty(email.EMail))
            {
                this.Message = "Unable to restore password, no e-mail address found!";
                return;
            }

            string password = Utils.RandomString(5);
            string hash = SystemUser.GeneratePasswordHash(user.Username, password, user.PasswordSalt);

            try
            {
                this.SendNewPassword(person.FullName, user.Username, password, email.Name);
                this.Message = "Your new password has been sent to your email address.";
                this.MessageCss = "alert alert-success";
                Db.Transact(() => { user.Password = hash; });
            }
            catch (Exception ex)
            {
                this.Message = "Mail server is currently unavailable.";
                this.MessageCss = "alert alert-danger";
                Starcounter.Logging.LogSource log = new Starcounter.Logging.LogSource(Application.Current.Name);
                log.LogException(ex);
            }
        }

        protected void SendNewPassword(string Name, string Username, string NewPassword, string Email)
        {
            SettingsMailServer settings = MailSettingsHelper.GetSettings();
            MailMessage mail = new MailMessage(settings.Username, Email);
            SmtpClient client = new SmtpClient();

            client.Port = settings.Port;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential(settings.Username, settings.Password);
            client.Host = settings.Host;
            client.EnableSsl = settings.EnableSsl;

            mail.Subject = "Restore password";
            mail.Body =
                string.Format(
                    "<h1>Hello {0}</h1><p>You have requested a new password for your <b>{1}</b> account.</p><p>Your new password is: <b>{2}</b>.</p>",
                    Name, Username, NewPassword);
            mail.IsBodyHtml = true;
            client.Send(mail);
        }

        protected MainFormPage MainForm
        {
            get { return this.Parent as MainFormPage; }
        }
    }
}