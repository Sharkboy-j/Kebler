using System;
using System.Collections.Generic;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

// ReSharper disable MemberCanBePrivate.Global

namespace Kebler.UI.Controls
{
    internal class SprocketControl : Control, IDisposable
    {
        #region Enums

        /// <summary>
        ///     Defines the Direction of Rotation
        /// </summary>
        public enum Direction
        {
            Clockwise,
            Anticlockwise
        }

        #endregion

        #region Construction

        /// <summary>
        ///     Ctor
        /// </summary>
        public SprocketControl()
        {
            _renderTimer = new Timer(Interval);
            _renderTimer.Elapsed += OnRenderTimerElapsed;

            // Set the minimum size of the SprocketControl
            //MinWidth = MINIMUM_CONTROL_SIZE.Width;
            //MinWidth = MINIMUM_CONTROL_SIZE.Height;

            // Calculate the spoke points based on the current size
            CalculateSpokesPoints();

            Loaded += Handler;

            // Event handler added to stop the timer if the control is no longer visible
            IsVisibleChanged += OnVisibilityChanged;
        }

        private async void Handler(object sender, RoutedEventArgs e)
        {
            Loaded -= Handler;
            await Dispatcher.InvokeAsync(() =>
            {
                if (IsIndeterminate && IsVisible) Start();
            });
        }

        #endregion

        #region Structs

        /// <summary>
        ///     Stores the details of each spoke
        /// </summary>
        private struct Spoke
        {
            public readonly Point StartPoint;
            public readonly Point EndPoint;

            public Spoke(Point pt1, Point pt2)
            {
                StartPoint = pt1;
                EndPoint = pt2;
            }
        }

        #endregion

        #region Constants

        private const double DefaultInterval = 60;
        private static readonly Color DefaultTickColor = Color.FromArgb(255, 58, 58, 58);
        private const double DefaultTickWidth = 3;

        private const int DefaultTickCount = 12;

        //private readonly Size MINIMUM_CONTROL_SIZE = new Size(2, 2);
        /*
                private const double MinimumPenWidth = 2;
        */
        private const double DefaultStartAngle = 270;
        private const double MinimumInnerRadiusFactor = 0.175;

        private const double MinimumOuterRadiusFactor = 0.3125;

        // The Lower limit of the Alpha value (The spokes will be shown in 
        // alpha values ranging from 255 to m_AlphaLowerLimit)
        private const int AlphaUpperLimit = 250;
        private const int AlphaLowerLimit = 0;
        private const double AlphaTickPercentageLowerLimit = 10;
        private const double DefaultProgressAlpha = 10;
        private const double DefaultProgress = 0.0;

        #endregion

        #region Fields

        private Point _centerPoint;
        private double _innerRadius;
        private double _outerRadius;
        private double _alphaChange;
        private double _angleIncrement;
        private double _renderStartAngle;
        private readonly Timer _renderTimer;
        private List<Spoke> _spokes;

        #endregion

        #region Dependency Properties

        #region AlphaTicksPercentage

        /// <summary>
        ///     AlphaTicksPercentage Dependency Property
        /// </summary>
        public static readonly DependencyProperty AlphaTicksPercentageProperty =
            DependencyProperty.Register(nameof(AlphaTicksPercentage), typeof(double), typeof(SprocketControl),
                new FrameworkPropertyMetadata(100.0, OnAlphaTicksPercentageChanged, CoerceAlphaTicksPercentage));

        /// <summary>
        ///     Gets or sets the AlphaTicksPercentage property. This dependency property
        ///     indicates the percentage of total ticks which must be considered for step by step reduction
        ///     of the alpha value. The remaining ticks remain at the LowestAlpha value.
        /// </summary>
        public double AlphaTicksPercentage
        {
            get => (double)GetValue(AlphaTicksPercentageProperty);
            set => SetValue(AlphaTicksPercentageProperty, value);
        }

