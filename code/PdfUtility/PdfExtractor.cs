using PdfSharp.Pdf;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.IO;
using System;
using System.IO;

namespace PdfUtility
{
    public class PdfExtractor
    {
        public static void ExtractImagesFromPdf(string inputPdfPath, string outputDirectory)
        {
            // Create output directory if it doesn't exist
            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            // Open the PDF document
            using var document = PdfReader.Open(inputPdfPath);
            var imageCount = 0;

            Console.WriteLine("Scanning PDF for extractable images...");

            // Process each page
            for (int pageIndex = 0; pageIndex < document.Pages.Count; pageIndex++)
            {
                var page = document.Pages[pageIndex];

                // Get the resources dictionary.
                var resources = page.Elements.GetDictionary("/Resources");
                if (resources == null)
                    continue;

                // Get the external objects dictionary.
                var xObjects = resources.Elements.GetDictionary("/XObject");
                if (xObjects == null)
                    continue;

                var items = xObjects.Elements.Values;
                // Iterate the references to external objects.
                foreach (var item in items)
                {
                    if (item is not PdfReference reference)
                        continue;

                    // Is external object an image?
                    if (reference.Value is PdfDictionary xObject && xObject.Elements.GetString("/Subtype") == "/Image")
                    {
                        ExportImage(xObject, ref imageCount, outputDirectory);
                    }
                }
            }

            if (imageCount == 0)
            {
                Console.WriteLine("No extractable images found in the PDF file.");
                Console.WriteLine("Note: This basic extractor may not be able to extract all types of embedded images.");
            }
            else
            {
                Console.WriteLine($"Successfully extracted {imageCount} images to {outputDirectory}");
            }
        }

        /// <summary>
        /// Currently extracts only JPEG images.
        /// </summary>
        private static void ExportImage(PdfDictionary image, ref int count, string outputDirectory)
        {
            var filter = image.Elements.GetValue("/Filter");
            // Do we have a filter array?
            if (filter is PdfArray array)
            {
                // PDF files sometimes contain "zipped" JPEG images.
                if (array.Elements.GetName(0) == "/DCTDecode")
                {
                    ExportJpegImage(image, ref count, outputDirectory);
                    return;
                }

                // TODO Deal with other encodings like "/FlateDecode" + "/CCITTFaxDecode"
            }

            // Do we have a single filter?
            var name = filter as PdfName;
            if (name != null)
            {
                var decoder = name.Value;
                switch (decoder)
                {
                    case "/DCTDecode":
                        ExportJpegImage(image, ref count, outputDirectory);
                        break;
                    
                    // TODO Deal with other encodings like "/FlateDecode" and "/CCITTFaxDecode"
                }
            }
        }

        /// <summary>
        /// Exports a JPEG image.
        /// </summary>
        private static void ExportJpegImage(PdfDictionary image, ref int count, string outputDirectory)
        {
            // Fortunately JPEG has native support in PDF and exporting an image is just writing the stream to a file.
            byte[] stream = image.Stream.Value;
            string filename = Path.Combine(outputDirectory, $"Image{count++}.jpeg");
            File.WriteAllBytes(filename, stream);
        }
    }
} 