﻿using DevExpress.XtraEditors;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QLVT_DATHANG
{
    public partial class FormLogin : DevExpress.XtraEditors.XtraForm
    {
        private SqlConnection PublisherConnection = new SqlConnection();
        private readonly string PublisherConnString = "Server=localhost; Database=QLVT_DATHANG; Integrated Security=True";
        private readonly string SubscriptionsViewName = "V_DS_PhanManh";
        private readonly string SPLogin = "dbo.SP_DANGNHAP";

        // 2. Get Subscriptions from Publisher
        private bool ConnectToPublisher()
        {
            if (PublisherConnection != null && PublisherConnection.State == ConnectionState.Open)
                PublisherConnection.Close();
            try
            {
                PublisherConnection.ConnectionString = PublisherConnString;
                PublisherConnection.Open();
                return true;
            }
            catch
            {
                MessageBox.Show("Error when connecting to Publisher Server.\nPlease check your Server name & Database in connection string");
                return false;
            }
        }
        private void LoadSubscriptionsToCombobox()
        {
            // Stop program when throwing connecting to server error 
            if (!ConnectToPublisher())
                return;

            var dataTable = new DataTable();
            if (PublisherConnection.State == ConnectionState.Closed)
                PublisherConnection.Open();

            var adapter = new SqlDataAdapter($"SELECT TOP 2 * FROM {SubscriptionsViewName}", PublisherConnection);
            adapter.Fill(dataTable);
            PublisherConnection.Close();

            Program.BSSubscriptionList.DataSource = dataTable;

            cb_branch.DataSource = dataTable;
            cb_branch.DisplayMember = "TenCN";
            cb_branch.ValueMember = "TenServer";

            cb_branch.SelectedIndex = 1;
            cb_branch.SelectedIndex = 0;
        }
        public FormLogin()
        {
            InitializeComponent();
        }

        private void FormLogin_Load(object sender, EventArgs e)
        {
            LoadSubscriptionsToCombobox();
        }

        // 3. Event when Branch Value changes in Combobox
        private void cb_branch_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                Program.ServerName = cb_branch.SelectedValue.ToString();
            }
            catch
            {
                Console.WriteLine("Error when loading global Server name");
            }
        }

        // 4. Handle Login
        private bool HandleLogin()
        {
            Program.UserId = txb_username.Text;
            Program.Password = txb_password.Text;

            // Open connection
            if (!Program.LoginToServer()) // Stop program when Logging in failed!
                return false;

            // When Uid & password are correct!
            var cmdExecSP = $"EXEC {SPLogin} '{Program.UserId}'";
            Program.Reader = Program.ExecSqlDataReader(cmdExecSP);
            if (Program.Reader == null)
                return false;

            Program.Reader.Read();
            Program.EmployeeId = Program.Reader[0].ToString();
            Program.FullName = Program.Reader[1].ToString();
            Program.Role = Program.Reader[2].ToString();

            Program.Reader.Close();
            Program.Connection.Close();

            if (Convert.IsDBNull(Program.EmployeeId)) // ???
            {
                MessageBox.Show("This login has been forbidden.\nPlease check your username & password.", "", MessageBoxButtons.OK);
                return false;
            }

            return true;
        }

        private void btn_login_Click(object sender, EventArgs e)
        {
            if (txb_username.Text.Trim() == "" || txb_password.Text.Trim() == "")
            {
                MessageBox.Show("Cannot blank username or password");
                return;
            }

            bool loginResult = HandleLogin();
            if (!loginResult)
                return;
 
            Program.FormMain.Show();
            Program.FormMain.MaNV.Text = "Employee ID: " + Program.EmployeeId.ToString();
            Program.FormMain.HoTen.Text = "Full name: " + Program.FullName.ToString();
            Program.FormMain.Nhom.Text = "Role: " + Program.Role.ToString();
        }
    }
}