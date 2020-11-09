using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
namespace XYZSDK
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.Hide();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SVGTo3WL SVGProcess = new SVGTo3WL();
            SVGProcess.SVGto3WLFile();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            int result = 0;
            if (MainParameter.inputString.Count() == 8)
            {
                try
                {
                    SVGTo3WL SVGProcess = new SVGTo3WL();
                    result = SVGProcess.SVGto3WLFile();
                    string path = MainParameter.AppdataPath + "\\" + MainParameter.AssemblyFileVersion;
                    if(Directory.Exists(path))
                    {
                        Console.WriteLine("Delete path : {0}", path);
                        Directory.Delete(path, true);
                    }
                    
                    Environment.Exit(result);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("SVGto3WLFile exception error : {0}", ex.ToString());
                    Environment.Exit(-1);
                }
            }
            else if(MainParameter.inputString.Count() == 4)
            {
                try
                {
                    SVGTo3WL SVGProcess = new SVGTo3WL();
                    result = SVGProcess.ExtractShrinkage();
                    string path = MainParameter.AppdataPath + "\\" + MainParameter.AssemblyFileVersion;
                    if (Directory.Exists(path))
                    {
                        Console.WriteLine("Delete path : {0}", path);
                        Directory.Delete(path, true);
                    }
                    Environment.Exit(result);
                }
                catch(Exception ex)
                {
                    Console.WriteLine("ExtractShrinkage exception error : {0}", ex.ToString());
                    Environment.Exit(-1);
                }

            }
            else
            {
                ////For standard test
                //for (int i = 0; i < 8; i++) MainParameter.inputString.Add("");

                //MainParameter.inputString[0] = "-svg_file";
                //MainParameter.inputString[1] = @"D:\data\Engines\xyz\ForAutoDesk\test.svg";
                //MainParameter.inputString[2] = "-printing_parameters";
                //MainParameter.inputString[3] = @"C:\ForAutoDesk\Parameter.ini";
                //MainParameter.inputString[4] = "-preview_images";
                //MainParameter.inputString[5] = @"C:\ForAutoDesk\FourWayImage";
                //MainParameter.inputString[6] = "-3wl_file";
                //MainParameter.inputString[7] = @"C:\ForAutoDesk\Output\test.3wl";
                //SVGTo3WL SVGProcess = new SVGTo3WL();
                //result = SVGProcess.SVGto3WLFile();
                ////End Test

                Console.WriteLine("Input argument count error.");
                Environment.Exit(-1);
            }

            
        }
    }
}
