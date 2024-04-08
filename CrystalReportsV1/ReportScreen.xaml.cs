using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
//using System.Windows.Controls;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using LSSERVICEPROVIDERLib;
using Patholab_Common;
using Patholab_DAL_V1;
using Xceed.Wpf.Toolkit;
using ComboBox = System.Windows.Controls.ComboBox;
using DateTimePicker = Xceed.Wpf.Toolkit.DateTimePicker;
using MessageBox = System.Windows.MessageBox;
using Orientation = System.Windows.Controls.Orientation;
using Table = CrystalDecisions.CrystalReports.Engine.Table;
using TextBox = System.Windows.Controls.TextBox;


namespace CrystalReportsV1
{
    /// <summary>
    /// Interaction logic for ReportScreen.xaml
    /// </summary>
    public partial class ReportScreen : Window
    {

        #region Private fields

        private bool debug;
        public INautilusUser NtlsUser { get; set; }
        private DataLayer dal;
        private readonly U_CRYSTAL_REPORT _currentReport;
        private readonly ICollection<U_REPORT_PARAMS_USER> _uReportParamsUser;
        private readonly INautilusDBConnection _ntlsCon;
        List<CustomParam> customParams = new List<CustomParam>();
        #endregion

        #region Ctor
        public ReportScreen() { }
        public ReportScreen(DataLayer dal, U_CRYSTAL_REPORT currentReport, INautilusUser ntlsUser, INautilusDBConnection ntlsCon)
        {

            debug = ntlsUser == null;
            NtlsUser = ntlsUser;
            this.dal = dal;
            _currentReport = currentReport;
            _uReportParamsUser = currentReport.U_REPORT_PARAMS_USER;
            _ntlsCon = ntlsCon;
            InitializeComponent();
            tbHeader.Text = currentReport.U_CRYSTAL_REPORT_USER.U_HEBREW_NAME;
            this.Title = tbHeader.Text;
            try
            {


                foreach (U_REPORT_PARAMS_USER prm in _uReportParamsUser.OrderBy(x => x.U_ORDER))
                {
                    LoadParam(prm);
                }

            }
            catch (Exception ex)
            {


                MessageBox.Show("פרמטרים לא הוגדרו כראוי");
            }
        }
        #endregion


        private void LoadParam(U_REPORT_PARAMS_USER prm)
        {


            //   UIElement element;

            var cp = new CustomParam();
            customParams.Add(cp);
            cp.FIELD_TYPE = prm.U_FIELD_TYPE;
            cp.FIELD_NAME = prm.U_FIELD_NAME;
            cp.PARAM_HEBREW = prm.U_PARAM_HEBREW;
            switch (prm.U_FIELD_TYPE)
            {
                case "D":
                    cp.UiElement = new Xceed.Wpf.Toolkit.DateTimePicker();
                    break;
                case "T":
                    var tb = new TextBox();
                    cp.UiElement = tb;
                    break;
                case "N":
                    var ud = new IntegerUpDown();
                    ud.Minimum = 1;
                    ud.DefaultValue = 1;
                    cp.UiElement = ud;
                    break;
                case "P":
                    ComboBox cmbp = SetPhraseDataSource(prm);
                    cp.UiElement = cmbp;
                    break;
                case "E":
                    ComboBox cmbe = SetEntityDataSource(prm);
                    cp.UiElement = cmbe;
                    break;
                case "L":
                    CheckComboBox clb = SetCLBDataSource(prm);
                    cp.UiElement = clb;
                    break;

            }
            AddToGrid(cp.UiElement, cp.PARAM_HEBREW);

        }

        private CheckComboBox SetCLBDataSource(U_REPORT_PARAMS_USER prm)
        {
            var clb = new CheckComboBox();
            if (prm.PHRASE_HEADER != null)
            {

                clb.ItemsSource = prm.PHRASE_HEADER.PHRASE_ENTRY.ToList();
                clb.DisplayMemberPath = "PHRASE_DESCRIPTION";
                clb.SelectedMemberPath = "PHRASE_NAME";

            }
            else if (prm.SCHEMA_TABLE != null)
            {
                string tn = prm.SCHEMA_TABLE.DATABASE_NAME;

                var d = dal.GetObjDetailses(tn, "");
                var l = d.ToList();
                clb.ItemsSource = l;
                clb.SelectedMemberPath = "ID";
                clb.DisplayMemberPath = "NAME";
            }


            return clb;
        }

