using EveryFileExplorer;
using Syroot.NintenTools.Bfres;
using Syroot.NintenTools.Bfres.WiiU;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace BfresIconMaker
{ 
    class Program
    {
        public static string? version { get; set; }
        public static string? folder { get; set; }
        public static string? outfolder { get; set; }

        static void Main(string[] args)
        {
            version = "wiiu";
            folder = Directory.GetCurrentDirectory();
            outfolder = folder + "/output";
            if (args.Length > 0)
            {
                if (args.Length == 1)
                {
                    // Argument can be version or input folder
                    checkArg0(args[0]);
                    outfolder = folder + "/output";
                }
                else if (args.Length == 2)
                {
                    // Argument0 can be version or input folder
                    // Argument1 can be input folder or output folder
                    checkArg1(args[0], args[1]);
                }
                else if (args.Length == 3)
                {
                    // Argument0 must be version
                    // Argument1 must be input folder
                    checkArg2(args[0], args[1]);
                    outfolder = args[2];
                } else
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
            Console.WriteLine("  .\\BfresIconMaker.exe [Version] [Source Folder] [Destination Folder]\n");
            Console.WriteLine("  [Version]              Choose either \"wiiu\" or \"switch\". If not specified, it defaults to \"wiiu\". [Optional]");
            Console.WriteLine("  [Source Folder]        The folder where your images are located. If not specified, it uses the current folder.");
            Console.WriteLine("  [Destination Folder]   The folder where your files will be saved. If not specified, it creates an \"output\" folder in the source folder.");
            System.Environment.Exit(1);
        }
        static void processFiles(string version, string folder, string outfolder)
        {
            int skipped = 0;
            int created = 0;
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
                string FileName = uniqueName + ".bitemico";
                Console.WriteLine("Creating bfres for " + uniqueName);
                try
                {
                    createBFRES(location, Name, FileName, version);
                    if (File.Exists(outfolder + "/" + FileName))
                    {
                        Byte[] newFile = YAZ0.Compress(outfolder + "/" + FileName);
                        string NewName = "/" + Name + ".sbitemico";
                        if (File.Exists(outfolder + NewName))
                        {
                            File.Delete(outfolder + NewName);
                        }
                        File.WriteAllBytes(outfolder + NewName, newFile);
                        File.Delete(outfolder + "/" + FileName);
                        created += 1;
                    } else
                    {
                        Console.WriteLine("Image too large to create sbitemico: " + uniqueName);
                        skipped += 1;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Failed to create sbitemico for: " + uniqueName);
                    skipped += 1;
                    Console.WriteLine(e.ToString());
                    if (File.Exists(outfolder + "/" + FileName))
                    {
                        File.Delete(outfolder + "/" + FileName);
                    }
                }
            });
            if (created == 0)
            {
                Directory.Delete(outfolder);
            }
            Console.WriteLine("\nCreated {0} sbitemico files.", created);
            Console.WriteLine("Skipped {0} image files.", skipped);
        }
        static bool isImage(string filePath)
        {
            string extension = Path.GetExtension(filePath).ToLower();
            return extension == ".jpg" || extension == ".jpeg" || extension == ".png" || extension == ".gif" || extension == ".bmp";
        }
        static void createBFRES(string filePath, string Name, string FileName, string version)
        {
            Bitmap bitmap = new Bitmap(Image.FromFile(filePath));
            if (bitmap.Height > 512 || bitmap.Width > 512)
            {
                return;
            }
            ResFile bfresFile = new ResFile();
            if (version == "switch")
            {
                bfresFile.VersionMajor = 4;
                bfresFile.VersionMajor2 = 5;
                bfresFile.VersionMinor = 0;
                bfresFile.VersionMinor2 = 3;
                bfresFile.Alignment = 4096;
                bfresFile.IsPlatformSwitch = true;

            }
            else
            {
                bfresFile.VersionMajor = 4;
                bfresFile.VersionMajor2 = 5;
                bfresFile.VersionMinor = 0;
                bfresFile.VersionMinor2 = 3;
                bfresFile.Alignment = 2048;
            }
            bfresFile.Name = Name + ".bitemico";

            Texture texture = new Texture();
            texture.Name = Name;
            texture.Width = (uint)bitmap.Width;
            texture.Height = (uint)bitmap.Height;
            texture.MipCount = 1;
            texture.Format = Syroot.NintenTools.Bfres.GX2.GX2SurfaceFormat.TCS_R8_G8_B8_A8_SRGB;
            texture.Use = Syroot.NintenTools.Bfres.GX2.GX2SurfaceUse.Texture;
            texture.Pitch = 96;
            texture.Alignment = 2048;
            byte[] Data = imageToByte(swapRedBlueChannels(bitmap));
            Syroot.NintenTools.Bfres.Swizzling.GX2.GX2Surface GX2SurfaceTexture = Syroot.NintenTools.Bfres.Swizzling.GX2.CreateGx2Texture(Data, Name, 4, 0, (uint)bitmap.Width, (uint)bitmap.Height, 1, 1050, 0, 1, 1);
            texture.Data = GX2SurfaceTexture.data;
            bfresFile.Textures.Add(texture.Name, texture);
            bfresFile.Save(outfolder + "/" + FileName);
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
            BitmapData bitmapData = null;
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