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
        public int RowIndex = 0; // When recover deleted row, insert that row to this index (like never far away)

        public FormEmployee()
        {
            InitializeComponent();
        }

        private void nhanVienBindingNavigatorSaveItem_Click(object sender, EventArgs e)
        {
            this.Validate();
            this.bds_NhanVien.EndEdit();
            this.TableAdapterManager.UpdateAll(this.DSEmployee);

        }

        private void FormEmployee_Load(object sender, EventArgs e)
        {
            // 1. The reason we disable enforece constraints because there is not only
            // Employee Table but also others Tables are foreign tables (constraints)
            DSEmployee.EnforceConstraints = false;

            // 2. Connection String in App.config just to generate Dataset
            // This connection string is the real one. 
            // We have logged in which loginname, we'll use that loginname
            NhanVienTableAdapter.Connection.ConnectionString = Program.ConnectionString;
                this.NhanVienTableAdapter.Fill(this.DSEmployee.NhanVien);
            datHangTableAdapter.Connection.ConnectionString = Program.ConnectionString;
                this.datHangTableAdapter.Fill(this.DSEmployee.DatHang);
            phieuXuatTableAdapter.Connection.ConnectionString = Program.ConnectionString;
                this.phieuXuatTableAdapter.Fill(this.DSEmployee.PhieuXuat);
            phieuNhapTableAdapter.Connection.ConnectionString = Program.ConnectionString;
                this.phieuNhapTableAdapter.Fill(this.DSEmployee.PhieuNhap);

            // 3. We have to store Branch ID (which we logged in)
            // NOTE: This may occur an error, YOU HAVE TO FIND OUT AND FIX IT!!!
            BranchId = ((DataRowView)bds_NhanVien[0])["MaCN"].ToString();

            // Load subs into combobox
            cb_branch.DataSource = Program.bds_subscriptionList;
            cb_branch.DisplayMember = "TenCN";
            cb_branch.ValueMember = "TenServer";
            cb_branch.SelectedIndex = Program.SubsIndex;

            // Just Company Role can change Branch
            if (Program.Role == "CongTy")
            {
                cb_branch.Enabled = true;
                cb_branch.Enabled =
                    btn_add.Enabled =
                    btn_delete.Enabled =
                    btn_edit.Enabled =
                    btn_undo.Enabled =
                    btn_save.Enabled = false;
            }
            else
            {
                cb_branch.Enabled =
                    btn_add.Enabled =
                    btn_delete.Enabled =
                    btn_edit.Enabled =
                    btn_undo.Enabled =
                    btn_save.Enabled = true;
            }
        }

        private void btn_add_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            RowIndex = bds_NhanVien.Position;
            // Group box now can can be editable
            gb_info.Enabled = true;

            bds_NhanVien.AddNew();
            txt_branchId.Text = BranchId;
            dte_birthday.EditValue = "";

            btn_add.Enabled =
                btn_edit.Enabled =
                btn_delete.Enabled =
                btn_reload.Enabled = false;
            btn_save.Enabled = btn_undo.Enabled = true;
            gc_NhanVien.Enabled = true;

            // Grid View is View, Interface
            // Grid Control is Data
        }

        private void btn_undo_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        { // Have to functions: Undo Adding, Undo Editing
            bds_NhanVien.CancelEdit();

            if (btn_add.Enabled == false)
                bds_NhanVien.Position = RowIndex;

            gc_NhanVien.Enabled = true;
            gb_info.Enabled = false;

            btn_add.Enabled =
                btn_edit.Enabled =
                btn_delete.Enabled =
                btn_reload.Enabled = true;
            btn_save.Enabled = btn_undo.Enabled = false;
        }

        private void btn_edit_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            RowIndex = bds_NhanVien.Position;
            gb_info.Enabled = true;

            btn_add.Enabled =
                btn_edit.Enabled =
                btn_delete.Enabled =
                btn_reload.Enabled = false;
            btn_save.Enabled = btn_undo.Enabled = true;
            gc_NhanVien.Enabled = false;
        }

        private void btn_reload_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                // Refill to Table NhanVien
                // NhanVienTableAdapter.Connection.ConnectionString = Program.ConnectionString;
                NhanVienTableAdapter.Fill(DSEmployee.NhanVien);
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

            if (bds_DatHang.Count > 0 || bds_PhieuNhap.Count > 0 || bds_PhieuXuat.Count > 0) // There is also another one (phieuXuat)
            {
                MessageBox.Show(
                    "This employee cannot be deleted because this employee has created Import Order, Import Invoice or Export Invoice!",
                    "", MessageBoxButtons.OK);
                return;
            }

            var deleteConfirm = MessageBox.Show("Are you sure to delete this employee?", "Confirm", MessageBoxButtons.OKCancel);
            if (deleteConfirm == DialogResult.OK)
            {
                try
                {
                    var idData = ((DataRowView)bds_NhanVien[bds_NhanVien.Position])["MaNV"].ToString();
                    EmployeeId = int.Parse(idData);
                    NhanVienTableAdapter.Connection.ConnectionString = Program.ConnectionString;
                    NhanVienTableAdapter.Update(DSEmployee.NhanVien);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error when deleting this employee. Please delete again" + ex.Message, "", MessageBoxButtons.OK);
                    NhanVienTableAdapter.Fill(DSEmployee.NhanVien);
                    bds_NhanVien.Position = bds_NhanVien.Find("MaNV", EmployeeId);
                    return;
                }
            }

            if (bds_NhanVien.Count == 0)
                btn_delete.Enabled = false;
        }
    }
}