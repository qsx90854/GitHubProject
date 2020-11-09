using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
namespace XYZSDK
{
    public class CostObjectData
    {
        public string name { get; set; }
        public string weight { get; set; }
        public string cost { get; set; }
        public string volume { get; set; }

        public CostObjectData(string name, double weight, double cost)
        {
            this.name = name;
            this.weight = weight.ToString("0.00");
            this.cost = cost.ToString("0.00");
        }

        //Name, Objects, Weight, New, Recycled, Rate, Time, Created, Cost
        public CostObjectData(string[] datas)
        {
            //if (datas.Length != CostingHistoryCSV.ParamAmount) return;
            if (datas == null) return;
            name = datas[0];
            weight = datas[4];
            cost = datas[10];
            volume = datas[datas.Length - 1];
        }

        public string[] GetDataArray()
        {
            string[] datas = new string[CostingHistoryCSV.ParamAmount];
            datas[0] = name;
            datas[1] = "-";
            datas[2] = "-";
            datas[3] = "-";
            datas[4] = weight;
            datas[5] = "-";
            datas[6] = "-";
            datas[7] = "-";
            datas[8] = "-";
            datas[9] = "-";
            datas[10] = cost;
            return datas;
        }
    }
    public class CostData
    {
        public string name { get; set; }
        public string objects { get; set; }
        public string weight { get; set; }
        public string newPrice { get; set; }
        public string recycledPrice { get; set; }
        public string rate { get; set; }
        public string time { get; set; }
        public string created { get; set; }
        public string cost { get; set; }
        public List<CostObjectData> costObjects { get; set; }
        public bool selected { get; set; }
        public int objectCount;
        public string profilename { get; set; }
        public string machinename { get; set; }

        //public CostData()
        //{

        //}

        //Name, Objects, Weight, New, Recycled, Rate, Time, Created, Cost
        public CostData(string[] datas = null, bool isReading = false, int index = 0)
        {
            #region remove code
            //if (isReading)
            //{
            //    if (datas.Length != CostingHistoryCSV.ParamAmount) return;
            //    name = datas[0];
            //    machinename = datas[1];
            //    profilename = datas[2];
            //    objects = datas[3];
            //    objectCount = Convert.ToInt16(objects);
            //    if (objectCount > 1)
            //    {
            //        costObjects = new List<CostObjectData>();
            //    }
            //    weight = datas[4];
            //    newPrice = datas[5];
            //    recycledPrice = datas[6];
            //    rate = datas[7];
            //    time = datas[8];
            //    created = datas[9];
            //    cost = datas[10];
            //}
            #endregion

            //modified code for fixing compatibility issue of old Costing History file.
            if (isReading)
            {
                List<Dictionary<string, string>> record_history = CostingHistoryCSV.BackUp_Records(CostingHistoryCSV.path);
                if (index < record_history.Count)
                {
                    Dictionary<string, string> record = record_history[index];
                    //if (datas.Length == CostingHistoryCSV.ParamAmount) return;
                    name = record.ContainsKey("Name") ? record["Name"] : "---";
                    profilename = record.ContainsKey("Profile Settings") ? record["Profile Settings"] : "---";
                    machinename = record.ContainsKey("Printer Name") ? record["Printer Name"] : "---";
                    objects = record.ContainsKey("Objects") ? record["Objects"] : "---";
                    if (objects != "-")
                        objectCount = Convert.ToInt16(objects);
                    if (objectCount > 1)
                    {
                        costObjects = new List<CostObjectData>();
                    }
                    weight = record.ContainsKey("Weight") ? record["Weight"] : "---";
                    newPrice = record.ContainsKey("New") ? record["New"] : "---";
                    recycledPrice = record.ContainsKey("Recycled") ? record["Recycled"] : "---";
                    rate = record.ContainsKey("Rate") ? record["Rate"] : "---";
                    time = record.ContainsKey("Time") ? record["Time"] : "---";
                    created = record.ContainsKey("Created") ? record["Created"] : "---";
                    cost = record.ContainsKey("Cost") ? record["Cost"] : "---";
                }
            }
            //<><><>
        }

