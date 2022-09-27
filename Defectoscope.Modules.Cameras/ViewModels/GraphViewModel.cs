using LaserScan.Core.NetStandart.Models;

using OxyPlot;
using OxyPlot.Series;

using Prism.Commands;
using Prism.Mvvm;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Threading;

namespace Defectoscope.Modules.Cameras.ViewModels
{
    public class GraphViewModel : BindableBase
    {
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


        public GraphViewModel(BaslerCameraModel currentCamera)
        {
            CurrentCamera = currentCamera;
            PlotModel = new PlotModel() { Title = CurrentCamera.ID };
            PlotModel.Series.Add(new LineSeries());
            PlotModel.Series[0].Title = Text;
            PlotModel.InvalidatePlot(true);
            //  (PlotModel.Series[0] as LineSeries).Points.AddRange(points);
            DispatcherTimer uiTimer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(200) };
            uiTimer.Tick += UiTimer_Tick;
            uiTimer.Start();
        }

        private void UiTimer_Tick(object sender, EventArgs e)
        {
            //List<DataPoint> points = new List<DataPoint>
            //                  {
            //                      new DataPoint(0, 4),
            //                      new DataPoint(10, 13),
            //                      new DataPoint(20, 15),
            //                      new DataPoint(30, 16),
            //                      new DataPoint(40, 12),
            //                      new DataPoint(50, 12),
            //                        new DataPoint(6144, 255)
            //                  };

            (PlotModel.Series[0] as LineSeries).Points.Clear();
            if (CurrentCamera.GraphPoints.Count==6144)
            {
                (PlotModel.Series[0] as LineSeries).Points.AddRange(CurrentCamera.GraphPoints);
            }
        
        }
    }
}
