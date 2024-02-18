using BfresLibrary;
using BfresLibrary.PlatformConverters;
using EveryFileExplorer;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace BOTWIconMaker
{
    class Program
    {
        public static string? version { get; set; }
        public static string? folder { get; set; }
        public static string? outfolder { get; set; }
        public static int created { get; set; }
        public static int skipped { get; set; }

        static void Main(string[] args)
        {
            version = "wiiu";
            folder = Directory.GetCurrentDirectory();
            outfolder = folder + "/output";
            if (args.Length > 0)
            {
                if (args.Length == 1)
                {
                    checkArg0(args[0]);
                    outfolder = folder + "/output";
                }
                else if (args.Length == 2)
                {
                    checkArg1(args[0], args[1]);
                }
                else if (args.Length == 3)
                {
                    checkArg2(args[0], args[1]);
                    outfolder = args[2];
                }
                else
                {
                  displayHelp();
                }
            }
            processFiles(version, folder, outfolder);
        }
        internal static void checkArg0(String arg)
        {
            if (isVersion(arg))
            {
                version = arg.ToLower();
            }
            else if (Directory.Exists(arg))
            {
                folder = arg;
            }
            else if (File.Exists(arg))
            {
                if (isImage(arg))
                {
                    folder = new FileInfo(arg).Directory.FullName;
                    outfolder = folder + "/output";
                    MemoryStream mem = new MemoryStream();
                    string Name = Path.GetFileNameWithoutExtension(arg);
                    if (!Directory.Exists(outfolder))
                        Directory.CreateDirectory(outfolder);
                    createBFRES_wiiu(mem, arg, Name, outfolder, true);
                    System.Environment.Exit(1);
                }
                else
                {
                    displayHelp();
                }
            }
            else
            {
                displayHelp();
            }
        }
        internal static void checkArg1(String arg0, String arg1)
        {
            checkArg0(arg0);
            if (isVersion(arg0))
            {
                if (Directory.Exists(arg1))
                {
                    folder = arg1;
                    outfolder = folder + "/output";
                }
                else
                {
                    displayHelp();
                }
            }
            else
            {
                outfolder = arg1;
            }
        }
        internal static void checkArg2(String arg1, String arg2)
        {
            if (isVersion(arg1))
            {
                version = arg1.ToLower();
                if (Directory.Exists(arg2))
                {
                    folder = arg2;
                    return;
                }
            }
            displayHelp();
        }
        private static bool isVersion(String version)
        {
            return version.ToLower() == "wiiu" || version.ToLower() == "switch";
        }
        private static void displayHelp()
        {
            Console.WriteLine("\n Sbitemico Maker\n");
            Console.WriteLine(" Usage:");
            Console.WriteLine("  .\\BfresIconMaker.exe [Version] [Source Folder] [Destination Folder]");
            Console.WriteLine("  .\\BfresIconMaker.exe [ImagePath(s)]\n");
            Console.WriteLine("  [Version]              Choose either \"wiiu\" or \"switch\". If not specified, it defaults to \"wiiu\". [Optional]");
            Console.WriteLine("  [Source Folder]        The folder where your images are located. If not specified, it uses the current folder.");
            Console.WriteLine("  [Destination Folder]   The folder where your files will be saved. If not specified, it creates an \"output\" folder in the source folder.");
            Console.WriteLine("  [Image Path]           The image you want to turn into an icon (Supports multiple images via Drag and Drop).");
            System.Environment.Exit(1);
        }
        static void processFiles(string version, string folder, string outfolder)
        {
            List<string> imageLocations = new List<string>();
            string[] files = Directory.GetFiles(folder);
            foreach (string file in files)
            {
                if (isImage(file))
                {
                    imageLocations.Add(file);
                }
            }
            if (imageLocations.Count == 0)
            {
                Console.WriteLine("No images found in: " + new DirectoryInfo(folder).Name);
                System.Environment.Exit(1);
            }
            if (!Directory.Exists(outfolder))
                Directory.CreateDirectory(outfolder);
            Parallel.ForEach(imageLocations, location =>
            {
                string Name = Path.GetFileNameWithoutExtension(location);
                string uniqueName = Path.GetFileName(location);
                Console.WriteLine("Creating bfres for " + uniqueName);
                
                try
                {
                    MemoryStream mem = new MemoryStream();
                    if (version == "wiiu")
                    {
                        createBFRES_wiiu(mem, location, Name, outfolder, true);
                    }
                    else
                    {
                        createBFRES_switch(mem, location, Name, outfolder);
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine("Failed to create sbitemico for: " + uniqueName);
                    skipped += 1;
                }
            });
            Console.WriteLine("\nCreated {0} sbitemico files.", created);
            Console.WriteLine("Skipped {0} image files.", skipped);
        }
        static bool isImage(string filePath)
        {
            string extension = Path.GetExtension(filePath).ToLower();
            return extension == ".jpg" || extension == ".jpeg" || extension == ".png" || extension == ".gif" || extension == ".bmp";
        }
        static ResFile createBFRES_wiiu(MemoryStream mem, string filePath, string Name, string outfolder, bool save)
        {
            Bitmap bitmap = new Bitmap(Image.FromFile(filePath));
            if (bitmap.Height > 512 || bitmap.Width > 512)
            {
                Console.WriteLine("Image too large to create sbitemico: " + filePath);
                skipped += 1;
                return null;
            }
            ResFile bfresFile = new ResFile();
            bfresFile.VersionMajor = 4;
            bfresFile.Alignment = 2048;
            bfresFile.VersionMajor2 = 5;
            bfresFile.VersionMinor = 0;
            bfresFile.VersionMinor2 = 3;
            bfresFile.Name = Name + ".bitemico";

            TextureShared texture = createTexture(bitmap, Name);
            bfresFile.Textures.Add(texture.Name, texture);

            if (save)
            {
                bfresFile.Save(mem);
                string path = outfolder + "/" + Name + ".sbitemico";
                File.WriteAllBytes(path, YAZ0.Compress(mem.ToArray()));
                created += 1;
                return null;
            }
            else
            {
                return bfresFile;
            }
        }
        static void createBFRES_switch(MemoryStream mem, string filePath, string Name, string outfolder)
        {
            Bitmap bitmap = new Bitmap(Image.FromFile(filePath));
            if (bitmap.Height > 512 || bitmap.Width > 512)
            {
                Console.WriteLine("Image too large to create sbitemico: " + filePath);
                skipped += 1;
                return;
            }
            ResFile bfresFile = new ResFile();
            bfresFile.VersionMajor = 0;
            bfresFile.Alignment = 4096;
            bfresFile.VersionMajor2 = 5;
            bfresFile.VersionMinor = 0;
            bfresFile.VersionMinor2 = 3;
            bfresFile.Name = Name + ".bitemico";
            bfresFile.IsPlatformSwitch = true;

            TextureShared texture = createTexture(bitmap, Name);

            ResFile resFile = createBFRES_wiiu(mem, filePath, Name, outfolder, false);
            if (resFile == null)
            {
                return;
            }
            resFile.ChangePlatform(true, 4096, 0, 5, 0, 3, ConverterHandle.BOTW);
            Console.WriteLine("Hello");
            resFile.Alignment = 0x0C;
            resFile.Save(mem);
            string path = outfolder + "/" + Name + ".sbitemico";
            File.WriteAllBytes(path, YAZ0.Compress(mem.ToArray()));
            created += 1;
        }
        static BfresLibrary.WiiU.Texture createTexture(Bitmap bitmap, String Name)
        {
            byte[] Data = imageToByte(swapRedBlueChannels(bitmap));

            BfresLibrary.WiiU.Texture texture = new BfresLibrary.WiiU.Texture();
            texture.Name = Name;
            texture.Width = (uint)bitmap.Width;
            texture.Height = (uint)bitmap.Height;
            texture.MipCount = 1;
            texture.Format = BfresLibrary.GX2.GX2SurfaceFormat.TCS_R8_G8_B8_A8_SRGB;
            texture.Use = BfresLibrary.GX2.GX2SurfaceUse.Texture;
            texture.Pitch = 96;
            texture.Alignment = 2048;

            BfresLibrary.Swizzling.GX2.GX2Surface GX2SurfaceTexture = BfresLibrary.Swizzling.GX2.CreateGx2Texture(Data, Name, 4, 0, (uint)bitmap.Width, (uint)bitmap.Height, 1, 1050, 0, 1, 1);
            texture.Data = GX2SurfaceTexture.data;
            return texture;
        }
        public static Bitmap swapRedBlueChannels(Bitmap bitmap)
        {
            Bitmap newBitmap = new Bitmap(bitmap.Width, bitmap.Height);
            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    Color pixelColor = bitmap.GetPixel(x, y);
                    Color newColor = Color.FromArgb(pixelColor.A, pixelColor.B, pixelColor.G, pixelColor.R);
                    newBitmap.SetPixel(x, y, newColor);
                }
            }
            return newBitmap;
        }
        public static byte[] imageToByte(Bitmap bitmap)
        {
            BitmapData bitmapData = new BitmapData();
            try
            {
                bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);
                int num = bitmapData.Stride * bitmap.Height;
                byte[] array = new byte[num];
                IntPtr scan = bitmapData.Scan0;
                Marshal.Copy(scan, array, 0, num);
                return array;
            }
            finally
            {
                if (bitmapData != null)
                {
                    bitmap.UnlockBits(bitmapData);
                }
            }
        }
    }
}