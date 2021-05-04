// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using HttpExtension.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace HttpExtension.Processors
{    
    /// <summary>Class <c>ImageProcessor</c> is responsible for processing an <paramref name="image"/>.
    /// </summary>
    public class ImageProcessor
    {
        private readonly ILogger logger;
        public ImageProcessor(ILogger logger)
        {
            this.logger = logger;
        }

        /// <summary>This method converts an image to grayscale and determines if its color intensity is dark or light
        /// <param name="image">The <paramref name="image"/> to process.</param>
        /// <returns>An Inferecence instance.</returns>
        /// <remarks>
        /// You can replace this class with the one including your image processing logic implementation. Your class should have a method named 
        /// IList<Inference> ProcessImage(Image image) that contains the implementation. Finally you'll have update the ScoreController's ProcessImage method to create an 
        /// instance of your class and invoke the ProcessImage method.
        /// </remarks>
        /// </summary>
        public InferenceResponse ProcessImage(Image image)
        {
            var grayScaleImage = ToGrayScale(image);

            byte[] imageBytes = GetBytes(grayScaleImage);

            var totalColor = imageBytes.Sum(x => x);

            double avgColor = totalColor / imageBytes.Length;
            string colorIntensity = avgColor < 127 ? "dark" : "light";

            logger.LogInformation($"Average color = {avgColor}");

            var response = new InferenceResponse { Inferences = new[] { new Inference
            {
                Type = "classification",
                SubType = "colorIntensity",
                Classification = new Classification()
                {
                    Confidence = 1.0,
                    Value = colorIntensity
                }
            }}};

            return response;
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
            graphics.DrawImage(source, new Rectangle(0, 0, source.Width, source.Height),
                0, 0, source.Width, source.Height, GraphicsUnit.Pixel, attributes);

            return grayscaleBitmap;
        }

        /// <summary>This method converts an image to a byte array 
        /// <param name="image">The <paramref name="image"/>.</param>
        /// <returns>A byte array.</returns>
        /// </summary>
        private byte[] GetBytes(Bitmap image)
        {
            MemoryStream stream = new MemoryStream();
            image.Save(stream, ImageFormat.Jpeg);
            return stream.ToArray();
        }
    }
}
