using System;
using System.Collections.Generic;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Drawing;
using System.Runtime.InteropServices;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System.Windows.Forms;
using System.Threading;
namespace XYZSDK
{
    class TestEmgucvDLL
    {
        public void testADDPloygn()
        {
            int Length = 10;
            double scale = 1.1f;
            Point[] points = new Point[Length / 2];
            for (int i = 0; i < Length; i += 2)
            {
                points[i / 2].X = Convert.ToInt32(i * scale, new CultureInfo("en-US"));
                points[i / 2].Y = Convert.ToInt32((i+1) * scale, new CultureInfo("en-US"));
            }
            int temp =  Convert.ToInt32(CvInvoke.ContourArea(new VectorOfPoint(points), true), new CultureInfo("en-US"));
            Console.WriteLine("test CvInvoke function");
        }
    }

    public class EnglishStreamWriter2 : StreamWriter
    {
        public EnglishStreamWriter2(Stream path)
            : base(path, Encoding.ASCII)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
        }

        public override IFormatProvider FormatProvider
        {
            get
            {
                return new CultureInfo("en-US");
            }
        }
    }
    class ThreeWLDataGenerator
    {
        string svgPath;
        string gcodePath;
        string dpPath;
        string dxfPath;
        float layerHeight;
        XmlReaderSettings settings;
        DynamicPowder DP;
        GCode gCode;
        DXF dxf;
        bool encryptDXF;
        public int layerAmount;

        int errorstatus = 0;

        public ThreeWLDataGenerator(string svgPath, string gcodePath, string dpPath, string dxfPath, float layerHeight, bool encryptDXF = true)
        {
            this.svgPath = svgPath;
            this.gcodePath = gcodePath;
            this.dpPath = dpPath;
            this.dxfPath = dxfPath;
            this.layerHeight = layerHeight;
            this.encryptDXF = encryptDXF;
        }

        public bool Init()
        {
            try
            {
                settings = new XmlReaderSettings() { ValidationType = ValidationType.None, ProhibitDtd = false, XmlResolver = null };
                DP = new DynamicPowder((int)PrinterSetting.PrintAreaWidth, (int)PrinterSetting.PrintAreaDepth);
                gCode = new GCode(gcodePath);
                //dxf = new DXF(Main.printerSettings.PrintAreaWidth, Main.printerSettings.PrintAreaDepth, dxfPath, encryptDXF);
                dxf = new DXF(230, 230, dxfPath, encryptDXF);
                layerAmount = GetLayerAmount();
                return true;
            }
            catch (Exception ex)
            {
                //Main.main.LogMessage("3WL Data", "Error encountered: Init");
                //Main.main.LogMessage("3WL Data", ex.Message);
                //Main.main.LogMessage("3WL Data", ex.ToString());
                Console.WriteLine("3WL Data, Error encountered: Init");
                Console.WriteLine("3WL Data", ex.ToString());
                SVGTo3WL.logFile.writer.WriteLine("3WL Data, Error encountered: Init");
                SVGTo3WL.logFile.writer.WriteLine("3WL Data", ex.ToString());
                return false;
            }
        }

        public bool GenerateAll()
        {
            if (!File.Exists(svgPath)) return false;
            try
            {
                using (XmlReader xmlReader = XmlReader.Create(svgPath, settings))
                {
                    gCode.Header((float)MainParameter.selLayerHeight, MainParameter.layercount);
                    int layerCount = -1;
                    while (xmlReader.Read())
                    {
                        if (xmlReader.NodeType == System.Xml.XmlNodeType.Element)
                        {
                            if (xmlReader.Name == "g")
                            {
                                layerCount++;
                                if (layerCount != 0)
                                {
                                    dxf.EndSection();
                                    gCode.EndLayer();
                                    DP.ComputePowder();
                                }

                                if (PrinterSetting.UseSlic3r == 1)
                                    gCode.StartLayer(Convert.ToSingle(xmlReader.GetAttribute("slic3r:z"), new CultureInfo("en-US")));
                                else
                                    gCode.StartLayer(Convert.ToSingle(xmlReader.GetAttribute("z"), new CultureInfo("en-US")));

                                dxf.CreateNew(layerCount);
                            }
                            else if (xmlReader.Name == "polygon")
                            {
                                string data = xmlReader.GetAttribute("points");
                                if (data != null)
                                {
                                    float[] points = Array.ConvertAll(data.Split(' ', ','), s => float.Parse(s, CultureInfo.InvariantCulture));
                                    if (points.Length % 2 == 0)
                                    {
                                        dxf.AddPolygon(points);
                                        DP.AddPolygon(points);
                                        gCode.AddPolygon(points);
                                    }
                                }
                            }
                        }
                    }
                    dxf.EndSection();
                    gCode.EndLayer();
                    gCode.EndAndRelease();
                    DP.ComputePowder();
                    DP.Output(dpPath);
                }
                return true;
            }
            catch (Exception ex)
            {
                //Main.main.LogMessage("3WL Data", "Error encountered.");
                //Main.main.LogMessage("3WL Data", ex.Message);
                //Main.main.LogMessage("3WL Data", ex.ToString());
                SVGTo3WL.logFile.writer.WriteLine("3WL Data, Error encountered.");
                SVGTo3WL.logFile.writer.WriteLine("3WL Data", ex.ToString());
                gCode.Release();
                dxf.Release();
                return false;
            }
        }
        
        public bool GeneratePowderFile()
        {
            SVGTo3WL.logFile.writer.WriteLine("3WL Data, PreTest: Generate powder file, svg file path: {0}.", svgPath);
            if (!File.Exists(svgPath)) return false;
            SVGTo3WL.logFile.writer.WriteLine("3WL Data, Start: Generate powder file.");
            try
            {
                using (XmlReader xmlReader = XmlReader.Create(svgPath, settings))
                {
                    int layerCount = -1;
                    while (xmlReader.Read())
                    {
                        if (xmlReader.NodeType == System.Xml.XmlNodeType.Element)
                        {
                            if (xmlReader.Name == "g")
                            {
                                //SVGTo3WL.logFile.writer.WriteLine("3WL Data, Generate powder file: Process xmlReader:g");
                                layerCount++;
                                if (layerCount != 0)
                                {
                                   // SVGTo3WL.logFile.writer.WriteLine("3WL Data, Generate powder file: Start DP.ComputePowder");
                                    DP.ComputePowder();
                                   // SVGTo3WL.logFile.writer.WriteLine("3WL Data, Generate powder file: End DP.ComputePowder");
                                }
                            }
                            else if (xmlReader.Name == "polygon")
                            {
                                //SVGTo3WL.logFile.writer.WriteLine("3WL Data, Generate powder file: Process xmlReader:polygon");
                                string data = xmlReader.GetAttribute("points");
                                if (data != null && data != "")
                                {
                                    string[] temp = data.Split(' ', ',');
                                    float[] points = Array.ConvertAll(temp, s=>float.Parse(s, CultureInfo.InvariantCulture));
                                    if (points.Length % 2 == 0)
                                    {
                                        //SVGTo3WL.logFile.writer.WriteLine("3WL Data, Generate powder file: Start DP.AddPolygon");
                                        DP.AddPolygon(points);
                                        //SVGTo3WL.logFile.writer.WriteLine("3WL Data, Generate powder file: End DP.AddPolygon");
                                    }
                                       
                                }
                            }
                        }
                    }
                    //SVGTo3WL.logFile.writer.WriteLine("3WL Data, Generate powder file: End File, Start DP.ComputePowder");
                    DP.ComputePowder();
                    //SVGTo3WL.logFile.writer.WriteLine("3WL Data, Generate powder file: End File, End DP.ComputePowder");
                    SVGTo3WL.logFile.writer.WriteLine("3WL Data, Generate powder file: End File, Start DP.Output");
                    SVGTo3WL.logFile.writer.WriteLine("3WL Data, Generate powder file: End File, Start DP.Output Path :{0}", dpPath);
                    DP.Output(dpPath);
                    SVGTo3WL.logFile.writer.WriteLine("3WL Data, Generate powder file: End File, End DP.Output");
                    SVGTo3WL.logFile.writer.Flush();
                    return true;
                }
            }
            catch (Exception ex)
            {
                //Main.main.LogMessage("3WL Data", "Error encountered: Generate powder file.");
                //Main.main.LogMessage("3WL Data", ex.Message);
                //Main.main.LogMessage("3WL Data", ex.ToString());
                SVGTo3WL.logFile.writer.WriteLine("3WL Data, Error encountered: Generate powder file.");
                SVGTo3WL.logFile.writer.WriteLine("3WL Data exception: {0}", ex.ToString());
                SVGTo3WL.logFile.writer.WriteLine("3WL Data error status: {0}", DP.errorstatus);
                SVGTo3WL.logFile.writer.Flush();
                Console.WriteLine("GeneratePowderFile error.");
                Console.WriteLine("GeneratePowderFile exception: {0}.", ex.ToString());
                
                gCode.Release();
                return false;
            }
        }   

        public bool GenerateGCode()
        {
            if (!File.Exists(svgPath)) return false;
            try
            {
                using (XmlReader xmlReader = XmlReader.Create(svgPath, settings))
                {
                    int layerCount = -1;
                    gCode.Header((float)MainParameter.selLayerHeight, MainParameter.layercount);
                    while (xmlReader.Read())
                    {
                        if (xmlReader.NodeType == System.Xml.XmlNodeType.Element)
                        {
                            if (xmlReader.Name == "g")
                            {
                                layerCount++;
                                if (layerCount != 0)
                                {
                                    gCode.EndLayer();
                                }
                                if (PrinterSetting.UseSlic3r == 1)
                                    gCode.StartLayer(Convert.ToSingle(xmlReader.GetAttribute("slic3r:z"), new CultureInfo("en-US")));
                                else
                                    gCode.StartLayer(Convert.ToSingle(xmlReader.GetAttribute("z"), new CultureInfo("en-US")));
                            }
                            else if (xmlReader.Name == "polygon")
                            {
                                string data = xmlReader.GetAttribute("points");
                                if (data != null && data != "")
                                {
                                    float[] points = Array.ConvertAll(data.Split(' ', ','), s => float.Parse(s, CultureInfo.InvariantCulture));
                                    if (points.Length % 2 == 0)
                                        gCode.AddPolygon(points);
                                }
                            }
                        }
                    }
                    gCode.EndLayer();
                    gCode.EndAndRelease();
                }
                return true;
            }
            catch (Exception ex)
            {
                //Main.main.LogMessage("3WL Data", "Error encountered: Generate GCode");
                //Main.main.LogMessage("3WL Data", ex.Message);
                //Main.main.LogMessage("3WL Data", ex.ToString());
                SVGTo3WL.logFile.writer.WriteLine("3WL Data, Error encountered: Generate GCode");
                SVGTo3WL.logFile.writer.WriteLine("3WL Data exception: {0}", ex.ToString());
                SVGTo3WL.logFile.writer.Flush();
                Console.WriteLine("3WL Data, Error encountered: Generate GCode");
                Console.WriteLine("3WL Data exception: {0}", ex.ToString());
                gCode.Release();
                return false;
            }
        }

        public bool GenerateDXF()
        {
            if (!File.Exists(svgPath)) return false;
            try
            {
                using (XmlReader xmlReader = XmlReader.Create(svgPath, settings))
                {
                    int layerCount = -1;

                    //Main.main.LogMessage("Main", "Generating layers...");
                    while (xmlReader.Read())
                    {
                        if (xmlReader.NodeType == System.Xml.XmlNodeType.Element)
                        {
                            if (xmlReader.Name == "g")
                            {
                                layerCount++;
                                if (layerCount != 0)
                                {
                                    dxf.EndSection();
                                }
                                dxf.CreateNew(layerCount);
                                //<Darwin> Change the format of progress bar
                                //Main.main.threedview.ui.BusyWindow.Dispatcher.Invoke(new Action(() =>
                                //{
                                //    Main.main.threedview.ui.sliceToDxf.ChangeProgressBar(false, Main.main.layercount, layerCount, "generating layer");
                                //}));
                                //<><><>
                            }
                            else if (xmlReader.Name == "polygon")
                            {
                                string data = xmlReader.GetAttribute("points");
                                if (data != null && data != "")
                                {
                                    float[] points = Array.ConvertAll(data.Split(' ', ','), s => float.Parse(s, CultureInfo.InvariantCulture));
                                    if (points.Length % 2 == 0)
                                        dxf.AddPolygon(points);
                                }
                            }
                        }
                    }
                    dxf.EndSection();
                }

                //Main.main.LogMessage("Main", "ESTIMATED TIME ----------------------");
                //Main.main.LogMessage("Main", "Total Contour Scan Time: " + Main.main.total_contourTime);
                //Main.main.LogMessage("Main", "Total Infill Scan Time: " + Main.main.total_infillTime);
                //Main.main.LogMessage("Main", "Total Estimated Marking Time: " + Main.main.totalEstimatedTime + " Second/s", "");
                //Main.main.LogMessage("Main", "Total Time (Add logo - Apply Marking mate Settings - Calling API function): " + (Main.main.TotalSpanTime / 1000).ToString("0.000") + " Second/s", "");

                //Main.main.totalPrintingTime += Convert.ToDouble(Main.main.threedview.ui.ProfileSettings_UI.initializationET());
                //#region formula is in the Buildtime Function
                ////Main.main.totalPrintingTime += Convert.ToDouble(Main.main.threedview.ui.ProfileSettings_UI.RecoatingET());
                ////Main.main.totalPrintingTime += Convert.ToDouble(Main.main.threedview.ui.ProfileSettings_UI.PistonHeightChangeET());
                ////Main.main.totalPrintingTime += Convert.ToDouble(Main.main.threedview.ui.ProfileSettings_UI.WarmUpET());
                ////Main.main.totalPrintingTime += Convert.ToDouble(Main.main.threedview.ui.ProfileSettings_UI.MarkingET()) ;
                //#endregion

                //Main.main.totalPrintingTime += Convert.ToDouble(Main.main.threedview.ui.ProfileSettings_UI.BuildTime());

                double estTime = 0.0;
                double hour = 0.0;
                double min = 0.0;
                double sec = 0.0;

                //Modified the computation of buildgap for abnormal buildgap Percentage.
                //int buildgap = Main.main.threedview.ui.ProfileSettings_UI.BuildGap(Convert.ToUInt32(Main.main.totalPrintingTime));
                //if ((buildgap * -1) < Main.main.totalPrintingTime)
                //{
                //    estTime = buildgap;
                //    hour = estTime / 3600000;
                //    min = (estTime % 3600000) / 60000;
                //    sec = (estTime % 60000) / 1000;
                //    Main.main.LogMessage("Main", "buildgap Time: " + Math.Truncate(hour).ToString("00") + ":" + Math.Truncate(min).ToString("00") + ":" + Math.Truncate(sec).ToString("00"), "");
                //    Main.main.totalPrintingTime += buildgap; //Convert.ToDouble(Main.main.threedview.ui.ProfileSettings_UI.BuildGap(Convert.ToUInt32(Main.main.totalPrintingTime)));
                //}
                //<><><>

                //double buildTime = (Convert.ToDouble(Main.main.threedview.ui.ProfileSettings_UI.BuildTime()) * 0.05);
               // estTime = buildTime;
               // hour = estTime / 3600000;
               // min = (estTime % 3600000) / 60000;
               // sec = (estTime % 60000) / 1000;
              //  Main.main.LogMessage("Main", "buildTime Time: " + Math.Truncate(hour).ToString("00") + ":" + Math.Truncate(min).ToString("00") + ":" + Math.Truncate(sec).ToString("00"), "");
     
               // Main.main.totalPrintingTime += buildTime;

                //Main.main.totalPrintingTime += Convert.ToDouble(Main.main.threedview.ui.ProfileSettings_UI.CooldownET()); remove Cooldown ET.
              //  estTime = Main.main.totalPrintingTime;
              //  hour = estTime / 3600000;
              //  min = (estTime % 3600000) / 60000;
              //  sec = (estTime % 60000) / 1000;

              //  TimeSpan TotalPrint = new TimeSpan((long)Main.main.totalPrintingTime * 1000);
             //   if (Main.main.IsComputeMarkingTime)
             //       Main.main.totalPrintTime_txt = Math.Truncate(hour).ToString("00") + "h:" + Math.Truncate(min).ToString("00") + "m";
             //   else
             //       Main.main.totalPrintTime_txt = "-";
             //   Main.main.LogMessage("Main", "Total Printing Time: " + Math.Truncate(hour).ToString("00") + ":" + Math.Truncate(min).ToString("00") + ":"+Math.Truncate(sec).ToString("00"), "");
               
             //   Main.main.generating_dxf = true;
            //    Main.main.LogMessage("Main", "--------------------------------------");

                return true;
            }
            catch (Exception ex)
            {
             //   Main.main.LogMessage("3WL Data", "Error encountered: Generate GCode");
             //   Main.main.LogMessage("3WL Data", ex.Message);
             //   Main.main.LogMessage("3WL Data", ex.ToString());
                dxf.Release();
                return false;
            }
        }

        public bool EditGcodeContent()
        {
            string temp_gcodePath = MainParameter.AppdataPath+ MainParameter.AssemblyFileVersion + "\\composition\\tempgcode.gcode";
            //string temp_gcodePath = Application.CommonAppDataPath + "\\composition\\tempgcode.gcode";
            string content =  "";
            string checker = "non-sintered powder";
            FileStream fs = File.Open(temp_gcodePath, FileMode.Create);
            EnglishStreamWriter2 writer = new EnglishStreamWriter2(fs);
            if (!File.Exists(gcodePath)) return false;
            try
            {
                using (StreamReader streamReader = new StreamReader(gcodePath/*, Encoding.UTF8*/))
                {
                    //var est_factor = Main.main.threedview.ui.ProfileSettings_UI.EstimatedTime_Factor;
                    //asd
                    double est_time = 0;// Main.main.totalEstimatedTime;
                    //if (!Main.main.IsComputeMarkingTime)
                    //    est_time = 0.00;
                    while (!streamReader.EndOfStream)
                    {
                        content = streamReader.ReadLine();
                        writer.Write(content+"\n");
                        if (content.Contains(checker))
                        {
                            writer.Write("; total dxf file size (kb): " + Convert.ToInt64(MainParameter.TotalDXFFileSize) + "\n");
                            writer.Write("; total Estimated Marking time (ms): " + est_time.ToString("0.00") + "\n");
                            //writer.WriteLine("; total dxf file size (kb): " + Math.Round(Main.main.TotalDXFFileSize, 2) + "\n");
                            //writer.Write("; total Estimated Marking time (ms): " + est_time.ToString("0.00") + "\n");
                        }
                    }
                    writer.Flush();
                    writer.Close();
                }

                File.Delete(gcodePath);
                File.Move(temp_gcodePath, gcodePath);

                return true;
            }
            catch(Exception ex)
            {
                SVGTo3WL.logFile.writer.WriteLine("3WL Data, Error encountered: EditGcodeContent.");
                SVGTo3WL.logFile.writer.WriteLine("3WL Data exception: {0}", ex.ToString());
                SVGTo3WL.logFile.writer.Flush();
                Console.WriteLine("3WL Data, Error encountered: EditGcodeContent");
                Console.WriteLine("3WL Data exception: {0}", ex.ToString());
                gCode.Release();
                return false;
            }
        }
        
        public bool GenerateHeaderFile()
        {
            //string headerFile = Application.CommonAppDataPath + "\\composition\\Header.txt";
            string headerFile = MainParameter.AppdataPath + MainParameter.AssemblyFileVersion + "\\composition\\Header.txt";
            FileStream fs = File.Open(headerFile, FileMode.Create);
            EnglishStreamWriter2 writer = new EnglishStreamWriter2(fs);
            try
            {
                writer.WriteLine("; total dxf file size (kb): " + Math.Round(MainParameter.TotalDXFFileSize, 2));
                writer.Write("; total Estimated Marking time (secs): " + Math.Round(MainParameter.totalEstimatedTime, 4));
                writer.Flush();
                writer.Close();
                return true;
            }
            catch(Exception ex)
            {
                SVGTo3WL.logFile.writer.WriteLine("3WL Data, Error encountered: GenerateHeaderFile.");
                SVGTo3WL.logFile.writer.WriteLine("3WL Data exception: {0}", ex.ToString());
                SVGTo3WL.logFile.writer.Flush();
                Console.WriteLine("3WL Data, Error encountered: GenerateHeaderFile");
                Console.WriteLine("3WL Data exception: {0}", ex.ToString());
                gCode.Release();
                return false;
            }
        }
        public bool GenerateDefineFile()
        {
            //string headerFile = Application.CommonAppDataPath + "\\composition\\Header.txt";
            string headerFile = MainParameter.AppdataPath + MainParameter.AssemblyFileVersion + "\\composition\\Autodesk.txt";
            MainParameter.DefineFile = headerFile;
            FileStream fs = File.Open(headerFile, FileMode.Create);
            EnglishStreamWriter2 writer = new EnglishStreamWriter2(fs);
            try
            {
                writer.WriteLine("This file generate by XYZSDK for Autodesk.");
                writer.Flush();
                writer.Close();
                return true;
            }
            catch(Exception ex)
            {
                SVGTo3WL.logFile.writer.WriteLine("3WL Data, Error encountered: GenerateDefineFile.");
                SVGTo3WL.logFile.writer.WriteLine("3WL Data exception: {0}", ex.ToString());
                SVGTo3WL.logFile.writer.Flush();
                Console.WriteLine("3WL Data, Error encountered: GenerateDefineFile");
                Console.WriteLine("3WL Data exception: {0}", ex.ToString());
                gCode.Release();
                return false;
            }
        }

        public bool GenerateRecordFile()
        {
            try
            {
                //Main.main.LogMessage("Main", "Generating Record file ....." , "");
                //string recordFilePath = Application.CommonAppDataPath + "\\composition\\RecordFile.txt";
                string recordFilePath = MainParameter.AppdataPath + MainParameter.AssemblyFileVersion + "\\composition\\RecordFile.txt";
                //StringBuilder shortPath = new StringBuilder(255);
                //Main.GetShortPathName(recordFilePath, shortPath, shortPath.Capacity);
                //recordFilePath = shortPath.ToString();
                FileStream fs = File.Open(recordFilePath, FileMode.Create);
                EnglishStreamWriter2 writer = new EnglishStreamWriter2(fs);
                //Main.main.threedview.ui.ProfileSettings_UI.Dispatcher.Invoke(new Action(() =>{
                //writer.WriteLine("Infill Scan Speed: "+Main.main.threedview.ui.ProfileSettings_UI.txt_InfillScanningSpeed.Text);
                //writer.WriteLine("Contour Scan Speed: " + Main.main.threedview.ui.ProfileSettings_UI.txt_ContourScanningSpeed.Text);
                //writer.WriteLine("Beam Offset: " + Main.main.threedview.ui.ProfileSettings_UI.txt_InfillContourOffset.Text);
                //writer.WriteLine("Fill Round Pitch: " + Main.main.threedview.ui.ProfileSettings_UI.txt_InfillBorderOffset.Text);
                //writer.WriteLine("Border: 0.12");
                //writer.WriteLine("Step Angle: 90");
                //writer.WriteLine("Offset Angle: 180");
                //writer.WriteLine("Infill Pitch: " + Main.main.threedview.ui.ProfileSettings_UI.txt_InfillSinterPitch.Text);
                //writer.WriteLine("Start Angle: " + Main.main.threedview.ui.ProfileSettings_UI.txt_StartAngle.Text);
                //writer.WriteLine("Powder Density:" + Main.main.threedview.ui.ProfileSettings_UI.Powder_Density.Text);
                //writer.WriteLine("Sintered Density:" + Main.main.threedview.ui.ProfileSettings_UI.Sintered_Density.Text);
                //writer.WriteLine("Laser Sintering Twice: " + Main.main.threedview.ui.ProfileSettings_UI.SelectedProfileSettings.SinteringTwice.ToString());
                ////<timmy><9-15-2020><add base/cover in RecordFile.txt>
                //writer.WriteLine("Base Layers: " + Main.main.threedview.ui.ProfileSettings_UI.SelectedProfileSettings.baselayercyclecount.ToString());
                //writer.WriteLine("Cover Layers: " + Main.main.threedview.ui.ProfileSettings_UI.SelectedProfileSettings.coverlayer.ToString());
                ////<><><>
                //}));
                
                writer.WriteLine("Infill Scan Speed: 0" );
                writer.WriteLine("Contour Scan Speed: 0");
                writer.WriteLine("Beam Offset: 0" );
                writer.WriteLine("Fill Round Pitch: 0" );
                writer.WriteLine("Border: 0");
                writer.WriteLine("Step Angle: 0");
                writer.WriteLine("Offset Angle: 0");
                writer.WriteLine("Infill Pitch: 0");
                writer.WriteLine("Start Angle: 0");
                writer.WriteLine("Powder Density:0");
                writer.WriteLine("Sintered Density:0" );
                writer.WriteLine("Laser Sintering Twice: 0" );
                //<timmy><9-15-2020><add base/cover in RecordFile.txt>
                writer.WriteLine("Base Layers: 0" );
                writer.WriteLine("Cover Layers: 0");
                //<><><>
                


                writer.Flush();
                writer.Dispose();

                //Main.main.LogMessage("Main", "Success: Record File was Generated.", "");
                return true;
            }
            catch(Exception ex)
            {
                //Main.main.LogMessage("Main", "Failed: Generating record file.", "");
                SVGTo3WL.logFile.writer.WriteLine("3WL Data, Error encountered: GenerateRecordFile.");
                SVGTo3WL.logFile.writer.WriteLine("3WL Data exception: {0}", ex.ToString());
                SVGTo3WL.logFile.writer.Flush();
                Console.WriteLine("3WL Data, Error encountered: GenerateRecordFile");
                Console.WriteLine("3WL Data exception: {0}", ex.ToString());
                gCode.Release();
                return false;
            }

        }

        public bool? IsPowderExceed(int limit)
        {
            if (DP == null) return null;
            if (DP.sum < 0) return null;
            if (DP.sum > limit)
                return true;
            else
                return false;
        }

        public int GetPowderSum()
        {
            if (DP != null)
                return DP.sum;
            return -1;
        }

        int GetLayerAmount()
        {
            int layerAmount = 0;
            using (XmlReader xmlReader = XmlReader.Create(svgPath, settings))
            {
                while (xmlReader.Read())
                {
                    if (xmlReader.NodeType == XmlNodeType.Element && xmlReader.Name == "g")
                        layerAmount++;
                }
                xmlReader.Close();
            }
            return layerAmount;
        }
        public void CloseGcode()
        {
            gCode.Release();
        }
        class GCode
        {
            float minX = float.MaxValue, maxX = float.MinValue, minY = float.MaxValue, maxY = float.MinValue;
            EnglishStreamWriter2 writer;
            float currentZ;

            public GCode(string path)
            {
                if (File.Exists(path)) File.Delete(path);
                FileStream fs = File.Open(path, FileMode.Create);
                writer = new EnglishStreamWriter2(fs);
            }

            public void Header(float layerHeight, int layerAmount)
            {
                float printDensity = (float)PrinterSetting.PrintAreaWidth * (float)PrinterSetting.PrintAreaDepth * layerAmount * layerHeight;
                //float packingDensity = ((float)MainParameter.gCodeVolume / printDensity) * 100f;
                float packingDensity = (float)MainParameter.PackingDensity;

                //List<model.PrintModel> models = Main.main.objectPlacement.models;
                writer.WriteLine("3DPSNKG13WTW0000");
                writer.WriteLine("; machine: SLS 1.0");
                writer.WriteLine("; ----------Summary Details---------- ");
                writer.WriteLine("; layer height: " + layerHeight.ToString());
                writer.WriteLine("; filled: no");
                writer.WriteLine("; volume: " + MainParameter.gCodeVolume.ToString("#,0.0000"));
                writer.WriteLine("; model width: 1");
                writer.WriteLine("; model length: 1");
                writer.WriteLine("; model height: {0}",MainParameter.selLayerHeight*MainParameter.layercount);
                writer.WriteLine("; layer count: " + layerAmount.ToString());
                writer.WriteLine("; parts count: " + MainParameter.ModelCount.ToString());
                writer.WriteLine("; packing density %: " + packingDensity.ToString("0.00"));
                writer.WriteLine("; non-sintered powder: " + (printDensity - MainParameter.gCodeVolume).ToString("0.00") + "\n\n");
                //if (MainParameter.ModelCount > 1)
                //{
                //    writer.WriteLine("; ----------Parts Details---------- ");
                //    for (int i = 0; i < models.Count; i++)
                //    {
                //        model.PrintModel mod = models[i];
                //        //<Carson(Taipei)><07-02-2019><Modified - Fixed non-ascii encryption error>
                //        //writer.WriteLine("; part" + "[" + (i + 1) + "]" + ": " + mod);
                //        writer.WriteLine("; part" + "[" + (i + 1) + "]" + ": ");
                //        //<><><>
                //        writer.WriteLine("; volume: " + mod.ActiveModel.Volume().ToString("0.00"));
                //        writer.WriteLine("; part width: " + (mod.xMax - mod.xMin).ToString("0.00"));
                //        writer.WriteLine("; part length: " + (mod.yMax - mod.yMin).ToString("0.00"));
                //        writer.WriteLine("; part height: " + (mod.zMax - mod.zMin).ToString("0.00"));
                //        writer.WriteLine("; layer count: " + Math.Floor((mod.zMax - mod.zMin) / layerHeight).ToString("0") + "\n");
                //    }
                //    writer.WriteLine();
                //}
                writer.WriteLine("M601 ; Laser Off"); ;
            }

            public void StartLayer(float z)
            {
                writer.WriteLine(";<LAYER>");
                currentZ = z;
            }

            public void EndLayer()
            {
                if (PrinterSetting.UseSlic3r == 1) writer.WriteLine("G1 L" + (currentZ * 1000000));
                else writer.WriteLine("G1 L" + currentZ);
            }

            public void AddPolygon(float[] ps)
            {
                writer.WriteLine(";<POLYGON>");
                writer.WriteLine("G1 X" + ps[0] + " Y" + ps[1]);
                writer.WriteLine("M600 ; Laser On");
                for (int i = 2; i < ps.Length; i += 2)
                {
                    writer.WriteLine("G1 X" + ps[i] + " Y" + ps[i + 1]);
                    minX = Math.Min(ps[i], minX);
                    maxX = Math.Max(ps[i], maxX);
                    minY = Math.Min(ps[i + 1], minY);
                    maxY = Math.Max(ps[i + 1], maxY);
                }
                writer.WriteLine("G1 X" + ps[0] + " Y" + ps[1]);
                writer.WriteLine("M601 ; Laser Off");
                writer.WriteLine(";<POLYGON>");
            }

            public void EndAndRelease()
            {
                writer.WriteLine(string.Format("\n; transpoints: {0},{1},{2},{3}", minX, minY, maxX, maxY));
                Release();
            }

            public void Release()
            {
                writer.Flush();
                writer.Dispose();
            }
        }

        class DynamicPowder
        {
            public int average = -1;
            public int sum = -1;
            List<int> layerPowderAmounts = new List<int>();
            int minPowder = 120;
            int midPowder = 140;
            int maxPowder = 180;
            double maxThreshold = 0.4;
            int scale = 4;

            int width;
            int depth;
            int[] layerRows;
            byte[] imgData;
            List<Polygon> polygons;

            public int errorstatus = 0;

            public DynamicPowder(int width, int depth)
            {
                this.width = width * scale;
                this.depth = depth * scale;
                polygons = new List<Polygon>();
            }

            public void AddPolygon(float[] ps)
            {
                errorstatus = 10000;//status detect, can remove.
                Point[] points = new Point[ps.Length / 2];
                errorstatus = 10001;//status detect, can remove.
                for (int i = 0; i < ps.Length; i += 2)
                {
                    errorstatus = 10011;//status detect, can remove.
                    points[i / 2].X = Convert.ToInt32(ps[i] * scale, new CultureInfo("en-US")); ;
                    points[i / 2].Y = Convert.ToInt32(ps[i + 1] * scale, new CultureInfo("en-US")); ;

                    errorstatus = 10021;//status detect, can remove.
                }
                errorstatus = 10031;//status detect, can remove.
                int c = Convert.ToInt32(CvInvoke.ContourArea(new VectorOfPoint(points), true), new CultureInfo("en-US"));
                polygons.Add(new Polygon(points, c, polygons.Count));
                errorstatus = 10041;//status detect, can remove.
            }

            public void ComputePowder()
            {
                errorstatus = 20000;//status detect, can remove.
                if (polygons.Count == 0)
                {
                    layerPowderAmounts.Add(minPowder * (int)(MainParameter.selLayerHeight / 0.1));
                    return;
                }
                errorstatus = 20001;//status detect, can remove.
                using (Mat img = new Mat(width, depth, 0, 1))
                {
                    errorstatus = 20002;//status detect, can remove.
                    using (VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint())
                    {
                        errorstatus = 20003;//status detect, can remove.
                        foreach (Polygon p in polygons)
                        {
                            contours.Push(new VectorOfPoint(p.points));
                        }
                        errorstatus = 20004;//status detect, can remove.
                        polygons.Sort((a, b) => b.area.CompareTo(a.area));
                        errorstatus = 20005;//status detect, can remove.
                        img.SetTo(new MCvScalar(0));
                        for (int i = 0; i < contours.Size; i++)
                        {
                            CvInvoke.DrawContours(img, contours, polygons[i].index, new MCvScalar(polygons[i].mask), -1);
                        }
                        errorstatus = 20006;//status detect, can remove.           
                        if (img.Cols > img.Rows) layerRows = new int[img.Cols];
                        else layerRows = new int[img.Rows];
                        errorstatus = 20007;//status detect, can remove.
                        
                        imgData = new byte[img.Cols * img.Rows];
                        errorstatus = 20008;//status detect, can remove.
                        Marshal.Copy(img.DataPointer, imgData, 0, img.Cols * img.Rows);
                        for (int i = 0; i < imgData.Length; i++)
                        {
                            //[Timothy 2020/08/14] Bug found, crash when img.Rows > img.Cols
                            if (imgData[i] > 0)
                            {
                                layerRows[i / depth] += (int)(imgData[i]);
                            }
                        }
                        errorstatus = 20009;//status detect, can remove.
                        int maxConut = layerRows.Max(m => m);
                        errorstatus = 20010;//status detect, can remove.
                        int layerPowderAmount = 0;
                        if (maxConut < depth * maxThreshold || maxThreshold >= 1)
                        {
                            layerPowderAmount = (int)Math.Ceiling(minPowder + (midPowder - minPowder) * ((double)maxConut / depth / maxThreshold));
                        }
                        else
                        {
                            layerPowderAmount = (int)Math.Ceiling(minPowder + (maxPowder - minPowder) * (double)maxConut / depth);
                        }
                        errorstatus = 20011;//status detect, can remove.
                        layerPowderAmount = (int)((layerPowderAmount * MainParameter.selLayerHeight) / 0.1);
                        layerPowderAmounts.Add(layerPowderAmount);
                        errorstatus = 20012;//status detect, can remove.
                        polygons.Clear();
                        Array.Clear(layerRows, 0, layerRows.Length);
                        errorstatus = 20013;//status detect, can remove.
                        Array.Clear(imgData, 0, imgData.Length);
                        errorstatus = 20014;//status detect, can remove.
                    }
                }
            }

            public void Output(string path)
            {
                average = (int)Math.Ceiling(layerPowderAmounts.Average(a => a));
                sum = layerPowderAmounts.Sum(b => b);
                FileStream fs = File.Open(path, FileMode.Create);
                using (EnglishStreamWriter2 writer = new EnglishStreamWriter2(fs))
                {
                    writer.WriteLine(String.Format("Sum-{0}", sum));
                    for (int i = 0; i < layerPowderAmounts.Count; i++)
                    {
                        writer.WriteLine(String.Format("Layer{0}-{1}", i, layerPowderAmounts[i]));
                    }
                    writer.Flush();
                } 
            }

            class Polygon
            {
                public Point[] points;
                public int area = 0;
                public int index = -1;
                public int mask;
                public Polygon(Point[] ps, int a, int i)
                {
                    points = ps;
                    area = Math.Abs(a);
                    index = i;
                    mask = a > 0 ? 1 : 0;
                }
            }
        }

        class DXF
        {
            float widthOffset;
            float depthOffset;
            EnglishStreamWriter2 writer;
            string dxfPath;
            string dxfFilePath;
            bool encrypt;

            public DXF(float width, float depth, string dxfPath, bool encrypt = true)
            {
                widthOffset = width / 2;
                depthOffset = depth / 2;
                this.dxfPath = dxfPath;
                this.encrypt = encrypt;
            }

            public void CreateNew(int index)
            {
                dxfFilePath = dxfPath + string.Format("\\layer{0}.dxf", index);
                FileStream fs = File.Open(dxfFilePath, FileMode.Create);
                writer = new EnglishStreamWriter2(fs);
                writer.WriteLine("  0\nSECTION\n  2\nENTITIES");
            }

            public void AddPolygon(float[] points)
            {
                writer.WriteLine("  0\nLINE\n8\nL1");
                writer.WriteLine(" 10");
                writer.WriteLine((points[0] - widthOffset).ToString("0.0000"));
                writer.WriteLine(" 20");
                writer.WriteLine(points[1] - depthOffset);
                writer.WriteLine(" 30");
                writer.WriteLine("0.0000");
                for (int i = 2; i < points.Length - 1; i += 2)
                {
                    float X = points[i] - widthOffset;
                    float Y = points[i + 1] - depthOffset;
                    writer.WriteLine(" 11");
                    writer.WriteLine(X.ToString());
                    writer.WriteLine(" 21");
                    writer.WriteLine(Y.ToString());
                    writer.WriteLine(" 31");
                    writer.WriteLine("0.0000");
                    writer.WriteLine("  0\nLINE\n8\nL1");
                    writer.WriteLine(" 10");
                    writer.WriteLine(X.ToString());
                    writer.WriteLine(" 20");
                    writer.WriteLine(Y.ToString());
                    writer.WriteLine(" 30");
                    writer.WriteLine("0.0000");
                }
                writer.WriteLine(" 11");
                writer.WriteLine(points[0] - widthOffset);
                writer.WriteLine(" 21");
                writer.WriteLine(points[1] - depthOffset);
                writer.WriteLine(" 31");
                writer.WriteLine("0.0000");
            }

            public void EndSection()
            {
                writer.WriteLine("  0\nENDSEC\n  0\nEOF");
                writer.Flush();
                writer.Close();


                
                //if (MainParameter.generating_dxf)
                //{
                    //Main.main.Add_logo_time = new System.Diagnostics.Stopwatch();
                    //Main.main.Add_logo_time.Start();
                    //Main.main.LoadDXF(true, dxfFilePath);
                //}
                //Calculate the DXF File Size in KB
                double size = new System.IO.FileInfo(dxfFilePath).Length;
                MainParameter.TotalDXFFileSize += Math.Round(size / 1024.0, 2);
                //<><><>
                
                if (encrypt)
                    Encrypt();
            }

            public void Release()
            {
                writer.Flush();
                writer.Dispose();
            }

            void Encrypt()
            {
                StringBuilder builder = XYZLib.XYZ.Encoder.MonoEncrypt(dxfFilePath);
                File.Delete(dxfFilePath);
                using (FileStream fs = new FileStream(dxfFilePath, FileMode.Create))
                {
                    fs.Write(Encoding.ASCII.GetBytes(builder.ToString()), 0, builder.Length);
                }              
            }
        }
    }

}
