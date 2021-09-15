using Kogerent.Components;
using Kogerent.Core;
using Kogerent.Services.Interfaces;
using Prism.Commands;
using Prism.Regions;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Kogerent.Services.Implementation
{
    public class Synchronizer : RegionViewModelBase, ISynchronizer
    {
        private CyclicTimer _timer;

        List<SensorPair> _pairs;

        public ISensorRepository SensorRepository { get; }
        public IApplicationCommands ApplicationCommands { get; }

        private bool _syncButtonIsChecked;
        public bool SyncButtonIsChecked
        {
            get { return _syncButtonIsChecked; }
            set
            {
                SetProperty(ref _syncButtonIsChecked, value);
                if (_syncButtonIsChecked) _timer.Start();
                else _timer.Stop();
            }
        }

        private double _timerHz = 100;
        public double TimerHz
        {
            get { return _timerHz; }
            set
            {
                SetProperty(ref _timerHz, value);
                _timer.Interval = _timerHz;
            }
        }

        private DelegateCommand _destroyCommand;
        public DelegateCommand DestroyCommand => _destroyCommand ??= new DelegateCommand(ExecuteDestroyCommand);

        

        public Synchronizer(IRegionManager regionManager, ISensorRepository sensorRepository, 
                            IApplicationCommands applicationCommands) : base(regionManager)
        {
            _timer = new CyclicTimer(TimerHz, SynchronizerTimerTick);
            SensorRepository = sensorRepository;
            _pairs = SensorRepository.SensorPairs;
            ApplicationCommands = applicationCommands;
            ApplicationCommands.DisposeAllSensors.RegisterCommand(DestroyCommand);
        }

        private async void SynchronizerTimerTick(object sender, System.EventArgs e)
        {
            for (int i = 0; i < _pairs.Count; i++)
            {
                var pair = _pairs[i];
                if (i % 2 == 0)
                {
                    pair.TopSensor.CanSendSync = true;
                    pair.BottomSensor.CanSendSync = true;
                }
                else
                {
                    pair.TopSensor.CanSendSync = false;
                    pair.BottomSensor.CanSendSync = false;
                }
            }

            await Task.Run(() => { ApplicationCommands.SendSyncEvenUnEven.Execute(null); });

            await Task.Delay(3);

            for (int i = 0; i < _pairs.Count; i++)
            {
                var pair = _pairs[i];
                if (i % 2 == 1)
                {
                    pair.TopSensor.CanSendSync = true;
                    pair.BottomSensor.CanSendSync = true;
                }
                else
                {
                    pair.TopSensor.CanSendSync = false;
                    pair.BottomSensor.CanSendSync = false;
                }
            }

            await Task.Run(() => { ApplicationCommands.SendSyncEvenUnEven.Execute(null); });
        }

        private void ExecuteDestroyCommand()
        {
            Destroy();
        }

        public override void Destroy()
        {
            _timer.Stop();
            _timer.CyclicAction -= SynchronizerTimerTick;
            base.Destroy();
        }
    }
}
