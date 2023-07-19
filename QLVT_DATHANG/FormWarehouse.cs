using DevExpress.XtraEditors;
using QLVT_DATHANG.DSEmployeeTableAdapters;
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
    public partial class FormWarehouse : DevExpress.XtraEditors.XtraForm
    {
        public string BranchId = ""; // When combox index changes, record branch ID
        public int RowIndex = 0; // When recovering deleted row, insert that row to this index (like never far away)
        public object NewRow;
        public FormWarehouse()
        {
            InitializeComponent();
        }

        private void khoBindingNavigatorSaveItem_Click(object sender, EventArgs e)
        {
            this.Validate();
            this.bds_kho.EndEdit();
            this.tableAdapterManager.UpdateAll(this.ds_warehouse);

        }
        private void GetBranchId()
        {
            if (bds_kho.Count > 0)
                BranchId = ((DataRowView)bds_kho[0])["MaCN"].ToString();
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
        private void FormWarehouse_Load(object sender, EventArgs e)
        { 
            ds_warehouse.EnforceConstraints = false;

            tbla_kho.Connection.ConnectionString = Program.ConnectionString;
            this.tbla_kho.Fill(this.ds_warehouse.Kho);
            tbla_datHang.Connection.ConnectionString = Program.ConnectionString;
            this.tbla_datHang.Fill(this.ds_warehouse.DatHang);
            tbla_phieuNhap.Connection.ConnectionString = Program.ConnectionString;
            this.tbla_phieuNhap.Fill(this.ds_warehouse.PhieuNhap);
            tbla_phieuXuat.Connection.ConnectionString = Program.ConnectionString;
            this.tbla_phieuXuat.Fill(this.ds_warehouse.PhieuXuat);

            GetBranchId();

            if (Program.Role == "CongTy")
            {
                btn_add.Enabled = btn_delete.Enabled = btn_edit.Enabled = btn_undo.Enabled = btn_save.Enabled = false;
            }
            else
            {
                btn_undo.Enabled = btn_save.Enabled = false;
                btn_add.Enabled = btn_delete.Enabled = btn_edit.Enabled = true;
            }
        }

        private void btn_add_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            RowIndex = bds_kho.Position;
            gb_info.Enabled = true;
            NewRow = bds_kho.AddNew();
            gc_kho.Enabled = false;

            txt_branchId.Text = BranchId;
            btn_add.Enabled = btn_edit.Enabled = btn_delete.Enabled = btn_reload.Enabled = false;
            btn_save.Enabled = btn_undo.Enabled = true;
        }

        private void btn_edit_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            RowIndex = bds_kho.Position;
            gb_info.Enabled = true;
            gc_kho.Enabled = false;

            btn_add.Enabled = btn_edit.Enabled = btn_delete.Enabled = btn_reload.Enabled = false;
            btn_save.Enabled = btn_undo.Enabled = true;
        }

        private void btn_delete_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            string warehouseId = "";
            MessageBox.Show(bds_datHang.Count + " - " + bds_phieuNhap.Count + " - " + bds_phieuXuat.Count);
            if (bds_datHang.Count > 0 || bds_phieuNhap.Count > 0 || bds_phieuXuat.Count > 0)
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
                    warehouseId = ((DataRowView)bds_kho[bds_kho.Position])["MaKho"].ToString();
                    bds_kho.RemoveCurrent();

                    tbla_kho.Connection.ConnectionString = Program.ConnectionString;
                    tbla_kho.Update(ds_warehouse.Kho);
                }
                catch (Exception ex) // There maybe it's deleted it in UI but not in DB -> Re fill the UI
                {
                    MessageBox.Show("Error when deleting this employee. Please delete again" + ex.Message, "", MessageBoxButtons.OK); // Sometimes, computers are crazy so ... 
                    tbla_kho.Fill(ds_warehouse.Kho);
                    bds_kho.Position = bds_kho.Find("MaKho", warehouseId); // Jump to Employee position
                    return;
                }
            }

            // Strict constraint -> If there are no employees, Cannot Delete
            if (bds_kho.Count == 0)
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
                bds_kho.EndEdit();
                bds_kho.ResetCurrentItem();

                tbla_kho.Connection.ConnectionString = Program.ConnectionString;
                tbla_kho.Update(ds_warehouse.Kho);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error when adding Employee!" + ex.Message, "Error", MessageBoxButtons.OK);
                return;
            }

            gc_kho.Enabled = true;
            gb_info.Enabled = false;

            btn_add.Enabled = btn_edit.Enabled = btn_delete.Enabled = btn_reload.Enabled = true;
            btn_save.Enabled = btn_undo.Enabled = false;
        }

        private void btn_undo_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            bds_kho.CancelEdit();
            if (NewRow != null)
                bds_kho.Remove(NewRow);

            if (btn_add.Enabled == false) // When adding or editing, this button will be unabled
                bds_kho.Position = RowIndex;

            gc_kho.Enabled = true;
            gb_info.Enabled = false;

            btn_add.Enabled = btn_edit.Enabled = btn_delete.Enabled = btn_reload.Enabled = true;
            btn_save.Enabled = btn_undo.Enabled = false;
        }

        private void btn_reload_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                // Refill to Table NhanVien
                tbla_kho.Fill(ds_warehouse.Kho);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error when reloading: " + ex.Message, "", MessageBoxButtons.OK);
                return;
            }
        }
    }
}