        public void AddObject(string[] datas)
        {
            costObjects.Add(new CostObjectData(datas));
        }

        public void AddObject(string name, double weight, double cost)
        {
            costObjects.Add(new CostObjectData(name, weight, cost));
        }

        public string[] GetDataArray()
        {
            string[] datas = new string[CostingHistoryCSV.ParamAmount];
            datas[0] = name;
            datas[1] = machinename;
            datas[2] = profilename;
            datas[3] = objects;
            datas[4] = weight;
            datas[5] = newPrice;
            datas[6] = recycledPrice;
            datas[7] = rate;
            datas[8] = time;
            datas[9] = created;
            datas[10] = cost;
            return datas;
        }
    }
    class CostingHistory
    {
        const int pageButtonAmount = 5;
        const int pageSize = 10;
        const int firstPage = 1;
        int pageIndex = 1;
        int totalPage = 0;
        public List<CostData> totalData;
        

        public void Init()
        {
            totalData = CostingHistoryCSV.GetData(true);
            totalPage = (int)Math.Ceiling((double)totalData.Count / pageSize);
            pageIndex = firstPage;
            UpdatePage();
        }

        public void AddHistory(CostData costData)
        {
            totalData.Insert(0, costData);
            totalPage = (int)Math.Ceiling((double)totalData.Count / pageSize);
            pageIndex = firstPage;
            //lastBtn.Content = totalPage;
            //firstBtn.Content = firstPage;
            UpdatePage();
            CostingHistoryCSV.Save(totalData);
            if (costData.objectCount > 0)
                CostingHistoryCSV.SaveCostDataFor3WL(costData); // this was to create a costing history which append in 3wl File.
        }

        public void RemoveHistory(List<CostData> costDatas)
        {
            foreach (CostData costData in costDatas)
            {
                totalData.Remove(costData);
            }
            totalPage = (int)Math.Ceiling((double)totalData.Count / pageSize);
            UpdatePage();
            CostingHistoryCSV.Save(totalData);
        }

        public void UpdatePage()
        {
            //costList.ItemsSource = totalData.Skip((pageIndex - 1) * pageSize).Take(pageSize);
           // currentPageTextBlock.Text = pageIndex.ToString();
            //selectAllCheckBox.IsChecked = false;
            //selectAllCheckBox_Click(selectAllCheckBox, null);
            //deleteBtn.Visibility = Visibility.Hidden;
            //if (costList.Items.Count == 0)
            //{
            //    num0Btn.Content = pageIndex - 2;
            //    UpdateVisible(etcMin, pageIndex > firstPage + 3);
            //    UpdateVisible(num0Btn, pageIndex > firstPage + 2);
            //    UpdateVisible(num1Btn, pageIndex > firstPage + 1); // <Rojin> <12-23-2019> Page indexes are shown instead of just one (1) even if there are no items at cost list.
            //    UpdateVisible(num2Btn, pageIndex < totalPage - 1);
            //    UpdateVisible(num3Btn, pageIndex < totalPage - 2);
            //    UpdateVisible(etcMax, pageIndex < totalPage - 3);
            //    UpdateVisible(firstBtn, pageIndex != firstPage);
            //    UpdateVisible(lastBtn, pageIndex == totalPage);
            //    //<><><>
            //}
            //else
            //{
            //    lastBtn.Content = totalPage;
            //    num0Btn.Content = pageIndex - 2;
            //    num1Btn.Content = pageIndex - 1;
            //    num2Btn.Content = pageIndex + 1;
            //    num3Btn.Content = pageIndex + 2;
            //    UpdateVisible(etcMin, pageIndex > firstPage + 3);
            //    UpdateVisible(num0Btn, pageIndex > firstPage + 2);
            //    UpdateVisible(num1Btn, pageIndex > firstPage + 1);
            //    UpdateVisible(firstBtn, pageIndex != firstPage);
            //    UpdateVisible(lastBtn, pageIndex != totalPage);
            //    UpdateVisible(num2Btn, pageIndex < totalPage - 1);
            //    UpdateVisible(num3Btn, pageIndex < totalPage - 2);
            //    UpdateVisible(etcMax, pageIndex < totalPage - 3);
            //}
        }
        
    }

