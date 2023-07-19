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

        public FormEmployee()
        {
            InitializeComponent();
        }

        private void nhanVienBindingNavigatorSaveItem_Click(object sender, EventArgs e)
        {
            this.Validate();
            this.bds_NhanVien.EndEdit();
            this.TableAdapterManager.UpdateAll(this.ds_Employee);
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

        private void FormEmployee_Load(object sender, EventArgs e)
        {
            // 1. The reason we disable enforece constraints because there is not only
            // Employee Table but also others Tables are foreign tables (constraints)
            ds_Employee.EnforceConstraints = false;

            // 2. Connection String in App.config just to generate Dataset
            // This connection string is the real one. 
            // We have logged in which loginname, we'll use that loginname
            NhanVienTableAdapter.Connection.ConnectionString = Program.ConnectionString;
                this.NhanVienTableAdapter.Fill(this.ds_Employee.NhanVien);
            datHangTableAdapter.Connection.ConnectionString = Program.ConnectionString;
                this.datHangTableAdapter.Fill(this.ds_Employee.DatHang);
            phieuXuatTableAdapter.Connection.ConnectionString = Program.ConnectionString;
                this.phieuXuatTableAdapter.Fill(this.ds_Employee.PhieuXuat);
            phieuNhapTableAdapter.Connection.ConnectionString = Program.ConnectionString;
                this.phieuNhapTableAdapter.Fill(this.ds_Employee.PhieuNhap);

            // 3. We have to store Branch ID (which we logged in)
            // NOTE: This may occur an error, YOU HAVE TO FIND OUT AND FIX IT!!!
            // I think when there is no employee, it will cause an error!
            GetBranchId();

            // 4. Load subs into combobox
            cb_branch.DataSource = Program.bds_subscriptionList;
            cb_branch.DisplayMember = "TenCN";
            cb_branch.ValueMember = "TenServer";
            cb_branch.SelectedIndex = Program.SubsIndex;

            // 5. Decentralization
            if (Program.Role == "CongTy")
            {
                cb_branch.Enabled = true;
                btn_add.Enabled = btn_delete.Enabled = btn_edit.Enabled = btn_undo.Enabled = btn_save.Enabled = false;
            }
            else
            {
                cb_branch.Enabled = btn_undo.Enabled = btn_save.Enabled = false;
                btn_add.Enabled = btn_delete.Enabled = btn_edit.Enabled = true;
            }
        }

        private void btn_add_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            RowIndex = bds_NhanVien.Position;
            gb_info.Enabled = true;
            NewRow = bds_NhanVien.AddNew();
            gc_NhanVien.Enabled = false; // Prevent when adding, click to a row, row fill to gb_info

            txt_branchId.Text = BranchId;
            dte_birthday.EditValue = "";

            btn_add.Enabled = btn_edit.Enabled = btn_delete.Enabled = btn_reload.Enabled = false;
            btn_save.Enabled = btn_undo.Enabled = true;
            // Grid View is View, Interface
            // Grid Control is Data
        }

        private void btn_undo_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        { // Have to functions: Cancel Adding, Cancel Editing

            bds_NhanVien.CancelEdit();
            if (NewRow != null && bds_NhanVien.Contains(NewRow))
                bds_NhanVien.Remove(NewRow);

            if (btn_add.Enabled == false) // When adding or editing, this button will be unabled
                bds_NhanVien.Position = RowIndex;

            gc_NhanVien.Enabled = true;
            gb_info.Enabled = false;

            btn_add.Enabled = btn_edit.Enabled = btn_delete.Enabled = btn_reload.Enabled = true;
            btn_save.Enabled = btn_undo.Enabled = false;
        }

        private void btn_edit_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            RowIndex = bds_NhanVien.Position;
            gb_info.Enabled = true;
            gc_NhanVien.Enabled = false;

            btn_add.Enabled = btn_edit.Enabled = btn_delete.Enabled = btn_reload.Enabled = false;
            btn_save.Enabled = btn_undo.Enabled = true;
        }

        private void btn_reload_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                // Refill to Table NhanVien
                NhanVienTableAdapter.Fill(ds_Employee.NhanVien);
            }
            catch (Exception ex) {
                MessageBox.Show("Error when reloading: " + ex.Message, "", MessageBoxButtons.OK);
                return;
            }
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

                    NhanVienTableAdapter.Connection.ConnectionString = Program.ConnectionString;
                    NhanVienTableAdapter.Update(ds_Employee.NhanVien);
                }
                catch (Exception ex) // There maybe it's deleted it in UI but not in DB -> Re fill the UI
                {
                    MessageBox.Show("Error when deleting this employee. Please delete again" + ex.Message, "", MessageBoxButtons.OK); // Sometimes, computers are crazy so ... 
                    NhanVienTableAdapter.Fill(ds_Employee.NhanVien);
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

                NhanVienTableAdapter.Connection.ConnectionString = Program.ConnectionString;
                NhanVienTableAdapter.Update(ds_Employee.NhanVien);
            }
            catch (Exception ex) 
            {
                MessageBox.Show("Error when adding Employee!" + ex.Message, "Error", MessageBoxButtons.OK);
                return;
            }
            NewRow = null;

            gc_NhanVien.Enabled = true;
            gb_info.Enabled = false;

            btn_add.Enabled = btn_edit.Enabled = btn_delete.Enabled = btn_reload.Enabled = true;
            btn_save.Enabled = btn_undo.Enabled = false;
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

            NhanVienTableAdapter.Connection.ConnectionString = Program.ConnectionString;
                NhanVienTableAdapter.Fill(ds_Employee.NhanVien);
            datHangTableAdapter.Connection.ConnectionString = Program.ConnectionString;
                datHangTableAdapter.Fill(ds_Employee.DatHang);
            phieuNhapTableAdapter.Connection.ConnectionString = Program.ConnectionString;
                phieuNhapTableAdapter.Fill(ds_Employee.PhieuNhap);
            phieuXuatTableAdapter.Connection.ConnectionString = Program.ConnectionString;
                phieuXuatTableAdapter.Fill(ds_Employee.PhieuXuat);
            
            GetBranchId(); // Duplication. Because Role Branch can edit data.
            // Role Company cannot edit data and branchid is just used to add data.
            // The reason it's here that is some subjects not only change branch but also update data.
        }
    }
}