        /// <summary>
        ///     Handles changes to the AlphaTicksPercentage property.
        /// </summary>
        private static void OnAlphaTicksPercentageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sprocket = (SprocketControl)d;
            var oldAlphaTicksPercentage = (double)e.OldValue;
            var newAlphaTicksPercentage = sprocket.AlphaTicksPercentage;
            //sprocket.OnAlphaTicksPercentageChanged(oldAlphaTicksPercentage, newAlphaTicksPercentage);
        }

        ///// <summary>
        /////     Provides derived classes an opportunity to handle changes to the AlphaTicksPercentage property.
        ///// </summary>
        //protected void OnAlphaTicksPercentageChanged(double oldAlphaTicksPercentage, double newAlphaTicksPercentage)
        //{
        //}

        /// <summary>
        ///     Coerces the AlphaTicksPercentage value.
        /// </summary>
        private static object CoerceAlphaTicksPercentage(DependencyObject d, object value)
        {
            var desiredAlphaTicksPercentage = (double)value;

            desiredAlphaTicksPercentage = desiredAlphaTicksPercentage switch
            {
                > 100.0 => 100.0,
                < AlphaTickPercentageLowerLimit => AlphaTickPercentageLowerLimit,
                _ => desiredAlphaTicksPercentage
            };

            return desiredAlphaTicksPercentage;
        }

        #endregion

        #region Interval

        /// <summary>
        ///     Interval Dependency Property
        /// </summary>
        public static readonly DependencyProperty IntervalProperty =
            DependencyProperty.Register(nameof(Interval), typeof(double), typeof(SprocketControl),
                new FrameworkPropertyMetadata(DefaultInterval,
                    OnIntervalChanged));

        /// <summary>
        ///     Gets or sets the Interval property. This dependency property
        ///     indicates duration at which the timer for rotation should fire.
        /// </summary>
        public double Interval
        {
            get => (double)GetValue(IntervalProperty);
            set => SetValue(IntervalProperty, value);
        }

