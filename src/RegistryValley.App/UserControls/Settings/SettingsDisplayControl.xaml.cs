using RegistryValley.App.Models;
using RegistryValley.App.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;

namespace RegistryValley.App.UserControls.Settings
{
    [ContentProperty(Name = nameof(SettingsActionableElement))]
    public sealed partial class SettingsDisplayControl : UserControl
    {
        #region propdp
        public FrameworkElement SettingsActionableElement { get; set; }

        public static readonly DependencyProperty AdditionalDescriptionContentProperty =
            DependencyProperty.Register(
                nameof(AdditionalDescriptionContent),
                typeof(FrameworkElement),
                typeof(SettingsDisplayControl),
                new PropertyMetadata(null)
                );

        public FrameworkElement AdditionalDescriptionContent
        {
            get => (FrameworkElement)GetValue(AdditionalDescriptionContentProperty);
            set => SetValue(AdditionalDescriptionContentProperty, value);
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(
                nameof(Title),
                typeof(string),
                typeof(SettingsDisplayControl),
                new PropertyMetadata(null)
                );

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register(
                nameof(Description),
                typeof(string),
                typeof(SettingsDisplayControl),
                new PropertyMetadata(null)
                );

        public string Description
        {
            get => (string)GetValue(DescriptionProperty);
            set => SetValue(DescriptionProperty, value);
        }

        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register(
                nameof(Icon),
                typeof(IconElement),
                typeof(SettingsDisplayControl),
                new PropertyMetadata(null)
                );

        public IconElement Icon
        {
            get => (IconElement)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }
        #endregion

        public SettingsDisplayControl()
        {
            InitializeComponent();

            VisualStateManager.GoToState(this, "NormalState", false);
        }

        private void MainPanel_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width == e.PreviousSize.Width)
                return;

            if (ActionableElement.ActualWidth > e.NewSize.Width / 3)
            {
                VisualStateManager.GoToState(this, "CompactState", false);
            }
            else
            {
                VisualStateManager.GoToState(this, "NormalState", false);
            }
        }
    }
}