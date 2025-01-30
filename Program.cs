using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

public class ImageSearcherConsole
{
    [DllImport("user32.dll")]
    private static extern bool SetCursorPos(int x, int y);
    [DllImport("user32.dll")]
    private static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);
    [DllImport("user32.dll")]
    private static extern int GetSystemMetrics(int nIndex);

    private const int SM_CXSCREEN = 0;
    private const int SM_CYSCREEN = 1;
    private const int MOUSEEVENTF_LEFTDOWN = 0x02;
    private const int MOUSEEVENTF_LEFTUP = 0x04;

    public static Point? FindImageOnScreen(Bitmap searchImage, double tolerance = 0.95)
    {
        using Bitmap screenImage = CaptureScreen();
        return FindImage(screenImage, searchImage, tolerance);
    }

    public static Point? FindImage(Bitmap sourceImage, Bitmap searchImage, double tolerance = 0.95)
    {
        int sourceWidth = sourceImage.Width;
        int sourceHeight = sourceImage.Height;
        int searchWidth = searchImage.Width;
        int searchHeight = searchImage.Height;

        for (int y = 0; y <= sourceHeight - searchHeight; y++)
        {
            for (int x = 0; x <= sourceWidth - searchWidth; x++)
            {
                bool match = true;
                for (int sy = 0; sy < searchHeight; sy++)
                {
                    for (int sx = 0; sx < searchWidth; sx++)
                    {
                        Color sourcePixel = sourceImage.GetPixel(x + sx, y + sy);
                        Color searchPixel = searchImage.GetPixel(sx, sy);

                        if (Math.Abs(sourcePixel.R - searchPixel.R) > 255 * (1 - tolerance) ||
                            Math.Abs(sourcePixel.G - searchPixel.G) > 255 * (1 - tolerance) ||
                            Math.Abs(sourcePixel.B - searchPixel.B) > 255 * (1 - tolerance))
                        {
                            match = false;
                            break;
                        }
                    }
                    if (!match) break;
                }

                if (match)
                {
                    return new Point(x + searchWidth / 2, y + searchHeight / 2);
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Captures a screenshot of the entire screen.
    /// </summary>
    /// <returns>A bitmap image of the screen.</returns>
    private static Bitmap CaptureScreen()
    {
        int screenWidth = GetSystemMetrics(SM_CXSCREEN);
        int screenHeight = GetSystemMetrics(SM_CYSCREEN);

        using var screenImage = new Bitmap(screenWidth, screenHeight, PixelFormat.Format32bppArgb);
        using var graphics = Graphics.FromImage(screenImage);
        graphics.CopyFromScreen(0, 0, 0, 0, screenImage.Size);
        return new Bitmap(screenImage);
    }

    /// <summary>
    /// Finds an image on the screen and performs a mouse click at the found location.
    /// </summary>
    /// <param name="searchImage">The image to find on the screen.</param>
    /// <param name="tolerance">The tolerance for the image search. Defaults to 0.95.</param>
    /// <returns>true if the image was found and clicked, false otherwise.</returns>
    public static bool ClickImage(Bitmap searchImage, double tolerance = 0.95)
    {
        Point? location = FindImageOnScreen(searchImage, tolerance);
        if (location.HasValue)
        {
            Console.WriteLine($"Image found at: {location.Value.X}, {location.Value.Y}");
            SetCursorPos(location.Value.X, location.Value.Y);
            mouse_event(MOUSEEVENTF_LEFTDOWN, location.Value.X, location.Value.Y, 0, 0);
            mouse_event(MOUSEEVENTF_LEFTUP, location.Value.X, location.Value.Y, 0, 0);
            Console.WriteLine("Mouse click performed.");
            return true;
        }
        else
        {
            Console.WriteLine("Image not found.");
            return false;
        }
    }

    /// <summary>
    /// The main entry point for the application.
    /// This application will search for two images on the screen and click on them.
    /// The first image is a variant of the Vortex download button, and the second is a variant of the Nexus download button.
    /// The application will loop until the user holds down the 'C' key.
    /// </summary>
    public static void Main(string[] args)
    {
        int tracker = 0;

        try
        {
            Console.WriteLine("Starting image search...");
            Thread.Sleep(2000);

            Bitmap imageToFind = (Bitmap)Image.FromFile("images/Vortex_Download_Button_Variant1.png");
            Bitmap imageVariantToFind = (Bitmap)Image.FromFile("images/Vortex_Download_Button_Variant2.png");
            Bitmap image2ToFind = (Bitmap)Image.FromFile("images/Nexus_Download_Button_Variant1.png");
            Bitmap image2VariantToFind = (Bitmap)Image.FromFile("images/Nexus_Download_Button_Variant2.png");

            Console.WriteLine("Performing Loop, hold C to stop");
            
            // while(!Console.KeyAvailable) {
            while (true)
            {
                bool success1 = false;
                bool success2 = false;

                while (!success1)
                {
                    success1 = ClickImage(imageToFind, .85);
                    if (!success1)
                    {
                        success1 = ClickImage(imageVariantToFind, .85);
                    }
                }
                if (!success1)
                {
                    break;
                }
                Console.WriteLine("Vortex Download Button Clicked");
                Thread.Sleep(500);

                while (!success2)
                {
                    success2 = ClickImage(image2ToFind, .85);
                    if (!success2)
                    {
                        success2 = ClickImage(image2VariantToFind, .85);
                    }
                }
                if (!success2)
                {
                    break;
                }
                Console.WriteLine("Nexus Download Button Clicked");
                Thread.Sleep(5000);

                success1 = false;
                success2 = false;

                Console.WriteLine($"Amount of files downloaded so far: {++tracker}");
            }

            Console.WriteLine("Image search and click completed.");
            Console.WriteLine($"Amount of files downloaded total: {tracker}");
            Console.Read();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
            Console.Read();
        }
    }
}
