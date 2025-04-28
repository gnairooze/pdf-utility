using PdfSharp.Pdf;
using PdfSharp.Drawing;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.IO;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using PdfSharp.Pdf.Filters;

namespace PdfUtility
{
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
                        PdfMerger.MergeImagesToPdf(inputPath, outputPath);
                        break;
                    case "extract":
                        PdfExtractor.ExtractImagesFromPdf(inputPath, outputPath);
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
    }
}
