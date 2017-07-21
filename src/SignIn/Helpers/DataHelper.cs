using Simplified.Ring6;
using Starcounter;

namespace SignIn
{
    public static class DataHelper
    {
        public static SignInSettings GetSettings()
        {
            var settings = Db.SQL<SignInSettings>("SELECT s FROM Simplified.Ring6.SignInSettings s").First;
            if (settings == null)
            {
                Db.Transact(() =>
                {
                    settings = new SignInSettings
                    {
                        Name = "Default SignIn settings",
                        SignInFormAsFullPage = true
                    };
                });
            }
            return settings;
        }
    }
}