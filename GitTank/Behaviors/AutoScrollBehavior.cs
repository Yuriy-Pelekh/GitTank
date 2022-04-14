using Microsoft.Xaml.Behaviors;
using System;
using System.Windows.Controls;

namespace GitTank.Behaviors
{
    public class AutoScrollBehavior : Behavior<ScrollViewer>
    {
        private double _height;
        private ScrollViewer _scrollViewer;

        protected override void OnAttached()
        {
            base.OnAttached();

            _scrollViewer = AssociatedObject;
            _scrollViewer.LayoutUpdated += OnScrollViewerLayoutUpdated;
        }

        private void OnScrollViewerLayoutUpdated(object sender, EventArgs e)
        {
            if (Math.Abs(_scrollViewer.ExtentHeight - _height) > 1)
            {
                _scrollViewer.ScrollToVerticalOffset(_scrollViewer.ExtentHeight);
                _height = _scrollViewer.ExtentHeight;
            }
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            if (_scrollViewer != null)
            {
                _scrollViewer.LayoutUpdated -= OnScrollViewerLayoutUpdated;
            }
        }
    }
}
