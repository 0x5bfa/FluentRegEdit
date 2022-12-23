namespace RegistryValley.App.Models
{
    public class AccessRuleAdvancedItem : ObservableObject
    {
        #region Basic Permission Properties
        private bool _allowFullControl;
        public bool AllowFullControl { get => _allowFullControl; set => SetProperty(ref _allowFullControl, value); }

        private bool _allowRead;
        public bool AllowRead { get => _allowRead; set => SetProperty(ref _allowRead, value); }

        private bool _allowSpecialPermissions;
        public bool AllowSpecialPermissions { get => _allowSpecialPermissions; set => SetProperty(ref _allowSpecialPermissions, value); }
        #endregion

        #region Advanced Permission Properties
        #endregion

        #region Methods
        #endregion
    }
}
