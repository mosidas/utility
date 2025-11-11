
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Diagnostics;
using System.Runtime.InteropServices;

#if WINDOWS
using System.Drawing;
using System.Windows.Forms;
#endif

namespace Helper;

public class ScreenCapture
{
  private static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
  private static bool IsMacOS => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

  /// <summary>
  /// プライマリスクリーンのスクリーンショットを保存
  /// </summary>
  /// <param name="filePath">保存先のファイルパス</param>
  /// <param name="quality">JPEG品質 (0-100)</param>
  /// <param name="scale">リサイズ倍率 (0より大きい値、1.0で元のサイズ)</param>
  public static async Task SaveCompressedScreenshotAsync(string filePath, int quality = 75, double scale = 1.0)
  {
    if (scale <= 0)
    {
      throw new ArgumentException("Scale must be greater than 0", nameof(scale));
    }

    Image<Rgba32> screenshot;

#if WINDOWS
    screenshot = CaptureScreenWindows();
#else
    if (IsMacOS)
    {
      screenshot = await CaptureScreenMacOSAsync();
    }
    else
    {
      throw new PlatformNotSupportedException("This platform is not supported for screen capture.");
    }
#endif

    if (scale != 1.0)
    {
      ResizeImage(screenshot, scale);
    }

    JpegEncoder encoder = new()
    {
      Quality = quality
    };

    await screenshot.SaveAsync(filePath, encoder);
  }

  /// <summary>
  /// すべてのモニターを1つの画像として結合してスクリーンショットを保存
  /// </summary>
  /// <param name="filePath">保存先のファイルパス</param>
  /// <param name="quality">JPEG品質 (0-100)</param>
  /// <param name="scale">リサイズ倍率 (0より大きい値、1.0で元のサイズ)</param>
  public static async Task SaveAllScreensAsync(string filePath, int quality = 75, double scale = 1.0)
  {
    if (scale <= 0)
    {
      throw new ArgumentException("Scale must be greater than 0", nameof(scale));
    }

    Image<Rgba32> screenshot;

#if WINDOWS
    screenshot = CaptureAllScreensWindows();
#else
    if (IsMacOS)
    {
      screenshot = await CaptureAllScreensMacOSAsync();
    }
    else
    {
      throw new PlatformNotSupportedException("This platform is not supported for screen capture.");
    }
#endif

    if (scale != 1.0)
    {
      ResizeImage(screenshot, scale);
    }

    JpegEncoder encoder = new()
    {
      Quality = quality
    };

    await screenshot.SaveAsync(filePath, encoder);
  }

  /// <summary>
  /// 各モニターを個別のファイルとして保存
  /// </summary>
  /// <param name="filePathFormat">ファイルパス形式 (例: "screen_{0}.jpg" の {0} にスクリーン番号が入る)</param>
  /// <param name="quality">JPEG品質 (0-100)</param>
  /// <param name="scale">リサイズ倍率 (0より大きい値、1.0で元のサイズ)</param>
  public static async Task SaveEachScreenAsync(string filePathFormat, int quality = 75, double scale = 1.0)
  {
    if (scale <= 0)
    {
      throw new ArgumentException("Scale must be greater than 0", nameof(scale));
    }

    JpegEncoder encoder = new()
    {
      Quality = quality
    };

#if WINDOWS
    await SaveEachScreenWindowsAsync(filePathFormat, encoder, scale);
#else
    if (IsMacOS)
    {
      await SaveEachScreenMacOSAsync(filePathFormat, encoder, scale);
    }
    else
    {
      throw new PlatformNotSupportedException("This platform is not supported for screen capture.");
    }
#endif
  }

  #region Windows Implementation

#if WINDOWS
  [System.Runtime.Versioning.SupportedOSPlatform("windows6.1")]
  private static Image<Rgba32> CaptureScreenWindows()
  {
    var primaryScreen = Screen.PrimaryScreen!;
    return CaptureScreenWindows(primaryScreen);
  }

