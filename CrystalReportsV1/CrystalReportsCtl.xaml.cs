using System;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using LSExtensionWindowLib;
using LSSERVICEPROVIDERLib;
using Patholab_Common;

using Patholab_DAL_V1;

using Binding = System.Windows.Data.Binding;
using DataGrid = System.Windows.Controls.DataGrid;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using ListBox = System.Windows.Controls.ListBox;
using MessageBox = System.Windows.MessageBox;
using MessageBoxOptions = System.Windows.MessageBoxOptions;


//using MessageBox = System.Windows.Controls.MessageBox;


namespace CrystalReportsV1
{
    /// <summary>
    /// Interaction logic for CrystalReportsCtl.xaml
    /// </summary>
    public partial class CrystalReportsCtl : System.Windows.Controls.UserControl
    {


        public CrystalReportsCtl(INautilusServiceProvider sp, INautilusProcessXML xmlProcessor, INautilusDBConnection _ntlsCon, IExtensionWindowSite2 _ntlsSite, INautilusUser _ntlsUser)
        {
            InitializeComponent();
            // TODO: Complete member initialization
            this.xmlProcessor = xmlProcessor;
            this._ntlsCon = _ntlsCon;
            this._ntlsSite = _ntlsSite;
            this._ntlsUser = _ntlsUser;
            this.sp = sp;
            this.dal = dal;
            this.DataContext = this;
        }


        #region Private fields

        private INautilusProcessXML xmlProcessor;
        private INautilusUser _ntlsUser;
        private IExtensionWindowSite2 _ntlsSite;
        private INautilusServiceProvider sp;
        private INautilusDBConnection _ntlsCon;

        public List<U_CRYSTAL_REPORT> ReportsA { get; set; }
        public List<U_CRYSTAL_REPORT> ReportsB { get; set; }
        private DataLayer dal;
        public bool DEBUG = true;



        #endregion

        public bool CloseQuery()
        {
            if (dal != null) dal.Close();

            return true;
        }

        public void PreDisplay()
        {

            xmlProcessor = Utils.GetXmlProcessor(sp);

            _ntlsUser = Utils.GetNautilusUser(sp);

            InitializeData();
        }

        public void SetServiceProvider(object serviceProvider)
        {
            sp = serviceProvider as NautilusServiceProvider;
            _ntlsCon = Utils.GetNtlsCon(sp);


        }

        public void InitializeData()
        {

            try
            {
                dal = new DataLayer();

                if (DEBUG)
                    dal.MockConnect();
                else
                    dal.Connect(_ntlsCon);

                ReportsA = new List<U_CRYSTAL_REPORT>();
                ReportsB = new List<U_CRYSTAL_REPORT>();
                var reports = dal.GetAll<U_CRYSTAL_REPORT>().Include("U_CRYSTAL_REPORT_USER");//.GroupBy(x => x.U_CRYSTAL_REPORT_USER.U_COLUMN).ToList();

                foreach (U_CRYSTAL_REPORT rpt in reports)
                {
                    if (rpt.U_CRYSTAL_REPORT_USER.U_COLUMN == "A")
                    {
                        if (AllowRole(rpt))
                            ReportsA.Add(rpt);

                    }
                    if (rpt.U_CRYSTAL_REPORT_USER.U_COLUMN == "B")
                    {
                        if (AllowRole(rpt))
                            ReportsB.Add(rpt);

                    }
                }
            }
            catch (Exception e)
            {


                MessageBox.Show("Error " + e.Message, Constants.mboxHeader,
                         MessageBoxButton.OK, MessageBoxImage.Error);

                Logger.WriteLogFile(e);
            }

        }

        private bool AllowRole(U_CRYSTAL_REPORT rpt)
        {
            var roles = rpt.U_CRYSTAL_REPORT_USER.U_ROLE_ALLOWED;
            if (roles != null)
            {
                var splited = roles.Split(',', ';');
                if (!DEBUG)
                {
                    var roleId = _ntlsUser.GetRoleId().ToString();
                    if (splited.Contains(roleId))
                    {
                        return true;
                    }
                    return false;

                }
            }
            return true;
        }

        #region  events
        private void btnExit_Click(object sender, RoutedEventArgs e)
        {


            MessageBoxResult result = System.Windows.MessageBox.Show("?האם אתה בטוח שברצונך לצאת", Constants.mboxHeader, System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
                if (_ntlsSite != null) _ntlsSite.CloseWindow();
        }

        private void lb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox listBox = sender as ListBox;
            if (listBox == null) return;

            if (listBox.Name == "lb")
            {
                lb2.SelectedIndex = -1;
            }

            else if (listBox.Name == "lb2")
            {
                lb.SelectedIndex = -1;

            }

        }

        private void BtnRunReport_OnClick(object sender, RoutedEventArgs e)
        {

            if (lb.SelectedIndex > -1)
            {
                Lb_OnMouseDoubleClick(lb, null);
            }
            else if (lb2.SelectedIndex > -1)
            {
                Lb_OnMouseDoubleClick(lb2, null);

            }
            else
            {

                MessageBox.Show("אנא בחר דוח", Constants.mboxHeader, MessageBoxButton.OK, MessageBoxImage.Hand);

                return;

            }

        }

        private void Lb_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //    Debugger.Launch();

            ListBox listBox = sender as ListBox;
            U_CRYSTAL_REPORT cr = listBox.SelectedItem as U_CRYSTAL_REPORT;
            if (cr == null)
            {
                MessageBox.Show("אנא בחר דוח", Constants.mboxHeader, MessageBoxButton.OK, MessageBoxImage.Hand);
                return;
            }

            ReportScreen R = new ReportScreen(dal, cr, _ntlsUser, _ntlsCon);
            if (cr.U_REPORT_PARAMS_USER.Count == 0)
            {
                R.RunReport();
            }
            else
                R.ShowDialog();
        }
        #endregion



    }
    public class Constants
    {
        public static string mboxHeader = "הפקת דוחות";
    }
}



