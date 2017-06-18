using SignIn.Api;

namespace SignIn
{
    class Program
    {
        static void Main()
        {
            AuthorizationHelper.SetupPermissions();

            CommitHooks hooks = new CommitHooks();
            MainHandlers handlers = new MainHandlers();
            BlenderMaps ontology = new BlenderMaps();

            hooks.Register();
            handlers.Register();
            ontology.Register();
        }
    }
}