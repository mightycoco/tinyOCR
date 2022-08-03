using System;
using System.IO;
using System.Windows;
using System.Threading;
using System.Windows.Media.Imaging;
using Tesseract;

// publishing:
// dotnet publish -r win-x64 -c Release /p:PublishSingleFile=true
// dotnet publish -r win-x64 -c Release /p:PublishSingleFile=true /p:SelfContained=true /p:RuntimeIdentifier=true /p:PublishReadyToRun=true
namespace tinyOCR
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            string scpath = System.IO.Path.GetTempPath() + @"tinyOCR.png";

            if (StoreClipboardImage(scpath)) {
                using (var engine = new TesseractEngine(@"tessdata", "eng", EngineMode.Default)) {
                    using (var img = Pix.LoadFromFile(scpath))
                    {
                        using (var page = engine.Process(img))
                        {
                            var text = page.GetText();
                            Console.WriteLine(text);
                            Clipboard.SetData(DataFormats.Text, (Object)text);
                        }
                    }
                }
                File.Delete(scpath);
            }

            Console.WriteLine("done.");
            Thread.Sleep(500);
        }

        static bool StoreClipboardImage(string path)
        {
            bool ret = false;
            Thread staThread = new Thread(x =>
            {
                try
                {
                    BitmapSource img = Clipboard.GetImage();
                    if (img != null)
                    {
                        Console.WriteLine("writing file " + path);
                        using (var fileStream = new FileStream(path, FileMode.Create))
                        {
                            BitmapEncoder encoder = new PngBitmapEncoder();
                            encoder.Frames.Add(BitmapFrame.Create(img));
                            encoder.Save(fileStream);
                            ret = true;
                        }
                    }
                    else
                    {
                        Console.WriteLine("nothing in clipboard");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            });
            staThread.SetApartmentState(ApartmentState.STA);
            staThread.Start();
            staThread.Join();
            return ret;
        }
    }
}