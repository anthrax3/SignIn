using Starcounter;
using System;

namespace SignInApp.Server {

    [SignInUser_json]
    partial class SignInUser : Json {

        void Handle(Input.UserID action) {

            // TODO:
            Console.WriteLine("TODO: UserID action");
        }

        void Handle(Input.SignIn action) {

            // TODO:
            Console.WriteLine("TODO: SignIn action");

            // Successfull sign-in

            // Redirect to page that initiated this request
            // At the moment just redirect to root.
            this.RedirectToPage("/");

            // Close this page
        }

        /// <summary>
        /// Redirect to page
        /// </summary>
        /// <param name="url"></param>
        private void RedirectToPage(string url) {

            // TODO: Redirect to page
        }

        #region Base
        // Browsers will ask for "text/html" and we will give it to them
        // by loading the contents of the URI in our Html property
        public override string AsMimeType(MimeType type) {
            if (type == MimeType.Text_Html) {
                return X.GET<string>(Html);
            }
            return base.AsMimeType(type);
        }

        /// <summary>
        /// The way to get a URL for HTML partial if any.
        /// </summary>
        /// <returns></returns>
        public override string GetHtmlPartialUrl() {
            return Html;
        }

        /// <summary>
        /// Whenever we set a bound data object to this page, we update the
        /// URI property on this page.
        /// </summary>
        protected override void OnData() {
            base.OnData();
            var str = "";
            Json x = this;
            while (x != null) {
                if (x is SignInUser)
                    str = (x as SignInUser).UriFragment + str;
                x = x.Parent;
            }
            Uri = str;
        }

        /// <summary>
        /// Override to provide an URI fragment
        /// </summary>
        /// <returns></returns>
        protected virtual string UriFragment {
            get {
                return "";
            }
        }
        #endregion

    }
}
