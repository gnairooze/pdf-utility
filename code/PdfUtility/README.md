# PDF Utility

A simple .NET 9 console application that performs various PDF operations.

## Requirements

- .NET 9 SDK
- Windows, Linux, or macOS

## Usage

1. Build the application:
```bash
dotnet build
```

2. Run the application:
```bash
dotnet run <operation> <input_path> <output_path>
```

### Parameters

- `operation`: The operation to perform (supported: "merge" or "extract")
- `input_path`: Path to the input directory (for merge) or PDF file (for extract)
- `output_path`: Path where the output PDF file (for merge) or extracted images (for extract) will be saved

### Available Operations

#### Merge
Merges JPG images into a single PDF file.
```bash
dotnet run merge ./images ./output.pdf
```

#### Extract
Extracts images from a PDF file.
```bash
dotnet run extract ./input.pdf ./extracted_images
```

## Features

### Merge Operation
- Converts all JPG images in a directory to a single PDF file
- Each image is placed on a separate page
- Images are automatically scaled to fit the page while maintaining aspect ratio
- Supports error handling for individual images
- Orders images alphabetically by filename

### Extract Operation
- Extracts images embedded in a PDF file
- Currently supports JPEG images
- Creates a new directory for extracted images if it doesn't exist
- Images are saved with sequential numbering (Image0.jpeg, Image1.jpeg, etc.)
- Provides feedback on the number of images extracted

## Notes

- For merge operation: Only JPG files are supported
- For extract operation: Currently supports JPEG images only
- The application will skip any images that cannot be processed
- The output will be created even if some images fail to process
- For extract operation, the output directory will be created if it doesn't exist

## Dependencies

- PdfSharp (Version 6.1.1) - Used for PDF manipulation and image extraction
 