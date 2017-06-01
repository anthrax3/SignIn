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
            OntologyMap ontology = new OntologyMap();
            MappingHandlers mapping = new MappingHandlers();

            hooks.Register();
            handlers.Register();
            mapping.Register();
            ontology.Register();
        }
    }
}