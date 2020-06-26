using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Shell;


namespace Kebler.UI.CSControls.Window
{
    [ContentProperty(nameof(Content))]
    [TemplatePart(Name = PartNameMinimizeButton, Type = typeof(FrameworkElement))]
    [TemplatePart(Name = PartNameMaximizeButton, Type = typeof(FrameworkElement))]
    [TemplatePart(Name = PartNameRestoreButton, Type = typeof(FrameworkElement))]
    [TemplatePart(Name = PartNameCloseButton, Type = typeof(FrameworkElement))]
    [TemplatePart(Name = PartNameWindowHeader, Type = typeof(FrameworkElement))]
    public class CustomizableWindow : System.Windows.Window
    {

        public static readonly DependencyProperty HeaderHeightProperty = DependencyProperty.Register(nameof(HeaderHeight), typeof(double), typeof(CustomizableWindow), new PropertyMetadata(22.0));
        public static readonly DependencyProperty FullScreenProperty = DependencyProperty.Register(nameof(FullScreen), typeof(bool), typeof(CustomizableWindow), new PropertyMetadata(false, OnFullScreenChanged));
        public static readonly DependencyProperty HeaderVisibilityProperty = DependencyProperty.Register(nameof(HeaderVisibility), typeof(Visibility), typeof(CustomizableWindow), new PropertyMetadata(Visibility.Visible));
        public static readonly DependencyProperty HideMinimizeMaximizeButtonsProperty = DependencyProperty.Register(nameof(HideMinimizeMaximizeButtons), typeof(bool), typeof(CustomizableWindow), new PropertyMetadata(false));
        public static readonly DependencyProperty IsTitleVisibleProperty = DependencyProperty.Register(nameof(IsTitleVisible), typeof(bool), typeof(CustomizableWindow), new PropertyMetadata(false));
        public static readonly DependencyProperty ShowHeaderLineProperty = DependencyProperty.Register(nameof(ShowHeaderLine), typeof(bool), typeof(CustomizableWindow), new PropertyMetadata(false));

        protected const string PartNameWindowHeader = "PART_WindowHeader";
        protected const string PartNameCloseButton = "PART_CloseButton";
        protected const string PartNameRestoreButton = "PART_RestoreButton";
        protected const string PartNameMinimizeButton = "PART_MinimizeButton";
        protected const string PartNameMaximizeButton = "PART_MaximizeButton";
        private FrameworkElement _templatePartWindowHeader;
        private Button _closeButton;
        private Button _minimizeButton;
        private Button _maximizeButton;
        private Button _restoreButton;
        private readonly Thickness _padding;
        private Thickness _borderThickness;
        private double _tempHeaderHeight;
        private WindowState _tempWindowState;
        private bool _fullScreen;



        public double HeaderHeight
        {
            get => (double)GetValue(HeaderHeightProperty);
            private set => SetValue(HeaderHeightProperty, value);
        }

        public bool FullScreen
        {
            get => (bool)GetValue(FullScreenProperty);
            set => SetValue(FullScreenProperty, value);
        }

        public bool ShowHeaderLine
        {
            get => (bool)GetValue(ShowHeaderLineProperty);
            set => SetValue(ShowHeaderLineProperty, value);
        }

        public Visibility HeaderVisibility
        {
            get => (Visibility)GetValue(HeaderVisibilityProperty);
            set => SetValue(HeaderVisibilityProperty, value);
        }

        public bool HideMinimizeMaximizeButtons
        {
            get => (bool)GetValue(HideMinimizeMaximizeButtonsProperty);
            set => SetValue(HideMinimizeMaximizeButtonsProperty, value);
        }

        public bool IsTitleVisible
        {
            get => (bool)GetValue(IsTitleVisibleProperty);
            set => SetValue(IsTitleVisibleProperty, value);
        }

        private static Thickness WindowMaximizedPadding => new Thickness(WindowLocationStateExtensions.AutoHideEnabled() ? 0.0 : 8);

