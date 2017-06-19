using SignIn.Api;

namespace SignIn
{
    class Program
    {
        static void Main()
        {
            AuthorizationHelper.SetupPermissions();

            new Middleware().Register();
            new CommitHooks().Register();
            new MainHandlers().Register();
            new PartialHandlers().Register();
            new BlenderMapping().Register();
        }
    }
}