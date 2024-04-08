using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Documents;
using LSExtensionWindowLib;
using LSSERVICEPROVIDERLib;
using Patholab_Common;
using System.Runtime.InteropServices;
using System.Windows.Forms;

using Patholab_DAL_V1;


using System.Diagnostics;


namespace CrystalReportsV1
{


    [ComVisible(true)]
    [ProgId("CrystalReportsV1.CrystalReportsCls")]
    public partial class CrystalReportsCls : UserControl, IExtensionWindow
    {
        #region Private fields
        private INautilusProcessXML xmlProcessor;
        private INautilusUser _ntlsUser;
        private IExtensionWindowSite2 _ntlsSite;
        private INautilusServiceProvider sp;
        private INautilusDBConnection _ntlsCon;


        public bool DEBUG = true;

        #endregion

        #region Ctor

        public CrystalReportsCls()
        {
            InitializeComponent();
            BackColor = Color.FromName("Control");
        }


        #endregion




        public bool CloseQuery()
        {

            w.CloseQuery();
            return true;
        }

        public void Internationalise()
        {
        }

        public void SetSite(object site)
        {
            _ntlsSite = (IExtensionWindowSite2)site;
            _ntlsSite.SetWindowInternalName("הפקת דוחות");
            _ntlsSite.SetWindowRegistryName("הפקת דוחות");
            _ntlsSite.SetWindowTitle("הפקת דוחות");
        }


        private CrystalReportsCtl w;
        public void PreDisplay()
        {

            xmlProcessor = Utils.GetXmlProcessor(sp);

            _ntlsUser = Utils.GetNautilusUser(sp);

            w = new CrystalReportsCtl(sp, xmlProcessor, _ntlsCon, _ntlsSite, _ntlsUser);
            elementHost1.Child = w;
            w.DEBUG = false;

            w.InitializeData();
        }

        public WindowButtonsType GetButtons()
        {
            return LSExtensionWindowLib.WindowButtonsType.windowButtonsNone;
        }

        public bool SaveData()
        {
            return false;
        }

        public void SetServiceProvider(object serviceProvider)
        {
            sp = serviceProvider as NautilusServiceProvider;
            _ntlsCon = Utils.GetNtlsCon(sp);

        }

        public void SetParameters(string parameters)
        {

        }

        public void Setup()
        {

        }

        public WindowRefreshType DataChange()
        {
            return LSExtensionWindowLib.WindowRefreshType.windowRefreshNone;
        }

        public WindowRefreshType ViewRefresh()
        {
            return LSExtensionWindowLib.WindowRefreshType.windowRefreshNone;
        }

        public void refresh()
        {
        }

        public void SaveSettings(int hKey)
        {
        }

        public void RestoreSettings(int hKey)
        {
        }

        public void Close()
        {

        }














    }




}



