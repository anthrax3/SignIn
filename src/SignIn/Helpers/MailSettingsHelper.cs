using System.Net;
using Simplified.Ring6;
using Starcounter;
using Starcounter.Internal;

namespace SignIn
{
    /// <summary>
    /// Helper for SignIn mail server settings
    /// </summary>
    public static class MailSettingsHelper
    {
        /// <summary>
        /// Get default SignIn <see cref="SettingsMailServer"/>
        /// </summary>
        public static SettingsMailServer GetSettings()
        {
            var name = "SignInMailSettings";
            SettingsMailServer settings = Db.SQL<SettingsMailServer>("SELECT s FROM Simplified.Ring6.SettingsMailServer s WHERE s.Name = ?", name).First;

            if (settings == null)
            {
                Db.Transact(() => {
                    settings = new SettingsMailServer
                    {
                        Name = name,
                        Enabled = false,
                        Host = "",
                        Port = 587,
                        EnableSsl = true,
                        Username = "",
                        Password = "",
                        SiteHost = Dns.GetHostEntry(string.Empty).HostName,
                        SitePort = StarcounterEnvironment.Default.UserHttpPort
                    };
                });
            }
            return settings;
        }

        /// <summary>
        /// Check if the mail server settings are valid 
        /// </summary>
        public static bool IsValid(this SettingsMailServer settings)
        {
            if (string.IsNullOrEmpty(settings.Host))
            {
                return false;
            }

            if (settings.Port > IPEndPoint.MaxPort || (settings.Port < IPEndPoint.MinPort))
            {
                return false;
            }

            return true;
        }
    }
}
