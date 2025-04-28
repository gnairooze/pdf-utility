using PdfSharp.Pdf;
using PdfSharp.Drawing;
using System;
using System.IO;
using System.Linq;

namespace PdfUtility
{
    public class PdfMerger
    {
        public static void MergeImagesToPdf(string inputDirectory, string outputPdfPath)
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
    }
} 