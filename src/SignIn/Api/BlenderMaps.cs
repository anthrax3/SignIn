using Starcounter;

namespace SignIn.Api
{
    internal class BlenderMaps
    {
        public void Register()
        {
            Blender.MapUri("/signin/app-name", "app-name");
        }
    }
}
