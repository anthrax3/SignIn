using Starcounter;

namespace SignIn.Api
{
    internal class MappingHandlers
    {
        public void Register()
        {
            Handle.GET("/signin/app-name", () => new AppName());
        }
    }
}
