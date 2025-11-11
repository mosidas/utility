
using Helper;

namespace DebugConsole;

public class Program
{
  public static async Task Main(string[] args)
  {
    await ScreenCapture.SaveCompressedScreenshotAsync("screenshot-q100-s100.jpg", quality: 100);
    await ScreenCapture.SaveCompressedScreenshotAsync("screenshot-q090-s100.jpg", quality: 90);
    await ScreenCapture.SaveCompressedScreenshotAsync("screenshot-q080-s100.jpg", quality: 80);
    await ScreenCapture.SaveCompressedScreenshotAsync("screenshot-q070-s100.jpg", quality: 70);
    await ScreenCapture.SaveCompressedScreenshotAsync("screenshot-q060-s100.jpg", quality: 60);
    await ScreenCapture.SaveCompressedScreenshotAsync("screenshot-q050-s100.jpg", quality: 50);
    await ScreenCapture.SaveCompressedScreenshotAsync("screenshot-q040-s100.jpg", quality: 40);
    await ScreenCapture.SaveCompressedScreenshotAsync("screenshot-q030-s100.jpg", quality: 30);
    await ScreenCapture.SaveCompressedScreenshotAsync("screenshot-q020-s100.jpg", quality: 20);

    // await ScreenCapture.SaveCompressedScreenshotAsync("screenshot-q100-s090.jpg", quality: 100, scale: 0.9);
    // await ScreenCapture.SaveCompressedScreenshotAsync("screenshot-q100-s080.jpg", quality: 100, scale: 0.8);
    // await ScreenCapture.SaveCompressedScreenshotAsync("screenshot-q100-s070.jpg", quality: 100, scale: 0.7);
    // await ScreenCapture.SaveCompressedScreenshotAsync("screenshot-q100-s060.jpg", quality: 100, scale: 0.6);
    // await ScreenCapture.SaveCompressedScreenshotAsync("screenshot-q100-s050.jpg", quality: 100, scale: 0.5);

    // await ScreenCapture.SaveCompressedScreenshotAsync("screenshot-q090-s090.jpg", quality: 90, scale: 0.9);
    // await ScreenCapture.SaveCompressedScreenshotAsync("screenshot-q080-s080.jpg", quality: 80, scale: 0.8);
    // await ScreenCapture.SaveCompressedScreenshotAsync("screenshot-q070-s070.jpg", quality: 70, scale: 0.7);
    // await ScreenCapture.SaveCompressedScreenshotAsync("screenshot-q060-s060.jpg", quality: 60, scale: 0.6);
    // await ScreenCapture.SaveCompressedScreenshotAsync("screenshot-q050-s050.jpg", quality: 50, scale: 0.5);

    // await ScreenCapture.SaveCompressedScreenshotAsync("screenshot-q100-s010.jpg", quality: 100, scale: 0.1);
    // await ScreenCapture.SaveCompressedScreenshotAsync("screenshot-q010-s100.jpg", quality: 10, scale: 1.0);
    // await ScreenCapture.SaveCompressedScreenshotAsync("screenshot-q010-s010.jpg", quality: 10, scale: 0.1);

    Console.WriteLine("Screenshots saved.");
  }
}
