using Concepts.Ring1;
using Concepts.Ring2;
using Concepts.Ring3;
using SignInApp.Server;
using Starcounter;
using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
namespace SignInApp.Database {
    public class SampleData {

        static internal void Init() {

            // ** BUGGWORKAROUND **
            // Assure that assemblies is loaded

            var transaction = new Transaction(() => {
                Concepts.Ring1.Address b = new Concepts.Ring1.Address();
                Concepts.Ring2.AffiliatedOrganisation c = new Concepts.Ring2.AffiliatedOrganisation();
                Concepts.Ring3.ApplicationConfiguration d = new ApplicationConfiguration();
                Concepts.Ring4.ProjectEvent e = new Concepts.Ring4.ProjectEvent();
                Concepts.Ring5.SystemUserSession a = new Concepts.Ring5.SystemUserSession();
                return;
            });
            transaction.Rollback();


            Starcounter.Handle.GET("/signinapp/reset", (Request request) => {

                // Clean database
                ClearDatabase();
                return 200;
            });
        }

        static void ClearDatabase() {

            Db.Transaction(() => {

                var result = Db.SQL("SELECT o FROM Concepts.Ring5.SystemUserSession o");
                foreach (var item in result) {
                    item.Delete();
                }
            });

            Db.Transaction(() => {
                var result = Db.SQL<Concepts.Ring1.Person>("SELECT p FROM Concepts.Ring1.Person p");
                foreach (var item in result) {
                    item.Delete();
                }
            });

            Db.Transaction(() => {
                var result = Db.SQL<Concepts.Ring2.Company>("SELECT p FROM Concepts.Ring2.Company p");
                foreach (var item in result) {
                    item.Delete();
                }
            });

            Db.Transaction(() => {
                var result = Db.SQL<Concepts.Ring2.EMailAddress>("SELECT o FROM Concepts.Ring2.EMailAddress o");
                foreach (var item in result) {
                    item.Delete();
                }
            });

            Db.Transaction(() => {
                var result = Db.SQL<Concepts.Ring3.SystemUser>("SELECT u FROM Concepts.Ring3.SystemUser u");
                foreach (var item in result) {
                    item.Delete();
                }
            });

            Db.Transaction(() => {
                var result = Db.SQL<Concepts.Ring5.SystemUserTokenKey>("SELECT o FROM Concepts.Ring5.SystemUserTokenKey o");
                foreach (var item in result) {
                    item.Delete();
                }
            });
        }



    }
}
