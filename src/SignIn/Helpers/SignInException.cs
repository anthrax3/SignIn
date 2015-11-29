using System;

namespace SignIn {
    class SignInException : Exception {
        public SignInException() {
        }

        public SignInException(string message) : base(message) {
        }

        public SignInException(string message, Exception inner) : base(message, inner) {
        }
    }
}
