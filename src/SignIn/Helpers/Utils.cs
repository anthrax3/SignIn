using System;
using System.Net;
using System.Net.Mail;
using System.Text;
using Simplified.Ring2;
using Simplified.Ring3;
using Simplified.Ring6;
using Starcounter;

namespace SignIn
{
    public class Utils
    {
        /// <summary>
        /// Check if Email has the correct syntax
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        public static string RandomString(int size)
        {
            string input = "abcdefghijklmnopqrstuvwxyz0123456789";
            StringBuilder builder = new StringBuilder();
            Random random = new Random();
            char ch;

            for (int i = 0; i < size; i++)
            {
                ch = input[random.Next(0, input.Length)];
                builder.Append(ch);
            }

            return builder.ToString();
        }

        public static EmailAddress GetUserEmailAddress(SystemUser user)
        {
            Person person = user.WhoIs as Person;

            if (person == null)
            {
                return null;
            }

            EmailAddress email =
                Db.SQL<EmailAddress>(
                        "SELECT r.EmailAddress FROM Simplified.Ring3.EmailAddressRelation r WHERE r.Somebody = ?", person)
                    .First;

            return email;
        }

        /// <summary>
        /// Send Reset password email
        /// </summary>
        public static void SendResetPasswordMail(string toFullName, string toEmail, string link)
        {
            SettingsMailServer settings = MailSettingsHelper.GetSettings();

            try
            {
                if (!settings.Enabled)
                {
                    throw new InvalidOperationException("Mail service not enabled in the configuration.");
                }

                MailAddress fromAddress = new MailAddress(settings.Username, "Starcounter App");
                MailAddress toAddress = new MailAddress(toEmail);

                const string subject = "Starcounter App, Reset password request";

                string body = string.Format(
                    "Hi {0}<br><br>" +
                    "We received a request to reset your password<br><br>" +
                    "Click <a href='{1}'>here</a> to set a new password<br><br>" +
                    "Thanks<br>",

                    toFullName, link);

                var smtp = new SmtpClient
                {
                    Host = settings.Host,
                    Port = settings.Port,
                    EnableSsl = settings.EnableSsl,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(fromAddress.Address, settings.Password)
                };

                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    IsBodyHtml = true,
                    Body = body

                })
                {
                    smtp.Send(message);
                }
            }
            catch (Exception e)
            {
                throw e;
                // TODO:
                //LogWriter.WriteLine(string.Format("ERROR: Failed to send registration email event. {0}", e.Message));
            }
        }
    }
}