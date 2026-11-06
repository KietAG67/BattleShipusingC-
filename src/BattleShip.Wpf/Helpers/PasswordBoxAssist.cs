using System.Windows;
using System.Windows.Controls;

namespace BattleShip.Wpf.Helpers
{
    
    public static class PasswordBoxAssist
    {
        public static readonly DependencyProperty AttachProperty =
            DependencyProperty.RegisterAttached("Attach", typeof(bool), typeof(PasswordBoxAssist),
                new PropertyMetadata(false, OnAttachChanged));

        public static readonly DependencyProperty BoundPasswordProperty =
            DependencyProperty.RegisterAttached("BoundPassword", typeof(string), typeof(PasswordBoxAssist),
                new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnBoundPasswordChanged));

        private static readonly DependencyProperty IsUpdatingProperty =
            DependencyProperty.RegisterAttached("IsUpdating", typeof(bool), typeof(PasswordBoxAssist),
                new PropertyMetadata(false));

        public static bool GetAttach(DependencyObject d) => (bool)d.GetValue(AttachProperty);

        public static void SetAttach(DependencyObject d, bool value) => d.SetValue(AttachProperty, value);

        public static string GetBoundPassword(DependencyObject d) => (string)d.GetValue(BoundPasswordProperty);

        public static void SetBoundPassword(DependencyObject d, string value) => d.SetValue(BoundPasswordProperty, value);

        private static void OnAttachChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not PasswordBox box)
            {
                return;
            }

            if ((bool)e.OldValue)
            {
                box.PasswordChanged -= OnPasswordChanged;
            }

            if ((bool)e.NewValue)
            {
                box.PasswordChanged += OnPasswordChanged;
            }
        }

        private static void OnBoundPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not PasswordBox box || (bool)box.GetValue(IsUpdatingProperty))
            {
                return;
            }

            var newValue = e.NewValue as string ?? string.Empty;
            if (box.Password != newValue)
            {
                box.Password = newValue;
            }
        }

        private static void OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            var box = (PasswordBox)sender;
            box.SetValue(IsUpdatingProperty, true);
            SetBoundPassword(box, box.Password);
            box.SetValue(IsUpdatingProperty, false);
        }
    }
}
