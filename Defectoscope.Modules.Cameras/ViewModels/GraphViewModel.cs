using LaserScan.Core.NetStandart.Models;

using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

using Prism.Commands;
using Prism.Mvvm;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using System.Windows.Threading;

namespace Defectoscope.Modules.Cameras.ViewModels
{
    public class GraphViewModel : BindableBase
    {
        private int _graphPointsCounter;
        public int GraphPointsCounter
        {
            get { return _graphPointsCounter; }
            set { SetProperty(ref _graphPointsCounter, value); }
        }
        /// <summary>
        /// Настройки текущей камеры
        /// </summary>
        private BaslerCameraModel _currentCamera;
        public BaslerCameraModel CurrentCamera
        {
            get { return _currentCamera; }
            set { SetProperty(ref _currentCamera, value); }
        }
        private string _text;
        public string Text
        {
            get { return _text; }
            set { SetProperty(ref _text, value); }
        }

        private PlotModel _plotModel;
        public PlotModel PlotModel
        {
            get { return _plotModel; }
            set { SetProperty(ref _plotModel, value); }
        }

        Timer uiTimer;
        DispatcherTimer _visualTimer;
        public GraphViewModel(BaslerCameraModel currentCamera)
        {
            CurrentCamera = currentCamera;
            PlotModel = new PlotModel() { Title = CurrentCamera.ID, Background = OxyColors.White };
            LinearAxis xAxes = new LinearAxis() { Position = AxisPosition.Bottom, Minimum = 0, Maximum = 70 };
            PlotModel.Axes.Add(xAxes);
            LinearAxis yAxes = new LinearAxis() { Position = AxisPosition.Left, Minimum = 0, Maximum = 255 };
            PlotModel.Axes.Add(yAxes);
            PlotModel.Series.Add(new LineSeries());
            //     PlotModel.Series[0].Background = OxyColors.Black;
            PlotModel.Series.Add(new LineSeries());
            //  PlotModel.Series[1].Background = OxyColors.Red;
            PlotModel.Series.Add(new LineSeries());
            //    PlotModel.Series[2].Background = OxyColors.Green;
            PlotModel.Series.Add(new LineSeries());
            PlotModel.Series.Add(new LineSeries());

            PlotModel.Series[0].Title = Text;

            //  (PlotModel.Series[0] as LineSeries).Points.AddRange(points);
            uiTimer = new Timer(400);
            uiTimer.Elapsed += UiTimer_Tick;
            uiTimer.Start();


            _visualTimer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(1) };
            _visualTimer.Tick += _visualTimer_Tick;
            _visualTimer.Start();
        }



        int _counter = 0;
        private void _visualTimer_Tick(object sender, EventArgs e)
        {
            GraphPointsCounter = _counter;
        }

        private void UiTimer_Tick(object sender, EventArgs e)
        {
            //Выключение таймера
            try
            {
                uiTimer.Enabled = false;
            }
            catch
            {
                return;
            }

            if (PlotModel.PlotView != null)
            {
                List<DataPoint> points = new List<DataPoint>();
                if (CurrentCamera.GraphPointsQueue.Count > 0 && CurrentCamera.GraphPointsQueue.TryDequeue(out points))
                {
                    _counter += 1;
                    (PlotModel.Series[0] as LineSeries).Points.Clear();
                    (PlotModel.Series[1] as LineSeries).Points.Clear();

                    (PlotModel.Series[2] as LineSeries).Points.Clear();
                    //Верхняя граница
                    (PlotModel.Series[3] as LineSeries).Points.Clear();
                    //Нижняя граница
                    (PlotModel.Series[4] as LineSeries).Points.Clear();
                    (PlotModel.Series[0] as LineSeries).Points.AddRange(points);
                    for (int k = 0; k < points.Count; k++)
                    {
                        //     DataPoint multiPoint = new DataPoint(k, UsingMultiCalibrationDeltas(points[k].Y, k));
                        //     (PlotModel.Series[1] as LineSeries).Points.Add(multiPoint);
                        //      DataPoint summPoints = new DataPoint(k, UsingCalibrationDeltas(CurrentCamera.GraphPoints[k].Y, k));
                        //      (PlotModel.Series[2] as LineSeries).Points.Add(summPoints);
                        (PlotModel.Series[3] as LineSeries).Points.Add(new DataPoint(k, CurrentCamera.UpThreshold));
                        (PlotModel.Series[4] as LineSeries).Points.Add(new DataPoint(k, CurrentCamera.DownThreshold));
                        //  CurrentCamera.GraphPointsQueue.Clear();
                    }
                    //    CurrentCamera.GraphPointsQueue.Clear();
                    PlotModel.InvalidatePlot(true);
                }
            }
            //включение таймера
            try
            {
                uiTimer.Enabled = true;
            }
            catch
            {
                return;
            }
        }

        /// <summary>
        /// Калибровка умножением
        /// </summary>
        /// <param name="currentByte"></param>
        /// <param name="i"></param>
        /// <returns></returns>
        private double UsingMultiCalibrationDeltas(double currentValue, int i)
        {
            if ((currentValue * CurrentCamera.MultipleDeltas[i]) >= 255)
            {
                currentValue = 255;
            }
            else
                currentValue = (currentValue * CurrentCamera.MultipleDeltas[i]);
            return currentValue;
        }
        /// <summary>
        /// Калибровка сложением
        /// </summary>
        /// <param name="currentByte"></param>
        /// <param name="i"></param>
        /// <returns></returns>
        private double UsingCalibrationDeltas(double currentByte, int i)
        {
            if ((currentByte + CurrentCamera.Deltas[i]) >= 255)
            {
                currentByte = 255;
            }
            else
                if (currentByte + CurrentCamera.Deltas[i] <= 0)
            {
                currentByte = 0;
            }
            else
            {
                currentByte = (byte)((sbyte)currentByte + CurrentCamera.Deltas[i]);
            }
            return currentByte;
        }
    }
}
