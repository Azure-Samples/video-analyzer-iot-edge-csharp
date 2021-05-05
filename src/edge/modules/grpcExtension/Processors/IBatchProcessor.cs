using Microsoft.Azure.Media.LiveVideoAnalytics.Extensibility.Grpc.V1;
using System.Collections.Generic;
using System.Drawing;

namespace GrpcExtension.Processors
{
    public interface IBatchProcessor
    {
        IEnumerable<Inference> ProcessImages(List<Image> images, string colorIntensity);
        bool IsMediaFormatSupported(MediaDescriptor mediaDescriptor, out string errorMessage);
    }
}
