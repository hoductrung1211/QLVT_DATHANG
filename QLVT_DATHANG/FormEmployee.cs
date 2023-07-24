using DevExpress.XtraEditors;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QLVT_DATHANG
{
    public partial class FormEmployee : DevExpress.XtraEditors.XtraForm
    {
        public string BranchId = ""; // When combox index changes, record branch ID
        public int RowIndex = 0; // When recovering deleted row, insert that row to this index (like never far away)
        public object NewRow;
        public bool IsAdding = false;
        public FormEmployee()
        {
            InitializeComponent();
        }

        private void nhanVienBindingNavigatorSaveItem_Click(object sender, EventArgs e)
        {
            this.Validate();
            this.bds_NhanVien.EndEdit();
            this.tableAdapterManager.UpdateAll(this.DS);

        }
        private void GetBranchId()
        {
            if (bds_NhanVien.Count > 0)
                BranchId = ((DataRowView)bds_NhanVien[0])["MaCN"].ToString();
            else
            {
                var reader = Program.ExecSqlDataReader("SELECT MaCN FROM ChiNhanh");
                if (reader == null)
                {
                    MessageBox.Show("Error when trying to get the current branch.", "Error", MessageBoxButtons.OK);
                    return;
                }
                reader.Read();
                BranchId = reader[0].ToString();
            }
        }
         
        private void TurnOnEditingState()
        {
            RowIndex = bds_NhanVien.Position;
            gpc_info.Enabled = true;
            gdc_NhanVien.Enabled = false; // Prevent when adding, click to a row, row fill to gb_info

            btn_add.Enabled = btn_edit.Enabled = btn_delete.Enabled = btn_reload.Enabled = false;
            btn_save.Enabled = btn_undo.Enabled = true;
            // Grid View is View, Interface
            // Grid Control is Data
        }
        private void TurnOffEditingState()
        {
            gdc_NhanVien.Enabled = true;
            gpc_info.Enabled = false;

            btn_add.Enabled = btn_edit.Enabled = btn_delete.Enabled = btn_reload.Enabled = true;
            btn_save.Enabled = btn_undo.Enabled = false;
            IsAdding = false;
        }
        private void FormEmployee2_Load(object sender, EventArgs e)
        {
            // 1. The reason we disable enforece constraints because there is not only
            // Employee Table but also others Tables are foreign tables (constraints)
            DS.EnforceConstraints = false;

            // 2. Connection String in App.config just to generate Dataset
            // This connection string is the real one. 
            // We have logged in which loginname, we'll use that loginname
            tbla_NhanVien.Connection.ConnectionString = Program.ConnectionString;
            this.tbla_NhanVien.Fill(this.DS.NhanVien);
            tbla_DatHang.Connection.ConnectionString = Program.ConnectionString;
            this.tbla_DatHang.Fill(this.DS.DatHang);
            tbla_PhieuNhap.Connection.ConnectionString = Program.ConnectionString;
            this.tbla_PhieuNhap.Fill(this.DS.PhieuNhap);
            tbla_PhieuXuat.Connection.ConnectionString = Program.ConnectionString;
            this.tbla_PhieuXuat.Fill(this.DS.PhieuXuat);

            // 3. We have to store Branch ID (which we logged in)
            // NOTE: This may occur an error, YOU HAVE TO FIND OUT AND FIX IT!!!
            // I think when there is no employee, it will cause an error!
            GetBranchId();

            // 4. Load subs into combobox
            cb_branch.DropDownStyle = ComboBoxStyle.DropDownList;
            cb_branch.DataSource = Program.bds_subscriptionList;
            cb_branch.DisplayMember = "TenCN";
            cb_branch.ValueMember = "TenServer";
            cb_branch.SelectedIndex = Program.SubsIndex;

            dte_birthday.Properties.EditMask = "dd/MM/yyyy";
            dte_birthday.Properties.UseMaskAsDisplayFormat = true;

            colNgaySinh.DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime; 
            colNgaySinh.DisplayFormat.FormatString = "dd/MM/yyyy";

            gpc_info.Enabled = false;

            // 5. Decentralization
            if (Program.Role == "CongTy")
            {
                cb_branch.Enabled = true;
                gpc_info.Enabled = btn_add.Enabled = btn_delete.Enabled = btn_edit.Enabled = btn_undo.Enabled = btn_save.Enabled = false;
            }
            else
            {
                cb_branch.Enabled = btn_undo.Enabled = btn_save.Enabled = false;
                btn_add.Enabled = btn_delete.Enabled = btn_edit.Enabled = true;
            }
        }

        private void btn_add_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            TurnOnEditingState();
            NewRow = bds_NhanVien.AddNew();
            IsAdding = true;

            txt_branchId.Text = BranchId;
            dte_birthday.EditValue = "01/01/2000";
            txt_branchId.Enabled = false;
            cb_deleted.Checked = false;
            cb_deleted.Enabled = false;
        }

        private void btn_edit_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            TurnOnEditingState(); 
            cb_deleted.Enabled = true;
            txt_id.Enabled = false;
        }

        private void btn_delete_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            // 1. Check wheather this Employee can be deleted
            // A row can not be deleted if it's referenced to another Table (it's a FK)
            int EmployeeId = 0;

            if (bds_DatHang.Count > 0 || bds_PhieuNhap.Count > 0 || bds_PhieuXuat.Count > 0)
            {
                MessageBox.Show(
                    "This employee cannot be deleted because this employee has created Import Order or Import Invoice or Export Invoice!",
                    "", MessageBoxButtons.OK);
                return;
            }

            var deleteConfirm = MessageBox.Show("Are you sure to delete this employee?", "Confirm", MessageBoxButtons.OKCancel);
            if (deleteConfirm == DialogResult.OK)
            {
                try
                {
                    var idData = ((DataRowView)bds_NhanVien[bds_NhanVien.Position])["MaNV"].ToString();
                    EmployeeId = int.Parse(idData); // Store employeeID to roll back to this Employee position
                    bds_NhanVien.RemoveCurrent();

                    tbla_NhanVien.Connection.ConnectionString = Program.ConnectionString;
                    tbla_NhanVien.Update(DS.NhanVien);
                }
                catch (Exception ex) // There maybe it's deleted it in UI but not in DB -> Re fill the UI
                {
                    MessageBox.Show("Error when deleting this employee. Please delete again" + ex.Message, "", MessageBoxButtons.OK); // Sometimes, computers are crazy so ... 
                    tbla_NhanVien.Fill(DS.NhanVien);
                    bds_NhanVien.Position = bds_NhanVien.Find("MaNV", EmployeeId); // Jump to Employee position
                    return;
                }
            }

            // Strict constraint -> If there are no employees, Cannot Delete
            if (bds_NhanVien.Count == 0)
                btn_delete.Enabled = false;
        }

        private void btn_save_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (txt_id.Text.Trim() == "")
            {
                MessageBox.Show("Cannot blank Employee ID!", "", MessageBoxButtons.OK);
                txt_id.Focus();
                return;
            }
            if (txt_lastname.Text.Trim() == "")
            {
                MessageBox.Show("Cannot blank Employee last name!", "", MessageBoxButtons.OK);
                txt_lastname.Focus();
                return;
            }
            if (txt_firstname.Text.Trim() == "")
            {
                MessageBox.Show("Cannot blank Employee first name!", "", MessageBoxButtons.OK);
                txt_firstname.Focus();
                return;
            }
            // Please add BirthDate
            // Please add Salary Value Constraint in a domain

            // Employee ID cannot be the same in all subs

            try
            {
                bds_NhanVien.EndEdit();
                bds_NhanVien.ResetCurrentItem();

                tbla_NhanVien.Connection.ConnectionString = Program.ConnectionString;
                tbla_NhanVien.Update(DS.NhanVien);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error when adding Employee!" + ex.Message, "Error", MessageBoxButtons.OK);
                return;
            }

            TurnOffEditingState();
        }

        private void btn_undo_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            bds_NhanVien.CancelEdit();
            if (IsAdding)
            {
                var res = bds_NhanVien.Contains(NewRow);
                if (res)
                    bds_NhanVien.Remove(NewRow);
                IsAdding = false;
            }
                

            if (btn_add.Enabled == false) // When adding or editing, this button will be unabled
                bds_NhanVien.Position = RowIndex;

            TurnOffEditingState();
        }

        private void btn_reload_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                // Refill to Table NhanVien
                tbla_NhanVien.Fill(DS.NhanVien);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error when reloading: " + ex.Message, "", MessageBoxButtons.OK);
                return;
            }
        }

        private void cb_branch_SelectedIndexChanged(object sender, EventArgs e)
        {
            // If this combobox haven't had records
            if (cb_branch.SelectedValue.ToString() == "System.Data.DataRowView")
                return;

            Program.Server = cb_branch.SelectedValue.ToString();

            if (cb_branch.SelectedIndex != Program.SubsIndex)
            {
                Program.ConnectionUserId = Program.RemoteUserId;
                Program.ConnectionPassword = Program.RemotePassword;
            }
            else
            {
                Program.ConnectionUserId = Program.UserId;
                Program.ConnectionPassword = Program.Password;
            }

            if (!Program.LoginToServer())
            {
                MessageBox.Show("Error when connecting to new branch", "Error", MessageBoxButtons.OK);
                return;
            }

            tbla_NhanVien.Connection.ConnectionString = Program.ConnectionString;
            this.tbla_NhanVien.Fill(this.DS.NhanVien);
            tbla_DatHang.Connection.ConnectionString = Program.ConnectionString;
            this.tbla_DatHang.Fill(this.DS.DatHang);
            tbla_PhieuNhap.Connection.ConnectionString = Program.ConnectionString;
            this.tbla_PhieuNhap.Fill(this.DS.PhieuNhap);
            tbla_PhieuXuat.Connection.ConnectionString = Program.ConnectionString;
            this.tbla_PhieuXuat.Fill(this.DS.PhieuXuat);

            GetBranchId(); // Duplication. Because Role Branch can edit data.
            // Role Company cannot edit data and branchid is just used to add data.
            // The reason it's here that is some subjects not only change branch but also update data.
        }
    }
}