using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace CoffeeCafeProject
{
    public partial class FrmMember : Form
    {
        public FrmMember()
        {
            InitializeComponent();
            tbMemberPhone.MaxLength = 10;
            tbMemberPhone.KeyPress += tbMemberPhone_KeyPress;
        }

        private void FrmMember_Load(object sender, EventArgs e)
        {
            getAllMemberToListView();
            clearForm();
        }

        private void getAllMemberToListView()
        {
            string connectionString = @"Server=DESKTOP-9U4FO0V\SQLEXPRESS;Database=coffee_cafe_db;Trusted_Connection=True;";
            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                try
                {
                    sqlConnection.Open();
                    string strSQL = "SELECT memberId, memberPhone, memberName FROM member_tb";
                    using (SqlDataAdapter adapter = new SqlDataAdapter(strSQL, sqlConnection))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);

                        lvShowAllMember.Items.Clear();
                        lvShowAllMember.Columns.Clear();
                        lvShowAllMember.View = View.Details;
                        lvShowAllMember.FullRowSelect = true;

                        lvShowAllMember.Columns.Add("รหัสสมาชิก", 100);
                        lvShowAllMember.Columns.Add("เบอร์โทร", 120);
                        lvShowAllMember.Columns.Add("ชื่อสมาชิก", 150);

                        foreach (DataRow row in dt.Rows)
                        {
                            ListViewItem item = new ListViewItem(row["memberId"].ToString());
                            item.SubItems.Add(row["memberPhone"].ToString());
                            item.SubItems.Add(row["memberName"].ToString());
                            lvShowAllMember.Items.Add(item);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("เกิดข้อผิดพลาด: " + ex.Message);
                }
            }
        }

        private void btSave_Click(object sender, EventArgs e)
        {
            if (tbMemberPhone.Text.Length != 10)
            {
                MessageBox.Show("เบอร์โทรต้องมี 10 หลัก", "คำเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (tbMemberName.Text.Length == 0)
            {
                MessageBox.Show("กรุณากรอกชื่อสมาชิก", "คำเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string connectionString = @"Server=DESKTOP-9U4FO0V\SQLEXPRESS;Database=coffee_cafe_db;Trusted_Connection=True;";
            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                try
                {
                    sqlConnection.Open();
                    string strSQL = "INSERT INTO member_tb (memberPhone, memberName) VALUES (@phone, @name)";
                    using (SqlCommand cmd = new SqlCommand(strSQL, sqlConnection))
                    {
                        cmd.Parameters.AddWithValue("@phone", tbMemberPhone.Text);
                        cmd.Parameters.AddWithValue("@name", tbMemberName.Text);
                        cmd.ExecuteNonQuery();
                    }

                    MessageBox.Show("บันทึกข้อมูลสมาชิกเรียบร้อย", "ผลการทำงาน");
                    getAllMemberToListView();
                    clearForm();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("เกิดข้อผิดพลาด: " + ex.Message);
                }
            }
        }

        private void lvShowAllMember_ItemActivate(object sender, EventArgs e)
        {
            if (lvShowAllMember.SelectedItems.Count > 0)
            {
                var item = lvShowAllMember.SelectedItems[0];
                tbMemberId.Text = item.SubItems[0].Text;
                tbMemberPhone.Text = item.SubItems[1].Text;
                tbMemberName.Text = item.SubItems[2].Text;

                btSave.Enabled = false;
                btUpdate.Enabled = true;
                btDelete.Enabled = true;
            }
        }

        private void btUpdate_Click(object sender, EventArgs e)
        {
            if (tbMemberId.Text == "") return;

            string connectionString = @"Server=DESKTOP-9U4FO0V\SQLEXPRESS;Database=coffee_cafe_db;Trusted_Connection=True;";
            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                try
                {
                    sqlConnection.Open();
                    string strSQL = "UPDATE member_tb SET memberPhone = @phone, memberName = @name WHERE memberId = @id";
                    using (SqlCommand cmd = new SqlCommand(strSQL, sqlConnection))
                    {
                        cmd.Parameters.AddWithValue("@phone", tbMemberPhone.Text);
                        cmd.Parameters.AddWithValue("@name", tbMemberName.Text);
                        cmd.Parameters.AddWithValue("@id", int.Parse(tbMemberId.Text));
                        cmd.ExecuteNonQuery();
                    }

                    MessageBox.Show("อัปเดตข้อมูลเรียบร้อย", "ผลการทำงาน");
                    getAllMemberToListView();
                    clearForm();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("เกิดข้อผิดพลาด: " + ex.Message);
                }
            }
        }

        private void btDelete_Click(object sender, EventArgs e)
        {
            if (tbMemberId.Text == "") return;

            if (MessageBox.Show("คุณแน่ใจหรือไม่ที่จะลบข้อมูลสมาชิกนี้?", "ยืนยันการลบ", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                string connectionString = @"Server=DESKTOP-9U4FO0V\SQLEXPRESS;Database=coffee_cafe_db;Trusted_Connection=True;";
                using (SqlConnection sqlConnection = new SqlConnection(connectionString))
                {
                    try
                    {
                        sqlConnection.Open();
                        string strSQL = "DELETE FROM member_tb WHERE memberId = @id";
                        using (SqlCommand cmd = new SqlCommand(strSQL, sqlConnection))
                        {
                            cmd.Parameters.AddWithValue("@id", int.Parse(tbMemberId.Text));
                            cmd.ExecuteNonQuery();
                        }

                        MessageBox.Show("ลบสมาชิกเรียบร้อย", "ผลการทำงาน");
                        getAllMemberToListView();
                        clearForm();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("เกิดข้อผิดพลาด: " + ex.Message);
                    }
                }
            }
        }

        private void btCancel_Click(object sender, EventArgs e)
        {
            clearForm();
        }

        private void btClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void clearForm()
        {
            tbMemberId.Clear();
            tbMemberPhone.Clear();
            tbMemberName.Clear();
            btSave.Enabled = true;
            btUpdate.Enabled = false;
            btDelete.Enabled = false;
        }

        private void tbMemberPhone_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (char.IsControl(e.KeyChar)) return;
            if (!char.IsDigit(e.KeyChar)) e.Handled = true;

            TextBox tb = sender as TextBox;
            if (tb.Text.Length >= 10)
            {
                e.Handled = true;
            }
        }
    }
}
