namespace RegistryValley.App.Models
{
    public class AccessRuleMergedItem : ObservableObject
    {
        public ACCESS_MASK MaskAllowed { get; set; }
        public ACCESS_MASK MaskDenied { get; set; }

        private bool _isInheritedDeniedMask;
        public bool IsInheritedDeniedMask { get => _isInheritedDeniedMask; set => SetProperty(ref _isInheritedDeniedMask, value); }

        private bool _isInheritedAllowedMask;
        public bool IsInheritedAllowedMask { get => _isInheritedAllowedMask; set => SetProperty(ref _isInheritedAllowedMask, value); }

        #region Permission Properties
        public bool AllowFullControl
        {
            get => HasFlag(ACCESS_MASK.STANDARD_RIGHTS_REQUIRED, true);
            set => ToggleFlag(ACCESS_MASK.STANDARD_RIGHTS_REQUIRED, true, value);
        }

        public bool AllowRead
        {
            get => HasFlag(ACCESS_MASK.READ_CONTROL, true);
            set => ToggleFlag(ACCESS_MASK.READ_CONTROL, true, value);
        }

        public bool AllowSpecialPermissions
        {
            get => HasAllSpecialPermissions(true);
            set => ToggleFlag(ACCESS_MASK.STANDARD_RIGHTS_REQUIRED, true, value);
        }

        public bool DenyFullControl
        {
            get => HasFlag(ACCESS_MASK.STANDARD_RIGHTS_REQUIRED, false);
            set => ToggleFlag(ACCESS_MASK.STANDARD_RIGHTS_REQUIRED, false, value);
        }

        public bool DenyRead
        {
            get => HasFlag(ACCESS_MASK.READ_CONTROL, false);
            set => ToggleFlag(ACCESS_MASK.READ_CONTROL, false, value);
        }

        public bool DenySpecialPermissions
        {
            get => HasAllSpecialPermissions(false);
            set => ToggleFlag(ACCESS_MASK.STANDARD_RIGHTS_REQUIRED, false, value);
        }
        #endregion

        #region Methods
        private bool HasFlag(ACCESS_MASK target, bool allowedMask)
        {
            if (allowedMask)
                return (MaskAllowed & target) == target;
            else
                return (MaskDenied & target) == target;
        }

        private bool HasAllSpecialPermissions(bool allowedMask)
        {
            // TODO
            return false;
        }

        private void ToggleFlag(ACCESS_MASK target, bool allowedMask, bool value)
        {
            // TODO
        }
        #endregion
    }
}
