namespace SignIn
{
    class Program
    {
        static void Main()
        {
            AuthorizationHelper.SetupPermissions();

            CommitHooks hooks = new CommitHooks();
            MainHandlers handlers = new MainHandlers();

            hooks.Register();
            handlers.Register();
        }
    }
}