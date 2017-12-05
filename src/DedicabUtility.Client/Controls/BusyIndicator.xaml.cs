using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;

namespace DedicabUtility.Client.Controls
{
    [ContentProperty(nameof(Children))]
    public partial class BusyIndicator : UserControl
    {
        public static readonly DependencyPropertyKey ChildrenProperty = DependencyProperty.RegisterReadOnly(
            nameof(Children),  
            typeof(UIElementCollection),
            typeof(BusyIndicator),
            new PropertyMetadata());

        public UIElementCollection Children
        {
            get => (UIElementCollection)GetValue(ChildrenProperty.DependencyProperty);
            private set => SetValue(ChildrenProperty, value);
        }

        public BusyIndicator()
        {
            InitializeComponent();
            Children = ChildrenContainer.Children;
        }

        #region IsBusy Dependency Property

        public static readonly DependencyProperty IsBusyProperty =
            DependencyProperty.RegisterAttached("IsBusy", typeof(bool), typeof(BusyIndicator));

        public bool IsBusy
        {
            get => (bool)GetValue(IsBusyProperty);
            set => SetValue(IsBusyProperty, value);
        }

        #endregion

        #region BusyText Dependency Property

        public static readonly DependencyProperty BusyTextProperty =
            DependencyProperty.RegisterAttached("BusyText", typeof(string), typeof(BusyIndicator), 
                new PropertyMetadata("Loading..."));

        public string BusyText
        {
            get => (string) GetValue(BusyTextProperty);
            set => SetValue(BusyTextProperty, value);
        }
        #endregion

        #region WheelColor Dependency Property

        public static readonly DependencyProperty WheelColorProperty =
            DependencyProperty.RegisterAttached("WheelColor", typeof(Color), typeof(BusyIndicator), new PropertyMetadata(Colors.Gold, OnWheelColorPropertyChanged));

        public Color WheelColor
        {
            get => (Color)GetValue(WheelColorProperty);
            set => SetValue(WheelColorProperty, value);
        }
        
        private static void OnWheelColorPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var owner = (BusyIndicator)dependencyObject;

            owner.SpinningWheel.CircleColor = (Color)e.NewValue;
        }

        #endregion
    }
}
