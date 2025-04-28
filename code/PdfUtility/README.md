# PDF Utility

A simple .NET 8 console application that performs various PDF operations.

## Requirements

- .NET 8 SDK
- Windows, Linux, or macOS

## Usage

1. Build the application:
```bash
dotnet build
```

2. Run the application:
```bash
dotnet run <operation> <input_directory> <output_pdf_path>
```

### Parameters

- `operation`: The operation to perform (currently supported: "merge")
- `input_directory`: Path to the directory containing input files
- `output_pdf_path`: Path where the output PDF file will be saved

### Available Operations

#### Merge
Merges JPG images into a single PDF file.
```bash
dotnet run merge ./images ./output.pdf
```

## Features

### Merge Operation
- Converts all JPG images in a directory to a single PDF file
- Each image is placed on a separate page
- Images are automatically scaled to fit the page while maintaining aspect ratio
- Supports error handling for individual images
- Orders images alphabetically by filename

## Notes

- Only JPG files are supported for the merge operation
- The application will skip any images that cannot be processed
- The output PDF will be created even if some images fail to process 