using SignInApp.Database;
using Starcounter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignInApp.Server {
    public class Handlers {

        internal static void RegisterHandlers() {

            Starcounter.Handle.GET("/user", () => {
                var p = new SignIn() {
                    Html = "/signin.html",
                };
                return p;
            });

            Starcounter.Handle.GET("/menu", () => {
                var p = new SignIn() {
                    Html = "/signin.html",
                };
                return p;
            });
        }
    }
}
