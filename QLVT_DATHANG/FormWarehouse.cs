﻿using DevExpress.XtraEditors;
using DevExpress.XtraLayout.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TreeView;

namespace QLVT_DATHANG
{
    public partial class FormWarehouse : DevExpress.XtraEditors.XtraForm
    {
        public string BranchId = ""; // When combox index changes, record branch ID
        public int RowIndex = 0; // When recovering deleted row, insert that row to this index (like never far away)
        public object NewRow;
        public bool IsAdding;
        public FormWarehouse()
        {
            InitializeComponent();
        }

        private void khoBindingNavigatorSaveItem_Click(object sender, EventArgs e)
        {
            this.Validate();
            this.bds_Kho.EndEdit();
            this.tableAdapterManager.UpdateAll(this.DS);

        }
        private void GetBranchId()
        {
            if (bds_Kho.Count > 0)
                BranchId = ((DataRowView)bds_Kho[0])["MaCN"].ToString();
            else
            {
                var reader = Program.ExecSqlDataReader("SELECT MaCN FROM [Kho]");
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
            RowIndex = bds_Kho.Position;
            gpc_info.Enabled = true;
            gdc_Kho.Enabled = false;

            txt_branchId.Text = BranchId;
            btn_add.Enabled = btn_edit.Enabled = btn_delete.Enabled = btn_reload.Enabled = false;
            btn_save.Enabled = btn_undo.Enabled = true;

            txt_branchId.Enabled = false;
        }
        private void TurnOffEditingState()
        {

            gdc_Kho.Enabled = true;
            gpc_info.Enabled = false;

            btn_add.Enabled = btn_edit.Enabled = btn_delete.Enabled = btn_reload.Enabled = true;
            btn_save.Enabled = btn_undo.Enabled = false;

            IsAdding = false;
        }
        private void FormWarehouse2_Load(object sender, EventArgs e)
        {
            DS.EnforceConstraints = false;

            tbla_Kho.Connection.ConnectionString = Program.ConnectionString;
            this.tbla_Kho.Fill(this.DS.Kho);
            tbla_DatHang.Connection.ConnectionString = Program.ConnectionString;
            this.tbla_DatHang.Fill(this.DS.DatHang);
            tbla_PhieuNhap.Connection.ConnectionString = Program.ConnectionString;
            this.tbla_PhieuNhap.Fill(this.DS.PhieuNhap);
            tbla_PhieuXuat.Connection.ConnectionString = Program.ConnectionString;
            this.tbla_PhieuXuat.Fill(this.DS.PhieuXuat);

            GetBranchId();

            cb_branch.DropDownStyle = ComboBoxStyle.DropDownList;
            cb_branch.DataSource = Program.bds_subscriptionList;
            cb_branch.DisplayMember = "TenCN";
            cb_branch.ValueMember = "TenServer";
            cb_branch.SelectedIndex = Program.SubsIndex;

            gpc_info.Enabled = false;

            if (Program.Role == "CongTy")
            {
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
            TurnOnEditingState();
            NewRow = bds_Kho.AddNew();
            IsAdding = true;

            txt_branchId.Text = BranchId;
        }

        private void btn_edit_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            TurnOnEditingState();
        }

        private void btn_delete_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            string warehouseId = "";
            MessageBox.Show(bds_DatHang.Count + " - " + bds_PhieuNhap.Count + " - " + bds_PhieuXuat.Count);
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
                    warehouseId = ((DataRowView)bds_Kho[bds_Kho.Position])["MaKho"].ToString();
                    bds_Kho.RemoveCurrent();

                    tbla_Kho.Connection.ConnectionString = Program.ConnectionString;
                    tbla_Kho.Update(DS.Kho);
                }
                catch (Exception ex) // There maybe it's deleted it in UI but not in DB -> Re fill the UI
                {
                    MessageBox.Show("Error when deleting this employee. Please delete again" + ex.Message, "", MessageBoxButtons.OK); // Sometimes, computers are crazy so ... 
                    tbla_Kho.Fill(DS.Kho);
                    bds_Kho.Position = bds_Kho.Find("MaKho", warehouseId); // Jump to Employee position
                    return;
                }
            }

            // Strict constraint -> If there are no employees, Cannot Delete
            if (bds_Kho.Count == 0)
                btn_delete.Enabled = false;
        }

        private void btn_save_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (txt_id.Text.Trim() == "")
            {
                MessageBox.Show("Cannot blank Warehouse ID!", "", MessageBoxButtons.OK);
                txt_id.Focus();
                return;
            }
            if (txt_name.Text.Trim() == "")
            {
                MessageBox.Show("Cannot blank Warehouse Name!", "", MessageBoxButtons.OK);
                txt_name.Focus();
                return;
            }
            if (txt_address.Text.Trim() == "")
            {
                MessageBox.Show("Cannot blank Warehouse Address!", "", MessageBoxButtons.OK);
                txt_address.Focus();
                return;
            }

            try
            {
                bds_Kho.EndEdit();
                bds_Kho.ResetCurrentItem();

                tbla_Kho.Connection.ConnectionString = Program.ConnectionString;
                tbla_Kho.Update(DS.Kho);
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
            bds_Kho.CancelEdit(); 
            if (IsAdding)
            {
                var res = bds_Kho.Contains(NewRow);
                if (res)
                    bds_Kho.Remove(NewRow);
                IsAdding = false;
            }
            if (btn_add.Enabled == false) // When adding or editing, this button will be unabled
                bds_Kho.Position = RowIndex;

            TurnOffEditingState();
        }

        private void btn_reload_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                // Refill to Table NhanVien
                tbla_Kho.Fill(DS.Kho);
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

            tbla_Kho.Connection.ConnectionString = Program.ConnectionString;
            this.tbla_Kho.Fill(this.DS.Kho);
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