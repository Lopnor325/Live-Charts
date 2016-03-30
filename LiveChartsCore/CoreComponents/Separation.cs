using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using LiveCharts.CoreComponents;

//The MIT License(MIT)

//copyright(c) 2016 Alberto Rodriguez

//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:

//The above copyright notice and this permission notice shall be included in all
//copies or substantial portions of the Software.

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//SOFTWARE.

namespace LiveCharts
{
    internal class Separation
    {
        private readonly TimeSpan _anSpeed = TimeSpan.FromMilliseconds(500);

        internal TextBlock TextBlock { get; set; }
        internal Line Line { get; set; }
        internal double Value { get; set; }
        internal SeparationState State { get; set; }
        internal bool IsNew { get; set; }
        internal int AxisPosition { get; set; }

        public void Place(Chart chart, AxisTags direction, int axisIndex, Axis axis)
        {
            switch (State)
            {
                case SeparationState.Remove:
                    MoveFromCurrentAx(chart, direction, axisIndex, axis);
                    FadeOutAndRemove(chart);
                    break;
                case SeparationState.DrawOrKeep:
                    Place(chart, direction, axisIndex);
                    MoveFromPreviousAx(chart, direction, axisIndex, axis);
                    if (IsNew) FadeIn();
                    break;
                case SeparationState.InitialAdd:
                    Place(chart, direction, axisIndex);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void Place(Chart chart, AxisTags direction, int axisIndex)
        {
            var i = chart.ToPlotArea(Value, direction, axisIndex);

            Line.X1 = direction == AxisTags.Y
                ? chart.PlotArea.X
                : i;
            Line.X2 = direction == AxisTags.Y
                ? chart.PlotArea.X + chart.PlotArea.Width
                : i;

            Line.Y1 = direction == AxisTags.Y
                ? i
                : chart.PlotArea.Y;
            Line.Y2 = direction == AxisTags.Y
                ? i
                : chart.PlotArea.Y + chart.PlotArea.Height;

            if (direction == AxisTags.Y)
            {
                Canvas.SetTop(TextBlock, i - TextBlock.ActualHeight*.5);
            }
            else
            {
                Canvas.SetLeft(TextBlock, i - TextBlock.ActualWidth*.5);
            }
        }

        private void FadeOutAndRemove(Chart chart)
        {
            if (chart.DisableAnimation)
            {
                chart.Canvas.Children.Remove(TextBlock);
                chart.Canvas.Children.Remove(Line);
                return;
            }

            var anim = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = _anSpeed
            };

            anim.Completed += (sender, args) =>
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    chart.Canvas.Children.Remove(TextBlock);
                    chart.Canvas.Children.Remove(Line);
                }));
            };

            TextBlock.BeginAnimation(UIElement.OpacityProperty, anim);
            Line.BeginAnimation(UIElement.OpacityProperty, new DoubleAnimation(1, 0, _anSpeed));
        }

        private void FadeIn()
        {
            TextBlock.BeginAnimation(UIElement.OpacityProperty, new DoubleAnimation(0, 1, _anSpeed));
            Line.BeginAnimation(UIElement.OpacityProperty, new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(1000)));
        }

        private void MoveFromCurrentAx(Chart chart, AxisTags direction, int axisIndex, Axis axis)
        {
            var i = chart.ToPlotArea(Value, direction, axisIndex);

            if (direction == AxisTags.Y)
            {
                Line.BeginAnimation(Line.X1Property,
                    new DoubleAnimation(Line.X1, chart.PlotArea.X, _anSpeed));
                Line.BeginAnimation(Line.X2Property,
                    new DoubleAnimation(Line.X2, chart.PlotArea.X + chart.PlotArea.Width, _anSpeed));
                Line.BeginAnimation(Line.Y1Property,
                    new DoubleAnimation(Line.Y1, i, _anSpeed));
                Line.BeginAnimation(Line.Y2Property,
                    new DoubleAnimation(Line.Y2, i, _anSpeed));
            }
            else
            {
                Line.BeginAnimation(Line.X1Property,
                    new DoubleAnimation(Line.X1, i, _anSpeed));
                Line.BeginAnimation(Line.X2Property,
                    new DoubleAnimation(Line.X2, i, _anSpeed));
                Line.BeginAnimation(Line.Y1Property,
                    new DoubleAnimation(Line.Y1, chart.PlotArea.Y, _anSpeed));
                Line.BeginAnimation(Line.Y2Property,
                    new DoubleAnimation(Line.Y2, chart.PlotArea.Y + chart.PlotArea.Height, _anSpeed));
            }

            if (direction == AxisTags.Y)
            {
                TextBlock.BeginAnimation(Canvas.TopProperty,
                    new DoubleAnimation(i - TextBlock.ActualHeight * .5, _anSpeed));
            }
            else
            {
                TextBlock.BeginAnimation(Canvas.LeftProperty,
                    new DoubleAnimation(i - TextBlock.ActualWidth * .5, _anSpeed));
            }
        }

        private void MoveFromPreviousAx(Chart chart, AxisTags direction, int axisIndex, Axis axis)
        {
            var i = chart.ToPlotArea(Value, direction, axisIndex);

            if (direction == AxisTags.Y)
            {
                var y1 = IsNew ? axis.FromLastAxis(Value, direction, chart) : Line.Y1;
                var y2 = IsNew ? axis.FromLastAxis(Value, direction, chart) : Line.Y2;

                Line.BeginAnimation(Line.X1Property,
                    new DoubleAnimation(Line.X1, chart.PlotArea.X, _anSpeed));
                Line.BeginAnimation(Line.X2Property,
                    new DoubleAnimation(Line.X2, chart.PlotArea.X + chart.PlotArea.Width, _anSpeed));
                Line.BeginAnimation(Line.Y1Property,
                    new DoubleAnimation(y1, i, _anSpeed));
                Line.BeginAnimation(Line.Y2Property,
                    new DoubleAnimation(y2, i, _anSpeed));
            }
            else
            {
                var x1 = IsNew ? axis.FromLastAxis(Value, direction, chart) : Line.X1;
                var x2 = IsNew ? axis.FromLastAxis(Value, direction, chart) : Line.X2;

                Line.BeginAnimation(Line.X1Property,
                    new DoubleAnimation(x1, i, _anSpeed));
                Line.BeginAnimation(Line.X2Property,
                    new DoubleAnimation(x2, i, _anSpeed));
                Line.BeginAnimation(Line.Y1Property, 
                    new DoubleAnimation(Line.Y1, chart.PlotArea.Y, _anSpeed));
                Line.BeginAnimation(Line.Y2Property,
                    new DoubleAnimation(Line.Y2, chart.PlotArea.Y + chart.PlotArea.Height, _anSpeed));
            }

            if (direction == AxisTags.Y)
            {
                TextBlock.BeginAnimation(Canvas.TopProperty,
                    new DoubleAnimation(i - TextBlock.ActualHeight*.5, _anSpeed));
            }
            else
            {
                TextBlock.BeginAnimation(Canvas.LeftProperty,
                    new DoubleAnimation(i - TextBlock.ActualWidth*.5, _anSpeed));
            }
        }
    }

    internal enum SeparationState
    {
        Remove,
        DrawOrKeep,
        InitialAdd
    }
}