        private void AddToGrid(FrameworkElement ctl, string p)
        {

            StackPanel sp = new StackPanel() { Orientation = Orientation.Horizontal, Margin = new Thickness(12) };
            sp.Children.Add(new TextBlock { Text = p, Width = 180 });
            ctl.Margin = new Thickness(12);
            ctl.MinWidth = 230;
            sp.Children.Add(ctl);
            mainSp.Children.Add(sp);


        }

        private ComboBox SetEntityDataSource(U_REPORT_PARAMS_USER prm)
        {
            var cmb = new ComboBox();

            string tn = prm.SCHEMA_TABLE.DATABASE_NAME;
            var d = dal.GetObjDetailses(tn, "").OrderBy(x => x.NAME);
            var l = d.ToList();
            cmb.ItemsSource = l;
            cmb.SelectedValuePath = "ID";
            cmb.DisplayMemberPath = "NAME";
            return cmb;

        }

        private ComboBox SetPhraseDataSource(U_REPORT_PARAMS_USER prm)
        {
            var cmb = new ComboBox();
            cmb.ItemsSource = prm.PHRASE_HEADER.PHRASE_ENTRY.ToList();
            cmb.DisplayMemberPath = "PHRASE_DESCRIPTION";
            cmb.SelectedValuePath = "PHRASE_NAME";
            return cmb;
        }
        ReportDocument CR;
        bool IsProxy = false;
        public void RunReport()
        {
            if (!IsValid())
            {
                MessageBox.Show("חובה למלאות את כל הערכים", Constants.mboxHeader, MessageBoxButton.OK, MessageBoxImage.Stop);
                return;
            }
            try
            {

                string serverName;
                string nautilusUserName;
                string nautilusPassword;

                serverName = _ntlsCon.GetServerDetails();
                IsProxy = _ntlsCon.GetServerIsProxy();
                if (IsProxy)
                {
                    nautilusUserName = "";
                    nautilusPassword = "";
                }
                else
                {


                    nautilusUserName = _ntlsCon.GetUsername();
                    nautilusPassword = _ntlsCon.GetPassword();
                }




               
                var reportPath = _currentReport.U_CRYSTAL_REPORT_USER.U_PATH;
                //   var crp = new CrystalReport(serverName, nautilusUserName, nautilusPassword, reportPath);
                if (File.Exists(reportPath))
                {
                    //load
                    CR = new ReportDocument();
                    CR.Load(reportPath);
                }
                foreach (CustomParam customParam in customParams)
                {
                    string val = customParam.GetParamValue();
                    if (val != null)
                    {

                        CR.SetParameterValue(customParam.FIELD_NAME, val);
                        //  crp.SetReportParameterValue(customParam.FIELD_NAME, val);

                    }
                    else
                    {
                        MessageBox.Show("חסר ערך  ", Constants.mboxHeader, MessageBoxButton.OK, MessageBoxImage.Hand);
                        return;
                    }
                }
                Tables crTables;
                var crTableLoginInfo = new TableLogOnInfo();
                var crConnectionInfo = new ConnectionInfo();

                crConnectionInfo.ServerName = serverName;
                if (IsProxy)
                {
                    crConnectionInfo.IntegratedSecurity = true;
                }
                else
                {
                    crConnectionInfo.UserID = nautilusUserName;
                    crConnectionInfo.Password = nautilusPassword;
                }



                crTables = CR.Database.Tables;
                foreach (Table crTable in crTables)
                {
             
                    crTableLoginInfo = crTable.LogOnInfo;
                    crTableLoginInfo.ConnectionInfo = crConnectionInfo;
                    crTable.ApplyLogOnInfo(crTableLoginInfo);
                }
                //  string rp = CreateSavedPath(reportPath);

                Form1 f = new Form1(CR);
   
                //  crp.exportCrystalToWordRTFAndSave(rp);
                //    crp.close();
                //   string pdfPath = rp.Replace("rtf", "pdf");
                //    crp.exportWordRtfToPdf(rp, pdfPath);
                //    crp.showFile(pdfPath);
                f.ShowDialog();
                //delete word file
                //crp.deleteFile(wordPath);
                //delete pdf file
                //crp.deleteFile(pdfPath);
                //WriteToLogTable.WriteLog(CMD, "DatesFrm Report created successfully", "GenerateReportExt", "RunReport", "OK");
            }
            catch (Exception e)
            {

                MessageBox.Show("Error on RunReport : " + e.Message, Constants.mboxHeader, MessageBoxButton.OK, MessageBoxImage.Error);
                Logger.WriteLogFile(e);
                this.Close();
            }
        }