        /// <summary>
        ///     Handles changes to the Interval property.
        /// </summary>
        /// <param name="d">SprocketControl</param>
        /// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnIntervalChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sprocket = (SprocketControl)d;
            var oldInterval = (double)e.OldValue;
            var newInterval = sprocket.Interval;
            sprocket.OnIntervalChanged(oldInterval, newInterval);
        }

        /// <summary>
        ///     Provides derived classes an opportunity to handle changes to the Interval property.
        /// </summary>
        /// <param name="oldInterval">Old Value</param>
        /// <param name="newInterval">New Value</param>
        protected void OnIntervalChanged(double oldInterval, double newInterval)
        {
            if (_renderTimer == null)
                return;
            var isEnabled = _renderTimer.Enabled;
            _renderTimer.Enabled = false;
            _renderTimer.Interval = newInterval;
            _renderTimer.Enabled = isEnabled;
        }

        #endregion

        #region IsIndeterminate

        /// <summary>
        ///     IsIndeterminate Dependency Property
        /// </summary>
        public static readonly DependencyProperty IsIndeterminateProperty =
            DependencyProperty.Register(nameof(IsIndeterminateProperty), typeof(bool), typeof(SprocketControl),
                new FrameworkPropertyMetadata(true, OnIsIndeterminateChanged));

        /// <summary>
        ///     Gets or sets the IsIndeterminate property. This dependency property
        ///     indicates whether the SprocketControl's progress is indeterminate or not.
        /// </summary>
        public bool IsIndeterminate
        {
            get => (bool)GetValue(IsIndeterminateProperty);
            set => SetValue(IsIndeterminateProperty, value);
        }

        /// <summary>
        ///     Handles changes to the IsIndeterminate property.
        /// </summary>
        /// <param name="d">SprocketControl</param>
        /// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnIsIndeterminateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var target = (SprocketControl)d;
            var oldIsIndeterminate = (bool)e.OldValue;
            var newIsIndeterminate = target.IsIndeterminate;
            target.OnIsIndeterminateChanged(oldIsIndeterminate, newIsIndeterminate);
        }

        /// <summary>
        ///     Provides derived classes an opportunity to handle changes to the IsIndeterminate property.
        /// </summary>
        /// <param name="oldIsIndeterminate">Old Value</param>
        /// <param name="newIsIndeterminate">New Value</param>
        protected void OnIsIndeterminateChanged(in bool oldIsIndeterminate, in bool newIsIndeterminate)
        {
            if (oldIsIndeterminate == newIsIndeterminate)
                return;

            if (newIsIndeterminate && IsVisible)
            {
                // Start the renderTimer
                Start();
            }
            else
            {
                // Stop the renderTimer
                Stop();
                InvalidateVisual();
            }
        }

        #endregion

        #region InnerRadius

        /// <summary>
        ///     InnerRadius Dependency Property
        /// </summary>
        public static readonly DependencyProperty InnerRadiusProperty =
            DependencyProperty.Register(nameof(InnerRadius), typeof(double), typeof(SprocketControl),
                new FrameworkPropertyMetadata(MinimumInnerRadiusFactor, OnInnerRadiusChanged, CoerceInnerRadius));

        /// <summary>
        ///     Gets or sets the InnerRadius property. This dependency property
        ///     indicates the ratio of the Inner Radius to the Width of the SprocketControl.
        /// </summary>
        public double InnerRadius
        {
            get => (double)GetValue(InnerRadiusProperty);
            set => SetValue(InnerRadiusProperty, value);
        }

        /// <summary>
        ///     Handles changes to the InnerRadius property.
        /// </summary>
        private static void OnInnerRadiusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sprocket = (SprocketControl)d;
            //var oldInnerRadius = (double)e.OldValue;
            //var newInnerRadius = sprocket.InnerRadius;
            sprocket.OnInnerRadiusChanged();
        }

        /// <summary>
        ///     Provides derived classes an opportunity to handle changes to the InnerRadius property.
        /// </summary>
        protected void OnInnerRadiusChanged()
        {
            // Recalculate the spoke points
            CalculateSpokesPoints();
        }

        /// <summary>
        ///     Coerces the InnerRadius value.
        /// </summary>
        private static object CoerceInnerRadius(DependencyObject d, object value)
        {
            var desiredInnerRadius = (double)value;
            return desiredInnerRadius;
        }

        #endregion

        #region LowestAlpha

        /// <summary>
        ///     LowestAlpha Dependency Property
        /// </summary>
        public static readonly DependencyProperty LowestAlphaProperty =
            DependencyProperty.Register(nameof(LowestAlpha), typeof(int), typeof(SprocketControl),
                new FrameworkPropertyMetadata(AlphaLowerLimit, OnLowestAlphaChanged, CoerceLowestAlpha));

        /// <summary>
        ///     Gets or sets the LowestAlpha property. This dependency property
        ///     indicates the lowest Opacity value that must be used while rendering the SprocketControl's spokes.
        /// </summary>
        public int LowestAlpha
        {
            get => (int)GetValue(LowestAlphaProperty);
            set => SetValue(LowestAlphaProperty, value);
        }

        /// <summary>
        ///     Handles changes to the LowestAlpha property.
        /// </summary>
        private static void OnLowestAlphaChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //var sprocket = (SprocketControl)d;
            //var oldLowestAlpha = (int)e.OldValue;
            //var newLowestAlpha = sprocket.LowestAlpha;
            //sprocket.OnLowestAlphaChanged(oldLowestAlpha, newLowestAlpha);
        }

        /// <summary>
        ///     Provides derived classes an opportunity to handle changes to the LowestAlpha property.
        /// </summary>
        //protected void OnLowestAlphaChanged(int oldLowestAlpha, int newLowestAlpha)
        //{
        //}

        /// <summary>
        ///     Coerces the LowestAlpha value.
        /// </summary>
        private static object CoerceLowestAlpha(DependencyObject d, object value)
        {
            var desiredLowestAlpha = (int)value;

            desiredLowestAlpha = desiredLowestAlpha switch
            {
                < AlphaLowerLimit => AlphaLowerLimit,
                > AlphaUpperLimit => AlphaUpperLimit,
                _ => desiredLowestAlpha
            };

            return desiredLowestAlpha;
        }

        #endregion

        #region OuterRadius

        /// <summary>
        ///     OuterRadius Dependency Property
        /// </summary>
        public static readonly DependencyProperty OuterRadiusProperty =
            DependencyProperty.Register(nameof(OuterRadius), typeof(double), typeof(SprocketControl),
                new FrameworkPropertyMetadata(MinimumOuterRadiusFactor, OnOuterRadiusChanged, CoerceOuterRadius));

        /// <summary>
        ///     Gets or sets the OuterRadius property. This dependency property
        ///     indicates the ratio of the Outer Width to the width of the SprocketControl.
        /// </summary>
        public double OuterRadius
        {
            get => (double)GetValue(OuterRadiusProperty);
            set => SetValue(OuterRadiusProperty, value);
        }

        /// <summary>
        ///     Handles changes to the OuterRadius property.
        /// </summary>
        private static void OnOuterRadiusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sprocket = (SprocketControl)d;
            sprocket.OnOuterRadiusChanged();
        }

        /// <summary>
        ///     Provides derived classes an opportunity to handle changes to the OuterRadius property.
        /// </summary>
        protected void OnOuterRadiusChanged()
        {
            // Recalculate the spoke points
            CalculateSpokesPoints();
        }

        /// <summary>
        ///     Coerces the OuterRadius value.
        /// </summary>
        private static object CoerceOuterRadius(DependencyObject d, object value)
        {
            var desiredOuterRadius = (double)value;

            return desiredOuterRadius;
        }

        #endregion

        #region Progress

        /// <summary>
        ///     Progress Dependency Property
        /// </summary>
        public static readonly DependencyProperty ProgressProperty =
            DependencyProperty.Register(nameof(Progress), typeof(double), typeof(SprocketControl),
                new FrameworkPropertyMetadata(DefaultProgress, OnProgressChanged, CoerceProgress));

        /// <summary>
        ///     Gets or sets the Progress property. This dependency property
        ///     indicates the progress percentage.
        /// </summary>
        public double Progress
        {
            get => (double)GetValue(ProgressProperty);
            set => SetValue(ProgressProperty, value);
        }

        /// <summary>
        ///     Coerces the Progress value so that it stays in the range 0-100
        /// </summary>
        /// <param name="d">SprocketControl</param>
        /// <param name="value">New Value</param>
        /// <returns>Coerced Value</returns>
        private static object CoerceProgress(DependencyObject d, object value)
        {
            var progress = (double)value;

            if (progress < 0.0) return 0.0;

            return progress > 100.0 ? 100.0 : value;
        }

        /// <summary>
        ///     Handles changes to the Progress property.
        /// </summary>
        /// <param name="d">SprocketControl</param>
        /// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnProgressChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sprocket = (SprocketControl)d;
            //var oldProgress = (double)e.OldValue;
            //var newProgress = sprocket.Progress;
            sprocket.OnProgressChanged();
        }

        /// <summary>
        ///     Provides derived classes an opportunity to handle changes to the Progress property.
        /// </summary>
        /// <param name="oldProgress">Old Value</param>
        /// <param name="newProgress">New Value</param>
        protected void OnProgressChanged()
        {
            InvalidateVisual();
        }

        #endregion

        #region Rotation

        /// <summary>
        ///     Rotation Dependency Property
        /// </summary>
        public static readonly DependencyProperty RotationProperty =
            DependencyProperty.Register(nameof(Rotation), typeof(Direction), typeof(SprocketControl),
                new FrameworkPropertyMetadata(Direction.Clockwise, OnRotationChanged));

        /// <summary>
        ///     Gets or sets the Rotation property. This dependency property
        ///     indicates the direction of Rotation of the SprocketControl.
        /// </summary>
        public Direction Rotation
        {
            get => (Direction)GetValue(RotationProperty);
            set => SetValue(RotationProperty, value);
        }

        /// <summary>
        ///     Handles changes to the Rotation property.
        /// </summary>
        /// <param name="d">SprocketControl</param>
        /// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnRotationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sprocket = (SprocketControl)d;
            //var oldRotation = (Direction)e.OldValue;
            //var newRotation = sprocket.Rotation;
            sprocket.OnRotationChanged();
        }

        /// <summary>
        ///     Provides derived classes an opportunity to handle changes to the Rotation property.
        /// </summary>
        /// <param name="oldRotation">Old Value</param>
        /// <param name="newRotation">New Value</param>
        protected void OnRotationChanged()
        {
            // Recalculate the spoke points
            CalculateSpokesPoints();
        }

        #endregion

        #region StartAngle

        /// <summary>
        ///     StartAngle Dependency Property
        /// </summary>
        public static readonly DependencyProperty StartAngleProperty =
            DependencyProperty.Register(nameof(StartAngle), typeof(double), typeof(SprocketControl),
                new FrameworkPropertyMetadata(DefaultStartAngle, OnStartAngleChanged));

        /// <summary>
        ///     Gets or sets the StartAngle property. This dependency property
        ///     indicates the angle at which the first spoke (with max opacity) is drawn.
        /// </summary>
        public double StartAngle
        {
            get => (double)GetValue(StartAngleProperty);
            set => SetValue(StartAngleProperty, value);
        }

        /// <summary>
        ///     Handles changes to the StartAngle property.
        /// </summary>
        /// <param name="d">SprocketControl</param>
        /// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnStartAngleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sprocket = (SprocketControl)d;
            //var oldStartAngle = (double)e.OldValue;
            //var newStartAngle = sprocket.StartAngle;
            sprocket.OnStartAngleChanged();
        }

        /// <summary>
        ///     Provides derived classes an opportunity to handle changes to the StartAngle property.
        /// </summary>
        /// <param name="oldStartAngle">Old Value</param>
        /// <param name="newStartAngle">New Value</param>
        protected void OnStartAngleChanged()
        {
            // Recalculate the spoke points
            CalculateSpokesPoints();
        }

        #endregion

        #region TickColor

        /// <summary>
        ///     TickColor Dependency Property
        /// </summary>
        public static readonly DependencyProperty TickColorProperty =
            DependencyProperty.Register(nameof(TickColor), typeof(Color), typeof(SprocketControl),
                new FrameworkPropertyMetadata(DefaultTickColor, OnTickColorChanged));

        /// <summary>
        ///     Gets or sets the TickColor property. This dependency property
        ///     indicates the color of the Spokes in the SprocketControl.
        /// </summary>
        public Color TickColor
        {
            get => (Color)GetValue(TickColorProperty);
            set => SetValue(TickColorProperty, value);
        }

        /// <summary>
        ///     Handles changes to the TickColor property.
        /// </summary>
        /// <param name="d">SprocketControl</param>
        /// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnTickColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sprocket = (SprocketControl)d;
            //var oldTickColor = (Color)e.OldValue;
            //var newTickColor = sprocket.TickColor;
            sprocket.OnTickColorChanged();
        }

        /// <summary>
        ///     Provides derived classes an opportunity to handle changes to the TickColor property.
        /// </summary>
        /// <param name="oldTickColor">Old Value</param>
        /// <param name="newTickColor">New Value</param>
        protected void OnTickColorChanged()
        {
            InvalidateVisual();
        }

        #endregion

        #region TickCount

        /// <summary>
        ///     TickCount Dependency Property
        /// </summary>
        public static readonly DependencyProperty TickCountProperty =
            DependencyProperty.Register(nameof(TickCount), typeof(int), typeof(SprocketControl),
                new FrameworkPropertyMetadata(DefaultTickCount, OnTickCountChanged, CoerceTickCount));

        /// <summary>
        ///     Gets or sets the TickCount property. This dependency property
        ///     indicates the number of spokes of the SprocketControl.
        /// </summary>
        public int TickCount
        {
            get => (int)GetValue(TickCountProperty);
            set => SetValue(TickCountProperty, value);
        }

        /// <summary>
        ///     Coerces the TickCount value to an acceptable value
        /// </summary>
        /// <param name="d">SprocketControl</param>
        /// <param name="value">New Value</param>
        /// <returns>Coerced Value</returns>
        private static object CoerceTickCount(DependencyObject d, object value)
        {
            return (int)value <= 0 ? DefaultTickCount : value;
        }

        /// <summary>
        ///     Handles changes to the TickCount property.
        /// </summary>
        /// <param name="d">SprocketControl</param>
        /// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnTickCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sprocket = (SprocketControl)d;
            //var oldTickCount = (int)e.OldValue;
            //var newTickCount = sprocket.TickCount;
            sprocket.OnTickCountChanged();
        }

        /// <summary>
        ///     Provides derived classes an opportunity to handle changes to the TickCount property.
        /// </summary>
        /// <param name="oldTickCount">Old Value</param>
        /// <param name="newTickCount">New Value</param>
        protected void OnTickCountChanged()
        {
            // Recalculate the spoke points
            CalculateSpokesPoints();
        }

        #endregion

        #region TickStyle

        /// <summary>
        ///     TickStyle Dependency Property
        /// </summary>
        public static readonly DependencyProperty TickStyleProperty =
            DependencyProperty.Register(nameof(TickStyle), typeof(PenLineCap), typeof(SprocketControl),
                new FrameworkPropertyMetadata(PenLineCap.Round, OnTickStyleChanged));

        /// <summary>
        ///     Gets or sets the TickStyle property. This dependency property
        ///     indicates the style of the ends of each tick.
        /// </summary>
        public PenLineCap TickStyle
        {
            get => (PenLineCap)GetValue(TickStyleProperty);
            set => SetValue(TickStyleProperty, value);
        }

        /// <summary>
        ///     Handles changes to the TickStyle property.
        /// </summary>
        /// <param name="d">SprocketControl</param>
        /// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnTickStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sprocket = (SprocketControl)d;
            //var oldTickStyle = (PenLineCap)e.OldValue;
            //var newTickStyle = sprocket.TickStyle;
            sprocket.OnTickStyleChanged( );
        }

        /// <summary>
        ///     Provides derived classes an opportunity to handle changes to the TickStyle property.
        /// </summary>
        /// <param name="oldTickStyle">Old Value</param>
        /// <param name="newTickStyle">New Value</param>
        protected void OnTickStyleChanged()
        {
            InvalidateVisual();
        }

        #endregion

        #region TickWidth

        /// <summary>
        ///     TickWidth Dependency Property
        /// </summary>
        public static readonly DependencyProperty TickWidthProperty =
            DependencyProperty.Register(nameof(TickWidth), typeof(double), typeof(SprocketControl),
                new FrameworkPropertyMetadata(DefaultTickWidth, OnTickWidthChanged, CoerceTickWidth));

        /// <summary>
        ///     Gets or sets the TickWidth property. This dependency property
        ///     indicates the width of each spoke in the SprocketControl.
        /// </summary>
        public double TickWidth
        {
            get => (double)GetValue(TickWidthProperty);
            set => SetValue(TickWidthProperty, value);
        }

        /// <summary>
        ///     Coerces the TickWidth value so that it stays above 0.
        /// </summary>
        /// <param name="d">SprocketControl</param>
        /// <param name="value">New Value</param>
        /// <returns>Coerced Value</returns>
        private static object CoerceTickWidth(DependencyObject d, object value)
        {
            return (double)value < 0.0 ? DefaultTickWidth : value;
        }

        /// <summary>
        ///     Handles changes to the TickWidth property.
        /// </summary>
        /// <param name="d">SprocketControl</param>
        /// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnTickWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var target = (SprocketControl)d;
            //var oldTickWidth = (double)e.OldValue;
            //var newTickWidth = target.TickWidth;
            target.OnTickWidthChanged();
        }

        /// <summary>
        ///     Provides derived classes an opportunity to handle changes to the TickWidth property.
        /// </summary>
        /// <param name="oldTickWidth">Old Value</param>
        /// <param name="newTickWidth">New Value</param>
        protected void OnTickWidthChanged()
        {
            InvalidateVisual();
        }

        #endregion

        #endregion

        #region Helpers

        /// <summary>
        ///     Start the Tick Control rotation
        /// </summary>
        private void Start()
        {
            if (_renderTimer == null || _renderTimer.Enabled)
                return;

            _renderTimer.Interval = Interval;
            _renderTimer.Enabled = true;
        }

        /// <summary>
        ///     Stop the Tick Control rotation
        /// </summary>
        private void Stop()
        {
            if (_renderTimer != null) _renderTimer.Enabled = false;
        }

        /// <summary>
        ///     Converts Degrees to Radians
        /// </summary>
        /// <param name="degrees">Degrees</param>
        /// <returns>Radians</returns>
        private static double ConvertDegreesToRadians(in double degrees)
        {
            return Math.PI / 180 * degrees;
        }

        /// <summary>
        ///     Calculate the Spoke Points and store them
        /// </summary>
        private void CalculateSpokesPoints()
        {
            _spokes = new List<Spoke>();

            // Calculate the angle between adjacent spokes
            _angleIncrement = 360 / (double)TickCount;
            // Calculate the change in alpha between adjacent spokes
            _alphaChange = (int)((255 - LowestAlpha) / (AlphaTicksPercentage / 100.0 * TickCount));

            // Set the start angle for rendering
            _renderStartAngle = StartAngle;

            // Calculate the location around which the spokes will be drawn
            var width = Width < Height ? Width : Height;
            _centerPoint = new Point(Width / 2, Height / 2);
            // Calculate the inner and outer radii of the control. The radii should not be less than the
            // Minimum values
            //_innerRadius = (int)(width * INNER_RADIUS_FACTOR);
            //if (_innerRadius < MINIMUM_INNER_RADIUS)
            //    _innerRadius = MINIMUM_INNER_RADIUS;
            //_outerRadius = (int)(width * OUTER_RADIUS_FACTOR);
            //if (_outerRadius < MINIMUM_OUTER_RADIUS)
            //    _outerRadius = MINIMUM_OUTER_RADIUS;

            _innerRadius = (int)(width * InnerRadius);
            _outerRadius = (int)(width * OuterRadius);
            double angle = 0;

            for (var i = 0; i < TickCount; i++)
            {
                var pt1 = new Point(_innerRadius * (float)Math.Cos(ConvertDegreesToRadians(angle)),
                    _innerRadius * (float)Math.Sin(ConvertDegreesToRadians(angle)));
                var pt2 = new Point(_outerRadius * (float)Math.Cos(ConvertDegreesToRadians(angle)),
                    _outerRadius * (float)Math.Sin(ConvertDegreesToRadians(angle)));

                // Create a spoke based on the points generated
                var spoke = new Spoke(pt1, pt2);
                // Add the spoke to the List
                _spokes.Add(spoke);

                // If it is not it Indeterminate state, 
                // ensure that the spokes are drawn in clockwise manner
                if (!IsIndeterminate)
                    angle += _angleIncrement;
                else
                    switch (Rotation)
                    {
                        case Direction.Clockwise:
                            angle -= _angleIncrement;
                            break;
                        case Direction.Anticlockwise:
                            angle += _angleIncrement;
                            break;
                    }
            }
        }

        #endregion

        #region Overrides

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            // Calculate the spoke points based on the new size
            CalculateSpokesPoints();
        }

        protected override void OnRender(DrawingContext dc)
        {
            if (_spokes == null)
                return;

            var translate = new TranslateTransform(_centerPoint.X, _centerPoint.Y);
            dc.PushTransform(translate);
            var rotate = new RotateTransform(_renderStartAngle);
            dc.PushTransform(rotate);

            var alpha = (byte)255;

            // Get the number of spokes that can be drawn with zero transparency
            var progressSpokes = (int)Math.Floor(Progress * TickCount / 100.0);

            // Render the spokes
            for (var i = 0; i < TickCount; i++)
            {
                if (!IsIndeterminate)
                {
                    if (progressSpokes > 0)
                        alpha = (byte)(i < progressSpokes ? 255 : DefaultProgressAlpha);
                    else
                        alpha = (byte)DefaultProgressAlpha;
                }

                var p = new Pen(new SolidColorBrush(Color.FromArgb(alpha, TickColor.R, TickColor.G, TickColor.B)),
                    TickWidth);
                p.StartLineCap = p.EndLineCap = TickStyle;
                dc.DrawLine(p, _spokes[i].StartPoint, _spokes[i].EndPoint);

                if (IsIndeterminate)
                {
                    alpha -= (byte)_alphaChange;
                    if (alpha < LowestAlpha)
                        alpha = (byte)LowestAlpha;
                }
            }

            // Perform a reverse Rotation and Translation to obtain the original Transformation
            dc.Pop();
            dc.Pop();
        }

        #endregion

        #region Event Handlers

        /// <summary>
        ///     Handles the Elapsed event of the renderTimer
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">EventArgs</param>
        private async void OnRenderTimerElapsed(object sender, ElapsedEventArgs e)
        {
            await Dispatcher.InvokeAsync(() =>
            {
                switch (Rotation)
                {
                    case Direction.Clockwise:
                        _renderStartAngle += _angleIncrement;

                        if (_renderStartAngle >= 360)
                            _renderStartAngle -= 360;
                        break;
                    case Direction.Anticlockwise:
                        _renderStartAngle -= _angleIncrement;

                        if (_renderStartAngle <= -360)
                            _renderStartAngle += 360;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                // Force re-rendering of control
                InvalidateVisual();
            });
        }

        /// <summary>
        ///     Event handler to stop the timer if the control is no longer visible
        ///     and start it when it is visible
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnVisibilityChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // Needs to be handled only if the state of the progress bar is indeterminate
            if (!IsIndeterminate)
                return;

            if ((bool)e.NewValue)
                Start();
            else
                Stop();
        }

        #endregion

        #region IDisposable Implementation

        /// <summary>
        ///     Releases all resources used by an instance of the SprocketControl class.
        /// </summary>
        /// <remarks>
        ///     This method calls the virtual Dispose(bool) method, passing in 'true', and then suppresses
        ///     finalization of the instance.
        /// </remarks>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Releases unmanaged resources before an instance of the SprocketControl class is reclaimed by garbage collection.
        /// </summary>
        /// <remarks>
        ///     NOTE: Leave out the finalizer altogether if this class doesn't own unmanaged resources itself,
        ///     but leave the other methods exactly as they are.
        ///     This method releases unmanaged resources by calling the virtual Dispose(bool), passing in 'false'.
        /// </remarks>
        ~SprocketControl()
        {
            Dispose(false);
        }

        /// <summary>
        ///     Releases the unmanaged resources used by an instance of the SprocketControl class and optionally releases the
        ///     managed resources.
        /// </summary>
        /// <param name="disposing">
        ///     'true' to release both managed and unmanaged resources; 'false' to release only unmanaged
        ///     resources.
        /// </param>
        protected void Dispose(bool disposing)
        {
            if (!disposing)
                return;
            if (_renderTimer != null)
            {
                _renderTimer.Elapsed -= OnRenderTimerElapsed;
                _renderTimer.Dispose();
            }

            IsVisibleChanged -= OnVisibilityChanged;

            // free native resources if there are any.			
        }

        #endregion
    }
}