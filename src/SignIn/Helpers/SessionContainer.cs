using System;
using Starcounter;

namespace SignIn {
    internal class SessionContainer : Json {
        public SignInFormPage SignInForm {
            get;
            set;
        }

        public SignInPage SignIn {
            get;
            set;
        } 
    }
}
