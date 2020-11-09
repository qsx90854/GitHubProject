using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
namespace XYZSDK
{
    public static class MainParameter
    {
        public static List<string> inputString = new List<string>();

        //string app_path = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
        public static string SelectedSlicerPath = "D:\\CP2_SLS_Project\\SLSwareX64_20200817LastUpdate\\bin\\Release\\XYZSlicer2.exe";
        public static string AppdataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\XYZSDK";
        public static string AssemblyFileVersion = "\\6.24.20\\";
        //public static string Temp_folder = Path.Combine(AppdataPath, "Temp");


        public static bool generating_dxf = false;


        public static List<string> List_ModelImage = new List<string>();
        public static double TotalDXFFileSize = 0;
        public static double totalEstimatedTime = 0.00;
        public static double total_contourTime = 0.00;
        public static double total_infillTime = 0.00;
        public static double totalPrintingTime = 0.0;

        public static double selLayerHeight = 0.1;
        public static double gCodeVolume = 1.00;
        public static double heightLimit = 230.0;
        public static int ModelCount = 1;
        public static int layercount = 0;
        public static int totalParts = 0;

        public static double ShrinkageXAxis = 1.0;
        public static double ShrinkageYAxis = 1.0;
        public static double PackingDensity = 1.0;
        public static List<System.Drawing.Image> fourWayImage = new List<System.Drawing.Image>(); //System.Drawing.Image FromFile (string filename);


        

        public static string input_svgPath = "";//"C:\\Users\\tb395014\\AppData\\Roaming\\XYZware_SLS_v1.0\\6.24.20\\composition\\composition.svg";
        public static string input_parPath = "";
        public static string input_imgPath = "";
        public static string export3wlFileName = "";//"export.3wl";

        public static string input_profilePath = "";
        public static string output_shrinkagePath = "";

        public static string svgFileName = "Model";
        public static string proFileName = "AutodeskProfile";
        public static string macnineName = "Default";

        
        public static string zipPassword = "MfgPro230xS_22232439";
        public static string DefineFile = "";
        public static string logFileName = "\\log.txt";
        public static string XYZSDKInterVersionNumber = "XYZSDK.exe version: 1.0.1c";


        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern int GetShortPathName([MarshalAs(UnmanagedType.LPTStr)] string path, [MarshalAs(UnmanagedType.LPTStr)] StringBuilder shortPath, int shortPathLength);
    }
}
