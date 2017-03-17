using System.Net;
using Smorgasbord.PropertyMetadata;

namespace SignIn
{
    [SettingsPage_json]
    partial class SettingsPage : PropertyMetadataPage
    {
        void Handle(Input.SitePort action)
        {
            this.AssurePropertyMetadata_SitePort(action.Template.TemplateName, action.Value);
        }

        void Handle(Input.SiteHost action)
        {
            this.AssurePropertyMetadata_SiteHost(action.Template.TemplateName, action.Value);
        }

        void Handle(Input.Port action)
        {
            this.AssurePropertyMetadata_SitePort(action.Template.TemplateName, action.Value);
        }

        void Handle(Input.Save action)
        {
            this.AssurePropertiesMetadata();

            if (this.IsInvalid)
            {
                return;
            }

            if (this.Transaction.IsDirty)
            {
                this.Transaction.Commit();
            }

            this.SuccessMessage = "Changes saved";
        }

        void Handle(Input.Close action)
        {
            if (this.Transaction.IsDirty)
            {
                this.Transaction.Rollback();
            }

            this.RedirectUrl = "/signin/profile";
        }

        #region Validate Properties (Create Property metadata)

        /// <summary>
        /// Assure metadata for all fields
        /// </summary>
        virtual protected void AssurePropertiesMetadata()
        {
            AssurePropertyMetadata_SiteHost("SiteHost$", this.SiteHost);
            AssurePropertyMetadata_SitePort("SitePort$", this.SitePort);
            AssurePropertyMetadata_SitePort("Port$", this.Port);
        }

        protected void AssurePropertyMetadata_SiteHost(string propertyName, string value)
        {
            string message;
            this.Validate_SiteHost(value, out message);

            if (message != null)
            {
                PropertyMetadataItem item = this.CreatePropertyMetadata(propertyName, message);
                this.AddPropertyFeedback(item);
            }
            else
            {
                this.RemovePropertyFeedback(propertyName);
            }
        }

        protected void AssurePropertyMetadata_SitePort(string propertyName, long value)
        {
            string message;
            this.Validate_SitePort(value, out message);

            if (message != null)
            {
                PropertyMetadataItem item = this.CreatePropertyMetadata(propertyName, message);
                this.AddPropertyFeedback(item);
            }
            else
            {
                this.RemovePropertyFeedback(propertyName);
            }
        }

        private PropertyMetadataItem CreatePropertyMetadata(string propertyName, string message)
        {
            PropertyMetadataItem item = new PropertyMetadataItem();
            item.Message = message;
            item.ErrorLevel = 1;
            item.PropertyName = propertyName;
            return item;
        }

        #endregion

        #region Validate Properties

        /// <summary>
        /// Validate name
        /// </summary>
        /// <param name="value"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        private bool Validate_SitePort(long value, out string message)
        {
            message = null;

            if (value > IPEndPoint.MaxPort || (value < IPEndPoint.MinPort))
            {
                message = "Invalid port number";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Validate email
        /// </summary>
        /// <param name="value"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        private bool Validate_SiteHost(string value, out string message)
        {
            message = null;

            if (string.IsNullOrEmpty(value))
            {
                message = "Invalid Site name";
                return false;
            }
            return true;
        }

        #endregion
    }
}
