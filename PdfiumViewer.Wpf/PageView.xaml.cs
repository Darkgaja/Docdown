﻿using PdfiumViewer.Wpf.Util;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace PdfiumViewer.Wpf
{
    internal partial class PageView
    {
        private bool alreadySet = false;
        private Size? lastSize;
        private readonly DispatcherTimer timer;

        public PageView()
        {
            InitializeComponent();
            LayoutUpdated += PageLayoutUpdated;
            timer = new DispatcherTimer(TimeSpan.FromSeconds(1.0), DispatcherPriority.Normal, TimerCallback, Dispatcher);
        }

        private void PageLayoutUpdated(object sender, EventArgs e)
        {
            Update();
        }

        private void TimerCallback(object sender, EventArgs e)
        {
            timer.Stop();
            FullUpdate();
        }

        protected override Size MeasureOverride(Size constraint)
        {
            if (HasSizeChanged(constraint))
            {
                StartTimer();
            }

            if (DataContext is PageViewModel page)
            {
                var size = page.Resize(constraint);
                base.MeasureOverride(size);
                return size;
            }
            return Size.Empty;
        }

        private void FullUpdate()
        {
            alreadySet = false;
            Update();
        }

        private void Update()
        {
            var isVisible = PageIsVisible();
            if (!alreadySet && isVisible)
            {
                alreadySet = true;
                BeginSetImage();
            }
            else if (alreadySet && !isVisible)
            {
                alreadySet = false;
                UnsetImage();
            }
        }

        private bool HasSizeChanged(Size constraint)
        {
            if (lastSize is null)
            {
                lastSize = constraint;
                return false;
            }
            var temp = lastSize.Value;
            lastSize = constraint;
            return temp != constraint;
        }

        private void StartTimer()
        {
            timer.Start();
        }

        private bool PageIsVisible()
        {
            var scrollViewer = this.GetParentByType<ScrollViewer>();

            if (scrollViewer is null)
            {
                return true;
            }

            GeneralTransform childTransform = TransformToAncestor(scrollViewer);
            
            Rect rectangle = childTransform.TransformBounds(new Rect(new Point(0, 0), DesiredSize));
            Rect result = Rect.Intersect(new Rect(new Point(0, 0), scrollViewer.RenderSize), rectangle);

            return result != Rect.Empty;
        }

        private void BeginSetImage()
        {
            if (DataContext is PageViewModel page)
            {
                page.Render().ContinueWith(SetImage);
            }
        }

        private void SetImage(Task<BitmapSource> image)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                Img.Source = image.Result;
            }));
        }

        private void UnsetImage()
        {
            Img.Source = null;
        }
    }
}