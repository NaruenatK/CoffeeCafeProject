using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CoffeeCafeProject
{
    public partial class FrmMember : Form
    {
        public FrmMember()
        {
            InitializeComponent();
        }

        private void resetForm()
        {
            tbMemberId.Text = "";
            tbMemberPhone.Text = "";
            tbMemberName.Text = "";
            btSave.Enabled = true;
            btUpdate.Enabled = false;
            btDelete.Enabled = false;
        }

        private void getAllMemberToListView()
        {
            // string connectionString = @"Server=LAPTOP-NNRSHB5L\SQLEXPRESS;Database=coffee_cafe_db;Trusted_Connection=True;";
            using (SqlConnection sqlConnection = new SqlConnection(ShareResource.connectionString))
            {
                try
                {
                    sqlConnection.Open();
                    string strSQL = "SELECT memberId, memberPhone, memberName FROM member_tb";

                    using (SqlDataAdapter dataAdapter = new SqlDataAdapter(strSQL, sqlConnection))
                    {
                        DataTable dataTable = new DataTable();
                        dataAdapter.Fill(dataTable);

                        lvShowAllMember.Items.Clear();
                        lvShowAllMember.Columns.Clear();
                        lvShowAllMember.FullRowSelect = true;
                        lvShowAllMember.View = View.Details;

                        lvShowAllMember.Columns.Add("รหัสสมาชิก", 80, HorizontalAlignment.Left);
                        lvShowAllMember.Columns.Add("เบอร์โทรสมาชิก", 150, HorizontalAlignment.Left);
                        lvShowAllMember.Columns.Add("ชื่อสมาชิก", 100, HorizontalAlignment.Left);

                        foreach (DataRow dataRow in dataTable.Rows)
                        {
                            ListViewItem item = new ListViewItem(dataRow["memberId"].ToString());
                            item.SubItems.Add(dataRow["memberPhone"].ToString());
                            item.SubItems.Add(dataRow["memberName"].ToString());
                            lvShowAllMember.Items.Add(item);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("ไม่สามารถเชื่อมต่อฐานข้อมูลได้ กรุณาลองใหม่หรือติดต่อ IT\n" + ex.Message);
                }
            }
        }

        private void FrmMember_Load(object sender, EventArgs e)
        {
            getAllMemberToListView();
            btUpdate.Enabled = false;
            btDelete.Enabled = false;
        }

        private void showWarningMessage(string message)
        {
            MessageBox.Show(message, "คำเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void btSave_Click(object sender, EventArgs e)
        {
            if (tbMemberPhone.Text.Length == 0)
            {
                showWarningMessage("กรุณากรอกเบอร์โทรสมาชิก");
            }
            else if (tbMemberName.Text.Length == 0)
            {
                showWarningMessage("กรุณากรอกชื่อสมาชิก");
            }
            else
            {
                // string connectionString = @"Server=LAPTOP-NNRSHB5L\SQLEXPRESS;Database=coffee_cafe_db;Trusted_Connection=True;";
                using (SqlConnection sqlConnection = new SqlConnection(ShareResource.connectionString))
                {
                    try
                    {
                        sqlConnection.Open();
                        SqlTransaction sqlTransaction = sqlConnection.BeginTransaction();

                        string strSQL = "INSERT INTO member_tb (memberPhone, memberName) VALUES (@memberPhone, @memberName)";
                        using (SqlCommand sqlCommand = new SqlCommand(strSQL, sqlConnection, sqlTransaction))
                        {
                            sqlCommand.Parameters.Add("@memberPhone", SqlDbType.NVarChar, 50).Value = tbMemberPhone.Text;
                            sqlCommand.Parameters.Add("@memberName", SqlDbType.NVarChar, 100).Value = tbMemberName.Text;

                            sqlCommand.ExecuteNonQuery();
                            sqlTransaction.Commit();

                            MessageBox.Show("บันทึกข้อมูลสมาชิกเรียบร้อยแล้ว", "ผลการทำงาน", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            getAllMemberToListView();
                            resetForm();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("ไม่สามารถเชื่อมต่อฐานข้อมูลได้ กรุณาลองใหม่หรือติดต่อ IT\n" + ex.Message);
                    }
                }
            }
        }

        private void tbMemberPhone_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsDigit(e.KeyChar) && e.KeyChar != '\b')
            {
                e.Handled = true;
            }

            if (char.IsDigit(e.KeyChar) && tbMemberPhone.Text.Length >= 10)
            {
                e.Handled = true;
            }
        }

        private void lvShowAllMember_ItemActivate(object sender, EventArgs e)
        {
            tbMemberId.Text = lvShowAllMember.SelectedItems[0].SubItems[0].Text;
            tbMemberPhone.Text = lvShowAllMember.SelectedItems[0].SubItems[1].Text;
            tbMemberName.Text = lvShowAllMember.SelectedItems[0].SubItems[2].Text;

            btSave.Enabled = false;
            btUpdate.Enabled = true;
            btDelete.Enabled = true;
        }

        private void btUpdate_Click(object sender, EventArgs e)
        {
            if (tbMemberPhone.Text.Length == 0)
            {
                showWarningMessage("กรุณากรอกเบอร์โทรสมาชิก");
            }
            else if (tbMemberName.Text.Length == 0)
            {
                showWarningMessage("กรุณากรอกชื่อสมาชิก");
            }
            else
            {
                // string connectionString = @"Server=LAPTOP-NNRSHB5L\SQLEXPRESS;Database=coffee_cafe_db;Trusted_Connection=True;";
                using (SqlConnection sqlConnection = new SqlConnection(ShareResource.connectionString))
                {
                    try
                    {
                        sqlConnection.Open();
                        SqlTransaction sqlTransaction = sqlConnection.BeginTransaction();

                        string strSQL = "UPDATE member_tb SET memberPhone = @memberPhone, memberName = @memberName, memberScore = @memberScore WHERE memberId = @memberId";
                        using (SqlCommand sqlCommand = new SqlCommand(strSQL, sqlConnection, sqlTransaction))
                        {
                            sqlCommand.Parameters.Add("@memberId", SqlDbType.Int).Value = int.Parse(tbMemberId.Text);
                            sqlCommand.Parameters.Add("@memberPhone", SqlDbType.NVarChar, 50).Value = tbMemberPhone.Text;
                            sqlCommand.Parameters.Add("@memberName", SqlDbType.NVarChar, 100).Value = tbMemberName.Text;
                            sqlCommand.Parameters.Add("@memberScore", SqlDbType.Int).Value = 0;

                            sqlCommand.ExecuteNonQuery();
                            sqlTransaction.Commit();

                            MessageBox.Show("อัพเดทข้อมูลสมาชิกเรียบร้อยแล้ว", "ผลการทำงาน", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            getAllMemberToListView();
                            resetForm();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("ไม่สามารถเชื่อมต่อฐานข้อมูลได้ กรุณาลองใหม่หรือติดต่อ IT\n" + ex.Message);
                    }
                }
            }
        }

        private void btDelete_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("ต้องการลบเมนูหรือไม่", "ยืนยัน", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                // string connectionString = @"Server=LAPTOP-NNRSHB5L\SQLEXPRESS;Database=coffee_cafe_db;Trusted_Connection=True;";
                using (SqlConnection sqlConnection = new SqlConnection(ShareResource.connectionString))
                {
                    try
                    {
                        sqlConnection.Open();
                        string strSQL = "DELETE FROM member_tb WHERE memberId = @memberId";
                        using (SqlCommand sqlCommand = new SqlCommand(strSQL, sqlConnection))
                        {
                            sqlCommand.Parameters.Add("@memberId", SqlDbType.Int).Value = int.Parse(tbMemberId.Text);
                            sqlCommand.ExecuteNonQuery();

                            MessageBox.Show("ลบเมนูเรียบร้อยแล้ว", "ผลการทำงาน", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            getAllMemberToListView();
                            resetForm();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("ไม่สามารถเชื่อมต่อฐานข้อมูลได้ กรุณาลองใหม่หรือติดต่อ IT\n" + ex.Message);
                    }
                }
            }
        }

        private void btCancel_Click(object sender, EventArgs e)
        {
            resetForm();
        }

        private void btClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
