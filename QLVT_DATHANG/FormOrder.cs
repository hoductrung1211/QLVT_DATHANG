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
    public partial class FormOrder : DevExpress.XtraEditors.XtraForm
    {
        public FormOrder()
        {
            InitializeComponent();
        }

        private void datHangBindingNavigatorSaveItem_Click(object sender, EventArgs e)
        {
            this.Validate();
            this.bds_DatHang.EndEdit();
            this.tableAdapterManager.UpdateAll(this.DS);

        }

        private void FormOrder_Load(object sender, EventArgs e)
        {
            DS.EnforceConstraints = false;

            tbla_DatHang.Connection.ConnectionString = Program.ConnectionString;
            this.tbla_DatHang.Fill(this.DS.DatHang);
            tbla_Kho.Connection.ConnectionString = Program.ConnectionString;
            this.tbla_Kho.Fill(this.DS.Kho);
            tbla_DSNV.Connection.ConnectionString = Program.ConnectionString;
            this.tbla_DSNV.Fill(this.DS.DsNV);
            tbla_CTDDH.Connection.ConnectionString = Program.ConnectionString;
            this.tbla_CTDDH.Fill(this.DS.CTDDH);
            tbla_VatTu.Connection.ConnectionString = Program.ConnectionString;
            this.tbla_VatTu.Fill(this.DS.VatTu);


        }
    }
}