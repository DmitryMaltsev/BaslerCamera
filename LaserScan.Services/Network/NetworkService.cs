using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using Kogerent.Core;
using Kogerent.Services.Interfaces;

namespace Kogerent.Services.Implementation
{
    public class NetworkService:INetworkService
    {
        public NetworkService(IFooterRepository footerRepository, ISensorRepository sensorRepository)
        {
            FooterRepository = footerRepository;
            SensorRepository = sensorRepository;
        }

        public IFooterRepository FooterRepository { get; }
        public ISensorRepository SensorRepository { get; }

        public async void ExecuteScanNetwork(int startIp, int endIp, int myIp)
        {
            List<Task<string>> tasks = new();
            ConcurrentBag<string> addreses = new();
            string subnet = "192.168.113.";
            await Task.Run(() =>
            {
                Parallel.For(startIp, endIp, i =>
                {
                    if (i != myIp)
                    {
                        string addr = $"{subnet + i}";
                        var ping = new Ping();
                        var pingReply = ping.Send(addr, 5);
                        if (pingReply.Status == IPStatus.Success)
                        {
                            addreses.Add(addr);
                        }
                        FooterRepository.Text = $"Попытка установить соединение с {addr}: {pingReply.Status}";
                    }
                });
            });
            if (addreses.Count == 0)
            {
                MessageBox.Show($"В подсети {subnet}, в диапазоне {startIp}-{endIp}\n не найдено устройств.\n Проверьте подключение",
                                     "Информация о сети", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            }
            else
            {
                StringBuilder sb = new();
                foreach (string ip in addreses)
                {
                    sb.AppendLine(ip);
                }
                var res = MessageBox.Show($"В подсети {subnet} найдены устройства: \n{sb}\n\nЖелаете скорректировать настройки системы?",
                                               "Информация о сети", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (res == MessageBoxResult.Yes)
                {
                    List<SensorSettings> tempList = new();
                    foreach (var ip in addreses)
                    {
                        var sensor = SensorRepository.Sensors.FirstOrDefault(x => x.Ip == ip);
                        if (sensor == null) continue;
                        tempList.Add(sensor);
                    }
                    var sensors = new ObservableCollection<SensorSettings>();
                    SensorRepository.Sensors.Clear();
                    if (tempList.Count == 0)
                    {
                        string msg = $"В системе не найдено настроек для данных устройств\n\nЖелаете создать настройки по умолчанию?";
                        var res2 = MessageBox.Show(msg, "Информация о сети", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (res2 == MessageBoxResult.Yes)
                        {
                            string[] ips = addreses.ToArray();
                            string[] cFiles = { "671.fst", "672.fst", "673.fst", "674.fst" };
                            for (int i = 0, j = 2; i < ips.Length; i++, j += 2)
                            {
                                SensorRepository.Sensors.Add(new SensorSettings()
                                {
                                    Name = $"{i}",
                                    CalibrationFileName = cFiles[i],
                                    Ip = ips[i],
                                    ManualDataPort = (ushort)(34500 + j),
                                });
                            }
                        }
                    }
                    else
                    {
                        foreach (SensorSettings sensor in tempList)
                        {
                            SensorRepository.Sensors.Add(sensor);
                        }
                    }

                }
            }
        }
    }
}
