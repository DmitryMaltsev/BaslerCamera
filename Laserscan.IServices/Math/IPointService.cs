using Kogerent.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;

namespace Kogerent.Services.Interfaces
{
    public interface IPointService
    {
        List<PointF> DiscretizedProfileToPoints(float[] xy, float leftBorder, float discrete);
        List<IntXFloatYPoint> XyzwToIntXFloatYPoint(float[] xyzw);
        List<PointF> XyzwToPointF(float[] xyzw);
        Task<IEnumerable<PointF>> XyzwToPointFAsync(float[] xyzw);
        List<PointF> XyzwToPointFList(float[] xyzw);
        List<PointF> XyzwToPointFList(Span<float> xyzw);
        List<PointF> XyzwToPointFSimple(float[] xyzw);
        Span<PointF> XyzwToPointFSpan(Span<float> xyzw);
        Tuple<List<PointF>, double[], double[]> XyzwToPointFTuple(Span<float> xyzw);
    }
}