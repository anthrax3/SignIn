using SignIn.Api;

namespace SignIn
{
    class Program
    {
        static void Main()
        {
            AuthorizationHelper.SetupPermissions();

            new CommitHooks().Register();
            new MainHandlers().Register();
            new BlenderMapping().Register();
        }
    }
}