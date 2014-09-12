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

                AddSampleSombodies();
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

        /// <summary>
        /// Add some sample users
        /// </summary>
        static void AddSampleSombodies() {

            // Somebody
            //    Group
            //       Everybody
            //    LegalEntity
            //        Person
            //        Organisation
            //            Company

            AddPerson("demo", "demo", "demo", "demo@demo.com", "demo");
            AddPerson("Anders", "Wahlgren", "andwah", "anders.wahlgren@mydomain.com", "demo");
            AddPerson("Albert", "Einstein", "albein", "albert.einstein@mydomain.com", "demo");

            AddCompany("Starcounter AB", "starcounter", "Starcounter@mydomain.com", "demo");

        }

        /// <summary>
        /// Add Person with a system user
        /// </summary>
        /// <param name="firstName"></param>
        /// <param name="surname"></param>
        /// <param name="email"></param>
        public static void AddPerson(string firstName, string surname, string username, string email, string password) {

            if (firstName == null) {
                throw new ArgumentNullException("firstname");
            }

            if (surname == null) {
                throw new ArgumentNullException("surname");
            }

            if (username == null) {
                throw new ArgumentNullException("username");
            }

            if (email == null) {
                throw new ArgumentNullException("email");
            }

            if (string.IsNullOrEmpty(firstName)) {
                throw new ArgumentException("firstname");
            }

            if (string.IsNullOrEmpty(surname)) {
                throw new ArgumentException("surname");
            }

            if (string.IsNullOrEmpty(email)) {
                throw new ArgumentException("email");
            }

            // Validation

            // Check for duplicated email
            string emailLow = email.ToLowerInvariant();

            if (!Utils.IsValidEmail(email)) {
                throw new ArgumentException("email", "Invalid email address");
            }
            var dupEmail = Db.SQL<Concepts.Ring2.EMailAddress>("SELECT o FROM Concepts.Ring2.EMailAddress o WHERE o.EMail=?", emailLow).First;
            if (dupEmail != null) {
                throw new ArgumentException("username", "Duplicated email");
            }

            // Check for duplicated username
            var dupUserName = Db.SQL<Concepts.Ring3.SystemUser>("SELECT o FROM Concepts.Ring3.SystemUser o WHERE o.Username=?", username).First;
            if (dupUserName != null) {
                throw new ArgumentException("username", "Duplicated username");
            }

            Db.Transaction(() => {

                Person person = new Person() { FirstName = firstName, Surname = surname };
                Concepts.Ring3.SystemUser systemUser = new Concepts.Ring3.SystemUser(person);
                systemUser.Username = username;
                string hashedPassword;
                Concepts.Ring5.SystemUserPassword.GeneratePasswordHash(systemUser.Username, password, out hashedPassword);
                systemUser.Password = hashedPassword;

                // Add ability to also sign in with email
                EMailAddress emailRel = new EMailAddress();
                emailRel.SetToWhat(systemUser);
                emailRel.EMail = emailLow.ToLowerInvariant();

                emailRel = new EMailAddress();
                emailRel.SetToWhat(person);
                emailRel.EMail = emailLow.ToLowerInvariant();
                person.ImageURL = Utils.GetGravatarUrl(emailRel.EMail);
            });
        }

        /// <summary>
        /// Add company with a system user
        /// </summary>
        /// <param name="name"></param>
        /// <param name="email"></param>
        public static void AddCompany(string name, string username, string email, string password) {

            if (name == null) {
                throw new ArgumentNullException("name");
            }

            if (username == null) {
                throw new ArgumentNullException("username");
            }

            if (email == null) {
                throw new ArgumentNullException("email");
            }

            if (string.IsNullOrEmpty(name)) {
                throw new ArgumentException("name");
            }

            if (string.IsNullOrEmpty(email)) {
                throw new ArgumentException("email");
            }

            // Check for duplicated email
            string emailLow = email.ToLowerInvariant();

            if (!Utils.IsValidEmail(email)) {
                throw new ArgumentException("email", "Invalid email address");
            }
            var dupEmail = Db.SQL<Concepts.Ring2.EMailAddress>("SELECT o FROM Concepts.Ring2.EMailAddress o WHERE o.EMail=?", emailLow).First;
            if (dupEmail != null) {
                throw new ArgumentException("username", "Duplicated email");
            }

            // Check for duplicated username
            var dupUserName = Db.SQL<Concepts.Ring3.SystemUser>("SELECT o FROM Concepts.Ring3.SystemUser o WHERE o.Username=?", username).First;
            if (dupUserName != null) {
                throw new ArgumentException("username", "Duplicated username");
            }


            Db.Transaction(() => {
                Company company = new Company() { Name = name };

                Concepts.Ring3.SystemUser systemUser = new Concepts.Ring3.SystemUser(company);
                systemUser.Username = username;
                string hashedPassword;
                Concepts.Ring5.SystemUserPassword.GeneratePasswordHash(systemUser.Username, password, out hashedPassword);
                systemUser.Password = hashedPassword;

                // Add ability to also sign in with email
                EMailAddress emailRel = new EMailAddress();
                emailRel.SetToWhat(systemUser);
                emailRel.EMail = emailLow;

                emailRel = new EMailAddress();
                emailRel.SetToWhat(company);
                emailRel.EMail = emailLow;
                company.ImageURL = Utils.GetGravatarUrl(emailRel.EMail);
            });
        }


    }
}