    static public class CostingHistoryCSV
    {
        public static string path = MainParameter.AppdataPath + "\\CostingHistory_SLS.csv";
        const string version = "20201007";
        const string title = "Name,Printer Name,Profile Settings,Objects,Weight,New,Recycled,Rate,Time,Created,Cost";
        public const int ParamAmount = 12;

        static public void CreateNew()
        {
            StreamWriter writer = new StreamWriter(path);
            writer.WriteLine(version);
            writer.WriteLine(title);
            writer.Close();
        }

        static public void Upgrade()
        {
            StreamReader reader = new StreamReader(path);
            string[] ver = reader.ReadLine().Split(',');
            reader.Close();
            if (ver[0] == version)
            {
                return;
            }
            else if (ver[0] == "20190618")
            {
                List<CostData> costDetails = GetData(false);
                costDetails.Sort((a, b) => b.created.CompareTo(a.created));
                Save(costDetails);
            }
            else
                CreateNew();
        }

        static public List<CostData> GetData(bool check)
        {
            if (check)
            {
                if (!File.Exists(path))
                {
                    CreateNew();
                }
                Upgrade();
            }
            #region remove code
            //StreamReader reader = new StreamReader(path);
            //reader.ReadLine();
            //reader.ReadLine();
            //List<CostData> costDetails = new List<CostData>();
            //string line;
            //int index = 0;

            //while (!reader.EndOfStream)
            //{
            //    line = reader.ReadLine();
            //    if (line == "") continue;
            //    //CostData costDetail = new CostData(line.Split(',')); // remove code
            //   CostData costDetail = new CostData(line.Split(','), true, index); //modified code for compatibility of files.
            //    costDetails.Add(costDetail);
            //    index++;
            //    if (costDetail.objectCount > 1)
            //    {
            //        while (!reader.EndOfStream)
            //        {
            //            line = reader.ReadLine();
            //            if (line != "")
            //            {
            //                string[] costHeader = line.Split(',');
            //                if (costHeader[0] != "")
            //                {
            //                    string[] type = costHeader[0].Split('.');
            //                    if (type.Count() == 2 && type[type.Count() - 1] != "3wl")
            //                    {
            //                     costDetail.AddObject(line.Split(','));
            //                        index++;
            //                    }
            //                    if (type[type.Count() - 1] != "3mf" && costDetail.name != costHeader[0])
            //                    {
            //                        if (Convert.ToInt32(costHeader[3]) != 1)
            //                            continue;
            //                        else
            //                        {
            //                            costDetail = new CostData(line.Split(','), true, index); //reset the costdetail item & reset to create the next costdetail. 
            //                            costDetails.Add(costDetail);
            //                        }
            //                    }
            //                }
            //            }
            //            else
            //            {
            //                break;
            //            }
            //        }
            //    }

            //}

            //reader.Close();
            #endregion

            Dictionary<string, string> costitems = new Dictionary<string, string>();
            CostData costDetail = new CostData();
            List<CostData> costDetails = new List<CostData>();
            List<Dictionary<string, string>> record_history = CostingHistoryCSV.BackUp_Records(CostingHistoryCSV.path);
            bool HasMultipleObject = false;
            for (int i = 0; i < record_history.Count; i++)
            {
                costitems = record_history[i];
                if (costitems["Name"].Substring(costitems["Name"].Length - 4) == ".3wl" || HasMultipleObject)
                {
                    costDetail = new CostData();
                    costDetail.costObjects = new List<CostObjectData>();
                    costDetail.name = costitems["Name"] != "" || costitems["Name"] != "-" ? costitems["Name"] : "";
                    costDetail.objects = costitems["Objects"] != "" || costitems["Objects"] != "-" ? costitems["Objects"] : "";
                    costDetail.objectCount = Convert.ToInt32(costDetail.objects);
                    costDetail.weight = costitems["Weight"] != "" || costitems["Weight"] != "-" ? costitems["Weight"] : "";
                    costDetail.newPrice = costitems["New"] != "" || costitems["New"] != "-" ? costitems["New"] : "";
                    costDetail.recycledPrice = costitems["Recycled"] != "" || costitems["Recycled"] != "-" ? costitems["Recycled"] : "";
                    costDetail.rate = costitems["Rate"] != "" || costitems["Rate"] != "-" ? costitems["Rate"] : "";
                    costDetail.time = costitems["Time"] != "" || costitems["Time"] != "-" ? costitems["Time"] : "";
                    costDetail.created = costitems["Created"] != "" || costitems["Created"] != "-" ? costitems["Created"] : "";
                    costDetail.cost = costitems["Cost"] != "" || costitems["Cost"] != "-" ? costitems["Cost"] : "";
                    costDetail.profilename = costitems.ContainsKey("Profile Settings") ? costitems["Profile Settings"] != "" || costitems["Profile Settings"] != "---" ? costitems["Profile Settings"] : "" : "---";
                    costDetail.machinename = costitems.ContainsKey("Printer Name") ? costitems["Printer Name"] != "" || costitems["Printer Name"] != "---" ? costitems["Printer Name"] : "" :
                                             costitems.ContainsKey("Machine Name") ? costitems["Machine Name"] : "---";
                    costDetails.Add(costDetail);
                }
                else if (costitems["Name"].Substring(costitems["Name"].Length - 4) == ".3mf" || costitems["Name"].Substring(costitems["Name"].Length - 4) == ".stl")
                {
                    string name, weight, cost, volume;
                    name = costitems["Name"] != "" || costitems["Name"] != "-" ? costitems["Name"] : "";
                    weight = costitems["Weight"] != "" || costitems["Weight"] != "-" ? costitems["Weight"] : "";
                    cost = costitems["Cost"] != "" || costitems["Cost"] != "-" ? costitems["Cost"] : "";
                    costDetail.AddObject(name, Convert.ToDouble(weight), Convert.ToDouble(cost));
                }

            }

            return costDetails;
        }

