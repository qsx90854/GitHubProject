using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZSDK
{
    class Costing
    {
        CostData costData;
        public void CalculateTotalCost()
        {
            //if (totalCostText == null) return;

            //Console.WriteLine("\nCalculateTotalCost()======================================");
            //Console.WriteLine(" powderDesity {0}", powderDesity); //<timmy>
            //Console.WriteLine(" sinteredDesity {0}", sinteredDesity); //<timmy>

            //<timmy><9-10-2020><add log for cost>
            // Main.main.LogMessage("Costing", "-----------Calculate---------------", "");
            // Main.main.LogMessage("Costing", string.Format("New Powder Price: {0}, Old Powder Price: {1}, New Powder Rate: {2} %", newMatPrice, recycMatPrice, matRate), "");
            // Main.main.LogMessage("Costing", string.Format("powderDesity: {0} (g/cm3), sinteredDesity: {1} (g/cm3)", powderDesity, sinteredDesity), "");
            //  Main.main.LogMessage("Costing", string.Format("powder Volume: {0} (cm3), model Volume: {1} (cm3)", (int)powderVolume, (int)totalVolume), "");

            ////<timmy><9-10-2020><try to add base/cover powder volume>
            //if (Main.main.IsSliced)
            //{   //export 3wl - use current selected profile
            //    baselayer = Main.main.threedview.ui.ProfileSettings_UI.SelectedProfileSettings.baselayercyclecount;
            //    coverlayer = Main.main.threedview.ui.ProfileSettings_UI.SelectedProfileSettings.coverlayer;
            //}
            //else
            //{  //load 3wl - use setting from 3wl
            //}
            //// 26 cm * 26 cm * 120% * (base layer + cover layer) * layer height (cm)
            ////baseCoverVolume = 26 * 26 * 1.2 * (baselayer + coverlayer) * layerHeight / 10;

            ////<timmy><9-21-2020><use same fumulation from Buildware>
            //baseCoverVolume = 26 * 26 *
            //   (MIN(5, baselayer) * 3.0 + (baselayer - MIN(5, baselayer) + coverlayer) * (1.2 + powderAdjustmentBaseCover))
            //   * layerHeight / 10;

            //baseCoverWeight = baseCoverVolume * powderDesity;
            //Main.main.LogMessage("Costing", string.Format("base: {0}, cover: {1}, volume: {2} (cm3), weight: {3} (g)"
            //    , baselayer, coverlayer, (int)baseCoverVolume, (int)baseCoverWeight), "");
            ////<><><>


            ////W=(AC+B(1-C))OE)/1000
            ////<timmy><8-24-2020><DEBUG: UI and default setting use range (0~100), so rate needs to be div 100 in formulation >
            ////totalPrice = ((newMatPrice * matRate / 100 + recycMatPrice * (1 - matRate / 100)) * powderVolume * powderDesity) / 1000;
            ////totalPrice = ((newMatPrice * matRate + recycMatPrice * (1 - matRate)) * powderVolume * powderDesity) / 1000;

            //totalPrice = ((newMatPrice * matRate / 100 + recycMatPrice * (1 - matRate / 100)) * (powderVolume + baseCoverVolume) * powderDesity) / 1000;

            ////Z=B(OE-PF)/1000
            ////recycPrice = recycMatPrice * (powderVolume * powderDesity - totalVolume * sinteredDesity) / 1000;
            //recycPrice = recycMatPrice * ((powderVolume + baseCoverVolume) * powderDesity - totalVolume * sinteredDesity) / 1000;
            //totalCost = totalPrice - recycPrice;

            ////Console.WriteLine(" powderVolume: {0}, totalVolume: {1}", powderVolume, totalVolume); //<timmy>
            ////Console.WriteLine(" new Powder Price: {0}, old Powder Price: {1}, New matRate: {2} %", newMatPrice, recycMatPrice, matRate); //<timmy>

            ////Console.WriteLine(" totalPrice: {0}", totalPrice); //<timmy>
            ////Console.WriteLine(" recycPrice: {0}", recycPrice); //<timmy>
            ////Console.WriteLine(" totalCost: [{0}]", totalCost); //<timmy>

            ////<timmy><8-12-2020><add line>
            //totalCostText.Text = string.Format("${0:0.00}", totalCost);
            //Console.WriteLine(" CalculateTotalCost: totalCostText [{0}]", totalCostText.Text); //<timmy>

            //totalWeigt = powderVolume * powderDesity;
            ////totalWeigt = (powderVolume + baseCoverVolume) * powderDesity;

            //Main.main.LogMessage("Costing", string.Format("total powder weight (build with models): {0} (g) ", (int)totalWeigt), "");
            //Main.main.LogMessage("Costing", string.Format("total powder weight (build with models + base + cover): {0} (g)", (int)(totalWeigt + baseCoverWeight), ""));
            //Main.main.LogMessage("Costing", string.Format("totalCost: [{0:0.00}] ($)", totalCost), "");

            ////grid_est.Visibility = System.Windows.Visibility.Hidden;
            //grid_est.Visibility = System.Windows.Visibility.Visible;
            //if (Main.main.IsComputeMarkingTime)
            //{
            //    TimeSpan span = TimeSpan.FromSeconds(Main.main.totalEstimatedTime);

            //    #region old
            //    //scanningTime.Text = span.Days + "D " + span.Hours + "H " + span.Minutes + " M";
            //    //double estTime = Main.main.totalPrintingTime;
            //    //double hour = estTime % 3600000;
            //    //double min = (estTime % 3600000) / 60000;
            //    //double sec = (estTime % 60000) / 1000;
            //    //scanningTime.Text = Math.Truncate(hours).ToString("00") + "h " + Math.Truncate(mins).ToString("00") + "m";
            //    #endregion 

            //    int estTime = Convert.ToInt32(Main.main.totalPrintingTime);
            //    int days = estTime / 86400000;
            //    estTime -= 86400000 * days;
            //    int hours = estTime / 3600000;
            //    estTime -= 3600000 * hours;
            //    int mins = estTime / 60000;
            //    estTime -= mins * 60000;
            //    int sec = estTime / 1000;

            //    TimeSpan TotalPrint = new TimeSpan((long)Main.main.totalPrintingTime * 1000);
            //    if (Main.main.IsComputeMarkingTime)
            //    {
            //        scanningTime.Text = "";
            //        if (days < 10)
            //            scanningTime.Text += "0" + days.ToString() + "d ";
            //        else
            //            scanningTime.Text += days.ToString() + "d ";
            //        if (hours < 10)
            //            scanningTime.Text += "0" + hours.ToString() + "h ";
            //        else
            //            scanningTime.Text += hours.ToString() + "h ";
            //        if (mins < 10)
            //            scanningTime.Text += "0" + mins.ToString() + "m";
            //        else
            //            scanningTime.Text += mins.ToString() + "m";
            //    }
            //    else
            //        scanningTime.Text = "--:--";
            //    Main.main.LogMessage("Costing", "Total Printing Time: " + days.ToString() + ":" + hours.ToString() + ":" + mins.ToString() + ":" + sec.ToString(), "");
            //}
            //else if (!b_loading3wl)
            //{
            //    scanningTime.Text = "-";
            //}

            ////if(b_loading3wl) //<timmy><8-28-2020><show time info after reload 3wl>
            ////    grid_est.Visibility = System.Windows.Visibility.Visible;
            //totalWeigtText.Text = totalWeigt.ToString("0.00") + " g";
            //Console.WriteLine(" totalWeigt: {0}", totalWeigt); //<timmy>

            ////<timmy><9-17-2020><powder height>
            //double powerHeight = (powderVolume + baseCoverVolume) / (26 * 26) * 10;
            //totalPowderText.Text = string.Format(" {0} mm ({1}%)", (int)powerHeight, (int)(powerHeight * 100 / 335));
            ////<><><>

            ////<timmy><9-16-2020><show new powder amount on UI>
            //totalWeigtText.Text =
            //    //"(" + totalWeigt.ToString("0")
            //    //+ " + " + (baseCoverWeight).ToString("0") + ") = "
            //    //+ 
            //    (totalWeigt + baseCoverWeight).ToString("0") + " g "
            //    + (b_withBaseCoverIn3wl || Main.main.IsSliced ? "" : " * ")
            //    //+  " (" + this.baselayer + ", "  + this.coverlayer + ")"
            //    ;
            ////<><><>

            //Main.main.threedview.ui.toolbar.costingBtn.Content = "Estimated Cost " + totalCostText.Text; //Tonton<06-04-19> Append costing button 
            //Main.main.threedview.ui.toolbar.costingBtn1.Content = "Estimated Cost " + totalCostText.Text; //Tonton<06-04-19> Append costing button 

            ////<timmy><8-6-2020><DEBUG: update global variable, otherwise cost value be rewrited to old one>
            //if (Main.main.TotalCostData != null)
            //    Main.main.TotalCostData.cost = totalCostText.Text;

            ////Console.WriteLine(" Main.main.threedview.ui.toolbar.costingBtn.Content [ {0} ]", Main.main.threedview.ui.toolbar.costingBtn.Content); //<timmy>
            //UpdateObject();

            //Main.main.LogMessage("Costing", "----------End of Calculate---------", "");
        }
        public void AddHistory()
        {
            //CostingHistory history = Main.main.threedview.ui.costingHistory;
            //history.Init();
            TimeSpan timeSpan = TimeSpan.FromSeconds(MainParameter.totalEstimatedTime);
            costData = new CostData()
            {
                name = MainParameter.svgFileName,
                machinename = MainParameter.macnineName,
                profilename = MainParameter.proFileName,
                objects = MainParameter.ModelCount.ToString(),
                //<timmy><9-16-2020><add base cover>
                weight = "0",
                //weight = totalWeigt.ToString("0"),
                //<><><>
                newPrice = "0",
                recycledPrice = "0",
                rate = "0",
                time = "0",
                created = DateTime.Now.ToString("yyyy/MM/dd"),
                cost = "0",
                objectCount = MainParameter.ModelCount

            };
        }
    }
}
