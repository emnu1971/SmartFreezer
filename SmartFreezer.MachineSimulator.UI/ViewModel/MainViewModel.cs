using Prism.Commands;
using Prism.Mvvm;
using SmartFreezer.Domain.Models;
using SmartFreezer.EventHub.Sender;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace SmartFreezer.MachineSimulator.UI.ViewModel
{
    public class MainViewModel : BindableBase
    {
        #region Storage

        private IFreezerDataSender _freezerDataSender;
        private DispatcherTimer _dispatcherTimer;

        private string _city;

        public string City
        {
            get { return _city; }
            set
            {
                _city = value;
                RaisePropertyChanged();
            }
        }

        private string _serialNumber;

        public string SerialNumber
        {
            get { return _serialNumber; }
            set
            {
                _serialNumber = value;
                RaisePropertyChanged();
            }
        }

        private int _maxContent;

        public int MaxContent
        {
            get { return _maxContent; }
            set
            {
                _maxContent = value;
                RaisePropertyChanged();
            }
        }

        private bool _freezerState;

        public bool FreezerState
        {
            get { return _freezerState; }
            set
            {
                _freezerState = value;
                RaisePropertyChanged();
            }
        }

        private int _freezerContent;

        public int FreezerContent
        {
            get { return _freezerContent; }
            set
            {
                _freezerContent = value;
                RaisePropertyChanged();
            }
        }

        private bool _isSendingPeriodically;

        public bool IsSendingPeriodically
        {
            get { return _isSendingPeriodically; }
            set
            {
                if (_isSendingPeriodically != value)
                {
                    _isSendingPeriodically = value;
                    if (_isSendingPeriodically)
                    {
                        _dispatcherTimer.Start();
                    }
                    else
                    {
                        _dispatcherTimer.Stop();
                    }

                    RaisePropertyChanged();
                }
            }
        }

        private int _freezerTemp;

        public int FreezerTemp
        {
            get { return _freezerTemp; }
            set
            {
                _freezerTemp = value;
                RaisePropertyChanged();
            }
        }

        private int _coolLiquidLevel;

        public int CoolLiquidLevel
        {
            get { return _coolLiquidLevel; }
            set
            {
                _coolLiquidLevel = value;
                RaisePropertyChanged();
            }
        }

        public ICommand AddContentCommand { get; private set; }
        public ICommand RemoveContentCommand { get; private set; }
        public ICommand FreezerStateCommand { get; private set; }

        public ObservableCollection<string> EventLogs { get; private set; }

        #endregion Storage


        #region C'tor

        public MainViewModel(IFreezerDataSender freezerDataSender)
        {
            // initialize datasender
            _freezerDataSender = freezerDataSender;

            // initialize properties
            this.SerialNumber = Guid.NewGuid().ToString().Substring(0, 8);
            this.FreezerTemp = 15;
            this.CoolLiquidLevel = 80;
            this.FreezerState = true;
            this.City = "Brussels";
            this.MaxContent = 10;

            // initialize commands
            this.AddContentCommand = new DelegateCommand(AddContentAsync);
            this.RemoveContentCommand = new DelegateCommand(RemoveContentAsync);
            this.FreezerStateCommand = new DelegateCommand(ChangeFreezerStateAsync);

            // initialize event_logging
            this.EventLogs = new ObservableCollection<string>();

            // timer for timed events (freezertemp & cool liquid level)
            _dispatcherTimer = new DispatcherTimer()
            {
                Interval = TimeSpan.FromSeconds(5)
            };
            _dispatcherTimer.Tick += DispatcherTimer_Tick;

        }

       

        private async void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            var freezerTempEventData = CreateFreezerEventData(nameof(FreezerTemp), FreezerTemp);
            var coolLiquidLevelEventData = CreateFreezerEventData(nameof(CoolLiquidLevel), CoolLiquidLevel);

            // send and log freezer temp and cool liquid level on periodical intervals
            await SendTelemetryDataAsync(new[] { freezerTempEventData, coolLiquidLevelEventData });
        }

        #endregion C'tor

        #region Command Implementation

        private async void AddContentAsync()
        {
            FreezerContent++;
            FreezerEventData freezerEventData = CreateFreezerEventData(nameof(FreezerContent), FreezerContent);
            //send & log
            await SendTelemetryDataAsync(freezerEventData);
        }

        private async void RemoveContentAsync()
        {
            FreezerContent--;
            FreezerEventData freezerEventData = CreateFreezerEventData(nameof(FreezerContent), FreezerContent);
            //send & log
            await SendTelemetryDataAsync(freezerEventData);
        }

        private async void ChangeFreezerStateAsync()
        {
            if(FreezerState == false && IsSendingPeriodically)
            {
                // stop sending periodically because freezer is down ...
                IsSendingPeriodically = false;
            }

            FreezerEventData freezerEventData = CreateFreezerEventData(nameof(FreezerState), FreezerState);
            //send & log
            await SendTelemetryDataAsync(freezerEventData);

        }

        #endregion Command Implementation

        #region Private Interface

        private async Task SendTelemetryDataAsync(FreezerEventData freezerEventData)
        {
            try
            {
                await _freezerDataSender.SendEventDataAsync(freezerEventData);
                WriteLog($"Send event-data: {freezerEventData}");
            }
            catch (Exception ex)
            {
                WriteLog($"Exception: {ex.Message}");
            }
            
        }

        private async Task SendTelemetryDataAsync(IEnumerable<FreezerEventData> freezerEventDatas)
        {
            try
            {
                await _freezerDataSender.SendEventDataAsync(freezerEventDatas);
                foreach (var freezerEventData in freezerEventDatas)
                {
                    WriteLog($"Send event-data: {freezerEventData}");
                }
                
            }
            catch (Exception ex)
            {
                WriteLog($"Exception: {ex.Message}");
            }

        }

        private void WriteLog(string logMessage)
        {
            this.EventLogs.Insert(0, logMessage);
        }

        private FreezerEventData CreateFreezerEventData(string sensorType, int sensorValue)
        {
            return new FreezerEventData
            {
                City = City,
                SerialNumber = SerialNumber,
                SensorType = sensorType,
                SensorValue = sensorType == nameof(FreezerTemp) ? sensorValue * -1 : sensorValue,
                RecordingTime = DateTime.UtcNow,
                MaxContent = MaxContent
            };
        }

        private FreezerEventData CreateFreezerEventData(string sensorType, bool sensorValue)
        {
            return new FreezerEventData
            {
                City = City,
                SerialNumber = SerialNumber,
                SensorType = sensorType,
                SensorValue = sensorValue ? 1 : 0,
                RecordingTime = DateTime.UtcNow,
                MaxContent = MaxContent
            };
        }

        #endregion Private Interface

    }
}