  [System.Runtime.Versioning.SupportedOSPlatform("windows6.1")]
  private static Image<Rgba32> CaptureScreenWindows(Screen screen)
  {
    var bounds = screen.Bounds;

    // System.Drawing.Bitmapでスクリーンキャプチャ
    using Bitmap bitmap = new(bounds.Width, bounds.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
    using Graphics graphics = Graphics.FromImage(bitmap);
    graphics.CopyFromScreen(bounds.X, bounds.Y, 0, 0, new System.Drawing.Size(bounds.Width, bounds.Height), CopyPixelOperation.SourceCopy);

    // ImageSharp形式に変換
    using MemoryStream memoryStream = new();
    bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
    memoryStream.Position = 0;

    return SixLabors.ImageSharp.Image.Load<Rgba32>(memoryStream);
  }

  [System.Runtime.Versioning.SupportedOSPlatform("windows6.1")]
  private static Image<Rgba32> CaptureAllScreensWindows()
  {
    var screens = Screen.AllScreens;

    // 全モニターを含む仮想スクリーン全体の境界を計算
    int minX = screens.Min(s => s.Bounds.X);
    int minY = screens.Min(s => s.Bounds.Y);
    int maxX = screens.Max(s => s.Bounds.Right);
    int maxY = screens.Max(s => s.Bounds.Bottom);

    int width = maxX - minX;
    int height = maxY - minY;

    // すべてのモニターを含む大きなビットマップを作成
    using Bitmap bitmap = new(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
    using Graphics graphics = Graphics.FromImage(bitmap);

    // 各モニターをキャプチャして結合
    foreach (var screen in screens)
    {
      var bounds = screen.Bounds;
      graphics.CopyFromScreen(
        bounds.X, bounds.Y,
        bounds.X - minX, bounds.Y - minY,
        new System.Drawing.Size(bounds.Width, bounds.Height),
        CopyPixelOperation.SourceCopy
      );
    }

    // ImageSharp形式に変換
    using MemoryStream memoryStream = new();
    bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
    memoryStream.Position = 0;

    return SixLabors.ImageSharp.Image.Load<Rgba32>(memoryStream);
  }

  [System.Runtime.Versioning.SupportedOSPlatform("windows6.1")]
  private static async Task SaveEachScreenWindowsAsync(string filePathFormat, JpegEncoder encoder, double scale)
  {
    var screens = Screen.AllScreens;

    for (int i = 0; i < screens.Length; i++)
    {
      var screenshot = CaptureScreenWindows(screens[i]);

      if (scale != 1.0)
      {
        ResizeImage(screenshot, scale);
      }

      string filePath = string.Format(filePathFormat, i);
      await screenshot.SaveAsync(filePath, encoder);
    }
  }
#endif

  #endregion

  #region macOS Implementation

  private static async Task<Image<Rgba32>> CaptureScreenMacOSAsync()
  {
    // macOSではプライマリスクリーンのみをキャプチャ
    string tempFile = Path.GetTempFileName() + ".png";

    try
    {
      var startInfo = new ProcessStartInfo
      {
        FileName = "screencapture",
        Arguments = $"-x -t png \"{tempFile}\"",
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
        CreateNoWindow = true
      };

      using var process = Process.Start(startInfo);
      if (process == null)
      {
        throw new InvalidOperationException("Failed to start screencapture process");
      }

      await process.WaitForExitAsync();

      if (process.ExitCode != 0)
      {
        string error = await process.StandardError.ReadToEndAsync();
        throw new InvalidOperationException($"screencapture failed: {error}");
      }

      return await SixLabors.ImageSharp.Image.LoadAsync<Rgba32>(tempFile);
    }
    finally
    {
      if (File.Exists(tempFile))
      {
        File.Delete(tempFile);
      }
    }
  }

  private static async Task<Image<Rgba32>> CaptureAllScreensMacOSAsync()
  {
    // macOSではすべてのディスプレイをキャプチャ（デフォルト動作）
    string tempFile = Path.GetTempFileName() + ".png";

    try
    {
      var startInfo = new ProcessStartInfo
      {
        FileName = "screencapture",
        Arguments = $"-x -t png \"{tempFile}\"",
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
        CreateNoWindow = true
      };

      using var process = Process.Start(startInfo);
      if (process == null)
      {
        throw new InvalidOperationException("Failed to start screencapture process");
      }

      await process.WaitForExitAsync();

      if (process.ExitCode != 0)
      {
        string error = await process.StandardError.ReadToEndAsync();
        throw new InvalidOperationException($"screencapture failed: {error}");
      }

      return await SixLabors.ImageSharp.Image.LoadAsync<Rgba32>(tempFile);
    }
    finally
    {
      if (File.Exists(tempFile))
      {
        File.Delete(tempFile);
      }
    }
  }

  private static async Task SaveEachScreenMacOSAsync(string filePathFormat, JpegEncoder encoder, double scale)
  {
    // macOSでディスプレイIDを取得
    var displayIds = await GetMacOSDisplayIdsAsync();

    for (int i = 0; i < displayIds.Count; i++)
    {
      var screenshot = await CaptureScreenMacOSByDisplayIdAsync(displayIds[i]);

      if (scale != 1.0)
      {
        ResizeImage(screenshot, scale);
      }

      string filePath = string.Format(filePathFormat, i);
      await screenshot.SaveAsync(filePath, encoder);
    }
  }

  private static async Task<List<string>> GetMacOSDisplayIdsAsync()
  {
    var startInfo = new ProcessStartInfo
    {
      FileName = "system_profiler",
      Arguments = "SPDisplaysDataType",
      RedirectStandardOutput = true,
      UseShellExecute = false,
      CreateNoWindow = true
    };

    using var process = Process.Start(startInfo);
    if (process == null)
    {
      throw new InvalidOperationException("Failed to start system_profiler process");
    }

    string output = await process.StandardOutput.ReadToEndAsync();
    await process.WaitForExitAsync();

    // ディスプレイ数を推定（簡易実装）
    // 実際にはディスプレイIDを正確に取得する必要がありますが、
    // 簡易的に1つのディスプレイとして扱う
    var displayIds = new List<string> { "1" };

    return displayIds;
  }

  private static async Task<Image<Rgba32>> CaptureScreenMacOSByDisplayIdAsync(string displayId)
  {
    string tempFile = Path.GetTempFileName() + ".png";

    try
    {
      var startInfo = new ProcessStartInfo
      {
        FileName = "screencapture",
        Arguments = $"-x -D {displayId} -t png \"{tempFile}\"",
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
        CreateNoWindow = true
      };

      using var process = Process.Start(startInfo);
      if (process == null)
      {
        throw new InvalidOperationException("Failed to start screencapture process");
      }

      await process.WaitForExitAsync();

      if (process.ExitCode != 0)
      {
        // エラーが発生した場合は全画面キャプチャにフォールバック
        return await CaptureScreenMacOSAsync();
      }

      return await SixLabors.ImageSharp.Image.LoadAsync<Rgba32>(tempFile);
    }
    finally
    {
      if (File.Exists(tempFile))
      {
        File.Delete(tempFile);
      }
    }
  }

  #endregion

  /// <summary>
  /// 画像を指定された倍率でリサイズ
  /// </summary>
  /// <param name="image">リサイズ対象の画像</param>
  /// <param name="scale">リサイズ倍率</param>
  private static void ResizeImage(Image<Rgba32> image, double scale)
  {
    int newWidth = (int)(image.Width * scale);
    int newHeight = (int)(image.Height * scale);

    image.Mutate(x => x.Resize(newWidth, newHeight));
  }
}
