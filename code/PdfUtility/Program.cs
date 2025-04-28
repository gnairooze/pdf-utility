using PdfSharp.Pdf;
using PdfSharp.Drawing;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.IO;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using PdfSharp.Pdf.Filters;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length < 3)
        {
            Console.WriteLine("Usage: PdfUtility <operation> <input_path> <output_path>");
            Console.WriteLine("Available operations:");
            Console.WriteLine("  merge - Merge JPG images into a single PDF");
            Console.WriteLine("  extract - Extract images from a PDF file");
            return;
        }

        string operation = args[0].ToLower();
        string inputPath = args[1];
        string outputPath = args[2];

        try
        {
            switch (operation)
            {
                case "merge":
                    MergeImagesToPdf(inputPath, outputPath);
                    break;
                case "extract":
                    ExtractImagesFromPdf(inputPath, outputPath);
                    break;
                default:
                    Console.WriteLine($"Unknown operation: {operation}");
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    static void MergeImagesToPdf(string inputDirectory, string outputPdfPath)
    {
        // Get all JPG files from the input directory
        var imageFiles = Directory.GetFiles(inputDirectory, "*.jpg")
                                .OrderBy(f => f)
                                .ToArray();

        if (imageFiles.Length == 0)
        {
            Console.WriteLine("No JPG files found in the specified directory.");
            return;
        }

        // Create PDF document
        using var document = new PdfDocument();

        // Add each image to the PDF
        foreach (var imageFile in imageFiles)
        {
            try
            {
                // Create a new page
                var page = document.AddPage();
                var gfx = XGraphics.FromPdfPage(page);

                // Load and draw the image
                using var image = XImage.FromFile(imageFile);
                
                // Calculate image dimensions to fit page while maintaining aspect ratio
                var pageWidth = page.Width.Point;
                var pageHeight = page.Height.Point;
                var imageWidth = image.PixelWidth;
                var imageHeight = image.PixelHeight;
                
                var ratio = Math.Min(pageWidth / imageWidth, pageHeight / imageHeight);
                var newWidth = imageWidth * ratio;
                var newHeight = imageHeight * ratio;
                
                // Center the image on the page
                var x = (pageWidth - newWidth) / 2;
                var y = (pageHeight - newHeight) / 2;
                
                gfx.DrawImage(image, x, y, newWidth, newHeight);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing image {imageFile}: {ex.Message}");
            }
        }

        // Save the PDF
        document.Save(outputPdfPath);
        Console.WriteLine($"PDF created successfully at: {outputPdfPath}");
    }

    static void ExtractImagesFromPdf(string inputPdfPath, string outputDirectory)
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
                var reference = item as PdfReference;
                if (reference == null)
                    continue;

                var xObject = reference.Value as PdfDictionary;
                // Is external object an image?
                if (xObject != null && xObject.Elements.GetString("/Subtype") == "/Image")
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
    static void ExportImage(PdfDictionary image, ref int count, string outputDirectory)
    {
        var filter = image.Elements.GetValue("/Filter");
        // Do we have a filter array?
        var array = filter as PdfArray;
        if (array != null)
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

                case "/FlateDecode":
                    ExportAsPngImage(image, ref count);
                    break;

                // TODO Deal with other encodings like "/CCITTFaxDecode"
            }
        }
    }
    /// <summary>
    /// Exports a JPEG image.
    /// </summary>
    static void ExportJpegImage(PdfDictionary image, ref int count, string outputDirectory)
    {
        // Fortunately JPEG has native support in PDF and exporting an image is just writing the stream to a file.
        byte[] stream = image.Stream.Value;
        string filename = Path.Combine(outputDirectory, $"Image{count++}.jpeg");
        File.WriteAllBytes(filename, stream);
    }

    /// <summary>
    /// Exports image in PNG format.
    /// </summary>
    static void ExportAsPngImage(PdfDictionary image, ref int count)
    {
        var width = image.Elements.GetInteger(PdfImage.Keys.Width);
        var height = image.Elements.GetInteger(PdfImage.Keys.Height);
        var bitsPerComponent = image.Elements.GetInteger(PdfImage.Keys.BitsPerComponent);

        // TODO: You can put the code here that converts from PDF internal image format to a Windows bitmap.
        // and use GDI+ to save it in PNG format.
        // It is the work of a day or two for the most important formats. Take a look at the file
        // PdfSharp.Pdf.Advanced/PdfImage.cs to see how we create the PDF image formats.
        // We don't need that feature at the moment and therefore will not implement it.
        // If you write the code for exporting images I would be pleased to publish it in a future release
        // of PDFsharp.
    }
}
