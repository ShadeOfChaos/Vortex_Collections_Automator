using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;

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

    /// <summary>
    /// Searches for the given searchImage on the given sourceImage and returns the location of the center of the first match found.
    /// </summary>
    /// <param name="sourceImage">The source image to search in. Normally a screenshot of the desktop.</param>
    /// <param name="searchImage">The image to search for.</param>
    /// <param name="tolerance">The tolerance of the image search. Defaults to 0.95.</param>
    /// <returns>The location of the center of the first match found, or null if no match was found.</returns>
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
    /// Captures the current screen as a bitmap.
    /// </summary>
    /// <returns>The captured screen as a bitmap.</returns>
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
    /// Searches for the given images on the screen and clicks on the first match found.
    /// </summary>
    /// <param name="searchImageList">The images to search for.</param>
    /// <param name="tolerance">The tolerance of the image search. Defaults to 0.95.</param>
    /// <returns>True if a match was found and clicked, false otherwise.</returns>
    public static bool ClickImage(List<Bitmap> searchImageList, double tolerance = 0.95)
    {
        Point? location = null;
        using Bitmap screenImage = CaptureScreen();
        
        foreach(Bitmap image in searchImageList) {
            location = FindImage(screenImage, image, tolerance);
            if(location.HasValue) {
                break;
            }
        }

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
            Console.WriteLine("No image found.");
            return false;
        }
    }

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    /// <remarks>
    /// This program will search for the given images on the screen and click on them when found.
    /// The search is continuous until the program is stopped.
    /// </remarks>
    public static void Main(string[] args)
    {
        Config? config = ReadConfig();
        if(config == null) {
            Console.WriteLine("Could not read settings.json. Aborting. Press any key to exit.");
            Console.Read();
            Environment.Exit(0);
        }

        int tracker = 0;
        int failures = 0;
        bool success = false;

        try
        {
            Console.WriteLine("Starting image search...");

            List<Bitmap> imageToFindList = new List<Bitmap>();
            string[] imageFiles = Directory.GetFiles(config.imageFolder, "*.*", SearchOption.TopDirectoryOnly);
            foreach (string file in imageFiles)
            {
                imageToFindList.Add((Bitmap)Image.FromFile(file));
            }

            Console.Write("");
            Console.WriteLine("Performing Loop, hold ANY key to stop or wait till finished.");
            Console.Write("");

            while (!Console.KeyAvailable && failures < config.maxFailuresBeforeStop)
            {
                success = ClickImage(imageToFindList, config.imageSimilarityTolerance);

                if(success == true) {
                    Console.WriteLine($"Amount of buttons clicked so far: {++tracker}");
                    failures = 0;
                } else {
                    failures++;
                }

                success = false;
                Thread.Sleep(config.imageSearchDelayInMs);
            }

            int filesTracked = (int)MathF.Floor(tracker / 2f);

            Console.WriteLine("");
            Console.WriteLine("Task finished or canceled");
            Console.WriteLine($"Amount of files downloaded total: { filesTracked}");
            Console.Read();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
            Console.Read();
        }
    }

    /// <summary>
    /// Reads the configuration from settings.json.
    /// If the file does not exist, it will be created with default values.
    /// </summary>
    /// <returns>The configuration read from the file, or null if the file cannot be read.</returns>
    private static Config? ReadConfig() {
        const string SETTINGS_FILENAME = "settings.json";

        if(!File.Exists(@SETTINGS_FILENAME)) {
            Console.WriteLine("No settings.json found. Creating one.");

            Config newConfig = new Config
            {
                imageFolder = "images",
                maxFailuresBeforeStop = 10,
                imageSimilarityTolerance = 0.95,
                imageSearchDelayInMs = 6000
            };

            string json = JsonSerializer.Serialize(newConfig);
            File.WriteAllText(@SETTINGS_FILENAME, json, Encoding.UTF8);
        }

        if(!File.Exists(@SETTINGS_FILENAME)) {
            Console.WriteLine("Could not create settings.json. Aborting.");
            return null;
        }

        try {
            string settings = File.ReadAllText(@SETTINGS_FILENAME);
            if(settings.Length <= 0) {
                return null;
            }


            Config? config = JsonSerializer.Deserialize<Config>(settings);
            return config;
        } catch(Exception) {
            return null;
        }
    }
}

public class Config
{
    public required string imageFolder { get; set; }
    public required int maxFailuresBeforeStop { get; set; }
    public required double imageSimilarityTolerance { get; set; }
    public required int imageSearchDelayInMs { get; set; }
    
}
