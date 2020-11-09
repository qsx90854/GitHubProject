using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using Ionic.Zip;
namespace XYZSDK
{
    class MessageToLog
    {
        string logPath = "";
        public StreamWriter writer;
        public void init(string path)
        {
            logPath = path;
            if (File.Exists(logPath)) File.Delete(logPath);
            writer = new StreamWriter(logPath);
        }
        public void release()
        {
            writer.Flush();
            writer.Close();
        }
    }
    class SVGTo3WL
    {
        public string StlFilePath = "C:\\Users\\tb395014\\AppData\\Roaming\\XYZware_SLS_v1.0\\6.24.20\\composition\\composition.stl";
        public string dxfPath = "C:\\Users\\tb395014\\AppData\\Roaming\\XYZware_SLS_v1.0\\6.24.20\\composition";
        private string exportFilename = null;
        public FileInfo fileIn;
        public string curPath;


        public int powderLimit = 330000;
        public int powderSum = 0;

        Costing CostFile = new Costing();
        public static MessageToLog logFile;

        //SVGTo3WL()
        //{

        //}
        public static int ParserZipPath()
        {
            int parseNumber = 0;
            if (MainParameter.inputString.IndexOf("-machine_profile") != -1)
            {
                int index = MainParameter.inputString.IndexOf("-machine_profile");
                MainParameter.input_profilePath = MainParameter.inputString[index + 1];
                parseNumber++;
            }
            if (MainParameter.inputString.IndexOf("-shrinkage") != -1)
            {
                int index = MainParameter.inputString.IndexOf("-shrinkage");
                MainParameter.output_shrinkagePath = MainParameter.inputString[index + 1];
                parseNumber++;
            }
            if (parseNumber == 2)
            {
                return 0;
            }
            else
            {
                Console.WriteLine("Error Code :{0}", 1000);
                return 1000;
            }

        }
        public static int ParserPath()
        {
            int parseNumber = 0;
            if(MainParameter.inputString.IndexOf("-svg_file") != -1)
            {
                int index = MainParameter.inputString.IndexOf("-svg_file");
                MainParameter.input_svgPath = MainParameter.inputString[index + 1];
                parseNumber++;
            }
            if (MainParameter.inputString.IndexOf("-printing_parameters") != -1)
            {
                int index = MainParameter.inputString.IndexOf("-printing_parameters");
                MainParameter.input_parPath = MainParameter.inputString[index + 1];
                parseNumber++;
            }
            if (MainParameter.inputString.IndexOf("-preview_images") != -1)
            {
                int index = MainParameter.inputString.IndexOf("-preview_images");
                MainParameter.input_imgPath = MainParameter.inputString[index + 1];
                parseNumber++;
            }
            if (MainParameter.inputString.IndexOf("-3wl_file") != -1)
            {
                int index = MainParameter.inputString.IndexOf("-3wl_file");
                MainParameter.export3wlFileName = MainParameter.inputString[index + 1];
                parseNumber++;
            }
            if(parseNumber == 4)
            {
                return 0;
            }
            else
            {
                Console.WriteLine("Error Code :{0}", 1000);
                logFile.writer.WriteLine("ParserPath fail, Error Code :{0}", 1000);
                logFile.writer.Flush();
                logFile.writer.Close();
                return 1000;
            }
            
        }
        public static int CheckParameterFile(string path)
        {
            string str = null;
            try { str = File.ReadAllText(path); }
            catch (Exception ex)
            {
                Console.WriteLine("Parameter file path error.");
                Console.WriteLine("Error Code :{0}", 1301);
                logFile.writer.WriteLine("Parameter file path error.");
                logFile.writer.WriteLine("Error Code :{0}", 1301);
                logFile.writer.Flush();
                logFile.writer.Close();
                return 1301;
            }

            if (!str.Contains("PartLayerHeight")) { Console.WriteLine("Error Code :{0}", 2301); return 2301; }
            if (!str.Contains("TotalParts")) { Console.WriteLine("Error Code :{0}", 2301); return 2301; }
            if (!str.Contains("TotalVolume")) { Console.WriteLine("Error Code :{0}", 2301); return 2301; }
            if (!str.Contains("TotalCount")) { Console.WriteLine("Error Code :{0}", 2301); return 2301; }
            if (!str.Contains("ShrinkageXAxis")) { Console.WriteLine("Error Code :{0}", 2301); return 2301; }
            if (!str.Contains("ShrinkageYAxis")) { Console.WriteLine("Error Code :{0}", 2301); return 2301; }
            if (!str.Contains("PackingDensity")) { Console.WriteLine("Error Code :{0}", 2301); return 2301; }
            return 0;
        }
        public int LoadParameterFile(string path)
        {
            string[] str = null;
            try { str = File.ReadAllLines(path); }
            catch (Exception ex) { Console.WriteLine("Error Code :{0}", 1301); return 1301; }

            try
            {
                for (int i = 0; i < str.Length; ++i)
                {
                    string s = str[i];
                    if (string.IsNullOrEmpty(s)) continue;
                    string[] keyVal = s.Split(':');
                    if (keyVal.Length < 2) continue;
                    switch (keyVal[0])
                    {
                        case "PartLayerHeight": double.TryParse(keyVal[1], NumberStyles.Any, new CultureInfo("en-US"), out MainParameter.selLayerHeight); break;
                        case "TotalParts": int.TryParse(keyVal[1], NumberStyles.Any, new CultureInfo("en-US"), out MainParameter.totalParts); break;
                        case "TotalVolume": double.TryParse(keyVal[1], NumberStyles.Any, new CultureInfo("en-US"), out MainParameter.gCodeVolume); break;
                        case "TotalCount": int.TryParse(keyVal[1], NumberStyles.Any, new CultureInfo("en-US"), out MainParameter.layercount); break;
                        case "ShrinkageXAxis": double.TryParse(keyVal[1], NumberStyles.Any, new CultureInfo("en-US"), out MainParameter.ShrinkageXAxis); break;
                        case "ShrinkageYAxis": double.TryParse(keyVal[1], NumberStyles.Any, new CultureInfo("en-US"), out MainParameter.ShrinkageYAxis); break;
                        case "PackingDensity": double.TryParse(keyVal[1], NumberStyles.Any, new CultureInfo("en-US"), out MainParameter.PackingDensity); break;
                        default: break;
                    }
                }
                #region ConfirmParameter
                logFile.writer.WriteLine("selLayerHeight :{0} mm.", MainParameter.selLayerHeight);
                logFile.writer.WriteLine("totalParts :{0}.", MainParameter.totalParts);
                logFile.writer.WriteLine("gCodeVolume :{0} mm^3.", MainParameter.gCodeVolume);
                logFile.writer.WriteLine("layercount :{0}.", MainParameter.layercount);
                logFile.writer.WriteLine("ShrinkageXAxis :{0}.", MainParameter.ShrinkageXAxis);
                logFile.writer.WriteLine("ShrinkageYAxis :{0}.", MainParameter.ShrinkageYAxis);
                logFile.writer.WriteLine("PackingDensity :{0} %.", MainParameter.PackingDensity);
                if (!MainParameter.selLayerHeight.Equals(0.08) && !MainParameter.selLayerHeight.Equals(0.1) && !MainParameter.selLayerHeight.Equals(0.15) && !MainParameter.selLayerHeight.Equals(0.2))
                {
                    //2301
                    Console.WriteLine("selLayerHeight error,  iuput :{0}, input parameter should be (0.08/0.1/0.15/0.2) mm.", MainParameter.selLayerHeight);
                    Console.WriteLine("Error Code :{0}", 2301);
                    logFile.writer.WriteLine("selLayerHeight error,  iuput :{0}, input parameter should be (0.08/0.1/0.15/0.2) mm.", MainParameter.selLayerHeight);
                    logFile.writer.WriteLine("Error Code :{0}", 2301);
                    logFile.writer.Flush();
                    logFile.writer.Close();
                    return 2301;
                }
                if (MainParameter.layercount <= 0)
                {
                    Console.WriteLine("layercount error,  iuput :{0}, the input parameter should be over zero.", MainParameter.layercount);
                    Console.WriteLine("Error Code :{0}", 2301);
                    logFile.writer.WriteLine("layercount error,  iuput :{0}, the input parameter should be over zero.", MainParameter.layercount);
                    logFile.writer.WriteLine("Error Code :{0}", 2301);
                    logFile.writer.Flush();
                    logFile.writer.Close();
                    return 2301;
                }
                if ((MainParameter.selLayerHeight * (double)MainParameter.layercount) > MainParameter.heightLimit)
                {
                    Console.WriteLine("build height error,  selLayerHeight x  layercount :{0}, the input parameter should be over zero and lower than 230 mm.", (MainParameter.selLayerHeight * (double)MainParameter.layercount));
                    Console.WriteLine("Error Code :{0}", 2301);
                    logFile.writer.WriteLine("build height error,  selLayerHeight x  layercount :{0}, the input parameter should be over zero and lower than 230 mm.", (MainParameter.selLayerHeight * (double)MainParameter.layercount));
                    logFile.writer.WriteLine("Error Code :{0}", 2301);
                    logFile.writer.Flush();
                    logFile.writer.Close();
                    return 2301;
                }
                if (MainParameter.gCodeVolume <= 0.0)
                {
                    Console.WriteLine("gCodeVolume error,  iuput :{0}, the input parameter should be over zero.", MainParameter.gCodeVolume);
                    Console.WriteLine("Error Code :{0}", 2301);
                    logFile.writer.WriteLine("gCodeVolume error,  iuput :{0}, the input parameter should be over zero.", MainParameter.gCodeVolume);
                    logFile.writer.WriteLine("Error Code :{0}", 2301);
                    logFile.writer.Flush();
                    logFile.writer.Close();
                    return 2301;
                }
                if (MainParameter.totalParts <= 0)
                {
                    Console.WriteLine("totalParts error,  iuput :{0}, the input parameter should be over zero.", MainParameter.totalParts);
                    Console.WriteLine("Error Code :{0}", 2301);
                    logFile.writer.WriteLine("totalParts error,  iuput :{0}, the input parameter should be over zero.", MainParameter.totalParts);
                    logFile.writer.WriteLine("Error Code :{0}", 2301);
                    logFile.writer.Flush();
                    logFile.writer.Close();
                    return 2301;
                }
                if (MainParameter.PackingDensity <= 0 || MainParameter.PackingDensity > 100.0)
                {
                    Console.WriteLine("PackingDensity error,  iuput :{0}, the input parameter range should be 0~100(%).", MainParameter.PackingDensity);
                    Console.WriteLine("Error Code :{0}", 2301);
                    logFile.writer.WriteLine("PackingDensity error,  iuput :{0}, the input parameter range should be 0~100(%).", MainParameter.PackingDensity);
                    logFile.writer.WriteLine("Error Code :{0}", 2301);
                    logFile.writer.Flush();
                    logFile.writer.Close();
                    return 2301;
                }
                #endregion
            }
            catch (Exception ex)
            {
                Console.WriteLine("LoadParameterFile Exception error:{0}.", ex.ToString());
                Console.WriteLine("Error Code :{0}", -1);
                logFile.writer.WriteLine("LoadParameterFile Exception error :{0}.", ex.ToString());
                logFile.writer.WriteLine("Error Code :{0}", -1);
                logFile.writer.Flush();
                logFile.writer.Close();
                return -1;
            }
            return 0;
        
        }
        public int GenerateShrinkeFile(List<List<string>> str)
        {
            //string headerFile = Application.CommonAppDataPath + "\\composition\\Header.txt";
            string headerFile = MainParameter.output_shrinkagePath;

            string tmp_path = Path.GetDirectoryName(headerFile);
            Console.WriteLine("output folder:{0}.", tmp_path);
            if (!Directory.Exists(tmp_path))
            {
                Console.WriteLine("output folder does not exist.");
                Console.WriteLine("Error Code :{0}", 1102);
                logFile.writer.WriteLine("output folder does not exist.");
                logFile.writer.WriteLine("Error Code :{0}", 1102);
                logFile.writer.Flush();
                logFile.writer.Close();
                return 1102;
            }


            StreamWriter writer = new StreamWriter(headerFile);
            try
            {
                if (str.Count != 4)
                {
                    Console.WriteLine("GenerateShrinkeFile unknow error, str.Count != 4.");
                    return -1;
                } 
                for (int i = 0; i < str[0].Count; i++)
                {
                    writer.WriteLine("MaterialName:{0}", str[0][i]);
                    writer.WriteLine("x:{0}", str[1][i]);
                    writer.WriteLine("y:{0}", str[2][i]);
                    writer.WriteLine("z:{0}", str[3][i]);
                }
                writer.Flush();
                writer.Close();
                return 0;
            }
            catch
            {
                Console.WriteLine("GenerateShrinkeFile unknow error.");
                return -1;
            }
        }
        public int ExtractShrinkage()
        {
            Console.WriteLine("ExtractShrinkage");
            try
            {
                int IsLegitFile = 0;
                ParserZipPath();
                Console.WriteLine("End ParserZipPath.");
                string ExtractPath = MainParameter.AppdataPath + MainParameter.AssemblyFileVersion + "composition\\";
                if(!File.Exists(MainParameter.input_profilePath))
                {
                    Console.WriteLine("Input profile path error.");
                    Console.WriteLine("Error Code :{0}", 1101);
                    logFile.writer.WriteLine("Input profile path error.");
                    logFile.writer.WriteLine("Error Code :{0}", 1101);
                    logFile.writer.Flush();
                    logFile.writer.Close();
                    return 1101;
                }

                #region CreateTempFolder
                if (!Directory.Exists(MainParameter.AppdataPath))
                {
                    Directory.CreateDirectory(MainParameter.AppdataPath);
                    if (!Directory.Exists(MainParameter.AppdataPath))
                    {
                        Console.WriteLine("Create temp folder fail, temp folder :{0}", MainParameter.AppdataPath);
                        Console.WriteLine("Please confirm system permissions.");
                        logFile.writer.WriteLine("Create temp folder fail, temp folder :{0}", MainParameter.AppdataPath);
                        logFile.writer.WriteLine("Please confirm system permissions.");
                        logFile.writer.Flush();
                        logFile.writer.Close();
                        return -1;
                    }
                }
                if (!Directory.Exists(MainParameter.AppdataPath + MainParameter.AssemblyFileVersion))
                {
                    Directory.CreateDirectory(MainParameter.AppdataPath + MainParameter.AssemblyFileVersion);
                    if (!Directory.Exists(MainParameter.AppdataPath + MainParameter.AssemblyFileVersion))
                    {
                        Console.WriteLine("Create temp folder fail, temp folder :{0}", MainParameter.AppdataPath + MainParameter.AssemblyFileVersion);
                        Console.WriteLine("Please confirm system permissions.");
                        logFile.writer.WriteLine("Create temp folder fail, temp folder :{0}", MainParameter.AppdataPath + MainParameter.AssemblyFileVersion);
                        logFile.writer.WriteLine("Please confirm system permissions.");
                        logFile.writer.Flush();
                        logFile.writer.Close();
                        return -1;
                    }
                }
                if (!Directory.Exists(MainParameter.AppdataPath + MainParameter.AssemblyFileVersion + "composition"))
                {
                    Directory.CreateDirectory(MainParameter.AppdataPath + MainParameter.AssemblyFileVersion + "composition");
                    if (!Directory.Exists(MainParameter.AppdataPath + MainParameter.AssemblyFileVersion + "composition"))
                    {
                        Console.WriteLine("Create temp folder fail, temp folder :{0}", MainParameter.AppdataPath + MainParameter.AssemblyFileVersion + "composition");
                        Console.WriteLine("Please confirm system permissions.");
                        logFile.writer.WriteLine("Create temp folder fail, temp folder :{0}", MainParameter.AppdataPath + MainParameter.AssemblyFileVersion + "composition");
                        logFile.writer.WriteLine("Please confirm system permissions.");
                        logFile.writer.Flush();
                        logFile.writer.Close();
                        return -1;
                    }
                }
                #endregion
                #region CreateLogFile
                logFile = new MessageToLog();
                logFile.init(MainParameter.AppdataPath + MainParameter.logFileName);
                logFile.writer.WriteLine(MainParameter.XYZSDKInterVersionNumber);
                #endregion

                using (var archive = Ionic.Zip.ZipFile.Read(MainParameter.input_profilePath))
                {
                    foreach (var entry in archive.Entries)
                    {
                        if (entry.FileName.Contains("MaterialList.csv") || entry.FileName.Contains("SettingProfiles.csv") || entry.FileName.Contains("config.ini")
                            || entry.FileName.Contains("ETFactor.txt") || entry.FileName.Contains("ObjDefalutValue.ini"))
                        {
                            IsLegitFile++;
                            //printer = entry.FileName.Split('/');
                        }
                    }
                    if (IsLegitFile != 5)
                    {
                        //var result = System.Windows.Forms.MessageBox.Show(" File does not have a complete Print Profiles.\n Please import correct Print Profiles", "ERROR!", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
                        //if (result == System.Windows.Forms.DialogResult.OK && Main.main.threedview.ui.ProfileSettings_UI.cmb_Printer.Items.Count != 0)
                        {
                            //printer = null;
                            Console.WriteLine("File does not have a complete Print Profiles.\n Please import correct Print Profiles");
                            Console.WriteLine("Error Code :{0}", 3101);
                            logFile.writer.WriteLine("File does not have a complete Print Profiles.\n Please import correct Print Profiles");
                            logFile.writer.WriteLine("Error Code :{0}", 3101);
                            logFile.writer.Flush();
                            logFile.writer.Close();
                            return 3101;
                        }
                    }
                }
                //Console.WriteLine("MainParameter.input_profilePath {0}", MainParameter.input_profilePath);
                using (Ionic.Zip.ZipFile zip = Ionic.Zip.ZipFile.Read(MainParameter.input_profilePath))
                {
                    zip.Password = MainParameter.zipPassword;
                    zip.ExtractAll(ExtractPath, Ionic.Zip.ExtractExistingFileAction.OverwriteSilently);
                    //Console.WriteLine("zip Name {0}", zip.Name);

                    //Main.main.threedview.ui.ProfileSettings_UI.InitializePrinterOnCMB();
                    //Console.WriteLine("ExtractPath {0}", ExtractPath);
                    DirectoryInfo di = new DirectoryInfo(ExtractPath + "MP230xS-96D0012");
                    List<List<string>> outPutShrinkage = new List<List<string>>();
                    foreach (var fi in di.GetFiles("MaterialList.csv"))
                    {
                        //Console.WriteLine("Load MaterialList.csv {0}", fi.FullName);
                        System.IO.StreamReader sr1 = new System.IO.StreamReader(fi.FullName);
                        char[] split_char1 = { ',' };
                        string[] para_title = sr1.ReadLine().Split(split_char1);

                        int MaterialNameIndex = -1;
                        int WidthIndex = -1;
                        int DepthIndex = -1;
                        int HeightIndex = -1;
                        int index = 0;
                        
                        foreach (string s in para_title)
                        {
                           // Console.WriteLine(s);
                            if (s == "MaterialName") { MaterialNameIndex = index; outPutShrinkage.Add(new List<string>()); }
                            if (s == "Width") { WidthIndex = index; outPutShrinkage.Add(new List<string>()); }
                            if (s == "Depth") { DepthIndex = index; outPutShrinkage.Add(new List<string>()); }
                            if (s == "Height") { HeightIndex = index; outPutShrinkage.Add(new List<string>()); }
                            index++;
                        }
                        if (MaterialNameIndex == -1 || WidthIndex == -1 || DepthIndex == -1 || HeightIndex == -1)
                        {
                            Console.WriteLine("Profile extract shrinkage error.");
                            Console.WriteLine("Error Code :{0}", 2101);
                            logFile.writer.WriteLine("Profile extract shrinkage error.");
                            logFile.writer.WriteLine("Error Code :{0}", 2101);
                            logFile.writer.Flush();
                            logFile.writer.Close();
                            return 2101;
                        }

                        while (sr1.Peek() > 0)
                        {
                            string[] para_arr = sr1.ReadLine().Split(split_char1);
                            outPutShrinkage[0].Add(para_arr[MaterialNameIndex]);
                            outPutShrinkage[1].Add(para_arr[WidthIndex]);
                            outPutShrinkage[2].Add(para_arr[DepthIndex]);
                            outPutShrinkage[3].Add(para_arr[HeightIndex]);
                        }
                        int result = GenerateShrinkeFile(outPutShrinkage);
                        if (result!=0)
                        {
                            Console.WriteLine("Generate ShrinkeFile error.");
                            logFile.writer.WriteLine("Generate ExtractShrinkage error.");
                            logFile.writer.Flush();
                            logFile.writer.Close();
                            sr1.Close();
                            return result;
                        }
                        sr1.Close();
                    }
                    
                }
            }
            catch(Exception e)
            {
                Console.WriteLine("Unknow ExtractShrinkage error.");
                logFile.writer.WriteLine("Unknow ExtractShrinkage error.");
                logFile.writer.Flush();
                logFile.writer.Close();
                return -1;
            }

            logFile.writer.WriteLine("End ExtractShrinkage.");
            logFile.writer.Flush();
            logFile.writer.Close();
            return 0;
        }
        public int SVGto3WLFile()
        {
            Console.WriteLine("SVGto3WLFile");
            int result = 0;
            MainParameter.fourWayImage.Clear();

            string processName = MainParameter.SelectedSlicerPath;
            string svgPath = "";// MainParameter.input_svgPath;
            string gcodePath = "";// Path.ChangeExtension(StlFilePath, ".gcode");
            string powderPath = "";// dxfPath + "\\Powder.txt";

            StlFilePath = MainParameter.AppdataPath + MainParameter.AssemblyFileVersion + "composition\\composition.stl";
            dxfPath = MainParameter.AppdataPath + MainParameter.AssemblyFileVersion + "composition";
            gcodePath = Path.ChangeExtension(StlFilePath, ".gcode");
            powderPath = dxfPath + "\\Powder.txt";
            //Console.WriteLine("StlFilePath {0}", StlFilePath);
            //Console.WriteLine("gcodePath {0}", gcodePath);
            //Console.WriteLine("powderPath {0}", powderPath);
            try
            {
                #region CreateTempFolder
                if (!Directory.Exists(MainParameter.AppdataPath))
                {
                    Directory.CreateDirectory(MainParameter.AppdataPath);
                    if (!Directory.Exists(MainParameter.AppdataPath))
                    {
                        Console.WriteLine("Create temp folder fail, temp folder :{0}", MainParameter.AppdataPath);
                        Console.WriteLine("Please confirm system permissions.");
                        logFile.writer.WriteLine("Create temp folder fail, temp folder :{0}", MainParameter.AppdataPath);
                        logFile.writer.WriteLine("Please confirm system permissions.");
                        logFile.writer.Flush();
                        logFile.writer.Close();
                        return -1;
                    }
                }
                if (!Directory.Exists(MainParameter.AppdataPath + MainParameter.AssemblyFileVersion))
                {
                    Directory.CreateDirectory(MainParameter.AppdataPath + MainParameter.AssemblyFileVersion);
                    if (!Directory.Exists(MainParameter.AppdataPath + MainParameter.AssemblyFileVersion))
                    {
                        Console.WriteLine("Create temp folder fail, temp folder :{0}", MainParameter.AppdataPath + MainParameter.AssemblyFileVersion);
                        Console.WriteLine("Please confirm system permissions.");
                        logFile.writer.WriteLine("Create temp folder fail, temp folder :{0}", MainParameter.AppdataPath + MainParameter.AssemblyFileVersion);
                        logFile.writer.WriteLine("Please confirm system permissions.");
                        logFile.writer.Flush();
                        logFile.writer.Close();
                        return -1;
                    }
                }
                if (!Directory.Exists(MainParameter.AppdataPath + MainParameter.AssemblyFileVersion + "composition"))
                {
                    Directory.CreateDirectory(MainParameter.AppdataPath + MainParameter.AssemblyFileVersion + "composition");
                    if (!Directory.Exists(MainParameter.AppdataPath + MainParameter.AssemblyFileVersion + "composition"))
                    {
                        Console.WriteLine("Create temp folder fail, temp folder :{0}", MainParameter.AppdataPath + MainParameter.AssemblyFileVersion + "composition");
                        Console.WriteLine("Please confirm system permissions.");
                        logFile.writer.WriteLine("Create temp folder fail, temp folder :{0}", MainParameter.AppdataPath + MainParameter.AssemblyFileVersion + "composition");
                        logFile.writer.WriteLine("Please confirm system permissions.");
                        logFile.writer.Flush();
                        logFile.writer.Close();
                        return -1;
                    }
                }
                #endregion
                #region CreateLogFile
                logFile = new MessageToLog();
                logFile.init(MainParameter.AppdataPath + MainParameter.logFileName);
                logFile.writer.WriteLine(MainParameter.XYZSDKInterVersionNumber);
                #endregion


                result = ParserPath();
                if (result != 0) return result;

                svgPath = MainParameter.input_svgPath;

                result = CheckParameterFile(MainParameter.input_parPath);
                if (result != 0) return result;

                result = LoadParameterFile(MainParameter.input_parPath);
                if (result != 0) return result;

                #region GetInputImage
                if (Directory.Exists(MainParameter.input_imgPath) == true)
                {
                    Console.WriteLine("Input Image Path: {0}",MainParameter.input_imgPath);
                    logFile.writer.WriteLine("Input Image Path: {0}", MainParameter.input_imgPath);
                    DirectoryInfo di = new DirectoryInfo(MainParameter.input_imgPath);
                    foreach (var fi in di.GetFiles("*Image1*"))//makesure if folder not only have Image1.png and  also have Image1.jpg, software only load once.
                    {
                        Console.WriteLine(fi.FullName);
                        MainParameter.fourWayImage.Add(System.Drawing.Image.FromFile(fi.FullName));
                        break;
                    }
                    foreach (var fi in di.GetFiles("*Image2*"))
                    {
                        Console.WriteLine(fi.FullName);
                        MainParameter.fourWayImage.Add(System.Drawing.Image.FromFile(fi.FullName));
                        break;
                    }
                    foreach (var fi in di.GetFiles("*Image3*"))
                    {
                        Console.WriteLine(fi.FullName);
                        MainParameter.fourWayImage.Add(System.Drawing.Image.FromFile(fi.FullName));
                        break;
                    }
                    foreach (var fi in di.GetFiles("*Image4*"))
                    {
                        Console.WriteLine(fi.FullName);
                        MainParameter.fourWayImage.Add(System.Drawing.Image.FromFile(fi.FullName));
                        break;
                    }
                    if (MainParameter.fourWayImage.Count() != 4)
                    {
                        Console.WriteLine("Error Code :{0}", 2401);
                        logFile.writer.WriteLine("Error Code :{0}", 2401);
                        logFile.writer.Flush();
                        logFile.writer.Close();
                        return 2401;
                    }
                }
                else
                {
                    Console.WriteLine("Error Code :{0}", 1401);
                    logFile.writer.WriteLine("Error Code :{0}", 1401);
                    logFile.writer.Flush();
                    logFile.writer.Close();
                    return 1401;
                }
                #endregion
                



                //string tmp_path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Path.DirectorySeparatorChar + "Temp";
                //if (!Directory.Exists(tmp_path)) Directory.CreateDirectory(tmp_path);
                string tmp_path = Path.GetDirectoryName(MainParameter.export3wlFileName);
                Console.WriteLine("output folder:{0}.", tmp_path);
                logFile.writer.WriteLine("output folder:{0}.", tmp_path);
                if (!Directory.Exists(tmp_path))
                {
                    Console.WriteLine("output folder does not exist.");
                    Console.WriteLine("Error Code :{0}", 1501);
                    logFile.writer.WriteLine("output folder does not exist.");
                    logFile.writer.WriteLine("Error Code :{0}", 1501);
                    logFile.writer.Flush();
                    logFile.writer.Close();
                    return 1501;
                }
                if (tmp_path != null)
                {
                    //Console.WriteLine("tmp exist, tmp path:{0}", tmp_path);
                    XYZLib.XYZ.Encoder.TempFolder = tmp_path;
                }
                else
                {
                    tmp_path = @"C:\";
                    //Console.WriteLine("tmp null, tmp path:{0}", tmp_path);
                    XYZLib.XYZ.Encoder.TempFolder = tmp_path;
                }


                if (!File.Exists(svgPath))
                {
                    Console.WriteLine("SVG file Path error : {0}.", svgPath);
                    Console.WriteLine("Error Code :{0}", 1201);
                    logFile.writer.WriteLine("SVG file Path error : {0}.", svgPath);
                    logFile.writer.WriteLine("Error Code :{0}", 1201);
                    logFile.writer.Flush();
                    logFile.writer.Close();
                    return 1201;
                }
                powderLimit = 330000;
                Console.WriteLine("3WL Data init.");
                logFile.writer.WriteLine("3WL Data init.");
                ThreeWLDataGenerator threeWLData = new ThreeWLDataGenerator(svgPath, gcodePath, powderPath, dxfPath, (float)MainParameter.selLayerHeight, false);
                if (!threeWLData.Init())
                {
                    threeWLData.CloseGcode();
                    Console.WriteLine("3WL Data init error.");
                    Console.WriteLine("Error Code :{0}", 3202);
                    logFile.writer.WriteLine("3WL Data init error.");
                    logFile.writer.WriteLine("Error Code :{0}", 3202);
                    logFile.writer.Flush();
                    logFile.writer.Close();
                    return 3202;
                }
                MainParameter.layercount = threeWLData.layerAmount;
                Console.WriteLine("Start GeneratePowderFile.");
                logFile.writer.WriteLine("Start GeneratePowderFile.");
                if (!threeWLData.GeneratePowderFile())
                {
                    Console.WriteLine("Powder file generated failed.");
                    Console.WriteLine("Error Code :{0}", 3205);
                    logFile.writer.WriteLine("Powder file generated failed.");
                    logFile.writer.WriteLine("Error Code :{0}", 3205);
                    logFile.writer.Flush();
                    logFile.writer.Close();
                    threeWLData.CloseGcode();
                    return 3205;
                }
                powderLimit = 335000; //for test
                if (threeWLData.IsPowderExceed(powderLimit) == true)
                {
                    Console.WriteLine("Consumption of powder exceeds the maximum powder capacity.\n" +
                        "Limit consumption: {0}, Current consumption: {1}\n" +
                        "Please resize the models, rearrange the models position or remove some models."
                        , powderLimit / 1000, threeWLData.GetPowderSum() / 1000);
                    Console.WriteLine("Error Code :{0}", 3210);
                    logFile.writer.WriteLine("Consumption of powder exceeds the maximum powder capacity.\n" +
                        "Limit consumption: {0}, Current consumption: {1}\n" +
                        "Please resize the models, rearrange the models position or remove some models."
                        , powderLimit / 1000, threeWLData.GetPowderSum() / 1000);
                    logFile.writer.WriteLine("Error Code :{0}", 3210);
                    logFile.writer.Flush();
                    logFile.writer.Close();
                    threeWLData.CloseGcode();
                    return 3210;
                }
                powderSum = threeWLData.GetPowderSum();
                Console.WriteLine("Start GenerateGCode.");
                logFile.writer.WriteLine("Start GenerateGCode.");
                if (!threeWLData.GenerateGCode())
                {
                    Console.WriteLine("Gcode generated failed.");
                    Console.WriteLine("Error Code :{0}", 3201);
                    logFile.writer.WriteLine("Gcode generated failed.");
                    logFile.writer.WriteLine("Error Code :{0}", 3201);
                    logFile.writer.Flush();
                    logFile.writer.Close();
                    threeWLData.CloseGcode();
                    return 3201;
                }
                MainParameter.generating_dxf = true;
                MainParameter.totalPrintingTime = 0.00;
                //if (!threeWLData.GenerateDXF())
                //{
                //    Console.WriteLine("DXF generated failed.");
                //    return 3202;
                //}
                if (!threeWLData.EditGcodeContent())
                {
                    Console.WriteLine("EditGcodeContent failed.");
                    Console.WriteLine("Error Code :{0}", 3202);
                    logFile.writer.WriteLine("EditGcodeContent failed.");
                    logFile.writer.WriteLine("Error Code :{0}", 3202);
                    logFile.writer.Flush();
                    logFile.writer.Close();
                    threeWLData.CloseGcode();
                    return 3202;
                }
                Console.WriteLine("Start GenerateHeaderFile."); 
                if (!threeWLData.GenerateHeaderFile())
                {
                    Console.WriteLine("Header generated failed.");
                    Console.WriteLine("Error Code :{0}", 3203);
                    logFile.writer.WriteLine("Header generated failed.");
                    logFile.writer.WriteLine("Error Code :{0}", 3203);
                    logFile.writer.Flush();
                    logFile.writer.Close();
                    threeWLData.CloseGcode();
                    return 3203;
                }
                Console.WriteLine("Start GenerateRecordFile.");
                if (!threeWLData.GenerateRecordFile())
                {
                    Console.WriteLine("Record generated failed.");
                    Console.WriteLine("Error Code :{0}", 3204);
                    logFile.writer.WriteLine("Record generated failed.");
                    logFile.writer.WriteLine("Error Code :{0}", 3204);
                    logFile.writer.Flush();
                    logFile.writer.Close();
                    threeWLData.CloseGcode();
                    return 3204;
                }
                if (!threeWLData.GenerateDefineFile())
                {
                    Console.WriteLine("Define generated failed.");
                    Console.WriteLine("Error Code :{0}", -1);
                    logFile.writer.WriteLine("Record generated failed.");
                    logFile.writer.WriteLine("Error Code :{0}", -1);
                    logFile.writer.Flush();
                    logFile.writer.Close();
                    threeWLData.CloseGcode();
                    return -1;
                }

                //Main.main.threedview.ui.costing.UpdateParameter();
                //Main.main.threedview.ui.costing.CalculateTotalCost();
                //Main.main.threedview.ui.costing.CalculateModelsCost();
                //CostFile.AddHistory();

                //Main.main.haveGcodeLoaded = true;
                ////Main.main.objectPlacement.RemoveAllObject(); // <Darwin> remove code which double the busy window.

                //Main.main.isLatest3wl = true;
                //Main.main.LoadDXFFiles(true);

                result = OnSliceCompleted();
                if (result != 0)
                    return result;
            }
            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
                logFile.writer.WriteLine("exception error : {0}", e.ToString());
                logFile.writer.Flush();
                logFile.writer.Close();
                return -1;
            }
            
