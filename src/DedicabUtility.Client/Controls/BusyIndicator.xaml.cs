using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;

namespace DedicabUtility.Client.Controls
{
    [ContentProperty(nameof(BusyIndicator.Children))]
    public partial class BusyIndicator : UserControl
    {
        public static readonly DependencyPropertyKey ChildrenProperty = DependencyProperty.RegisterReadOnly(
            nameof(BusyIndicator.Children),  
            typeof(UIElementCollection),
            typeof(BusyIndicator),
            new PropertyMetadata());

        public UIElementCollection Children
        {
            get => (UIElementCollection)this.GetValue(BusyIndicator.ChildrenProperty.DependencyProperty);
            private set => this.SetValue(BusyIndicator.ChildrenProperty, value);
        }

        public BusyIndicator()
        {
            InitializeComponent();
            this.Children = this.ChildrenContainer.Children;
        }

        #region IsBusy Dependency Property

        public static readonly DependencyProperty IsBusyProperty =
            DependencyProperty.RegisterAttached("IsBusy", typeof(bool), typeof(BusyIndicator));

        public bool IsBusy
        {
            get => (bool)this.GetValue(BusyIndicator.IsBusyProperty);
            set => this.SetValue(BusyIndicator.IsBusyProperty, value);
        }

        #endregion

        #region BusyText Dependency Property

        public static readonly DependencyProperty BusyTextProperty =
            DependencyProperty.RegisterAttached("BusyText", typeof(string), typeof(BusyIndicator), 
                new PropertyMetadata("Loading..."));

        public string BusyText
        {
            get => (string) this.GetValue(BusyIndicator.BusyTextProperty);
            set => this.SetValue(BusyIndicator.BusyTextProperty, value);
        }
        #endregion

        #region WheelColor Dependency Property

        public static readonly DependencyProperty WheelColorProperty =
            DependencyProperty.RegisterAttached("WheelColor", typeof(Color), typeof(BusyIndicator), new PropertyMetadata(Colors.Gold, BusyIndicator.OnWheelColorPropertyChanged));

        public Color WheelColor
        {
            get => (Color)this.GetValue(BusyIndicator.WheelColorProperty);
            set => this.SetValue(BusyIndicator.WheelColorProperty, value);
        }
        
        private static void OnWheelColorPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var owner = (BusyIndicator)dependencyObject;

            owner.SpinningWheel.CircleColor = (Color)e.NewValue;
        }

        #endregion
    }
}
