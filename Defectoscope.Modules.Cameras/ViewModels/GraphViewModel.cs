using OxyPlot;
using OxyPlot.Series;

using Prism.Commands;
using Prism.Mvvm;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Defectoscope.Modules.Cameras.ViewModels
{
    public class GraphViewModel : BindableBase
    {
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

        public GraphViewModel(string name)
        {
            PlotModel = new PlotModel() { Title = name };
            List<DataPoint> points = new List<DataPoint>
                              {
                                  new DataPoint(0, 4),
                                  new DataPoint(10, 13),
                                  new DataPoint(20, 15),
                                  new DataPoint(30, 16),
                                  new DataPoint(40, 12),
                                  new DataPoint(50, 12),
                                    new DataPoint(6144, 255)
                              };
            PlotModel.Series.Add(new LineSeries());
            PlotModel.Series[0].Title = Text;
            (PlotModel.Series[0] as LineSeries).Points.AddRange(points);
        }
    }
}
