namespace Sassa.Scanning.Settings;

public sealed class ScanningSettings
{
    public string PdfPath { get; init; } = string.Empty;
    public string OutputPath { get; init; } = string.Empty;
    public string ScannedPath { get; init; } = string.Empty;
    public string RejectPath { get; init; } = string.Empty;
    public int ScanDpi { get; init; } = 300;
    //public PageDimensionsSettings PageDimensions { get; init; } = new();
    public BarcodeSettings Barcode { get; init; } = new();
    public PreviewSettings PreviewWindow { get; init; } = new();
}


public sealed class BarcodeSettings
{
    public bool AutoRotate { get; init; } = true;
    public bool TryInverted { get; init; } = false;
    public bool TryHarder { get; init; } = true;
}

public sealed class PreviewSettings
{
    public bool Enabled { get; init; } = true;
    public int Width { get; init; } = 400;
    public int Height { get; init; } = 600;
}