        static public List<Dictionary<string, string>> BackUp_Records(string filename)
        {
            List<Dictionary<string, string>> imported_settings = new List<Dictionary<string, string>>();

            System.IO.StreamReader sr = new System.IO.StreamReader(filename);

            char[] split_char = { ',' };
            string[] keyArray = sr.ReadLine().Split(split_char);
            keyArray = sr.ReadLine().Split(split_char);
            int iRow = 0;
            while (sr.Peek() > 0)
            {
                string[] arr = sr.ReadLine().Split(split_char);

                int index = 0;
                bool errorRow = false;
                Dictionary<string, string> keyValue = new Dictionary<string, string>();
                foreach (string rec in arr)
                {
                    if (index >= keyArray.Length) // +1: accept last column that could be an empty (valid) trailer
                    {
                        if (!keyArray[keyArray.Length - 1].Equals(""))
                        {
                            errorRow = true;
                        }
                        break;
                    }
                    if ((index < keyArray.Length && rec != "") || keyArray[index] == "Time") keyValue.Add(keyArray[index], rec);
                    else
                        errorRow = true;
                    index++;
                }

                iRow++;
                if (errorRow) continue;
                imported_settings.Add(keyValue);
            }

            sr.Close(); sr.Dispose();

            return imported_settings;
        }
        static public List<Dictionary<string, string>> GetDensitiesInRecord(string filename)
        {
            List<Dictionary<string, string>> Record_data = new List<Dictionary<string, string>>();
            System.IO.StreamReader sr = new System.IO.StreamReader(filename);
            while (sr.Peek() > 0)
            {
                char[] splitter = { ':', '-' };
                Dictionary<string, string> record = new Dictionary<string, string>();
                string[] data = sr.ReadLine().Split(splitter);
                record.Add(data[0], data[1]);
                Record_data.Add(record);
            }
            sr.Close(); sr.Dispose();
            return Record_data;
        }
        static public List<Dictionary<string, string>> GetCostdetailsAndObjects(List<Dictionary<string, string>> cost)
        {
            List<Dictionary<string, string>> costing = new List<Dictionary<string, string>>();

            foreach (Dictionary<string, string> item in cost)
            {
                bool isObject = false;

            }



            return costing;
        }