            return 0;
        }
        private int OnSliceCompleted()
        {
            //isSlicedModelSave = false;
            //Main.main.haveGcodeLoaded = true;
            //Main.main.threedview.isSlicing = false;

            //Tonton<05-23-19> Create DxfToImage Class to convert dxf to images
            //DxfToImage conversion = new DxfToImage();
            //bool isProcFinished = false;
            //new Thread(() =>
            //{
            //    string folder = MainParameter.AppdataPath + MainParameter.AssemblyFileVersion + "\\composition";
            //    conversion.ConvertMultipleDxf(folder);
            //    isProcFinished = true;
            //}).Start();
            //while (!isProcFinished)
            //{
            //    Thread.Sleep(100);
            //}
            //end

            //BitmapImage background = new BitmapImage();
            //background.BeginInit();
            //background.UriSource = new Uri("pack://application:,,,/Resources/Assets/assets_edit_button/ic-function-bar-bgG3WL@2x.png");
            //background.EndInit();
            //Main.main.threedview.ui.left_stackPanel_Image.ImageSource = background;
            //<Carson(Taipei)><05-31-2019><Added>
            int result = Save3WL();
            if (result != 0)
                return result;


            return 0;
            //<><><>
            //<Carson(Taipei)><06-04-2019><Added>
            //Tonton<06-04-19> Append costing button and divider 
           
        }
        private string changeFileExtenstion(string filename, string new_ext)
        { // assumes that 'filename' is a file, not a directory
            string file = filename;
            int ind = filename.LastIndexOf('.');
            if (ind > 0) file = filename.Substring(0, ind);
            return file + new_ext;
        }
        public static byte[] imageToByteArray(System.Drawing.Image imageIn)
        {
            MemoryStream ms = new MemoryStream();
            imageIn.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            return ms.ToArray();
        }
        private void ZipAllImages(string costingCSV_path, string costing_path)
        {
            //<timmy><8-21-2020><updated - DEBUG: set encoding from default to UTF8 for Chinese char. in 3mf filename>
            //Ionic.Zip.ZipFile zip = new Ionic.Zip.ZipFile();
            Ionic.Zip.ZipFile zip = new Ionic.Zip.ZipFile(Encoding.UTF8);

            //foreach (string ImageFile in Main.main.List_ModelImage)
            //    zip.AddFile(ImageFile, "Costing");
            zip.AddFile(costingCSV_path, "Costing");
            zip.Save(costing_path);
        }
        static public void SaveEmptyDataFor3WL()
        {
            string costingCSV_path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "XYZware_SLS_v1.0" + Path.DirectorySeparatorChar);
            StringBuilder shortPath = new StringBuilder(255);
            MainParameter.GetShortPathName(costingCSV_path, shortPath, shortPath.Capacity);
            costingCSV_path = Path.Combine(shortPath.ToString(), "CostingHistory.csv");
            StreamWriter writer = new StreamWriter(costingCSV_path);
            //writer.WriteLine(version);
            //writer.WriteLine(title + ",volume");
            string data = "";
            double totalVolume = 0.00;
            writer.WriteLine();
            writer.Close();
        }
        int Save3WL()
        {
            string unencryted_file = changeFileExtenstion(StlFilePath, ".gcode");
            string encryted_file = changeFileExtenstion(StlFilePath, ".3wl");
            string powderFile = dxfPath + "//Powder.txt";
            string recordFile = dxfPath + "//RecordFile.txt";
            string headerFile = dxfPath + "//Header.txt";

            string tempDecodeFile = MainParameter.AppdataPath;
            string tempImageFile = tempDecodeFile;
            StringBuilder shortPath = new StringBuilder(255);
            string exportFolderPath = Path.GetDirectoryName(MainParameter.export3wlFileName);
            DirectoryInfo dirInfo = new DirectoryInfo(exportFolderPath);
            FileInfo fileInfo = new FileInfo(MainParameter.export3wlFileName);
            Console.WriteLine("export dir info full name {0}", dirInfo.FullName);
            Console.WriteLine("export file info full name {0}", fileInfo.FullName);
            try
            {
                //SaveEmptyDataFor3WL();
                if (!File.Exists(powderFile))
                {
                    //MessageBox.Show("Not found powder file");
                    Console.WriteLine("Not found powder file error");
                    logFile.writer.WriteLine("Not found powder file error");
                    logFile.writer.Flush();
                    logFile.writer.Close();
                    return -1;
                }
                exportFilename = MainParameter.export3wlFileName;

                //sfd.RestoreDirectory = true;
                fileIn = new FileInfo(exportFilename);
                curPath = fileIn.DirectoryName;

                XYZLib.XYZ.Encoder.MonoEncryptFile(encryted_file, unencryted_file);

                //string tempDecodeFile = Path.GetDirectoryName(System.Windows.Forms.Application.CommonAppDataPath);
                //string tempImageFile = Path.GetDirectoryName(System.Windows.Forms.Application.CommonAppDataPath);
            
                
                MainParameter.GetShortPathName(tempDecodeFile, shortPath, shortPath.Capacity);
                tempDecodeFile = shortPath.ToString();
                if (!tempDecodeFile.EndsWith(Path.DirectorySeparatorChar.ToString()))
                    tempDecodeFile += Path.DirectorySeparatorChar;
                if (!Directory.Exists(tempDecodeFile)) Directory.CreateDirectory(tempDecodeFile);
                tempDecodeFile += "model.3wl";

                if (!tempImageFile.EndsWith(Path.DirectorySeparatorChar.ToString()))
                    tempImageFile += Path.DirectorySeparatorChar;
                if (!Directory.Exists(tempImageFile)) Directory.CreateDirectory(tempImageFile);
                tempImageFile += "Images.3wl";

                if (File.Exists(tempDecodeFile)) File.Delete(tempDecodeFile);

                File.Copy(encryted_file, tempDecodeFile, true);

            
                BinaryWriter writer = new BinaryWriter(new FileStream(tempImageFile, FileMode.Create));
                int ImageNumber = 0;
                //foreach (int byteCount in Main.main.screenCapture.CountBytesPerImage)
                foreach(System.Drawing.Image img1 in MainParameter.fourWayImage)
                {
                    byte[] bs = imageToByteArray(img1);
                    writer.Write(bs.Count());
                }
                //foreach (byte[] bs in Main.main.screenCapture.byteList)
                foreach (System.Drawing.Image img1 in MainParameter.fourWayImage)
                {
                    byte[] bs = imageToByteArray(img1);
                    ImageNumber++;
                    byte[] bsName = Encoding.ASCII.GetBytes("Image" + ImageNumber);
                    writer.Write(bsName);
                    writer.Write(bs);
                }
                ImageNumber = 0;
                writer.Close();
            }
            catch (Exception ex)
            {
                //Main.main.LogMessage("Save 3WL", "Error encountered.");
                //Main.main.LogMessage("Save 3WL", ex.Message);
                //Main.main.threedview.BusyWindowHidden();
                Console.WriteLine("Write image.3wl Error");
                Console.WriteLine(ex.ToString());
                logFile.writer.WriteLine("Write image.3wl Error");
                logFile.writer.WriteLine("exception Error : {0}", ex.ToString());
                logFile.writer.Flush();
                logFile.writer.Close();
                return -1;
            }

            //get All image of every image and zip it <Darwin> 10-01-2019
            //string costing_path = @"C:\ProgramData\XYZprinting Inc\XYZware_SLS_v1.0\Costing.3wl";
            //string costingCSV_path = @"C:\ProgramData\XYZprinting Inc\XYZware_SLS_v1.0\CostingHistory.csv";
            string costing_path = dxfPath;// Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "XYZware_SLS_v1.0" + Path.DirectorySeparatorChar);


            shortPath = new StringBuilder(255);
            //MainParameter.GetShortPathName(costing_path, shortPath, shortPath.Capacity);
            //costing_path = shortPath.ToString();

            string costingCSV_path = costing_path;
            string FileVersionpath = costing_path;

            try
            {

                costing_path = Path.Combine(costing_path, "Costing.3wl");
                costingCSV_path = Path.Combine(costingCSV_path, "CostingHistory.csv");
                FileVersionpath = Path.Combine(FileVersionpath, "FileVersion.txt");

                System.Reflection.Assembly assm = System.Reflection.Assembly.GetEntryAssembly();
                string version = assm.GetName().Version.ToString();
                string[] splitVersions = version.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

                version = "FileVersion: " + string.Format("{0}.{1}.{2}",
                    ((splitVersions[0].Length > 2) ? splitVersions[0].Substring(0, 2) : splitVersions[0]),
                    ((splitVersions[1].Length > 2) ? splitVersions[1].Substring(0, 2) : splitVersions[1]),
                    ((splitVersions[2].Length > 2) ? splitVersions[2].Substring(0, 2) : splitVersions[2]));
                System.IO.File.WriteAllText(FileVersionpath, version);
                System.IO.File.WriteAllText(costingCSV_path, "");
                ZipAllImages(costingCSV_path, costing_path);

            
                using (Ionic.Zip.ZipFile zip = new Ionic.Zip.ZipFile())
                {
                    Console.WriteLine("tempImageFile : {0}.", tempImageFile);
                    Console.WriteLine("tempDecodeFile : {0}.", tempDecodeFile);
                    Console.WriteLine("powderFile : {0}.", powderFile);
                    Console.WriteLine("recordFile : {0}.", recordFile);
                    Console.WriteLine("headerFile : {0}.", headerFile);
                    Console.WriteLine("costing_path : {0}.", costing_path);
                    Console.WriteLine("FileVersionpath : {0}.", FileVersionpath);
                    //Console.WriteLine("MainParameter.DefineFile : {0}.", MainParameter.DefineFile);
                    //Console.WriteLine("MainParameter.export3wlFileName : {0}.", MainParameter.export3wlFileName);
                    Console.WriteLine("fileInfo.FullName : {0}.", fileInfo.FullName);
                    logFile.writer.WriteLine("Zip file.");
                    logFile.writer.WriteLine("tempImageFile : {0}.", tempImageFile);
                    logFile.writer.WriteLine("tempDecodeFile : {0}.", tempDecodeFile);
                    logFile.writer.WriteLine("powderFile : {0}.", powderFile);
                    logFile.writer.WriteLine("recordFile : {0}.", recordFile);
                    logFile.writer.WriteLine("headerFile : {0}.", headerFile);
                    logFile.writer.WriteLine("costing_path : {0}.", costing_path);
                    logFile.writer.WriteLine("FileVersionpath : {0}.", FileVersionpath);
                    logFile.writer.WriteLine("MainParameter.DefineFile : {0}.", MainParameter.DefineFile);
                    logFile.writer.WriteLine("MainParameter.export3wlFileName : {0}.", MainParameter.export3wlFileName); 
                    logFile.writer.WriteLine("fileInfo.FullName : {0}.", fileInfo.FullName);

                    zip.AddFile(tempImageFile, "ExtractedFiles");
                    zip.AddFile(tempDecodeFile, "ExtractedFiles");
                    zip.AddFile(powderFile, "ExtractedFiles");
                    zip.AddFile(recordFile, "ExtractedFiles");
                    zip.AddFile(headerFile, "ExtractedFiles");
                    zip.AddFile(costing_path, "ExtractedFiles");
                    zip.AddFile(FileVersionpath, "ExtractedFiles");
                    zip.AddFile(MainParameter.DefineFile, "ExtractedFiles");
                    zip.Save(fileInfo.FullName);
                }
            }
            catch (Exception ex)
            {
                //Main.main.LogMessage("Save 3WL", "Error encountered.");
                // Main.main.LogMessage("Save 3WL", ex.Message);
                // Main.main.threedview.BusyWindowHidden();
                Console.WriteLine("Zip File Error");
                Console.WriteLine(ex.ToString());
                logFile.writer.WriteLine("Zip File Error");
                logFile.writer.WriteLine("exception Error : {0}", ex.ToString());
                logFile.writer.Flush();
                logFile.writer.Close();
                return -1;
            }

            //string ImageFileToSave = Path.GetDirectoryName(sfd.FileName);
            //if (!ImageFileToSave.EndsWith(Path.DirectorySeparatorChar.ToString()))
            //    ImageFileToSave += Path.DirectorySeparatorChar;
            //ImageFileToSave += Path.GetFileNameWithoutExtension(sfd.FileName);
            //System.Drawing.Image img = Main.main.screenCapture.byteArrayToImage(Main.main.screenCapture.byteList.ElementAt(1));
            //img.Save(ImageFileToSave + ".png", System.Drawing.Imaging.ImageFormat.Png);

            File.Delete(costing_path);
            File.Delete(costingCSV_path);
            File.Delete(tempImageFile);
            File.Delete(tempDecodeFile);
            File.Delete(FileVersionpath);

            //isSlicedModelSave = true;

            //Main.main.SettingProjectName(sfd.FileName.ToString());

            //foreach (PrintModel stl in Main.main.objectPlacement.models)
            //{
            //    Main.main.LogMessage("Main", string.Format("Sliced Object Saved:" + stl.filename, ""), "");
            //}
            Console.WriteLine("End save 3wl.");
            logFile.writer.WriteLine("End save 3wl.");
            logFile.writer.Flush();
            logFile.writer.Close();
            return 0;
        }
    }
}
