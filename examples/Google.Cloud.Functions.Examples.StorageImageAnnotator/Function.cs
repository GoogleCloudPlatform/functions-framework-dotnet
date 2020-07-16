// Copyright 2020, Google LLC
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     https://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using CloudNative.CloudEvents;
using Google.Cloud.Functions.Framework;
using Google.Cloud.Storage.V1;
using Google.Cloud.Vision.V1;
using Google.Events.Protobuf.Cloud.Storage.V1;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Google.Cloud.Vision.V1.Feature.Types.Type;

namespace Google.Cloud.Functions.Examples.StorageImageAnnotator
{
    /// <summary>
    /// Function to watch for new image files being added to a storage bucket,
    /// use the Google Cloud Vision API to annotate them, then write a text file
    /// back into the bucket with the information from the Vision API.
    /// </summary>
    public class Function : ICloudEventFunction<StorageObjectData>
    {
        private const string JpegContentType = "image/jpeg";
        private const string TextContentType = "text/plain";
        private const string JpegExtension = ".jpg";
        private const string TextExtension = ".txt";

        private readonly ImageAnnotatorClient _visionClient;
        private readonly StorageClient _storageClient;
        private readonly ILogger _logger;

        /// <summary>
        ///  Constructor accepting all our dependencies. The clients are configured in
        ///  Startup.cs.
        /// </summary>
        public Function(ImageAnnotatorClient visionClient, StorageClient storageClient, ILogger<Function> logger) =>
            (_visionClient, _storageClient, _logger) = (visionClient, storageClient, logger);

        /// <summary>
        /// Entry point for the function. This is called whenever a new storage file is created (including
        /// when we create one ourselves - we need to be careful not to create an infinite loop!)
        /// </summary>
        /// <param name="payload">The storage object that's been uploaded.</param>
        /// <param name="context">Event context (event ID etc)</param>
        public async Task HandleAsync(CloudEvent cloudEvent, StorageObjectData data, CancellationToken cancellationToken)
        {
            // Only handle JPEG images. (This prevents us from trying to perform image recognition on
            // the files we upload.)
            if (data.ContentType != JpegContentType || Path.GetExtension(data.Name) != JpegExtension)
            {
                _logger.LogInformation("Ignoring file {name} with content type {contentType}",
                    data.Name, data.ContentType);
                return;
            }

            var annotations = await AnnotateImageAsync(data, cancellationToken);
            var text = DescribeAnnotations(annotations);
            string newObjectName = await CreateDescriptionFileAsync(data, text);
            _logger.LogInformation("Created object {object} with image annotations", newObjectName);
        }

        /// <summary>
        /// Use the Vision API to annotate the image. The Vision API is able to annotate Google Cloud Storage
        /// objects directly based on their name, so we don't need to download the object and then upload it
        /// to the Vision API.
        /// </summary>
        private Task<AnnotateImageResponse> AnnotateImageAsync(StorageObjectData payload, CancellationToken cancellationToken)
        {
            var features = new[] { FaceDetection, LandmarkDetection, TextDetection, LogoDetection, LabelDetection }
                .Select(type => new Feature { Type = type, MaxResults = 20 });
            var request = new AnnotateImageRequest
            {
                Image = Image.FromUri($"gs://{payload.Bucket}/{payload.Name}"),
                Features = { features }
            };
            return _visionClient.AnnotateAsync(request, cancellationToken);
        }

        /// <summary>
        /// Converts a result from the Google Cloud Vision API into a textual representation.
        /// This is very primitive, but is sufficient for demonstration purposes.
        /// An alternative approach would be to create a new image file that highlighted all the faces,
        /// added labels for all the landmarks, etc. (That would require more care to avoid an infinite loop,
        /// because we'd be uploading a file which would meet the current requirements for processing. We could
        /// add some object metadata to indicate that it shouldn't be processed further.)
        /// </summary>
        private string DescribeAnnotations(AnnotateImageResponse response)
        {
            var builder = new StringBuilder();
            builder.AppendLine("Detected annotations:");
            builder.AppendLine();
            if (response.FaceAnnotations.Any())
            {
                _logger.LogWarning("Did you mean to include faces in your storage bucket?");
                builder.AppendLine($"Faces: {response.FaceAnnotations.Count}");
            }
            MaybeAppend("Landmarks", response.LandmarkAnnotations, ann => $"{ann.Description} - {ann.Score}");
            // Multi-part descriptions come with line breaks; we'll just remove them for now. Text annotations have no score.
            MaybeAppend("Text", response.TextAnnotations, ann => ann.Description.Replace('\n', ' '));
            MaybeAppend("Logos", response.LogoAnnotations, ann => $"{ann.Description} - {ann.Score}");
            MaybeAppend("Labels", response.LabelAnnotations, ann => $"{ann.Description} - {ann.Score}");

            void MaybeAppend<T>(string name, IEnumerable<T> results, Func<T, string> converter)
            {
                if (results.Any())
                {
                    builder.AppendLine($"{name}:");
                    foreach (var result in results)
                    {
                        builder.AppendLine($"  {converter(result)}");
                    }
                }
            }
            return builder.ToString();
        }

        /// <summary>
        /// Creates a storage object alongside the original one, just renaming the extension to .txt.
        /// </summary>
        private async Task<string> CreateDescriptionFileAsync(StorageObjectData payload, string text)
        {
            var name = Path.ChangeExtension(payload.Name, TextExtension);
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(text)))
            {
                await _storageClient.UploadObjectAsync(payload.Bucket, name, TextContentType, stream);
            }
            return name;
        }
    }
}
