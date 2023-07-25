using DevExpress.UIAutomation;
using DevExpress.XtraEditors;
using DevExpress.XtraLayout.Filtering.Templates;
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
    public partial class FormImReceipt : DevExpress.XtraEditors.XtraForm
    {
        public int RowIndex = 0; // When recovering deleted row, insert that row to this index (like never far away)
        public object NewRow;
        public bool IsAdding = false;
        public FormImReceipt()
        {
            InitializeComponent();
        }

        private void SetUIConstraints()
        {
            // Bar Manager
            txt_importId.ReadOnly = true;
            txt_eeId.ReadOnly = true;
            txt_whsId.ReadOnly = true;

            // Group Control Infor
            gpc_info.Enabled = false;
            cbb_fullname.DropDownStyle = ComboBoxStyle.DropDownList;
            cbb_whsname.DropDownStyle = ComboBoxStyle.DropDownList;
            cbb_DDH.DropDownStyle = ComboBoxStyle.DropDownList;
            dte_date.Properties.EditMask = "dd/MM/yyyy";
            dte_date.Properties.UseMaskAsDisplayFormat = true;

            // Group control Phieu Nhap
            colMaPN.OptionsColumn.AllowEdit = false;
            colNgay.OptionsColumn.AllowEdit = false;
            colMaSoDDH.OptionsColumn.AllowEdit = false;
            colMaNV.OptionsColumn.AllowEdit = false;
            colMaKho.OptionsColumn.AllowEdit = false;

            colNgay.DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
            colNgay.DisplayFormat.FormatString = "dd/MM/yyyy";

            // Grid view CTPN
            colCTPNMaPN.ReadOnly = true;    colCTPNMaPN.HeaderText = "Mã Phiếu Nhập";
            colCTPNMaVT.ReadOnly = true;    colCTPNMaVT.HeaderText = "Mã Vật Tư";
            colCTPNSoLuong.ReadOnly = true; colCTPNSoLuong.HeaderText = "Số Lượng";
            colCTPNDonGia.ReadOnly = true;  colCTPNDonGia.HeaderText = "Đơn Giá";
        }

        private void TurnOnEditingState()
        {
            gpc_info.Enabled = true;
            gdc_PhieuNhap.Enabled = false;

            btn_add.Enabled = btn_edit.Enabled = btn_delete.Enabled = btn_reload.Enabled = false;
            btn_save.Enabled = btn_undo.Enabled = true;
        }

        private void TurnOffEditingState()
        {
            gpc_info.Enabled = false;
            gdc_PhieuNhap.Enabled = true;

            btn_add.Enabled = btn_edit.Enabled = btn_delete.Enabled = btn_reload.Enabled = true;
            btn_save.Enabled = btn_undo.Enabled = false;
            IsAdding = false;
        }

        private void FormReceipt_Load(object sender, EventArgs e)
        {
            DS.EnforceConstraints = false;

            tbla_PhieuNhap.Connection.ConnectionString = Program.ConnectionString;
            this.tbla_PhieuNhap.Fill(this.DS.PhieuNhap);
            tbla_DDH_Chua_Nhap.Connection.ConnectionString = Program.ConnectionString;
            this.tbla_DDH_Chua_Nhap.Fill(this.DS.DDH_Chua_Nhap);
            tbla_CTPN.Connection.ConnectionString = Program.ConnectionString;
            this.tbla_CTPN.Fill(this.DS.CTPN);
            tbla_Kho.Connection.ConnectionString = Program.ConnectionString;
            this.tbla_Kho.Fill(this.DS.Kho);
            tbla_VatTu.Connection.ConnectionString = Program.ConnectionString;
            this.tbla_VatTu.Fill(this.DS.VatTu);
            tbla_DSNV.Connection.ConnectionString = Program.ConnectionString;
            this.tbla_DSNV.Fill(this.DS.DsNV);

            // Load combobox branch
            cb_branch.DropDownStyle = ComboBoxStyle.DropDownList;
            cb_branch.DataSource = Program.bds_subscriptionList;
            cb_branch.DisplayMember = "TenCN";
            cb_branch.ValueMember = "TenServer";
            cb_branch.SelectedIndex = Program.SubsIndex;

            SetUIConstraints();

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

        private void cbb_fullname_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (cbb_fullname.SelectedValue != null)
                    txt_eeId.Text = cbb_fullname.SelectedValue.ToString();
            }catch { }
        }

        private void cbb_whsname_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (cbb_whsname.SelectedValue != null)
                    txt_whsId.Text = cbb_whsname.SelectedValue.ToString();
            }
            catch { }
        }

        private void btn_add_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            TurnOnEditingState();
            NewRow = bds_PhieuNhap.AddNew();
            IsAdding = true;

            // 
            txt_importId.ReadOnly = false;
            dte_date.Enabled = false;
            dte_date.EditValue = DateTime.Now.ToString();
            dte_date.Properties.DisplayFormat.FormatString = "dd/MM/yyyy";
        }

        private void btn_edit_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            TurnOnEditingState();
        }

        private void btn_delete_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {

        }

        private void btn_save_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (txt_importId.Text.Trim() == "")
            {
                MessageBox.Show("Cannot blank Import ID!", "", MessageBoxButtons.OK);
                txt_importId.Focus();
                return;
            }
            if (cbb_fullname.Text.Trim() == "")
            {
                MessageBox.Show("Cannot blank Employee!", "", MessageBoxButtons.OK);
                cbb_fullname.Focus();
                return;
            }
            if (cbb_whsname.Text.Trim() == "")
            {
                MessageBox.Show("Cannot blank Warehouse!", "", MessageBoxButtons.OK);
                cbb_whsname.Focus();
                return;
            } 
            // Have to check this PhieuNhap whether has any CTPNs?

            try
            {
                bds_PhieuNhap.EndEdit();
                bds_PhieuNhap.ResetCurrentItem();

                tbla_PhieuNhap.Connection.ConnectionString = Program.ConnectionString;
                tbla_PhieuNhap.Update(DS.PhieuNhap);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error when adding Product!" + ex.Message, "Error", MessageBoxButtons.OK);
                return;
            }

            TurnOffEditingState();
        }

        private void btn_undo_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            bds_PhieuNhap.CancelEdit();
            if (IsAdding)
            {
                var res = bds_PhieuNhap.Contains(NewRow);
                if (res)
                    bds_PhieuNhap.Remove(NewRow);
                IsAdding = false;
            }


            if (btn_add.Enabled == false) // When adding the Row Position points to the last row
                bds_PhieuNhap.Position = RowIndex;

            TurnOffEditingState();
        }

        private void btn_reload_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                tbla_PhieuNhap.Fill(DS.PhieuNhap);
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

            tbla_PhieuNhap.Connection.ConnectionString = Program.ConnectionString;
            this.tbla_PhieuNhap.Fill(this.DS.PhieuNhap);
            tbla_DDH_Chua_Nhap.Connection.ConnectionString = Program.ConnectionString;
            this.tbla_DDH_Chua_Nhap.Fill(this.DS.DDH_Chua_Nhap);
            tbla_CTPN.Connection.ConnectionString = Program.ConnectionString;
            this.tbla_CTPN.Fill(this.DS.CTPN);
            tbla_Kho.Connection.ConnectionString = Program.ConnectionString;
            this.tbla_Kho.Fill(this.DS.Kho);
            tbla_VatTu.Connection.ConnectionString = Program.ConnectionString;
            this.tbla_VatTu.Fill(this.DS.VatTu);
            tbla_DSNV.Connection.ConnectionString = Program.ConnectionString;
            this.tbla_DSNV.Fill(this.DS.DsNV);
        }
    }
}