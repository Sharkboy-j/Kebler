using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;


namespace Kebler.UI.Controls
{
    [TemplatePart(Name = TitleBarTemplateName, Type = typeof(Border))]
    [TemplatePart(Name = MinButtonTemplateName, Type = typeof(Button))]
    [TemplatePart(Name = MaxButtonTemplateName, Type = typeof(Button))]
    [TemplatePart(Name = CloseButtonTemplateName, Type = typeof(Button))]
    [TemplatePart(Name = LnSizeNorthTemplateName, Type = typeof(Line))]
    [TemplatePart(Name = LnSizeSouthTemplateName, Type = typeof(Line))]
    [TemplatePart(Name = LnSizeWestTemplateName, Type = typeof(Line))]
    [TemplatePart(Name = LnSizeEastTemplateName, Type = typeof(Line))]
    [TemplatePart(Name = RectSizeNorthWestTemplateName, Type = typeof(Rectangle))]
    [TemplatePart(Name = RectSizeNorthEastTemplateName, Type = typeof(Rectangle))]
    [TemplatePart(Name = RectSizeSouthWestTemplateName, Type = typeof(Rectangle))]
    [TemplatePart(Name = RectSizeSouthEastTemplateName, Type = typeof(Rectangle))]
    [TemplatePart(Name = WindowBorderTemplateName, Type = typeof(Border))]
    [TemplatePart(Name = TopBarMenuTemplateName, Type = typeof(Menu))]
    public class CustomWindow : Window
    {
        private const string TitleBarTemplateName = "TitleBar";
        private const string MinButtonTemplateName = "MinButton";
        private const string MaxButtonTemplateName = "MaxButton";
        private const string CloseButtonTemplateName = "CloseButton";
        private const string LnSizeNorthTemplateName = "lnSizeNorth";
        private const string LnSizeSouthTemplateName = "lnSizeSouth";
        private const string LnSizeWestTemplateName = "lnSizeWest";
        private const string LnSizeEastTemplateName = "lnSizeEast";
        private const string RectSizeNorthWestTemplateName = "rectSizeNorthWest";
        private const string RectSizeNorthEastTemplateName = "rectSizeNorthEast";
        private const string RectSizeSouthWestTemplateName = "rectSizeSouthWest";
        private const string RectSizeSouthEastTemplateName = "rectSizeSouthEast";
        private const string WindowBorderTemplateName = "PART_Border";
        private const string TopBarMenuTemplateName = "PART_TopBarMenu";
        private const int WmSyscommand = 0x112;
        private const int ScSize = 0xF000;
        private const int ScKeymenus = 0x0214;
        private const int WmNclButtonDownMessage = 0xA1;
        private const int TitleBarArea = 0x2;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        private Border _titleBar;
        private Button _minButton;
        private Button _maxButton;
        private Button _closeButton;
        private Line _lnSizeNorth;
        private Line _lnSizeSouth;
        private Line _lnSizeWest;
        private Line _lnSizeEast;
        private Rectangle _rectSizeNorthWest;
        private Rectangle _rectSizeNorthEast;
        private Rectangle _rectSizeSouthWest;
        private Rectangle _rectSizeSouthEast;
        private Border _windowBorder;
        private Menu _topBarMenu;

        public static readonly DependencyProperty TopBarMenuVisibilityProperty =
            DependencyProperty.Register(nameof(TopBarMenuVisibility), typeof(Visibility), typeof(CustomWindow),
                new PropertyMetadata(Visibility.Collapsed));

        public Visibility TopBarMenuVisibility
        {
            get => (Visibility)GetValue(TopBarMenuVisibilityProperty);
            set => SetValue(TopBarMenuVisibilityProperty, value);
        }

        public static readonly DependencyProperty ShowHeaderLineProperty =
            DependencyProperty.Register(nameof(ShowHeaderLine), typeof(bool), typeof(CustomWindow),
                new PropertyMetadata(false));

        public bool ShowHeaderLine
        {
            get => (bool)GetValue(ShowHeaderLineProperty);
            set => SetValue(ShowHeaderLineProperty, value);
        }


        public Visibility MaximizeButtonVisibility
        {
            get => (Visibility)GetValue(MaximizeButtonVisibilityProperty);
            set => SetValue(MaximizeButtonVisibilityProperty, value);
        }

