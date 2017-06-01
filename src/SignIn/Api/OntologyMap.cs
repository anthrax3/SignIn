using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Starcounter;

namespace SignIn.Api
{
    internal class OntologyMap
    {
        public void Register()
        {
            Blender.MapUri("/signin/app-name", "app-name");
        }
    }
}