        private bool IsValid()
        {

            foreach (CustomParam param in customParams)
            {
                if (string.IsNullOrEmpty(param.GetParamValue()))
                {
                    return false;
                }
            }
            return customParams.Select(customParam => customParam.GetParamValue()).All(val => val != null);
        }

        public string CreateSavedPath(string rptPath)
        {
            string workStationName = "PC1058";
            if (!debug)
            {
                workStationName = NtlsUser.GetWorkstationName();
            }

            string pathId = string.Format("{0:dd_MM_yyyy HH_mm_ss}", DateTime.Now);
            pathId = workStationName + "_" + pathId;
            string wordPath = rptPath.Replace(".rpt", pathId + ".rtf");
            var phrase = dal.GetPhraseEntries("System Parameters");
            if (phrase != null)
            {
                PHRASE_ENTRY phraseEntry = phrase.FirstOrDefault(x => x.PHRASE_NAME == "Crystal");
                if (phraseEntry != null)
                {
                    var startPath = phraseEntry.PHRASE_DESCRIPTION;
                    //    var startPath = dal.GetPhraseByName("System Parameters").PHRASE_ENTRY.FirstOrDefault(x => x.PHRASE_NAME == "Crystal").PHRASE_DESCRIPTION;
                    string endPath = wordPath.Substring(wordPath.LastIndexOf(@"\") + 1);
                    var fullPath = startPath + endPath;
                    return fullPath;
                }
            }
            MessageBox.Show("לא הוגדר הנתיב כראוי.", Constants.mboxHeader, MessageBoxButton.OK, MessageBoxImage.Error);
            return "";
        }


        #region Events
        private void RunReport_click(object sender, RoutedEventArgs e)
        {
            RunReport();
        }
        private void Back_click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        #endregion

    }


    internal class CustomParam
    {
        public string FIELD_NAME { get; set; }

        public string FIELD_TYPE { get; set; }

        public FrameworkElement UiElement { get; set; }

        public string PARAM_HEBREW { get; set; }


        internal string GetParamValue()
        {
            var tb = UiElement as TextBox;
            if (tb != null)
            {
                return tb.Text;
            }
            var ud = UiElement as IntegerUpDown;

            if (ud != null)
            {

                return ud.Value.ToString();
            }
            var dt = UiElement as DateTimePicker;
            if (dt != null)
            {
                return dt.Value.ToString();
            }
            var cb = UiElement as ComboBox;
            if (cb != null)
            {
                //for phrase selected value
                if (cb.SelectedValue != null)
                    return cb.SelectedValue.ToString();
            }
            var clb = UiElement as CheckComboBox;
            if (clb != null)
            {

                //    return clb.Text
                //       ;

                string ret = "";

                if (clb.SelectedItems != null && clb.SelectedItems.Count > 0)
                {
                    foreach (object entry in clb.SelectedItems)
                    {
                        //if list is Entity
                        var s = entry as ObjDetails;
                        if (s != null)
                        {
                            ret += s.ID + ",";

                        }
                        else //if list is  Phrase
                        {
                            var ss = entry as PHRASE_ENTRY;
                            if (ss != null)
                            {
                                ret += ss.PHRASE_NAME + ",";
                            }

                        }

                    }
                }

                if (ret.Length > 0)
                {
                    ret = ret.Remove(ret.Length - 1);
                }

                return ret;
            }



            return null;
        }
    }
}
