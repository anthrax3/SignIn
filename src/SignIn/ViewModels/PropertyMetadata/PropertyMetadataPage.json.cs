using Starcounter;
using System.Collections.Generic;

namespace Smorgasbord.PropertyMetadata {
    partial class PropertyMetadataPage : Json {

        protected override void OnData() {

            this.ClearAllPropertiesMetadata();

            base.OnData();
        }

        public bool IsDirty_ {
            get {
                return this.GetIsDiryFlag();
            }
        }

        public bool IsPristine_ {
            get {
                return !this.IsDirty_;
            }
        }

        public bool IsValid_ {
            get {

                foreach (var item in this.PropertyMetadataItems) {
                    if (item.ErrorLevel > 0) {
                        return false;
                    }
                }
                return true;
            }
        }

        public bool IsInvalid_ {
            get {
                return !this.IsValid_;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual bool GetIsDiryFlag() {

            if (this.Transaction != null) {
                return this.Transaction.IsDirty;
            }
            return false;
        }


        /// <summary>
        /// Sets the data to its pristine state.
        /// </summary>
        public void SetPristine() {

            if (this.Transaction != null && this.Transaction.IsDirty) {
                this.Transaction.Rollback();
                this.ClearAllPropertiesMetadata();
            }
        }

        public void AddPropertyFeedback(PropertyMetadataItem item) {

            // Assure only one property metadata/propertyname
            foreach (var obj in this.PropertyMetadataItems) {

                if (obj.PropertyName == item.PropertyName) {
                    obj.Readonly = item.Readonly;
                    obj.Visible = item.Visible;
                    obj.Enabled = item.Enabled;
                    obj.ErrorLevel = item.ErrorLevel;
                    obj.Message = item.Message;
                    return;
                }
            }

            this.PropertyMetadataItems.Add(item);
        }

        public void AddPropertyFeedback(string propertyName, int errorLevel, string message) {

            PropertyMetadataItem item = new PropertyMetadataItem();
            item.Message = message;
            item.ErrorLevel = errorLevel;
            item.PropertyName = propertyName;
            item.Readonly = false;
            item.Visible = true;
            item.Enabled = true;

            this.AddPropertyFeedback(item);
        }

        public void RemovePropertyFeedback(string propertyName) {

            List<PropertyMetadataItem> removeItems = new List<PropertyMetadataItem>();

            foreach (var item in this.PropertyMetadataItems) {

                if (item.PropertyName == propertyName) {
                    removeItems.Add(item);
                    // Note: Make sure that all item is removed
                }
            }

            foreach (var obj in removeItems) {
                this.PropertyMetadataItems.Remove(obj);
            }
        }

        private void ClearAllPropertiesMetadata() {

            while (this.PropertyMetadataItems.Count > 0) {
                this.PropertyMetadataItems.RemoveAt(0);
            }

            // There is a strange behaviour on the client (Array observe)
            //this.PropertyFeedbackItems.Items.Clear();
        }

        #region View-model Handlers

        /// <summary>
        /// Sets the data to its pristine state.
        /// </summary>
        /// <param name="action"></param>
        void Handle(Input.Rollback action) {
            this.SetPristine();
        }

        #endregion
    }
}
