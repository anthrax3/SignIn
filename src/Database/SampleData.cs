using Concepts.Ring3;
using Starcounter;
using System;
namespace SignInApp.Database {
    public class SampleData {

        static internal void Init() {

            // Clean database

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

            //Concepts.Ring1.Person person = Db.SQL<Concepts.Ring1.Person>("SELECT p FROM Concepts.Ring1.Person p WHERE p.FirstName=? AND p.Surname=?", "Albert", "Einstein").First;
            //if (person != null) {
            //    Db.Transaction(() => {
            //        person.Delete();
            //    });
            //}
            //Concepts.Ring1.Person person2 = Db.SQL<Concepts.Ring1.Person>("SELECT p FROM Concepts.Ring1.Person p WHERE p.FirstName=? AND p.Surname=?", "Stephen", "Hawking").First;
            //if (person2 != null) {
            //    Db.Transaction(() => {
            //        person2.Delete();
            //    });
            //}

            //SystemUser systemUser = Db.SQL<SystemUser>("SELECT o FROM Concepts.Ring3.SystemUser o WHERE o.Username=?", "albert").First;
            //if (systemUser != null) {
            //    Db.Transaction(() => {
            //        systemUser.Delete();
            //    });
            //}
            //SystemUser systemUser2 = Db.SQL<SystemUser>("SELECT o FROM Concepts.Ring3.SystemUser o WHERE o.Username=?", "demo").First;
            //if (systemUser2 != null) {
            //    Db.Transaction(() => {
            //        systemUser2.Delete();
            //    });
            //}

            // Create basic database objects
            Concepts.Ring1.Person person = null;

            Db.Transaction(() => {
                person = new Concepts.Ring1.Person() { FirstName = "Albert", Surname = "Einstein" };
                //person2 = new Concepts.Ring1.Person() { FirstName = "Stephen", Surname = "Hawking" };

            });

            Db.Transaction(() => {
                new SystemUser(person) { Name = "albert", Password = "albert" };
                new SystemUser() { Name = "demo", Password = "demo" };
            });
        }
    }
}
