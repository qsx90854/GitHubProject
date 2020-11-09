using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Threading;
using XYZware_SLS.model;
using System.Diagnostics;

namespace XYZSDK
{
    public class DxfToImage
    {
        private const string SOURCE_X = " 10";
        private const string SOURCE_Y = " 20";
        private const string DEST_X = " 11";
        private const string DEST_Y = " 21";

        public Bitmap ConvertedImage;
        public string ConvertedDxfFolder = "";

        public DxfToImage()
        {
            //translate();
            //Main.main.languageChanged += translate;
        }

        public void translate()
        {
            //Main.main.threedview.ui.bottomStatusBar.txtBlock_StatusBarMessage.Text = Trans.T("M_LOADING_LAYERS");
            //Main.main.threedview.ui.bottomStatusBar.txtBlock_StatusBarMessage.Text = Trans.T("M_LOADING_LAYERS");
            //Main.main.threedview.ui.bottomStatusBar.txtBlock_StatusBarMessage.Text = Trans.T("M_DELETE_DATA");
            //Main.main.threedview.ui.slicingProgress.txtBlock_StatusMessage.Text = Trans.T("M_DELETE_DATA");
        }

        //public static System.Windows.Media.ImageSource LoadDXFImage(int layerNumber)
        //{
        //    //string folder = Application.CommonAppDataPath + "\\" + "ConvertedDXF";
        //    string folder = MainParameter.AppdataPath + MainParameter.AssemblyFileVersion + "\\ConvertedDXF";
        //    //Directory.CreateDirectory(folder);

        //    string newFile = folder + "\\" + "Layer" + layerNumber;
        //    Bitmap image = (Bitmap)Image.FromFile(newFile + ".jpg");

        //    return MainParameter.screenCapture.CreateBitmapSourceFromBitmap(image);
        //}


        private string DecryptDXFFile(string filename)
        {
            FileInfo file = new FileInfo(filename);
            //string _filename = Application.CommonAppDataPath 
            //    + Path.DirectorySeparatorChar + file.Name;
            string _filename = MainParameter.AppdataPath + MainParameter.AssemblyFileVersion + "\\" + file.Name;

            if (File.Exists(_filename))
                File.Delete(_filename);

            string returnValue;
            returnValue = XYZLib.XYZ.Decoder.MonoDecryptFile(filename, false);

            File.WriteAllText(_filename, returnValue);
            return _filename;
        }

        private void deleteDirectory(string path)
        {
            string[] files = Directory.GetFiles(path);

            foreach (string file in files)
            {
                //System.GC.Collect();
                //System.GC.WaitForPendingFinalizers();
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }
            try
            {
                Directory.Delete(path, false);
            }
            catch
            {
                deleteDirectory(path);
            }
        }

