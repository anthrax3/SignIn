using Simplified.Ring3;
using Smorgasbord.PropertyMetadata;
using Starcounter;

namespace SignIn.ViewModels
{
    partial class SetPasswordPage : PropertyMetadataPage, IBound<SystemUser>
    {
        protected override void OnData()
        {
            base.OnData();
            //this.SessionUri = Session.Current.SessionUri;
        }
    }
}
