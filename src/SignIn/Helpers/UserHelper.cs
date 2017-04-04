using Simplified.Ring3;

namespace SignIn
{
    public class UserHelper
    {
        /// <summary>
        /// Set password
        /// </summary>
        public static void SetPassword(SystemUser user, string password)
        {
            string salt = SystemUser.GeneratePasswordSalt(16);
            string passwordHash = SystemUser.GeneratePasswordHash(user.Username, password, salt);

            user.Password = passwordHash;
            user.PasswordSalt = salt;
        }
    }
}