        public void ConvertMultipleDxf(string dxfFolder)
        {
            try
            {
                //ConvertedDxfFolder = Application.CommonAppDataPath 
                //    + Path.DirectorySeparatorChar + "ConvertedDXF";
                ConvertedDxfFolder = MainParameter.AppdataPath + MainParameter.AssemblyFileVersion + "\\ConvertedDXF";

                if (!Directory.Exists(ConvertedDxfFolder))
                {
                    Directory.CreateDirectory(ConvertedDxfFolder);
                }
                else
                {
                    deleteDirectory(ConvertedDxfFolder);
                    Directory.CreateDirectory(ConvertedDxfFolder);
                }
                Stopwatch reduceTime;
                Stopwatch reduceTime1;
                reduceTime = Stopwatch.StartNew();
              
                ////Main.main.Invoke(new Action(() => Main.main.threedview.ui.bottomStatusBar.txtBlock_StatusBarMessage.Text = Trans.T("M_LOADING_LAYERS") + "...")); //Tonton<06-14-19> Change statusbar when loading dxf layer and converted dxf
               // Main.main.Invoke(new Action(() => Main.main.threedview.ui.slicingProgress.txtBlock_StatusMessage.Text = Trans.T("m_LOADING_LAYERS") + "..."));
                var fileList = new DirectoryInfo(dxfFolder).GetFiles("*.dxf", SearchOption.TopDirectoryOnly);
                int count = fileList.Count();

                if (fileList.Count() > 0)
                {
                    int layerCount = 0;
                  //  Main.main.LogMessage("Main", "Loading layers...");
                    foreach (var file in fileList)
                    {
                        reduceTime1 = Stopwatch.StartNew();
                        Convert(file, System.Drawing.Imaging.ImageFormat.Png, ConvertedDxfFolder);
                    
                        //<Darwin> Change the format of progress bar
                        //Main.main.threedview.ui.BusyWindow.Dispatcher.Invoke(new Action(() =>
                        //{
                        //    Main.main.threedview.ui.sliceToDxf.ChangeProgressBar(false, count, layerCount, "loading layer");
                        //    layerCount++;
                        //}));
                        //Console.WriteLine(file.Name + " reduceTime1: " + reduceTime1.ElapsedMilliseconds + " ms");
                        reduceTime1.Stop();
                        reduceTime1.Reset();
                        
                        //<><><>
                    }
                   // //Main.main.Invoke(new Action(() => Main.main.threedview.ui.bottomStatusBar.txtBlock_StatusBarMessage.Text = Trans.T("M_DELETE_DATA") + "...")); //Tonton<06-14-19> Change statusbar when deleting previous dxf layer and converted dxf
                    //Main.main.Invoke(new Action(() => Main.main.threedview.ui.slicingProgress.txtBlock_StatusMessage.Text = Trans.T("M_DELETE_DATA") + "..."));
                }
                Console.WriteLine("reduceTime: " + reduceTime.ElapsedMilliseconds + " ms");
                reduceTime.Stop();
                reduceTime.Reset();
            }
            catch (Exception ex)
            {
                //Main.main.LogMessage("Main", "Error on dxf2image: " + ex.ToString());
                //MessageBox.Show("Operation failure!", "DXF Image Generate", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            
        }

        private void Convert(FileInfo dxfFile, System.Drawing.Imaging.ImageFormat imageFormat, string DestFile)
        {
            //Tonton<06-14-19> Fixed bug: Converted DXF Image size does not match with Scan Path view
            ConvertedImage = new Bitmap(248, 236);

            var lines = File.ReadAllLines(dxfFile.FullName);
            string prevString = "";

            float centerX = 124.0f;
            float centerY = 118.0f;
            float sourceX = 0.0f;
            float sourceY = 0.0f;
            float destX = 0.0f;
            float destY = 0.0f;
            using (Graphics g = Graphics.FromImage(ConvertedImage))
            {
                g.DrawLine(new Pen(Color.Black, 2), 0, 0, 0, 236);
                g.DrawLine(new Pen(Color.Black, 2), 0, 0, 248, 0);
                g.DrawLine(new Pen(Color.Black, 2), 0, 236, 248, 236);
                g.DrawLine(new Pen(Color.Black, 2), 248, 0, 248, 236);
                g.Clear(Color.White);
                //end
                foreach (var line in lines)
                {
                    if (prevString != SOURCE_X && prevString != SOURCE_Y
                        && prevString != DEST_X && prevString != DEST_Y)
                    {
                        prevString = line;
                        continue;
                    }

                    if (prevString == SOURCE_X)
                    {
                        sourceX = centerX + System.Convert.ToSingle(line, System.Globalization.CultureInfo.InvariantCulture);
                        //sourceX = centerX + float.Parse(line, System.Globalization.CultureInfo.InvariantCulture);
                    }
                    else if (prevString == SOURCE_Y)
                    {
                        sourceY = centerY - System.Convert.ToSingle(line, System.Globalization.CultureInfo.InvariantCulture);
                        //sourceY = centerY - float.Parse(line, System.Globalization.CultureInfo.InvariantCulture);
                    }
                    else if (prevString == DEST_X)
                    {
                        destX = centerX + System.Convert.ToSingle(line, System.Globalization.CultureInfo.InvariantCulture);
                        //destX = centerX + float.Parse(line, System.Globalization.CultureInfo.InvariantCulture);
                    }
                    else if (prevString == DEST_Y)
                    {
                        destY = centerY - System.Convert.ToSingle(line, System.Globalization.CultureInfo.InvariantCulture);
                        //destY = centerY - float.Parse(line, System.Globalization.CultureInfo.InvariantCulture);

                        g.DrawLine(new Pen(Color.Blue), sourceX, sourceY, destX, destY);
                    }

                    prevString = line;
                }
                string newFile = DestFile + "\\" + Path.GetFileNameWithoutExtension(dxfFile.Name);
                if (File.Exists(newFile))
                    File.Delete(newFile);

                ConvertedImage.Save(newFile + ".jpg", imageFormat);
            }
        }
    }
}
