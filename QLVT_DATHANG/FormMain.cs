using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace QLVT_DATHANG
{
    public partial class FormMain : DevExpress.XtraBars.Ribbon.RibbonForm
    {
        public FormMain()
        {
            InitializeComponent();
        }
        // 1. Load Form
        private Form CheckExists(Type formType)
        {
            foreach (var form in this.MdiChildren)
            {
                if (form.GetType() == formType)
                    form.Show();
                    return form;
            }
            return null;
        }

        private void LoadLoginForm()
        {
            var form = CheckExists(typeof(FormLogin));
            if (form != null)
                form.Activate();
            else
            {
                var fLogin = new FormLogin();
                fLogin.MdiParent = this;
                WindowState = FormWindowState.Normal;
                Dock = DockStyle.Fill;
                fLogin.Show();
            }
        }

        private void btnLogin_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            LoadLoginForm();
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            LoadLoginForm();
            
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            
        }
         
    }
}
