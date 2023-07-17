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

        private void btnLogout_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {

            var formMain = Program.FormMain;
            formMain.page_business.Visible = 
                formMain.page_cat.Visible = 
                formMain.page_report.Visible = false;

            formMain.btn_logout.Visibility = DevExpress.XtraBars.BarItemVisibility.OnlyInCustomizing;
            formMain.btn_login.Visibility = DevExpress.XtraBars.BarItemVisibility.OnlyInRuntime;

            WindowState = FormWindowState.Normal;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            foreach (var item in formMain.MdiChildren)
            {
                item.Close();
            }

            LoadLoginForm();
        }

        private void LoadEmployeeForm()
        {
            var form = CheckExists(typeof(FormEmployee));
            if (form != null)
                form.Activate();
            else
            {
                var fEmployee = new FormEmployee();
                fEmployee.MdiParent = this;
                fEmployee.Show();
            }
        }

        private void btn_employee_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            LoadEmployeeForm();
        }
    }
}
