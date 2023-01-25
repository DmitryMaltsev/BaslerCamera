
using Kogerent.Collections;
using Kogerent.Services.Interfaces;

using LaserScan.Core.NetStandart.Models;

using MathNet.Numerics;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Kogerent.Services.Implementation
{
    public class CalibrateService : ICalibrateService
    {
        public (double[], int[]) Calibrate(byte[] data)
        {

            if (data == null)
            {
                throw new ArgumentNullException();
            }
            if (data.Length <= 0)
            {
                throw new ArgumentException();
            }
            var xs = new double[data.Length];
            var ys = new double[data.Length];
            var deltas = new int[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                xs[i] = i;
                ys[i] = 127 - data[i];
            }

            double[] p = Fit.Polynomial(xs, ys, 3);

            for (int i = 0; i < deltas.Length; i++)
            {
                double p0 = p[0];
                double p1 = p[1] * i;
                double p2 = p[2] * Math.Pow(i, 2);
                double p3 = p[3] * Math.Pow(i, 3);
                deltas[i] = (int)(p0 + p1 + p2 + p3);
            }
            return (p, deltas);
        }


        public sbyte[] CalibrateRaw(byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException();
            }
            if (data.Length <= 0)
            {
                throw new ArgumentException();
            }
            sbyte[] raw = new sbyte[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                raw[i] = (sbyte)(127 - data[i]);
                // raw[i] = (127 / (double)data[i]);
            }
            return raw;
        }

        public double[] CalibrateMultiRaw(byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException();
            }
            if (data.Length <= 0)
            {
                throw new ArgumentException();
            }
            double[] raw = new double[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                raw[i] = 127 / (double)data[i];
            }
            return raw;
        }

        public (int, double) CalibrateExposureTimeRaw(byte[] data, int currentExposure)
        {
            double averageByte = data.Average();
            if (averageByte < 90) currentExposure += 500;
            else
            if (averageByte > 160) currentExposure = currentExposure -= 500;
            return (currentExposure, averageByte);
        }

        public sbyte[] DefaultCalibration(double[] p, int count)
        {
            sbyte[] result = new sbyte[count];
            for (int i = 0; i < count; i++)
            {
                double p0 = p[0];
                double p1 = p[1] * i;
                double p2 = p[2] * Math.Pow(i, 2);
                double p3 = p[3] * Math.Pow(i, 3);
                result[i] = (sbyte)(p0 + p1 + p2 + p3);

            }
            return result;
        }

        public List<byte> CreateAverageDataForCalibration(ConcurrentQueue<BufferData> _concurentBuffer, int countArraysInSection, int width)
        {
            int bufCount = _concurentBuffer.Count;
            //Массив, в который получаем все элементы с concurrent коллекции, когда накопили достаточно значений
            byte[,] rawPointsBuffer = new byte[bufCount * countArraysInSection, width];
            //Массив для суммирования всех значение для дальнейшего усреднения
            double[] pointsSumm = new double[width];
            //Результирующая коллекция для сохранения в Xml файл и использования для нахождения дефектов
            List<byte> calibratedPointsList = new();
            int _cnt = 0;
            for (int i = 0; i < bufCount; i++)
            {
                if (_concurentBuffer.TryDequeue(out BufferData etalonPointsBuffer) && etalonPointsBuffer != default)
                {

                    Buffer.BlockCopy(etalonPointsBuffer.Data, 0, rawPointsBuffer, _cnt * width, etalonPointsBuffer.Data.Length);
                    _cnt += countArraysInSection;
                    if (_cnt == bufCount * countArraysInSection)
                    {
                        for (int xpointsNum = 0; xpointsNum < rawPointsBuffer.GetLength(1); xpointsNum++)
                        {
                            for (int yPointsNum = 0; yPointsNum < rawPointsBuffer.GetLength(0); yPointsNum++)
                            {
                                pointsSumm[xpointsNum] += rawPointsBuffer[yPointsNum, xpointsNum];
                            }
                        }
                    }
                }
            }
            for (int xpointsNum = 0; xpointsNum < rawPointsBuffer.GetLength(1); xpointsNum++)
            {
                calibratedPointsList.Add((byte)(pointsSumm[xpointsNum] / rawPointsBuffer.GetLength(0)));
            }

            return calibratedPointsList;
        }

        public List<byte> CreateAverageElementsForCalibration(byte[,] _arrayOfRawPoints, int height, int width)
        {

            //Результирующая коллекция для сохранения в Xml файл и использования для нахождения дефектов
            List<byte> calibratedPointsList = new();
    
            for (int xpointsNum = 0; xpointsNum < width; xpointsNum++)
            {
                List<byte> bufferList = new();
                for (int yPointsNum = 0; yPointsNum < height; yPointsNum++)
                {
                    bufferList.Add(_arrayOfRawPoints[yPointsNum, xpointsNum]);
                }
                bufferList.Sort();
                calibratedPointsList.Add(bufferList[bufferList.Count / 2]);
            }
            return calibratedPointsList;
        }

        /// <summary>
        /// Определяем, нужно ли нам менять экспозицию(сравниваем с гранизцами maxBoundsLight или minBoundsLight)
        /// Если нужно, то возвращаем положительное или отрицательное изменение changeExpoisitionValue, которое
        /// нужно добавить или отнять от текущей экспозиции.
        /// </summary>
        /// <param name="_concurentBuffer"></param>
        /// <param name="countArraysInSection"></param>
        /// <param name="width"></param>
        /// <param name="xMinIndex"></param>
        /// <param name="xMaxIndex"></param>
        /// <param name="maxBoundsLight"></param>
        /// <param name="minBoundsLight"></param>
        /// <param name="changeExpoisitionValue"></param>
        /// <returns></returns>
        public bool NeedChangeExposition(ConcurrentQueue<BufferData> _concurentBuffer, int countArraysInSection, int width,
            int xMinIndex, int xMaxIndex, int minBoundsLight, int maxBoundsLight, out int changeExpoisitionValue)
        {
            int bufCount = _concurentBuffer.Count;
            byte[,] rawPointsBuffer = new byte[bufCount * countArraysInSection, width];
            int _cnt = 0;
            for (int i = 0; i < bufCount; i++)
            {
                if (_concurentBuffer.TryDequeue(out BufferData etalonPointsBuffer) && etalonPointsBuffer != default)
                {
                    Buffer.BlockCopy(etalonPointsBuffer.Data, 0, rawPointsBuffer, _cnt * width, etalonPointsBuffer.Data.Length);
                    _cnt += countArraysInSection;
                    etalonPointsBuffer.Dispose();

                }
            }
            // _concurentBuffer.Clear();
            List<byte> pointsToFindMaxY = new();
            for (int xpointsNum = 0; xpointsNum < rawPointsBuffer.GetLength(1); xpointsNum++)
            {
                if (xpointsNum <= xMinIndex || xpointsNum >= xMaxIndex)
                {
                    pointsToFindMaxY.Add(0);
                }
                else
                {
                    pointsToFindMaxY.Add(rawPointsBuffer[0, xpointsNum]);
                }
            }
            pointsToFindMaxY.Sort();
            if (pointsToFindMaxY[pointsToFindMaxY.Count - 250] > maxBoundsLight)
            {
                changeExpoisitionValue = -30;
                return true;

            }
            else
               if (pointsToFindMaxY[pointsToFindMaxY.Count - 250] < minBoundsLight)
            {
                changeExpoisitionValue = 50;
                return true;
            }
            changeExpoisitionValue = 0;
            return false;
        }


        public void AddCalibrateSettingsToMaterial(ObservableCollection<BaslerCameraModel> baslerCameraCollection,
                                                  MaterialModel currentMaterial)
        {
            for (int i = 0; i < baslerCameraCollection.Count; i++)
            {
                currentMaterial.CameraDeltaList[i] = (new()
                {
                    CameraId = baslerCameraCollection[i].ID,
                    Deltas = baslerCameraCollection[i].Deltas,
                    MultipleDeltas = baslerCameraCollection[i].MultipleDeltas,
                    UpThreshhold = baslerCameraCollection[i].UpThreshold,
                    DownThreshhold = baslerCameraCollection[i].DownThreshold
                });
            }
        }


        public ObservableCollection<MaterialModel> CreateDefaultMaterialCollection()
        {

            ObservableCollection<MaterialModel> materialModelCollection = new ObservableCollection<MaterialModel>();
            materialModelCollection.Add(new MaterialModel()
            {
                MaterialName = "Default",
                CameraDeltaList = new List<CameraDelta>()
            });
            materialModelCollection[0].CameraDeltaList.Add(new CameraDelta());
            materialModelCollection[0].CameraDeltaList.Add(new CameraDelta());
            materialModelCollection[0].CameraDeltaList.Add(new CameraDelta());
            return materialModelCollection;
        }
    }
}
