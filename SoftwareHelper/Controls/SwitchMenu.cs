using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace SoftwareHelper.Controls
{
    public class SwitchMenu : Selector
    {
        private readonly double offset = 70;
        private Button PART_DownButton;
        private Button PART_NextButton;
        private Button PART_PreviousButton;
        private Rectangle PART_Rectangle;
        private ScrollViewer PART_ScrollViewer;
        private Button PART_UpButton;
        private double recordAnimationOffset;

        static SwitchMenu()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SwitchMenu),
                new FrameworkPropertyMetadata(typeof(SwitchMenu)));
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            var item = new ContentControl();
            item.MouseLeftButtonUp += item_MouseLeftButtonUp;
            return item;
        }

        protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
        {
            if (e.Delta < 0)
                BeginScrollViewerAnimation(ActualHeight);
            else
                BeginScrollViewerAnimation(-ActualHeight);
        }

        private void item_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            PART_PreviousButton = GetTemplateChild("PART_PreviousButton") as Button;
            PART_NextButton = GetTemplateChild("PART_NextButton") as Button;
            PART_UpButton = GetTemplateChild("PART_UpButton") as Button;
            PART_DownButton = GetTemplateChild("PART_DownButton") as Button;
            PART_ScrollViewer = GetTemplateChild("PART_ScrollViewer") as ScrollViewer;
            PART_Rectangle = GetTemplateChild("PART_Rectangle") as Rectangle;
            if (PART_PreviousButton != null) PART_PreviousButton.Click += PART_PreviousButton_Click;
            if (PART_NextButton != null) PART_NextButton.Click += PART_NextButton_Click;
            if (PART_UpButton != null) PART_UpButton.Click += PART_UpButton_Click;
            if (PART_DownButton != null) PART_DownButton.Click += PART_DownButton_Click;
            if (PART_ScrollViewer != null) PART_ScrollViewer.ScrollChanged += PART_ScrollViewer_ScrollChanged;
            //PART_ScrollViewer.MouseMove += PART_ScrollViewer_MouseMove;
            //PART_ScrollViewer.MouseLeave += PART_ScrollViewer_MouseLeave;
            //MouseMove += SwitchMenu_MouseMove;
            //MouseLeave += SwitchMenu_MouseLeave;
            if (PART_Rectangle != null) PART_Rectangle.IsVisibleChanged += PART_Rectangle_IsVisibleChanged;

            PART_ScrollViewer.SetValue(ScrollViewerBehavior.VerticalOffsetProperty, 0.0);
        }

        private void PART_Rectangle_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            //SwitchMenu_MouseLeave(null,null);
            OnPreviewDragLeave(null);
        }

        protected override void OnPreviewDragLeave(DragEventArgs e)
        {
            if (PART_ScrollViewer != null)
            {
                if (Orientation == Orientation.Horizontal)
                {
                    PART_PreviousButton.Visibility = Visibility.Collapsed;
                    PART_NextButton.Visibility = Visibility.Collapsed;
                }
                else
                {
                    if (PART_UpButton.Visibility != Visibility.Collapsed
                        &&
                        PART_DownButton.Visibility != Visibility.Collapsed)
                    {
                        PART_UpButton.Visibility = Visibility.Collapsed;
                        PART_DownButton.Visibility = Visibility.Collapsed;
                    }
                }
            }
        }

        private void SwitchMenu_MouseLeave(object sender, MouseEventArgs e)
        {
            if (PART_ScrollViewer != null)
            {
                if (Orientation == Orientation.Horizontal)
                {
                    PART_PreviousButton.Visibility = Visibility.Collapsed;
                    PART_NextButton.Visibility = Visibility.Collapsed;
                }
                else
                {
                    PART_UpButton.Visibility = Visibility.Collapsed;
                    PART_DownButton.Visibility = Visibility.Collapsed;
                }
            }
        }

        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            if (PART_ScrollViewer != null && !IsDragDrop)
            {
                if (Orientation == Orientation.Horizontal)
                {
                    PART_PreviousButton.Visibility = PART_ScrollViewer.HorizontalOffset == 0.0
                        ? Visibility.Hidden
                        : Visibility.Visible;
                    PART_NextButton.Visibility = PART_ScrollViewer.ScrollableWidth == PART_ScrollViewer.HorizontalOffset
                        ? Visibility.Hidden
                        : Visibility.Visible;
                }
                else
                {
                    PART_UpButton.Visibility =
                        PART_ScrollViewer.VerticalOffset == 0.0 ? Visibility.Hidden : Visibility.Visible;
                    PART_DownButton.Visibility = PART_ScrollViewer.ScrollableHeight == PART_ScrollViewer.VerticalOffset
                        ? Visibility.Hidden
                        : Visibility.Visible;
                }
            }
        }
        //private void SwitchMenu_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        //{
        //    if (PART_ScrollViewer != null && !IsDragDrop)
        //    {
        //        if (Orientation == Orientation.Horizontal)
        //        {
        //            PART_PreviousButton.Visibility = (PART_ScrollViewer.HorizontalOffset == 0.0) ? Visibility.Hidden : Visibility.Visible;
        //            PART_NextButton.Visibility = (PART_ScrollViewer.ScrollableWidth == PART_ScrollViewer.HorizontalOffset) ? Visibility.Hidden : Visibility.Visible;
        //        }
        //        else
        //        {
        //            PART_UpButton.Visibility = (PART_ScrollViewer.VerticalOffset == 0.0) ? Visibility.Hidden : Visibility.Visible;
        //            PART_DownButton.Visibility = (PART_ScrollViewer.ScrollableHeight == PART_ScrollViewer.VerticalOffset) ? Visibility.Hidden : Visibility.Visible;
        //        }
        //    }
        //}

        private void PART_UpButton_Click(object sender, RoutedEventArgs e)
        {
            BeginScrollViewerAnimation(-ActualHeight);
            //ScrollToOffset(Orientation.Vertical, -offset);
        }

        private void PART_DownButton_Click(object sender, RoutedEventArgs e)
        {
            BeginScrollViewerAnimation(ActualHeight);
            //ScrollToOffset(Orientation.Vertical, offset);
        }

        private void PART_ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
        }

        private void PART_PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            ScrollToOffset(Orientation.Horizontal, -offset);
        }

        private void PART_NextButton_Click(object sender, RoutedEventArgs e)
        {
            ScrollToOffset(Orientation.Horizontal, offset);
        }

        private void ScrollToOffset(Orientation orientation, double scrollOffset)
        {
            if (PART_ScrollViewer == null) return;
            switch (orientation)
            {
                case Orientation.Horizontal:
                    PART_ScrollViewer.ScrollToHorizontalOffset(PART_ScrollViewer.HorizontalOffset + scrollOffset);
                    break;
                case Orientation.Vertical:
                    PART_ScrollViewer.ScrollToVerticalOffset(PART_ScrollViewer.VerticalOffset + scrollOffset);
                    break;
            }
        }

        private void BeginScrollViewerAnimation(double animationOffset)
        {
            EasingFunctionBase easeFunction = new CubicEase
            {
                EasingMode = EasingMode.EaseOut
            };
            var doubleAnimation = new DoubleAnimation
            {
                Duration = new Duration(TimeSpan.FromSeconds(0.6)),
                EasingFunction = easeFunction
            };

            recordAnimationOffset = recordAnimationOffset + animationOffset;
            if (recordAnimationOffset > PART_ScrollViewer.ExtentHeight
                ||
                recordAnimationOffset < 0)
            {
                recordAnimationOffset = recordAnimationOffset - animationOffset;
                return;
            }

            doubleAnimation.To = recordAnimationOffset;
            doubleAnimation.Completed += DoubleAnimation_Completed;

            PART_ScrollViewer.BeginAnimation(ScrollViewerBehavior.VerticalOffsetProperty, doubleAnimation);
            PART_ScrollViewer.ScrollToVerticalOffset(PART_ScrollViewer.VerticalOffset + recordAnimationOffset);
        }

        private void DoubleAnimation_Completed(object sender, EventArgs e)
        {
            //Console.WriteLine($"PART_ScrollViewer.VerticalOffset Completed {PART_ScrollViewer.VerticalOffset }");
            if (PART_ScrollViewer != null && !IsDragDrop)
                if (Orientation == Orientation.Vertical)
                {
                    PART_UpButton.Visibility = PART_ScrollViewer.VerticalOffset < ActualHeight
                        ? Visibility.Hidden
                        : Visibility.Visible;
                    PART_DownButton.Visibility = PART_ScrollViewer.VerticalOffset == PART_ScrollViewer.VerticalOffset
                        ? Visibility.Hidden
                        : Visibility.Visible;
                }
        }

        public static class ScrollViewerBehavior
        {
            public static readonly DependencyProperty HorizontalOffsetProperty =
                DependencyProperty.RegisterAttached("HorizontalOffset", typeof(double), typeof(ScrollViewerBehavior),
                    new UIPropertyMetadata(0.0, OnHorizontalOffsetChanged));

            public static readonly DependencyProperty VerticalOffsetProperty =
                DependencyProperty.RegisterAttached("VerticalOffset", typeof(double), typeof(ScrollViewerBehavior),
                    new UIPropertyMetadata(0.0, OnVerticalOffsetChanged));

            public static void SetHorizontalOffset(FrameworkElement target, double value)
            {
                target.SetValue(HorizontalOffsetProperty, value);
            }

            public static double GetHorizontalOffset(FrameworkElement target)
            {
                return (double)target.GetValue(HorizontalOffsetProperty);
            }

            private static void OnHorizontalOffsetChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
            {
                (target as ScrollViewer)?.ScrollToHorizontalOffset((double)e.NewValue);
            }

            public static void SetVerticalOffset(FrameworkElement target, double value)
            {
                target.SetValue(VerticalOffsetProperty, value);
            }

            public static double GetVerticalOffset(FrameworkElement target)
            {
                return (double)target.GetValue(VerticalOffsetProperty);
            }

            private static void OnVerticalOffsetChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
            {
                (target as ScrollViewer)?.ScrollToVerticalOffset((double)e.NewValue);
            }
        }

        #region 依赖属性

        #region Orientation

        public Orientation Orientation
        {
            get => (Orientation)GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }

        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register("Orientation", typeof(Orientation), typeof(SwitchMenu),
                new PropertyMetadata(Orientation.Horizontal));

        public string SearchText
        {
            get => (string)GetValue(SearchTextProperty);
            set => SetValue(SearchTextProperty, value);
        }

        public static readonly DependencyProperty SearchTextProperty =
            DependencyProperty.Register("SearchText", typeof(string), typeof(SwitchMenu),
                new PropertyMetadata(string.Empty));

        public bool IsDragDrop
        {
            get => (bool)GetValue(IsDragDropProperty);
            set => SetValue(IsDragDropProperty, value);
        }

        // Using a DependencyProperty as the backing store for IsDragDrop.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsDragDropProperty =
            DependencyProperty.Register("IsDragDrop", typeof(bool), typeof(SwitchMenu),
                new PropertyMetadata(false, UpdateDragDropPropertyMetadata));

        private static void UpdateDragDropPropertyMetadata(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
            }
        }

        #endregion

        #endregion
    }
}