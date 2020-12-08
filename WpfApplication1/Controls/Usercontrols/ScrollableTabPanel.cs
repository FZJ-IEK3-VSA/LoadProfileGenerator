//-----------------------------------------------------------------------

// <copyright>
//
// Copyright (c) TU Chemnitz, Prof. Technische Thermodynamik
// Written by Noah Pflugradt.
// Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//
// Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//  Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer
// in the documentation and/or other materials provided with the distribution.
//  All advertising materials mentioning features or use of this software must display the following acknowledgement:
//  “This product includes software developed by the TU Chemnitz, Prof. Technische Thermodynamik and its contributors.”
//  Neither the name of the University nor the names of its contributors may be used to endorse or promote products
//  derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE UNIVERSITY 'AS IS' AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING,
// BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE UNIVERSITY OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, S
// PECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; L
// OSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT,
// STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
// ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

// </copyright>

//-----------------------------------------------------------------------

using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Common;
using JetBrains.Annotations;
using Point = System.Windows.Point;
using Size = System.Windows.Size;
using Vector = System.Windows.Vector;

#pragma warning disable SA1623 // Property summary documentation must match accessors

#pragma warning disable SA1615 // Element return value must be documented

namespace LoadProfileGenerator.Controls.Usercontrols {
    /// <summary>
    ///     A scrollable TabPanel control.
    /// </summary>
    [SuppressMessage("ReSharper", "NotNullMemberIsNotInitialized")]
    [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse")]
    [SuppressMessage("ReSharper", "HeuristicUnreachableCode")]
    [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
    [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
    public class ScrollableTabPanel : Panel, IScrollInfo, INotifyPropertyChanged {
        [ItemNotNull] [JetBrains.Annotations.NotNull] private static readonly GradientStopCollection _gscOpacityMaskStopsTransparentOnLeft =
            new GradientStopCollection {new GradientStop(Colors.Transparent, 0), new GradientStop(Colors.Black, 0.5)};

        // For a description of the members below, refer to the respective property's description.

        // The following GradientStopCollections are being used for assigning an OpacityMask
        // to child-controls that are only partially visible.
        [ItemNotNull] [JetBrains.Annotations.NotNull]
        private static readonly GradientStopCollection _gscOpacityMaskStopsTransparentOnLeftAndRight =
            new GradientStopCollection {
                new GradientStop(Colors.Transparent, 0.0),
                new GradientStop(Colors.Black, 0.2),
                new GradientStop(Colors.Black, 0.8),
                new GradientStop(Colors.Transparent, 1.0)
            };

        [ItemNotNull] [JetBrains.Annotations.NotNull] private static readonly GradientStopCollection _gscOpacityMaskStopsTransparentOnRight =
            new GradientStopCollection {new GradientStop(Colors.Black, 0.5), new GradientStop(Colors.Transparent, 1)};

        /// <summary>
        ///     This will apply the present scroll-position resp. -offset.
        /// </summary>
        [JetBrains.Annotations.NotNull] private readonly TranslateTransform _ttScrollTransform = new TranslateTransform();

        [JetBrains.Annotations.NotNull] private ScrollViewer _svOwningScrollViewer;

        private Vector _vOffset;

        #region --- C'tor ---

        public ScrollableTabPanel()
        {
            RenderTransform = _ttScrollTransform;
            SizeChanged += ScrollableTabPanel_SizeChanged;
        }

        #endregion

        #region --- Helpers ---

        /// <summary>
        ///     Calculates the HorizontalOffset for a given child-control, based on a desired value.
        /// </summary>
        /// <param name="dblViewportLeft"> The left offset of the Viewport. </param>
        /// <param name="dblViewportRight"> The right offset of the Viewport. </param>
        /// <param name="dblChildLeft"> The left offset of the control in question. </param>
        /// <param name="dblChildRight"> The right offset of the control in question. </param>
        /// <returns>the return value </returns>
        private static double CalculateNewScrollOffset(double dblViewportLeft, double dblViewportRight,
            double dblChildLeft, double dblChildRight)
        {
            // Retrieve basic information about the position of the Viewport within the Extent of the control.
            var fIsFurtherToLeft = dblChildLeft < dblViewportLeft && dblChildRight < dblViewportRight;
            var fIsFurtherToRight = dblChildRight > dblViewportRight && dblChildLeft > dblViewportLeft;
            var fIsWiderThanViewport = dblChildRight - dblChildLeft > dblViewportRight - dblViewportLeft;

            if (!fIsFurtherToRight && !fIsFurtherToLeft) {
                return dblViewportLeft;
            }

            if (fIsFurtherToLeft && !fIsWiderThanViewport) {
                return dblChildLeft;
            }

            // The child is to be placed with its right edge equal to the right edge of the Viewport's present offset.
            return dblChildRight - (dblViewportRight - dblViewportLeft);
        }

        /// <summary>
        ///     Compares the present sizes (Extent/Viewport) against the local values and updates them, if required.
        /// </summary>
        /// <param name="szExtent">todo: describe szExtent parameter on UpdateMembers</param>
        /// <param name="szViewportSize">todo: describe szViewportSize parameter on UpdateMembers</param>
        private void UpdateMembers(Size szExtent, Size szViewportSize)
        {
            if (szExtent != Extent) {
                // The Extent of the control has changed.
                Extent = szExtent;
                ScrollOwner.InvalidateScrollInfo();
            }

            if (szViewportSize != Viewport) {
                // The Viewport of the panel has changed.
                Viewport = szViewportSize;
                ScrollOwner.InvalidateScrollInfo();
            }

            // Prevent from getting off to the right
            if (HorizontalOffset + Viewport.Width + RightOverflowMargin > ExtentWidth) {
                SetHorizontalOffset(HorizontalOffset + Viewport.Width + RightOverflowMargin);
            }

            // Notify UI-subscribers
            NotifyPropertyChanged("CanScroll");
            NotifyPropertyChanged("CanScrollLeft");
            NotifyPropertyChanged("CanScrollRight");
        }

        /// <summary>
        ///     Returns the left position of the requested child (in Viewport-coordinates).
        /// </summary>
        /// <param name="uieChild"> The child to retrieve the position for. </param>
        private double GetLeftEdge([JetBrains.Annotations.NotNull] UIElement uieChild)
        {
            double dblWidthTotal = 0;

            // Loop through all child controls, summing up their required width
            foreach (UIElement uie in InternalChildren) {
                // The width of the current child control
                var dblWidth = uie.DesiredSize.Width;

                if (Equals(uieChild, uie)) {
                    return dblWidthTotal;
                }

                // Sum up the overall width while the child control in question hasn't been hit.
                dblWidthTotal += dblWidth;
            }

            // This shouldn't really be hit as the requested control should've been found beforehand.
            return dblWidthTotal;
        }

        /// <summary>
        ///     Determines the visible part of the passed child control, measured between 0 (completely invisible) and 1
        ///     (completely visible), that is overflowing into the right invisible portion of the panel.
        /// </summary>
        /// <param name="uieChild"> The child control to be tested. </param>
        /// <returns>
        ///     <para>
        ///         A number between 0 (the control is completely invisible resp. outside of
        ///         the Viewport) and 1 (the control is completely visible).
        ///     </para>
        ///     <para>
        ///         All values between 0 and 1 indicate the part that is visible
        ///         (i.e. 0.4 would mean that 40% of the control is visible, the remaining
        ///         60% will overflow into the right invisible portion of the panel.
        ///     </para>
        /// </returns>
        private double PartlyVisiblePortion_OverflowToRight([JetBrains.Annotations.NotNull] UIElement uieChild)
        {
            var rctIntersect = GetIntersectionRectangle(uieChild);
            double dblVisiblePortion = 1;
            if (rctIntersect != Rect.Empty && CanScrollRight && rctIntersect.Width < uieChild.DesiredSize.Width &&
                rctIntersect.X > 0) {
                dblVisiblePortion = rctIntersect.Width / uieChild.DesiredSize.Width;
            }

            return dblVisiblePortion;
        }

        /// <summary>
        ///     Determines the visible part of the passed child control, measured between 0 (completely invisible) and 1
        ///     (completely visible), that is overflowing into the left invisible portion of the panel.
        /// </summary>
        /// <param name="uieChild"> The child control to be tested. </param>
        /// <returns>
        ///     <para>
        ///         A number between 0 (the control is completely invisible resp. outside of
        ///         the Viewport) and 1 (the control is completely visible).
        ///     </para>
        ///     <para>
        ///         All values between 0 and 1 indicate the part that is visible
        ///         (i.e. 0.4 would mean that 40% of the control is visible, the remaining
        ///         60% will overflow into the left invisible portion of the panel.
        ///     </para>
        /// </returns>
        private double PartlyVisiblePortion_OverflowToLeft([JetBrains.Annotations.NotNull] UIElement uieChild)
        {
            var rctIntersect = GetIntersectionRectangle(uieChild);
            double dblVisiblePortion = 1;
            if (rctIntersect != Rect.Empty && CanScrollLeft && rctIntersect.Width < uieChild.DesiredSize.Width &&
                Math.Abs(rctIntersect.X) < Constants.Ebsilon) {
                dblVisiblePortion = rctIntersect.Width / uieChild.DesiredSize.Width;
            }

            return dblVisiblePortion;
        }

        /// <summary>
        ///     Returns the currently rendered rectangle that makes up the Viewport.
        /// </summary>
        private Rect GetScrollViewerRectangle() => new Rect(new Point(0, 0), ScrollOwner.RenderSize);

        /// <summary>
        ///     Returns the rectangle that defines the outer bounds of a child control.
        /// </summary>
        /// <param name="uieChild"> The child/control for which to return the bounding rectangle. </param>
        private Rect GetChildRectangle([JetBrains.Annotations.NotNull] UIElement uieChild)
        {
            // Retrieve the position of the requested child inside the ScrollViewer control
            var childTransform = uieChild.TransformToAncestor(ScrollOwner);
            return childTransform.TransformBounds(new Rect(new Point(0, 0), uieChild.RenderSize));
        }

        /// <summary>
        ///     Returns a Rectangle that contains the intersection between the ScrollViewer's and the passed child control's
        ///     boundaries, that is, the portion of the child control which is currently visibile within the ScrollViewer's
        ///     Viewport.
        /// </summary>
        /// <param name="uieChild"> The child for which to retrieve Rectangle. </param>
        /// <returns> the return value</returns>
        private Rect GetIntersectionRectangle([JetBrains.Annotations.NotNull] UIElement uieChild)
        {
            // Retrieve the ScrollViewer's rectangle
            var rctScrollViewerRectangle = GetScrollViewerRectangle();
            var rctChildRect = GetChildRectangle(uieChild);

            // Return the area/rectangle in which the requested child and the ScrollViewer control's Viewport intersect.
            return Rect.Intersect(rctScrollViewerRectangle, rctChildRect);
        }

        /// <summary>
        ///     Will remove the OpacityMask for all child controls.
        /// </summary>
        private void RemoveOpacityMasks()
        {
            foreach (UIElement uieChild in Children) {
                RemoveOpacityMask(uieChild);
            }
        }

        /// <summary>
        ///     Will remove the OpacityMask for all child controls.
        /// </summary>
        /// <param name="uieChild">todo: describe uieChild parameter on RemoveOpacityMask</param>
        private static void RemoveOpacityMask([JetBrains.Annotations.NotNull] UIElement uieChild) => uieChild.OpacityMask = null;

        /// <summary>
        ///     Will check all child controls and set their OpacityMasks.
        /// </summary>
        private void UpdateOpacityMasks()
        {
            foreach (UIElement uieChild in Children) {
                UpdateOpacityMask(uieChild);
            }
        }

        /// <summary>
        ///     Takes the given child control and checks as to whether the control is completely visible (in the Viewport). If not
        ///     (i.e. if it's only partially visible), an OpacityMask will be applied so that it fades out into nothingness.
        /// </summary>
        /// <param name="uieChild">todo: describe uieChild parameter on UpdateOpacityMask</param>
        private void UpdateOpacityMask([CanBeNull] UIElement uieChild)
        {
            if (uieChild == null) {
                return;
            }

            // Retrieve the ScrollViewer's rectangle
            var rctScrollViewerRectangle = GetScrollViewerRectangle();
            if (rctScrollViewerRectangle == Rect.Empty) {
                return;
            }

            // Retrieve the child control's rectangle
            var rctChildRect = GetChildRectangle(uieChild);

            if (rctScrollViewerRectangle.Contains(rctChildRect)) {
                // This child is completely visible, so dump the OpacityMask.
                uieChild.OpacityMask = null;
            }
            else {
                var dblPartlyVisiblePortionOverflowToLeft = PartlyVisiblePortion_OverflowToLeft(uieChild);
                var dblPartlyVisiblePortionOverflowToRight = PartlyVisiblePortion_OverflowToRight(uieChild);

                if (dblPartlyVisiblePortionOverflowToLeft < 1 && dblPartlyVisiblePortionOverflowToRight < 1) {
                    uieChild.OpacityMask = new LinearGradientBrush(_gscOpacityMaskStopsTransparentOnLeftAndRight,
                        new Point(0, 0), new Point(1, 0));
                }
                else if (dblPartlyVisiblePortionOverflowToLeft < 1) {
                    uieChild.OpacityMask = new LinearGradientBrush(_gscOpacityMaskStopsTransparentOnLeft,
                        new Point(1 - dblPartlyVisiblePortionOverflowToLeft, 0), new Point(1, 0));
                }
                else if (dblPartlyVisiblePortionOverflowToRight < 1) {
                    uieChild.OpacityMask = new LinearGradientBrush(_gscOpacityMaskStopsTransparentOnRight,
                        new Point(0, 0), new Point(dblPartlyVisiblePortionOverflowToRight, 0));
                }
                else {
                    uieChild.OpacityMask = null;
                }
            }
        }

        #endregion

        #region --- Overrides ---

        /// <summary>
        ///     This is the 1st pass of the layout process. Here, the Extent's size is being determined.
        /// </summary>
        /// <param name="availableSize"> The Viewport's rectangle, as obtained after the 1st pass (MeasureOverride). </param>
        /// <returns> The Viewport's final size. </returns>
        protected override Size MeasureOverride(Size availableSize)
        {
            // The default size will not reflect any width (i.e., no children) and always the default height.
            var resultSize = new Size(0, availableSize.Height);

            // Loop through all child controls ...
            foreach (UIElement uieChild in InternalChildren) {
                // ... retrieve the desired size of the control ...
                uieChild.Measure(availableSize);
                // ... and pass this on to the size we need for the Extent
                resultSize.Width += uieChild.DesiredSize.Width;
            }

            UpdateMembers(resultSize, availableSize);

            var dblNewWidth = double.IsPositiveInfinity(availableSize.Width) ? resultSize.Width : availableSize.Width;

            resultSize.Width = dblNewWidth;
            return resultSize;
        }

        /// <summary>
        ///     This is the 2nd pass of the layout process, where child controls are being arranged within the panel.
        /// </summary>
        /// <param name="finalSize"> The Viewport's rectangle, as obtained after the 1st pass (MeasureOverride). </param>
        /// <returns> The Viewport's final size. </returns>
        protected override Size ArrangeOverride(Size finalSize)
        {
            if (InternalChildren == null || InternalChildren.Count < 1) {
                return finalSize;
            }

            double dblWidthTotal = 0;
            foreach (UIElement uieChild in InternalChildren) {
                var dblWidth = uieChild.DesiredSize.Width;
                uieChild.Arrange(new Rect(dblWidthTotal, 0, dblWidth, uieChild.DesiredSize.Height));
                dblWidthTotal += dblWidth;
            }

            return finalSize;
        }

        protected override void OnVisualChildrenChanged([CanBeNull] DependencyObject visualAdded,
            [CanBeNull] DependencyObject visualRemoved)
        {
            base.OnVisualChildrenChanged(visualAdded, visualRemoved);
            UpdateOpacityMasks();
        }

        protected override void OnChildDesiredSizeChanged([CanBeNull] UIElement child)
        {
            base.OnChildDesiredSizeChanged(child);
            UpdateOpacityMasks();
        }

        #endregion

        #region IScrollInfo Members

        /// <summary>
        ///     Sets or retrieves whether the control is allowed to scroll horizontally.
        /// </summary>
        public bool CanHorizontallyScroll { get; set; } = true;

        /// <summary>
        ///     Sets or retrieves whether the control is allowed to scroll vertically.
        /// </summary>
        /// <remarks>
        ///     This is DISABLED for the control! Due to the internal plumbing of the ScrollViewer control, this property needs to
        ///     be accessible without an exception being thrown; however, setting this property will do plain nothing.
        /// </remarks>
        public bool CanVerticallyScroll {
            // We'll never be able to vertically scroll.
            get { return false; }
#pragma warning disable S108 // Nested blocks of code should not be left empty
#pragma warning disable S3237 // "value" parameters should be used
#pragma warning disable RECS0029 // Warns about property or indexer setters and event adders or removers that do not use the value parameter
            set { }
#pragma warning restore RECS0029 // Warns about property or indexer setters and event adders or removers that do not use the value parameter
#pragma warning restore S3237 // "value" parameters should be used
#pragma warning restore S108 // Nested blocks of code should not be left empty
        }

        /// <summary>
        ///     Retrieves the height of the control; since no vertical scrolling has been implemented, this will return the same
        ///     value at all times.
        /// </summary>
        public double ExtentHeight => Extent.Height;

        /// <summary>
        ///     Retrieves the overall width of the content hosted in the panel (i.e., the width measured between [far left of the
        ///     scrollable portion] and [far right of the scrollable portion].
        /// </summary>
        public double ExtentWidth => Extent.Width;

        /// <summary>
        ///     Retrieves the current horizontal scroll offset.
        /// </summary>
        /// <remarks>
        ///     The setter is private to the class.
        /// </remarks>
        public double HorizontalOffset {
            get => _vOffset.X;
            private set => _vOffset.X = value;
        }

        /// <summary>
        ///     Increments the vertical offset.
        /// </summary>
        /// <remarks>
        ///     This is unsupported.
        /// </remarks>
        public void LineDown()
        {
            // not supported
        }

        /// <summary>
        ///     Decrements the horizontal offset by the amount specified in the <see cref="LineScrollPixelCount" /> property.
        /// </summary>
        public void LineLeft() => SetHorizontalOffset(HorizontalOffset - LineScrollPixelCount);

        /// <summary>
        ///     Increments the horizontal offset by the amount specified in the <see cref="LineScrollPixelCount" /> property.
        /// </summary>
        public void LineRight() => SetHorizontalOffset(HorizontalOffset + LineScrollPixelCount);

        /// <summary>
        ///     Decrements the vertical offset.
        /// </summary>
        /// <remarks>
        ///     This is unsupported.
        /// </remarks>
        public void LineUp()
        {
            // not supported
        }

        /// <summary>
        ///     Scrolls a child of the panel (Visual) into view.
        /// </summary>
        /// <param name="visual">todo: describe visual parameter on MakeVisible</param>
        /// <param name="rectangle">todo: describe rectangle parameter on MakeVisible</param>
        public Rect MakeVisible([CanBeNull] Visual visual, Rect rectangle)
        {
            var rect = rectangle;
            if (rect.IsEmpty || visual == null || Equals(visual, this) || !IsAncestorOf(visual)) {
                return Rect.Empty;
            }

            double dblOffsetX = 0;
            UIElement uieControlToMakeVisible = null;
            for (var i = 0; i < InternalChildren.Count; i++) {
                if (Equals(InternalChildren[i], visual)) {
                    uieControlToMakeVisible = InternalChildren[i];
                    dblOffsetX = GetLeftEdge(InternalChildren[i]);
                    break;
                }
            }

            // Set the offset only if the desired element is not already completely visible.
            if (uieControlToMakeVisible != null) {
                if (Equals(uieControlToMakeVisible, InternalChildren[0])) {
                    dblOffsetX = 0;
                }
                else if (Equals(uieControlToMakeVisible, InternalChildren[InternalChildren.Count - 1])) {
                    dblOffsetX = ExtentWidth - Viewport.Width;
                }
                else {
                    dblOffsetX = CalculateNewScrollOffset(HorizontalOffset, HorizontalOffset + Viewport.Width,
                        dblOffsetX, dblOffsetX + uieControlToMakeVisible.DesiredSize.Width);
                }

                SetHorizontalOffset(dblOffsetX);
                rect = new Rect(HorizontalOffset, 0, uieControlToMakeVisible.DesiredSize.Width, Viewport.Height);
            }

            return rect;
        }

        public void MouseWheelDown()
        {
            // We won't be responding to the mouse-wheel.
        }

        public void MouseWheelLeft()
        {
            // We won't be responding to the mouse-wheel.
        }

        public void MouseWheelRight()
        {
            // We won't be responding to the mouse-wheel.
        }

        public void MouseWheelUp()
        {
            // We won't be responding to the mouse-wheel.
        }

        public void PageDown()
        {
            // We won't be responding to vertical paging.
        }

        public void PageLeft()
        {
            // We won't be responding to horizontal paging.
        }

        public void PageRight()
        {
            // We won't be responding to horizontal paging.
        }

        public void PageUp()
        {
            // We won't be responding to vertical paging.
        }

        /// <summary>
        ///     Sets or retrieves the ScrollViewer control that hosts the panel.
        /// </summary>
        [JetBrains.Annotations.NotNull]
        public ScrollViewer ScrollOwner {
            get => _svOwningScrollViewer;
            set {
                _svOwningScrollViewer = value;
                if (_svOwningScrollViewer != null) {
                    ScrollOwner.Loaded += ScrollOwner_Loaded;
                }
                else {
                    ScrollOwner.Loaded -= ScrollOwner_Loaded;
                }
            }
        }

        public void SetHorizontalOffset(double offset)
        {
            // Remove all OpacityMasks while scrolling.
            RemoveOpacityMasks();

            // Assure that the horizontal offset always contains a valid value
            HorizontalOffset = Math.Max(0, Math.Min(ExtentWidth - Viewport.Width, Math.Max(0, offset)));

            ScrollOwner.InvalidateScrollInfo();

            // If you don't want the animation, you would replace all the code further below (up to but not including)
            // the call to InvalidateMeasure() with the following line:
            // _ttScrollTransform.X = (-HorizontalOffset)

            // Animate the new offset
            var daScrollAnimation = new DoubleAnimation(_ttScrollTransform.X, -HorizontalOffset,
                new Duration(AnimationTimeSpan), FillBehavior.HoldEnd) {
                // Note that, depending on distance between the original and the target scroll-position and
                // the duration of the animation, the  acceleration and deceleration effects might be more
                // or less unnoticeable at runtime.
                AccelerationRatio = 0.5,
                DecelerationRatio = 0.5
            };

            // The childrens' OpacityMask can only be set reliably after the scroll-animation
            // has finished its work, so attach to the animation's Completed event where the
            // masks will be re-created.
            daScrollAnimation.Completed += DaScrollAnimationCompleted;

            _ttScrollTransform.BeginAnimation(TranslateTransform.XProperty, daScrollAnimation, HandoffBehavior.Compose);

            InvalidateMeasure();
        }

        public void SetVerticalOffset(double offset)
        {
            throw new InvalidOperationException();
        }

        public double VerticalOffset => 0;

        public double ViewportHeight => Viewport.Height;

        public double ViewportWidth => Viewport.Width;

        #endregion

        #region --- Additional Properties ---

        /// <summary>
        ///     Retrieves the overall resp. internal/inner size of the control/panel.
        /// </summary>
        /// <remarks>
        ///     The setter is private to the class.
        /// </remarks>
        [UsedImplicitly]
        public Size Extent { get; private set; } = new Size(0, 0);

        /// <summary>
        ///     Retrieves the outer resp. visible size of the control/panel.
        /// </summary>
        /// <remarks>
        ///     The setter is private to the class.
        /// </remarks>
        [UsedImplicitly]
        public Size Viewport { get; private set; } = new Size(0, 0);

        /// <summary>
        ///     Retrieves whether the panel's scroll-position is on the far left (i.e. cannot scroll further to the left).
        /// </summary>
        [UsedImplicitly]

        public bool IsOnFarLeft => Math.Abs(HorizontalOffset) < 0.000001;

        /// <summary>
        ///     Retrieves whether the panel's scroll-position is on the far right (i.e. cannot scroll further to the right).
        /// </summary>
        [UsedImplicitly]
        public bool IsOnFarRight => Math.Abs(HorizontalOffset + Viewport.Width - ExtentWidth) < 0.0000001;

        /// <summary>
        ///     Retrieves whether the panel's viewport is larger than the control's extent, meaning there is hidden content that
        ///     the user would have to scroll for in order to see it.
        /// </summary>
        [UsedImplicitly]
        public bool CanScroll => ExtentWidth > Viewport.Width;

        /// <summary>
        ///     Retrieves whether the panel's scroll-position is NOT on the far left (i.e. can scroll to the left).
        /// </summary>
        public bool CanScrollLeft => CanScroll && !IsOnFarLeft;

        /// <summary>
        ///     Retrieves whether the panel's scroll-position is NOT on the far right (i.e. can scroll to the right).
        /// </summary>
        public bool CanScrollRight => CanScroll && !IsOnFarRight;

        #endregion

        #region --- Additional Dependency Properties ---

        [CanBeNull] public static readonly DependencyProperty RightOverflowMarginProperty =
            DependencyProperty.Register("RightOverflowMargin", typeof(int), typeof(ScrollableTabPanel),
                new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender));

#pragma warning disable WPF0001 // Backing field for a DependencyProperty should match registered name.
        [CanBeNull] public static readonly DependencyProperty AnimationTimeSpanProperty =
#pragma warning restore WPF0001 // Backing field for a DependencyProperty should match registered name.
            DependencyProperty.Register("AnimationTimeSpanProperty", typeof(TimeSpan), typeof(ScrollableTabPanel),
                new FrameworkPropertyMetadata(new TimeSpan(0, 0, 0, 0, 100),
                    FrameworkPropertyMetadataOptions.AffectsRender));

        // The amount of pixels to scroll by for the LineLeft() and LineRight() methods.
        [CanBeNull] public static readonly DependencyProperty LineScrollPixelCountProperty =
            DependencyProperty.Register("LineScrollPixelCount", typeof(int), typeof(ScrollableTabPanel),
                new FrameworkPropertyMetadata(15, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        ///     Sets or retrieves the Margin that will be applied to the rightmost item in the panel; This allows for the item
        ///     applying a negative margin, i.e. when selected. If set to a value other than zero (being the default), the control
        ///     will add the value specified here to the item's right extent.
        /// </summary>
        [UsedImplicitly]
        public int RightOverflowMargin {
            get => (int) GetValue(RightOverflowMarginProperty);
            set => SetValue(RightOverflowMarginProperty ?? throw new InvalidOperationException(), value);
        }

        /// <summary>
        ///     Sets or retrieves the the duration (default: 100ms) for the panel's transition-animation that is started when an
        ///     item is selected (scroll from the previously selected item to the presently selected one).
        /// </summary>
        [UsedImplicitly]
        public TimeSpan AnimationTimeSpan {
            get => (TimeSpan) GetValue(AnimationTimeSpanProperty);
            set => SetValue(AnimationTimeSpanProperty ?? throw new InvalidOperationException(), value);
        }

        /// <summary>
        ///     Sets or retrieves the count of pixels to scroll by when the LineLeft or LineRight methods are called (default:
        ///     15px).
        /// </summary>
        [UsedImplicitly]
        public int LineScrollPixelCount {
            get => (int) GetValue(LineScrollPixelCountProperty);
            set => SetValue(LineScrollPixelCountProperty ?? throw new InvalidOperationException(), value);
        }

        #endregion

        #region --- INotifyPropertyChanged ---

        /// <summary>
        ///     Raised when a property on this object has a new value.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        ///     Called from within this class whenever subscribers (i.e. bindings) are to be notified of a property-change
        /// </summary>
        /// <param name="strPropertyName"> The name of the property that has changed. </param>
        private void NotifyPropertyChanged([CanBeNull] string strPropertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(strPropertyName));

        #endregion

        #region --- Event Handlers ---

        /// <summary>
        ///     Fired when the ScrollViewer is initially loaded/displayed. Required in order to initially setup the childrens'
        ///     OpacityMasks.
        /// </summary>
        /// <param name="sender">todo: describe sender parameter on ScrollOwner_Loaded</param>
        /// <param name="e">todo: describe e parameter on ScrollOwner_Loaded</param>
        private void ScrollOwner_Loaded([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) =>
            UpdateOpacityMasks();

        /// <summary>
        ///     Fired when the scroll-animation has finished its work, that is, at the point in time when the ScrollViewerer has
        ///     reached its final scroll-position resp. offset, which is when the childrens' OpacityMasks can be updated.
        /// </summary>
        /// <param name="sender">todo: describe sender parameter on DaScrollAnimationCompleted</param>
        /// <param name="e">todo: describe e parameter on DaScrollAnimationCompleted</param>
        private void DaScrollAnimationCompleted([CanBeNull] object sender, [CanBeNull] EventArgs e)
        {
            UpdateOpacityMasks();

            // This is required in order to update the TabItems' FocusVisual
            foreach (UIElement uieChild in InternalChildren) {
                uieChild.InvalidateArrange();
            }
        }

        private void ScrollableTabPanel_SizeChanged([CanBeNull] object sender, [CanBeNull] SizeChangedEventArgs e) =>
            UpdateOpacityMasks();

        #endregion
    }
}
#pragma warning restore SA1615 // Element return value must be documented