        public static readonly DependencyProperty MaximizeButtonVisibilityProperty =
            DependencyProperty.Register(nameof(MaximizeButtonVisibility), typeof(Visibility), typeof(CustomWindow),
                new PropertyMetadata(System.Windows.Visibility.Visible));



        public Visibility MinimizeButtonVisibility
        {
            get => (Visibility)GetValue(MinimizeButtonVisibilityProperty);
            set => SetValue(MinimizeButtonVisibilityProperty, value);
        }

        public static readonly DependencyProperty MinimizeButtonVisibilityProperty =
            DependencyProperty.Register(nameof(MinimizeButtonVisibility), typeof(Visibility), typeof(CustomWindow),
                new PropertyMetadata(System.Windows.Visibility.Visible));



        //public bool IsTitleBarVisible
        //{
        //    get => (bool)GetValue(dp: IsTitleBarVisibleProperty);
        //    set => SetValue(dp: IsTitleBarVisibleProperty, value: value);
        //}

        //public static readonly DependencyProperty IsTitleBarVisibleProperty =
        //    DependencyProperty.Register(name: nameof(IsTitleBarVisible), propertyType: typeof(bool), ownerType: typeof(CustomWindow), typeMetadata: new PropertyMetadata(defaultValue: false));

        static CustomWindow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                    typeof(CustomWindow),
                 new FrameworkPropertyMetadata(typeof(CustomWindow)));
        }

        public CustomWindow()
        {
            _titleBar = new Border();
            _minButton = new Button();
            _maxButton = new Button();
            _closeButton = new Button();
            _lnSizeNorth = new Line();
            _lnSizeSouth = new Line();
            _lnSizeWest = new Line();
            _lnSizeEast = new Line();
            _rectSizeNorthWest = new Rectangle();
            _rectSizeNorthEast = new Rectangle();
            _rectSizeSouthWest = new Rectangle();
            _rectSizeSouthEast = new Rectangle();
            _windowBorder = new Border();
            _topBarMenu = new Menu();
            this.MinWidth = 800;
            this.MinHeight = 600;

            SizeChanged += CustomWindow_SizeChanged;
        }


        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();


            if (GetTemplateChild(childName: TitleBarTemplateName) is Border titleBar)
            {
                _titleBar = titleBar;
                _titleBar.MouseLeftButtonDown += TitleBarMouseLeftButtonDown;
                _titleBar.MouseMove += TitleBarMouseMove;
            }

            if (GetTemplateChild(childName: MinButtonTemplateName) is Button minButton)
            {
                _minButton = minButton;
                _minButton.Click += MinButtonClick;
            }

            if (GetTemplateChild(childName: MaxButtonTemplateName) is Button maxButton)
            {
                _maxButton = maxButton;
                _maxButton.Click += MaxButtonClick;
                _maxButton.MouseEnter += MaxButton_MouseEnter;
            }

            if (GetTemplateChild(childName: CloseButtonTemplateName) is Button closeButton)
            {
                _closeButton = closeButton;
                _closeButton.Click += CloseButtonClick;
            }

            if (GetTemplateChild(childName: LnSizeNorthTemplateName) is Line lnSizeNorth)
            {
                _lnSizeNorth = lnSizeNorth;
                _lnSizeNorth.MouseDown += OnSizeNorth;
            }
            if (GetTemplateChild(childName: LnSizeSouthTemplateName) is Line lnSizeSouth)
            {
                _lnSizeSouth = lnSizeSouth;
                _lnSizeSouth.MouseDown += OnSizeSouth;
            }
            if (GetTemplateChild(childName: LnSizeWestTemplateName) is Line lnSizeWest)
            {
                _lnSizeWest = lnSizeWest;
                _lnSizeWest.MouseDown += OnSizeWest;
            }
            if (GetTemplateChild(childName: LnSizeEastTemplateName) is Line lnSizeEast)
            {
                _lnSizeEast = lnSizeEast;
                _lnSizeEast.MouseDown += OnSizeEast;
            }

            if (GetTemplateChild(childName: RectSizeNorthWestTemplateName) is Rectangle rectSizeNorthWest)
            {
                _rectSizeNorthWest = rectSizeNorthWest;
                _rectSizeNorthWest.MouseDown += OnSizeNorthWest;
            }
            if (GetTemplateChild(childName: RectSizeNorthEastTemplateName) is Rectangle rectSizeNorthEast)
            {
                _rectSizeNorthEast = rectSizeNorthEast;
                _rectSizeNorthEast.MouseDown += OnSizeNorthEast;
            }
            if (GetTemplateChild(childName: RectSizeSouthWestTemplateName) is Rectangle rectSizeSouthWest)
            {
                _rectSizeSouthWest = rectSizeSouthWest;
                _rectSizeSouthWest.MouseDown += OnSizeSouthWest;
            }
            if (GetTemplateChild(childName: RectSizeSouthEastTemplateName) is Rectangle rectSizeSouthEast)
            {
                _rectSizeSouthEast = rectSizeSouthEast;
                _rectSizeSouthEast.MouseDown += OnSizeSouthEast;
            }

            if (GetTemplateChild(childName: WindowBorderTemplateName) is Border windowBorder)
            {
                _windowBorder = windowBorder;
            }

            if (GetTemplateChild(childName: TopBarMenuTemplateName) is Menu menu)
            {
                _topBarMenu = menu;
            }
        }



        private static void MaxButton_MouseEnter(object sender, MouseEventArgs e)
        {
            sender.ForWindowFromTemplate(w =>
            {
                SendMessage(w.GetWindowHandle(), ScKeymenus, IntPtr.Zero, IntPtr.Zero);
            });

        }

        private static void OnSizeSouth(object sender, MouseButtonEventArgs e) { OnSize(sender, SizingAction.South); }
        private static void OnSizeNorth(object sender, MouseButtonEventArgs e) { OnSize(sender, SizingAction.North); }
        private static void OnSizeEast(object sender, MouseButtonEventArgs e) { OnSize(sender, SizingAction.East); }
        private static void OnSizeWest(object sender, MouseButtonEventArgs e) { OnSize(sender, SizingAction.West); }
        private static void OnSizeNorthWest(object sender, MouseButtonEventArgs e) { OnSize(sender, SizingAction.NorthWest); }
        private static void OnSizeNorthEast(object sender, MouseButtonEventArgs e) { OnSize(sender, SizingAction.NorthEast); }
        private static void OnSizeSouthEast(object sender, MouseButtonEventArgs e) { OnSize(sender, SizingAction.SouthEast); }
        private static void OnSizeSouthWest(object sender, MouseButtonEventArgs e) { OnSize(sender, SizingAction.SouthWest); }



        private static void OnSize(object sender, SizingAction action)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                sender.ForWindowFromTemplate(w =>
                {
                    if (w.WindowState == WindowState.Normal)
                        DragSize(w.GetWindowHandle(), action);
                });
            }
        }

        private static void CloseButtonClick(object sender, RoutedEventArgs e)
        {
            sender.ForWindowFromTemplate(w => w.Close());
        }

        private static void MinButtonClick(object sender, RoutedEventArgs e)
        {
            sender.ForWindowFromTemplate(w => w.WindowState = WindowState.Minimized);
        }

        private static void MaxButtonClick(object sender, RoutedEventArgs e)
        {
            sender.ForWindowFromTemplate(w => w.WindowState = (w.WindowState == WindowState.Maximized) ? WindowState.Normal : WindowState.Maximized);
        }


        private void CustomWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _windowBorder.BorderThickness = this.WindowState == WindowState.Normal ? new Thickness(1) : new Thickness(0);
        }

        private void TitleBarMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount > 1)
            {
                if (MaximizeButtonVisibility != Visibility.Collapsed)
                    MaxButtonClick(sender, e);
            }
        }

        private static void TitleBarMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                sender.ForWindowFromTemplate(w => w.DragMove());

                sender.ForWindowFromTemplate(w =>
                {
                    if (w.WindowState == WindowState.Maximized)
                    {
                        SendMessage(w.GetWindowHandle(), WmNclButtonDownMessage, (IntPtr)TitleBarArea, IntPtr.Zero);
                    }
                });
            }
        }

        private static void DragSize(IntPtr handle, SizingAction sizingAction)
        {
            SendMessage(handle, WmSyscommand, (IntPtr)(ScSize + sizingAction), IntPtr.Zero);
            SendMessage(handle, 514, IntPtr.Zero, IntPtr.Zero);
        }

        public enum SizingAction
        {
            North = 3,
            South = 6,
            East = 2,
            West = 1,
            NorthEast = 5,
            NorthWest = 4,
            SouthEast = 8,
            SouthWest = 7
        }

    }
}
