// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using Microsoft.Azure.Media.LiveVideoAnalytics.Extensibility.Grpc.V1;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using Rect = System.Drawing.Rectangle;

namespace GrpcExtension.Processors
{
    /// <summary>Class <c>BatchImageProcessor</c> is responsible for processing an Image/>.
    /// </summary>
    public class BatchImageProcessor : IBatchProcessor
    {
        private readonly ILogger logger;
        public BatchImageProcessor(ILogger logger)
        {
            this.logger = logger;
        }

        /// <summary>This method converts an image to grayscale and determines if its color intensity is dark or light
        /// <param name="image">The <paramref name="image"/> to process.</param>
        /// <param name="colorIntensity">The color intensity</param>
        /// <returns>An Inferecence instance.</returns>
        /// <remarks>
        /// You can replace this class with the one including your image processing logic implementation. Your class should have a method named 
        /// IList<Inference> ProcessImage(Image image) that contains the implementation. Finally you'll have update the ScoreController's ProcessImage method to create an 
        /// instance of your class and invoke the ProcessImage method.
        /// </remarks>
        /// </summary>
        public IEnumerable<Inference> ProcessImages(List<Image> images, string colorIntensity)
        {
            var inferences = new List<Inference>();
            try
            {
                foreach (var image in images)
                {
                    var grayScaleImage = ToGrayScale(image);

                    byte[] imageBytes = GetBytes(grayScaleImage);

                    var totalColor = imageBytes.Sum(x => x);

                    double avgColor = totalColor / imageBytes.Length;

                    bool verificationSucceeded = false;

                    if(colorIntensity == "dark")
                    {
                        verificationSucceeded = avgColor < 127;
                    }
                    else
                    {
                        verificationSucceeded = avgColor > 127;
                    }


                    inferences.Add(new Inference
                    {
                        Type = Inference.Types.InferenceType.Classification,
                        Subtype = "colorIntensityCheck",
                        Classification = new Classification()
                        {
                            Tag = new Tag()
                            {
                                Value = verificationSucceeded.ToString(),
                                Confidence = 1.0f
                            }
                        }
                    });

                    logger.LogInformation($"Average color = {avgColor}");
                }
            }
            catch (Exception ex)
            {
                logger.LogInformation($"Error in processor: {ex.Message}.");
            }

            return inferences;
        }

        /// <summary>
        /// Validate Media Format
        /// </summary>
        /// <param name="mediaDescriptor">The Media session preamble</param>
        /// <param name="errorMessage">Error message</param>
        /// <returns></returns>
        public bool IsMediaFormatSupported(MediaDescriptor mediaDescriptor, out string errorMessage)
        {
            errorMessage = null;
            switch (mediaDescriptor.MediaSampleFormatCase)
            {
                case MediaDescriptor.MediaSampleFormatOneofCase.VideoFrameSampleFormat:

                    var videoSampleFormat = mediaDescriptor.VideoFrameSampleFormat;

                    if (videoSampleFormat.Encoding != VideoFrameSampleFormat.Types.Encoding.Raw)
                    {
                        errorMessage = $"{videoSampleFormat.Encoding} encoding is not supported. Supported: Raw";
                        return false;
                    }

                    return true;

                default:

                    errorMessage = $"Unsupported sample format: {mediaDescriptor.MediaSampleFormatCase}";
                    return false;
            }
        }


        /// <summary>This method converts an image to grayscale
        /// <param name="image">The <paramref name="image"/>.</param>
        /// <returns>An Bitmap.</returns>
        /// </summary>
        private Bitmap ToGrayScale(Image source)
        {
            Bitmap grayscaleBitmap = new Bitmap(source.Width, source.Height);

            using Graphics graphics = Graphics.FromImage(grayscaleBitmap);

            //create the grayscale ColorMatrix
            ColorMatrix colorMatrix = new ColorMatrix(
                new float[][]
                {
                        new float[] {.3f, .3f, .3f, 0, 0},
                        new float[] {.59f, .59f, .59f, 0, 0},
                        new float[] {.11f, .11f, .11f, 0, 0},
                        new float[] {0, 0, 0, 1, 0},
                        new float[] {0, 0, 0, 0, 1}
                });

            ImageAttributes attributes = new ImageAttributes();

            //set the color matrix attribute
            attributes.SetColorMatrix(colorMatrix);

            //draw the original image on the new image
            //using the grayscale color matrix
            graphics.DrawImage(source, new System.Drawing.Rectangle(0, 0, source.Width, source.Height),
                0, 0, source.Width, source.Height, GraphicsUnit.Pixel, attributes);

            return grayscaleBitmap;
        }

        /// <summary>This method converts an image to a byte array 
        /// <param name="image">The <paramref name="image"/>.</param>
        /// <returns>A byte array.</returns>
        /// </summary>
        private byte[] GetBytes(Bitmap image)
        {
            var region = new Rect(0, 0, image.Width, image.Height);
            var bitmapData = image.LockBits(region, ImageLockMode.ReadOnly, image.PixelFormat);

            var ptr = bitmapData.Scan0;
            var length = Math.Abs(bitmapData.Stride) * image.Height;

            var rgbValues = new byte[length];

            Marshal.Copy(ptr, rgbValues, 0, length);

            image.UnlockBits(bitmapData);

            return rgbValues;
        }
    }
}