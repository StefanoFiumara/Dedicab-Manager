using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace DedicabUtility.Client.Behaviors
{
    public static class ScaleToWindowSizeBehavior
    {
        #region ParentWindow

        public static readonly DependencyProperty ParentWindowProperty =
            DependencyProperty.RegisterAttached("ParentWindow",
                typeof(Window),
                typeof(ScaleToWindowSizeBehavior),
                new FrameworkPropertyMetadata(null, OnParentWindowChanged));

        public static void SetParentWindow(FrameworkElement element, Window value)
        {
            element.SetValue(ParentWindowProperty, value);
        }

        public static Window GetParentWindow(FrameworkElement element)
        {
            return (Window)element.GetValue(ParentWindowProperty);
        }

        private static void OnParentWindowChanged(DependencyObject target,
            DependencyPropertyChangedEventArgs e)
        {
            FrameworkElement mainElement = target as FrameworkElement;
            Window window = e.NewValue as Window;

            ScaleTransform scaleTransform = new ScaleTransform();
            scaleTransform.CenterX = 0;
            scaleTransform.CenterY = 0;
            Binding scaleValueBinding = new Binding
            {
                Source = window,
                Path = new PropertyPath(ScaleValueProperty)
            };
            BindingOperations.SetBinding(scaleTransform, ScaleTransform.ScaleXProperty, scaleValueBinding);
            BindingOperations.SetBinding(scaleTransform, ScaleTransform.ScaleYProperty, scaleValueBinding);
            mainElement.LayoutTransform = scaleTransform;
            mainElement.SizeChanged += mainElement_SizeChanged;
        }

        #endregion // ParentWindow

        #region ScaleValue

        public static readonly DependencyProperty ScaleValueProperty =
            DependencyProperty.RegisterAttached("ScaleValue",
                typeof(double),
                typeof(ScaleToWindowSizeBehavior),
                new UIPropertyMetadata(1.0, OnScaleValueChanged, OnCoerceScaleValue));

        public static double GetScaleValue(DependencyObject target)
        {
            return (double)target.GetValue(ScaleValueProperty);
        }
        public static void SetScaleValue(DependencyObject target, double value)
        {
            target.SetValue(ScaleValueProperty, value);
        }

        private static void OnScaleValueChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
        }

        private static object OnCoerceScaleValue(DependencyObject d, object baseValue)
        {
            if (baseValue is double)
            {
                double value = (double)baseValue;
                if (double.IsNaN(value))
                {
                    return 1.0f;
                }
                value = Math.Max(0.1, value);
                return value;
            }
            return 1.0f;
        }

        private static void mainElement_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            FrameworkElement mainElement = sender as FrameworkElement;
            Window window = GetParentWindow(mainElement);
            Size baseResolution = GetResolution(mainElement);
            CalculateScale(window, baseResolution);
        }

        private static void CalculateScale(Window window, Size baseResolution)
        {
            double xScale = window.ActualWidth / baseResolution.Width;
            double yScale = window.ActualHeight / baseResolution.Height;
            double value = Math.Min(xScale, yScale);
            
            SetScaleValue(window, value);

            window.Height = window.Width * (baseResolution.Height / baseResolution.Width);
        }

        #endregion // ScaleValue

        #region Base Resolution

        public static readonly DependencyProperty ResolutionProperty =
            DependencyProperty.RegisterAttached("Resolution",
                typeof(Size),
                typeof(ScaleToWindowSizeBehavior),
                new UIPropertyMetadata(new Size(1280, 720)));

        public static Size GetResolution(DependencyObject target)
        {
            return (Size)target.GetValue(ResolutionProperty);
        }
        public static void SetResolution(DependencyObject target, Size value)
        {
            target.SetValue(ResolutionProperty, value);
        }

        #endregion // Denominators
    }
}