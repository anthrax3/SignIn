using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using Starcounter;
using Simplified.Ring3;

namespace SignIn {
    class Program {
         static void Main() {
             CommitHooks hooks = new CommitHooks();
             MainHandlers handlers = new MainHandlers();

             hooks.Register();
             handlers.Register();
         }
    }
}
