using System;
using System.Net;
using System.Net.Mail;
using Starcounter;
using Simplified.Ring2;
using Simplified.Ring3;
using Simplified.Ring6;

namespace SignIn
{
    partial class RestorePasswordFormPage : Page
    {
        void Handle(Input.SignInClick Action)
        {
            Action.Cancel();

            if (this.MainForm != null)
            {
                this.MainForm.OpenSignIn();
            }
        }

        void Handle(Input.Username action) // Makes the Reset Password clickable again.
        {
            this.RestoreClick = 0;
        }

        void Handle(Input.RestoreClick Action)
        {
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

            if (person == null || email == null)
            {
                this.Message = "Unable to restore password, no e-mail address found!";
                return;
            }

            string password = Utils.RandomString(5);
            string hash = SystemUser.GenerateClientSideHash(password);

            SystemUser.GeneratePasswordHash(user.Username, hash, user.PasswordSalt, out hash);

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
            SettingsMailServer settings = this.GetSettings();
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

        protected SettingsMailServer GetSettings()
        {
            string name = "SignInRestorePassword";
            SettingsMailServer settings =
                Db.SQL<SettingsMailServer>("SELECT s FROM Simplified.Ring6.SettingsMailServer s WHERE s.Name = ?", name)
                    .First;

            if (settings == null)
            {
                Db.Transact(() =>
                {
                    settings = new SettingsMailServer()
                    {
                        Name = name,
                        Port = 587,
                        Host = "mail.your-server.de",
                        Username = "signinapp@starcounter.io",
                        Password = "*****", // replace for real password
                        EnableSsl = true
                    };
                });
            }

            return settings;
        }

        protected MainFormPage MainForm
        {
            get { return this.Parent as MainFormPage; }
        }
    }
}