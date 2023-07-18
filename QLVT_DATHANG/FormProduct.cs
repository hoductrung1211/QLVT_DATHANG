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
    public partial class FormProduct : DevExpress.XtraEditors.XtraForm
    {
        public static int RowIndex = 0;
        public static object NewRow;
        public FormProduct()
        {
            InitializeComponent();
        }

        private void vatTuBindingNavigatorSaveItem_Click(object sender, EventArgs e)
        {
            this.Validate();
            this.bds_VatTu.EndEdit();
            this.tableAdapterManager.UpdateAll(this.ds_product);

        }

        private void FormProduct_Load(object sender, EventArgs e)
        {
            ds_product.EnforceConstraints = false;
            tbla_VatTu.Connection.ConnectionString = Program.ConnectionString;
            this.tbla_VatTu.Fill(this.ds_product.VatTu);
            tbla_CTDHH.Connection.ConnectionString = Program.ConnectionString;
            this.tbla_CTDHH.Fill(this.ds_product.CTDDH);
            tbla_CTPN.Connection.ConnectionString = Program.ConnectionString;
            this.tbla_CTPN.Fill(this.ds_product.CTPN);
            tbla_CTPX.Connection.ConnectionString = Program.ConnectionString;
            this.tbla_CTPX.Fill(this.ds_product.CTPX);

            gb_info.Enabled = false;

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
            RowIndex = bds_VatTu.Position;
            gb_info.Enabled = true;
            NewRow = bds_VatTu.AddNew();
            gc_VatTu.Enabled = false; // Have to add new before unabling grid control

            btn_add.Enabled = btn_edit.Enabled = btn_delete.Enabled = btn_reload.Enabled = false;
            btn_save.Enabled = btn_undo.Enabled = true;
        }

        private void btn_edit_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            RowIndex = bds_VatTu.Position;
            gb_info.Enabled = true;
            gc_VatTu.Enabled = false;

            btn_add.Enabled = btn_edit.Enabled = btn_delete.Enabled = btn_reload.Enabled = false;
            btn_save.Enabled = btn_undo.Enabled = true;
        }

        private void btn_delete_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            int productId = 0;

            if (bds_CTDDH.Count > 0 || bds_CTPN.Count > 0 || bds_CTPX.Count > 0)
            {
                MessageBox.Show(
                    "This product cannot be deleted because it has been used in 'Chi Tiet Don Dat Hang' or 'Phieu Nhap' or 'Phieu Xuat'",
                    "Deleting Error",
                    MessageBoxButtons.OK
                );
                return;
            }

            var deletingConfirm = MessageBox.Show(
                "Are you sure to delete this product?", 
                "Deleting Confirm", 
                MessageBoxButtons.OKCancel
               );
            if (deletingConfirm  == DialogResult.OK)
            {
                try
                {
                    var idData = ((DataRowView)bds_VatTu[bds_VatTu.Position])["MaVT"].ToString();
                    productId = Convert.ToInt32(idData);
                    bds_VatTu.RemoveCurrent();

                    tbla_VatTu.Connection.ConnectionString = Program.ConnectionString;
                    tbla_VatTu.Update(ds_product.VatTu);
                }
                catch (Exception ex )
                {
                    MessageBox.Show(
                            "Error when deleting this product. Please delete again. " + ex.Message,
                            "Error",
                            MessageBoxButtons.OK
                        );
                    tbla_VatTu.Fill(ds_product.VatTu);
                    bds_VatTu.Position = bds_VatTu.Find("MaVT", productId);
                    return;
                }
            }

            if (bds_VatTu.Count == 0)
                btn_delete.Enabled = false;
        }

        private void btn_save_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (txt_id.Text.Trim() == "")
            {
                MessageBox.Show("Cannot blank Product ID!", "", MessageBoxButtons.OK);
                txt_id.Focus();
                return;
            }
            if (txt_name.Text.Trim() == "")
            {
                MessageBox.Show("Cannot blank Product Name!", "", MessageBoxButtons.OK);
                txt_name.Focus();
                return;
            }
            if (txt_unit.Text.Trim() == "")
            {
                MessageBox.Show("Cannot blank Product Unit!", "", MessageBoxButtons.OK);
                txt_unit.Focus();
                return;
            }
            if (spe_count.Text.Trim() == "")
            {
                MessageBox.Show("Cannot blank Product Count!", "", MessageBoxButtons.OK);
                spe_count.Focus();
                return;
            }

            try
            {
                bds_VatTu.EndEdit();
                bds_VatTu.ResetCurrentItem();

                tbla_VatTu.Connection.ConnectionString = Program.ConnectionString;
                tbla_VatTu.Update(ds_product.VatTu);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error when adding Product!" + ex.Message, "Error", MessageBoxButtons.OK);
                return;
            }

            gc_VatTu.Enabled = true;
            gb_info.Enabled = false;

            btn_add.Enabled = btn_edit.Enabled = btn_delete.Enabled = btn_reload.Enabled = true;
            btn_save.Enabled = btn_undo.Enabled = false;
        }

        private void btn_undo_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            bds_VatTu.CancelEdit();
            
            if (NewRow != null)
                bds_VatTu.Remove(NewRow);

            if (btn_add.Enabled == false)
                bds_VatTu.Position = RowIndex;

            gc_VatTu.Enabled = true;
            gb_info.Enabled = false;

            btn_add.Enabled = btn_edit.Enabled = btn_delete.Enabled = btn_reload.Enabled = true;
            btn_save.Enabled = btn_undo.Enabled = false;
        }

        private void btn_reload_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                tbla_VatTu.Fill(ds_product.VatTu);
            }
            catch(Exception ex)
            {
                MessageBox.Show("Error when reloading: " + ex.Message, "Error", MessageBoxButtons.OK);
                return;
            }
        }
    }
}