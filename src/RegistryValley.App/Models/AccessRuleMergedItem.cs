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
            get => HasFlag(ACCESS_MASK.GENERIC_ALL, true);
            set => ToggleFlag(ACCESS_MASK.GENERIC_ALL, true, value);
        }

        public bool AllowRead
        {
            get => HasFlag(ACCESS_MASK.GENERIC_READ, true);
            set => ToggleFlag(ACCESS_MASK.GENERIC_READ, true, value);
        }

        public bool AllowSpecialPermissions
        {
            get => HasAllSpecialPermissions(true);
            set => ToggleFlag(ACCESS_MASK.GENERIC_ALL, true, value);
        }

        public bool DenyFullControl
        {
            get => HasFlag(ACCESS_MASK.GENERIC_ALL, false);
            set => ToggleFlag(ACCESS_MASK.GENERIC_ALL, false, value);
        }

        public bool DenyRead
        {
            get => HasFlag(ACCESS_MASK.GENERIC_READ, false);
            set => ToggleFlag(ACCESS_MASK.GENERIC_READ, false, value);
        }

        public bool DenySpecialPermissions
        {
            get => HasAllSpecialPermissions(false);
            set => ToggleFlag(ACCESS_MASK.GENERIC_ALL, false, value);
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
