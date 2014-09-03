using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Starcounter;
using Concepts.Ring3;

namespace SignInApp.Database {
    public class SampleData {

        static internal void Init() {

            // Clean database

            Db.Transaction(() => {

                var result = Db.SQL("SELECT o FROM SignInApp.Database.SystemUserSession o");
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
                var result = Db.SQL<SignInApp.Database.SystemUserTokenKey>("SELECT o FROM SignInApp.Database.SystemUserTokenKey o");
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
