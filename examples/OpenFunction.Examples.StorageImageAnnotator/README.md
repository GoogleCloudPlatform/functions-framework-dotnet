# StorageImageAnnotator example function

This function demonstrates an event handler for Google Cloud Storage.

It should be attached to a trigger type of
"google.storage.object.finalize", so it will be called whenever an
object finishes uploading in a Storage bucket.

If the file is not a JPEG image (content type "image/jpeg", extension
".jpg"), it's skipped.

Otherwise, the function uses the [Google Cloud Vision
API](https://cloud.google.com/vision) to detect faces, text,
landmarks, logos and objects within the image. Once the Vision API
has returned a response, the function formats the response as a text
file, and uploads it as a new Google Cloud Storage object with the
same name as the original one, but an extension of ".txt".
