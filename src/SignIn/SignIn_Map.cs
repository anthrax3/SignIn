using Starcounter;

namespace SignIn_Map
{
    class Program
    {
        static void Main()
        {
            Blender.MapUri("/signin/app-name", "app-name");
            Blender.MapUri("/signin/user", "user"); //expandable icon
            Blender.MapUri("/signin/signinuser", "userform"); //inline form
            Blender.MapUri("/signin/signinuser?{?}", "userform-return"); //inline form
            Blender.MapUri("/signin/admin/settings", "settings");
            Blender.MapUri("/signin/user/authentication/password/{?}", "authentication-password");
            Blender.MapUri("/signin/user/authentication/settings/{?}", "authentication-settings");
            Blender.MapUri("/signin/partial/user/image", "userimage-default");  // default user image
        }
    }
}