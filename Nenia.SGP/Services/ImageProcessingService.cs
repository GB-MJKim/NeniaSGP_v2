using System;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Nenia.SGP.Services;

public sealed class ImageProcessingService
{
    public (string originalRel, string mainRel, string thumbRel) ImportAndProcess(string sourceFilePath)
    {
        var baseDir = AppContext.BaseDirectory;

        var originalsDir = Path.Combine(baseDir, "Images", "Originals");
        var mainDir = Path.Combine(baseDir, "Images", "Main");
        var thumbDir = Path.Combine(baseDir, "Images", "Thumbnails");
        Directory.CreateDirectory(originalsDir);
        Directory.CreateDirectory(mainDir);
        Directory.CreateDirectory(thumbDir);

        var ext = Path.GetExtension(sourceFilePath).ToLowerInvariant();
        if (ext is not ".jpg" and not ".jpeg" and not ".png")
            throw new InvalidOperationException("JPG/PNG만 지원합니다.");

        var guid = Guid.NewGuid().ToString("N");

        // 1) 원본 복사(확장자 유지)
        var originalFileName = $"{guid}{ext}";
        var originalAbs = Path.Combine(originalsDir, originalFileName);
        File.Copy(sourceFilePath, originalAbs, overwrite: true);

        // 2) 가공본은 jpg로 통일
        var processedFileName = $"{guid}.jpg";
        var mainAbs = Path.Combine(mainDir, processedFileName);
        var thumbAbs = Path.Combine(thumbDir, processedFileName);

        var src = LoadBitmapSource(sourceFilePath);

        var main = ResizeCoverSquare(src, 600);
        SaveJpeg(main, mainAbs, quality: 92);

        var thumb = ResizeCoverSquare(src, 100);
        SaveJpeg(thumb, thumbAbs, quality: 85);

        return (
            Path.Combine("Images", "Originals", originalFileName),
            Path.Combine("Images", "Main", processedFileName),
            Path.Combine("Images", "Thumbnails", processedFileName)
        );
    }

    private static BitmapSource LoadBitmapSource(string path)
    {
        var bmp = new BitmapImage();
        bmp.BeginInit();
        bmp.CacheOption = BitmapCacheOption.OnLoad;
        bmp.UriSource = new Uri(path, UriKind.Absolute);
        bmp.EndInit();
        bmp.Freeze();
        return bmp;
    }

    // 정사각형 센터 크롭 + 축소(왜곡 없이 “커버”)
    private static BitmapSource ResizeCoverSquare(BitmapSource src, int targetSize)
    {
        int w = src.PixelWidth;
        int h = src.PixelHeight;
        int side = Math.Min(w, h);

        int x = (w - side) / 2;
        int y = (h - side) / 2;

        var cropped = new CroppedBitmap(src, new System.Windows.Int32Rect(x, y, side, side));
        cropped.Freeze();

        double scale = (double)targetSize / side;
        var transformed = new TransformedBitmap(cropped, new ScaleTransform(scale, scale));
        transformed.Freeze();

        return transformed;
    }

    private static void SaveJpeg(BitmapSource bitmap, string filePath, int quality)
    {
        var encoder = new JpegBitmapEncoder { QualityLevel = quality };
        encoder.Frames.Add(BitmapFrame.Create(bitmap));

        using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write);
        encoder.Save(fs);
    }

    public static string ToAbsPath(string relOrAbs)
        => Path.IsPathRooted(relOrAbs) ? relOrAbs : Path.Combine(AppContext.BaseDirectory, relOrAbs);
}
