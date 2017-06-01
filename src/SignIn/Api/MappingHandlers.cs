using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