        static public void Save(List<CostData> costDatas)
        {
            StreamWriter writer = new StreamWriter(path);
            writer.WriteLine(version);
            writer.WriteLine(title);
            string data = "";
            foreach (CostData costData in costDatas)
            {
                data = "";
                foreach (string str in costData.GetDataArray())
                {
                    data += str + ',';
                }
                data = data.TrimEnd(',');
                writer.WriteLine(data);
                //<timmy><9-21-2020><udpate - let single object detail store in history csv>            
                if (costData.objectCount >= 1)
                //if (costData.objectCount > 1)
                {
                    try //<timmy><9-21-2020><add try/catch - compatible for old history format>
                    {
                        foreach (CostObjectData costObjectData in costData.costObjects)
                        {
                            data = "";
                            foreach (string str in costObjectData.GetDataArray())
                            {
                                data += str + ',';
                            }
                            data = data.TrimEnd(',');
                            writer.WriteLine(data);
                        }
                    }
                    catch
                    {
                        //Main.main.LogMessage("History", "exception in Save", "");
                    }
                }
                writer.WriteLine();
            }
            writer.Close();
        }

        static public void SaveCostDataFor3WL(CostData costData)
        {
            string costingCSV_path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "XYZware_SLS_v1.0" + Path.DirectorySeparatorChar);
            StringBuilder shortPath = new StringBuilder(255);
            MainParameter.GetShortPathName(costingCSV_path, shortPath, shortPath.Capacity);
            costingCSV_path = Path.Combine(shortPath.ToString(), "CostingHistory.csv");
            StreamWriter writer = new StreamWriter(costingCSV_path);
            writer.WriteLine(version);
            writer.WriteLine(title + ",volume");
            string data = "";
            double totalVolume = 0.00;
            //foreach (double vol in Main.main.List_ModelsVolume)
            //    totalVolume += vol;

            totalVolume = MainParameter.gCodeVolume;/// 1000;

            foreach (string str in costData.GetDataArray())
            {
                data += str + ',';
            }
            data = data.TrimEnd(',');
            writer.WriteLine(data + "," + totalVolume);


            ////<timmy><9-21-2020><udpate - let single object detail store in costing csv of 3wl>
            ////if (costData.objectCount >= 1)
            if (costData.objectCount > 1)
            {
                int index = 0;
                foreach (CostObjectData costObjectData in costData.costObjects)
                {
                    data = "";
                    foreach (string str in costObjectData.GetDataArray())
                    {
                        data += str + ',';
                    }
                    data = data.TrimEnd(',');
                    // writer.WriteLine(data + "," + Main.main.List_ModelsVolume[index]);
                    writer.WriteLine(data + "," + "0");
                    index++;
                }
            }
            writer.WriteLine();
            writer.Close();
        }
        static public void SaveEmptyDataFor3WL()
        {
            string costingCSV_path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "XYZware_SLS_v1.0" + Path.DirectorySeparatorChar);
            StringBuilder shortPath = new StringBuilder(255);
            MainParameter.GetShortPathName(costingCSV_path, shortPath, shortPath.Capacity);
            costingCSV_path = Path.Combine(shortPath.ToString(), "CostingHistory.csv");
            StreamWriter writer = new StreamWriter(costingCSV_path);
            writer.WriteLine(version);
            writer.WriteLine(title + ",volume");
            string data = "";
            double totalVolume = 0.00;
            writer.WriteLine();
            writer.Close();
        }
    }
}