        static CustomizableWindow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomizableWindow), new FrameworkPropertyMetadata(typeof(CustomizableWindow)));
        }

        public CustomizableWindow() : base()
        {
            SetResourceReference(StyleProperty, typeof(CustomizableWindow));
            var chrome = new WindowChrome()
            {
                CornerRadius = new CornerRadius(),
                GlassFrameThickness = new Thickness(0.0, 0.0, 0.0, 1.0),
                UseAeroCaptionButtons = false,
                ResizeBorderThickness = new Thickness(5),

            };
            BindingOperations.SetBinding(chrome, WindowChrome.CaptionHeightProperty, new Binding(nameof(HeaderHeight))
            {
                Source = this
            });
            WindowChrome.SetWindowChrome(this, chrome);
            _padding = Padding;
            Loaded += Onload;
        }


        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);
            if (SizeToContent != SizeToContent.WidthAndHeight)
                return;
            InvalidateMeasure();
        }

        protected override void OnStateChanged(EventArgs e)
        {
            base.OnStateChanged(e);
            AdjustButtonsVisibilityToWindowState();
            if (WindowState == WindowState.Maximized)
            {
                Padding = WindowMaximizedPadding;
                BorderThickness = new Thickness();
                _tempHeaderHeight = HeaderHeight;
                HeaderHeight += 8.0;
            }
            else
            {
                Padding = _padding;
                BorderThickness = _borderThickness;
                HeaderHeight = _tempHeaderHeight;
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _templatePartWindowHeader = GetTemplateChild("PART_WindowHeader") as FrameworkElement;
            _closeButton = GetTemplateChild("PART_CloseButton") as Button;
            _minimizeButton = GetTemplateChild("PART_MinimizeButton") as Button;
            _maximizeButton = GetTemplateChild("PART_MaximizeButton") as Button;
            _restoreButton = GetTemplateChild("PART_RestoreButton") as Button;
            AdjustButtonsVisibilityToWindowState();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            HwndSource.FromHwnd(new WindowInteropHelper(this).EnsureHandle()).AddHook(HwndSourceHook);
            base.OnSourceInitialized(e);
        }

        public virtual void Onload(object sender1, RoutedEventArgs e1)
        {
            CommandBindings.Add(new CommandBinding(SystemCommands.MinimizeWindowCommand, (sender2, e2) => WindowState = WindowState.Minimized));
            CommandBindings.Add(new CommandBinding(SystemCommands.MaximizeWindowCommand, (sender2, e2) => WindowState = WindowState.Maximized));
            CommandBindings.Add(new CommandBinding(SystemCommands.RestoreWindowCommand, (sender2, e2) => WindowState = WindowState.Normal));
            CommandBindings.Add(new CommandBinding(SystemCommands.CloseWindowCommand, (sender2, e2) => Close()));
            _borderThickness = BorderThickness;
            _tempHeaderHeight = HeaderHeight;
            if (WindowState == WindowState.Maximized)
            {
                BorderThickness = new Thickness();
                _tempHeaderHeight += 8.0;
            }
            _tempWindowState = WindowState;
            SwitchIsFullScreen(_fullScreen);
            if (WindowState != WindowState.Maximized)
                return;
            _tempHeaderHeight -= 8.0;
        }

        private void AdjustButtonsVisibilityToWindowState()
        {
            if (HideMinimizeMaximizeButtons)
            {
                _minimizeButton.Collapse();
                _maximizeButton.Collapse();
                _restoreButton.Collapse();
            }
            else
            {
                switch (WindowState)
                {
                    case WindowState.Normal:
                        _maximizeButton.Show();
                        _restoreButton.Collapse();
                        break;
                    case WindowState.Maximized:
                        _maximizeButton.Collapse();
                        _restoreButton.Show();
                        break;
                }
                switch (ResizeMode)
                {
                    case ResizeMode.NoResize:
                        _minimizeButton.Collapse();
                        _maximizeButton.Collapse();
                        _restoreButton.Collapse();
                        break;
                    case ResizeMode.CanMinimize:
                        _maximizeButton.Collapse();
                        _restoreButton.Collapse();
                        break;
                }
            }
        }

        private IntPtr HwndSourceHook(
          IntPtr hwnd,
          int msg,
          IntPtr wparam,
          IntPtr lparam,
          ref bool handled)
        {
            switch (msg)
            {
                case 36:
                    WindowLocationStateExtensions.GetMinMaxInfo(hwnd, lparam);
                    Padding = WindowState == WindowState.Maximized ? WindowMaximizedPadding : _padding;
                    handled = true;
                    break;
                case 71:
                    Padding = WindowState == WindowState.Maximized ? WindowMaximizedPadding : _padding;
                    break;
            }
            return IntPtr.Zero;
        }

        private static void OnFullScreenChanged(
          DependencyObject d,
          DependencyPropertyChangedEventArgs e)
        {
            ((CustomizableWindow)d).SwitchIsFullScreen((bool)e.NewValue);
        }

        private void SwitchIsFullScreen(bool isFullScreen)
        {
            if (_templatePartWindowHeader == null)
                _fullScreen = isFullScreen;
            else if (isFullScreen)
            {
                _tempHeaderHeight = HeaderHeight;
                HeaderHeight = 0.0;
                WindowStyle = WindowStyle.None;
                _tempWindowState = WindowState;
                WindowState = WindowState.Maximized;
                WindowState = WindowState.Minimized;
                WindowState = WindowState.Maximized;
            }
            else
            {
                HeaderHeight = _tempHeaderHeight;
                WindowState = _tempWindowState;
            }
        }
    }
}
