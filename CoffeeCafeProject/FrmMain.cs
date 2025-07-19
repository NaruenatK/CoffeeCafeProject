using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace CoffeeCafeProject
{
    public partial class FrmMain : Form
    {
        float[] menuPrice = new float[10];
        int memberId = 0;

        public FrmMain()
        {
            InitializeComponent();
        }

        private void resetFrom()
        {
            memberId = 0;
            rdMenberNo.Checked = false;
            rdMemberYes.Checked = false;
            tbMemberPhone.Clear();
            tbMemberPhone.Enabled = false;
            tbMemberName.Text = "(ชื่อสมาชิก)";
            lbMemberScore.Text = "0";
            lbOrderPay.Text = "0.00";
            lvOrderMenu.Items.Clear();
            lvOrderMenu.Columns.Clear();
            lvOrderMenu.FullRowSelect = true;
            lvOrderMenu.View = View.Details;
            lvOrderMenu.Columns.Add("ชื่อเมนู", 150, HorizontalAlignment.Left);
            lvOrderMenu.Columns.Add("ราคา", 80, HorizontalAlignment.Left);

            // string connectionString = @"Server=LAPTOP-NNRSHB5L\SQLEXPRESS;Database=coffee_cafe_db;Trusted_Connection=True;";
            using (SqlConnection sqlConnection = new SqlConnection(ShareResource.connectionString))
            {
                try
                {
                    sqlConnection.Open();

                    string strSQL = "SELECT menuName, menuPrice, menuImage FROM menu_tb";

                    using (SqlDataAdapter dataAdapter = new SqlDataAdapter(strSQL, sqlConnection))
                    {
                        DataTable dataTable = new DataTable();
                        dataAdapter.Fill(dataTable);

                        PictureBox[] pbMenuImage = { pbMenu1, pbMenu2, pbMenu3, pbMenu4, pbMenu5, pbMenu6, pbMenu7, pbMenu8, pbMenu9, pbMenu10 };
                        Button[] btMenuName = { btMenu1, btMenu2, btMenu3, btMenu4, btMenu5, btMenu6, btMenu7, btMenu8, btMenu9, btMenu10 };

                        for (int i = 0; i < 10; i++)
                        {
                            pbMenuImage[i].Image = Properties.Resources.menu;
                            btMenuName[i].Text = "Menu";
                        }

                        for (int i = 0; i < dataTable.Rows.Count; i++)
                        {
                            btMenuName[i].Text = dataTable.Rows[i]["menuName"].ToString();
                            menuPrice[i] = float.Parse(dataTable.Rows[i]["menuPrice"].ToString());

                            if (dataTable.Rows[i]["menuImage"] != DBNull.Value)
                            {
                                byte[] imgByte = (byte[])dataTable.Rows[i]["menuImage"];
                                using (var ms = new System.IO.MemoryStream(imgByte))
                                {
                                    pbMenuImage[i].Image = System.Drawing.Image.FromStream(ms);
                                }
                            }
                            else
                            {
                                pbMenuImage[i].Image = Properties.Resources.menu;
                            }
                        }
                    }
                }
                catch (SqlException ex)
                {
                    MessageBox.Show("เกิดข้อผิดพลาดในการเชื่อมต่อฐานข้อมูล: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btMenu_Click(object sender, EventArgs e)
        {
            FrmMenu frmMenu = new FrmMenu();
            frmMenu.ShowDialog();
            resetFrom();
        }

        private void btMember_Click(object sender, EventArgs e)
        {
            FrmMember frmMember = new FrmMember();
            frmMember.ShowDialog();
        }

        private void btCancel_Click(object sender, EventArgs e)
        {
            resetFrom();
        }

        private void FrmMain_Load(object sender, EventArgs e)
        {
            resetFrom();
        }

        private void rdMenberNo_CheckedChanged(object sender, EventArgs e)
        {
            tbMemberPhone.Clear();
            tbMemberPhone.Enabled = false;
            tbMemberName.Text = "(ชื่อสมาชิก)";
            lbMemberScore.Text = "0";
            memberId = 0;
        }

        private void rdMemberYes_CheckedChanged(object sender, EventArgs e)
        {
            tbMemberPhone.Clear();
            tbMemberPhone.Enabled = true;
            tbMemberPhone.Focus();
            tbMemberName.Text = "(ชื่อสมาชิก)";
            lbMemberScore.Text = "0";
        }

        private void tbMemberPhone_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                // string connectionString = @"Server=LAPTOP-NNRSHB5L\SQLEXPRESS;Database=coffee_cafe_db;Trusted_Connection=True;";
                using (SqlConnection sqlConnection = new SqlConnection(ShareResource.connectionString))
                {
                    try
                    {
                        sqlConnection.Open();

                        string strSQL = "SELECT memberId, memberName, memberScore FROM member_tb WHERE memberPhone = @memberPhone";

                        using (SqlCommand sqlCommand = new SqlCommand(strSQL, sqlConnection))
                        {
                            sqlCommand.Parameters.Add("@memberPhone", SqlDbType.NVarChar, 50).Value = tbMemberPhone.Text;

                            using (SqlDataAdapter dataAdapter = new SqlDataAdapter(sqlCommand))
                            {
                                DataTable dataTable = new DataTable();
                                dataAdapter.Fill(dataTable);

                                if (dataTable.Rows.Count == 1)
                                {
                                    tbMemberName.Text = dataTable.Rows[0]["memberName"].ToString();
                                    lbMemberScore.Text = dataTable.Rows[0]["memberScore"].ToString();
                                    memberId = int.Parse(dataTable.Rows[0]["memberId"].ToString());
                                }
                                else
                                {
                                    MessageBox.Show("ไม่พบข้อมูลสมาชิกที่มีเบอร์โทรศัพท์นี้", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                        }
                    }
                    catch (SqlException ex)
                    {
                        MessageBox.Show("เกิดข้อผิดพลาดในการเชื่อมต่อฐานข้อมูล: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private void btMenu1_Click(object sender, EventArgs e)
        {
            AddMenuToOrder(btMenu1.Text, menuPrice[0]);
        }

        private void btMenu2_Click(object sender, EventArgs e)
        {
            AddMenuToOrder(btMenu2.Text, menuPrice[1]);
        }

        private void btMenu3_Click(object sender, EventArgs e)
        {
            AddMenuToOrder(btMenu3.Text, menuPrice[2]);
        }

        private void btMenu4_Click(object sender, EventArgs e)
        {
            AddMenuToOrder(btMenu4.Text, menuPrice[3]);
        }

        private void btMenu5_Click(object sender, EventArgs e)
        {
            AddMenuToOrder(btMenu5.Text, menuPrice[4]);
        }

        private void btMenu6_Click(object sender, EventArgs e)
        {
            AddMenuToOrder(btMenu6.Text, menuPrice[5]);
        }

        private void btMenu7_Click(object sender, EventArgs e)
        {
            AddMenuToOrder(btMenu7.Text, menuPrice[6]);
        }

        private void btMenu8_Click(object sender, EventArgs e)
        {
            AddMenuToOrder(btMenu8.Text, menuPrice[7]);
        }

        private void btMenu9_Click(object sender, EventArgs e)
        {
            AddMenuToOrder(btMenu9.Text, menuPrice[8]);
        }

        private void btMenu10_Click(object sender, EventArgs e)
        {
            AddMenuToOrder(btMenu10.Text, menuPrice[9]);
        }

        private void AddMenuToOrder(string menuName, float price)
        {
            if (menuName != "Menu")
            {
                ListViewItem item = new ListViewItem(menuName);
                item.SubItems.Add(price.ToString());
                lvOrderMenu.Items.Add(item);

                if (tbMemberName.Text != "(ชื่อสมาชิก)")
                {
                    lbMemberScore.Text = (int.Parse(lbMemberScore.Text) + 1).ToString();
                }

                lbOrderPay.Text = (float.Parse(lbOrderPay.Text) + price).ToString();
            }
        }

        private void btSave_Click(object sender, EventArgs e)
        {
            if (lbOrderPay.Text == "0.00")
            {
                MessageBox.Show("เลือกเมนูที่จะสั่งด้วย!", "คำเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else if (!rdMemberYes.Checked && !rdMenberNo.Checked)
            {
                MessageBox.Show("เลือกสถานะสมาชิกด้วย!", "คำเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else if (rdMemberYes.Checked && tbMemberName.Text == "(ชื่อสมาชิก)")
            {
                MessageBox.Show("กรุณาค้นหาสมาชิกด้วย!", "คำเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

                        string strSQL = "INSERT INTO order_tb (memberId, orderPay, createAt, updateAt) " +
                                        "VALUES (@memberId, @orderPay, @createAt, @updateAt);" +
                                        "SELECT CAST(SCOPE_IDENTITY() AS INT)";

                        int orderId;

                        using (SqlCommand sqlCommand = new SqlCommand(strSQL, sqlConnection, sqlTransaction))
                        {
                            sqlCommand.Parameters.Add("@memberId", SqlDbType.Int).Value = memberId;
                            sqlCommand.Parameters.Add("@orderPay", SqlDbType.Float).Value = float.Parse(lbOrderPay.Text);
                            sqlCommand.Parameters.Add("@createAt", SqlDbType.DateTime).Value = DateTime.Now;
                            sqlCommand.Parameters.Add("@updateAt", SqlDbType.DateTime).Value = DateTime.Now;

                            orderId = (int)sqlCommand.ExecuteScalar();
                        }

                        foreach (ListViewItem item in lvOrderMenu.Items)
                        {
                            string strSQL2 = "INSERT INTO order_detail_tb (orderId, menuName, menuPrice) " +
                                             "VALUES (@orderId, @menuName, @menuPrice)";

                            using (SqlCommand sqlCommand = new SqlCommand(strSQL2, sqlConnection, sqlTransaction))
                            {
                                sqlCommand.Parameters.Add("@orderId", SqlDbType.Int).Value = orderId;
                                sqlCommand.Parameters.Add("@menuName", SqlDbType.NVarChar, 100).Value = item.SubItems[0].Text;
                                sqlCommand.Parameters.Add("@menuPrice", SqlDbType.Float).Value = float.Parse(item.SubItems[1].Text);
                                sqlCommand.ExecuteNonQuery();
                            }
                        }

                        if (rdMemberYes.Checked)
                        {
                            string strSQL3 = "UPDATE member_tb SET memberScore = @memberScore WHERE memberId = @memberId";
                            using (SqlCommand sqlCommand = new SqlCommand(strSQL3, sqlConnection, sqlTransaction))
                            {
                                sqlCommand.Parameters.Add("@memberScore", SqlDbType.Int).Value = int.Parse(lbMemberScore.Text);
                                sqlCommand.Parameters.Add("@memberId", SqlDbType.Int).Value = memberId;
                                sqlCommand.ExecuteNonQuery();
                            }
                        }

                        sqlTransaction.Commit();
                        MessageBox.Show("บันทึกข้อมูลเรียบร้อยแล้ว", "ผลการทำงาน", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        resetFrom();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("ไม่สามารถเชื่อมต่อฐานข้อมูลได้ กรุณาลองใหม่หรือติดต่อ IT\n" + ex.StackTrace);
                    }
                }
            }
        }

        private void lvOrderMenu_ItemActivate(object sender, EventArgs e)
        {
            if (MessageBox.Show("ต้องการลบเมนูนี้หรือไม่?", "ยืนยัน", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                if (lvOrderMenu.SelectedItems.Count > 0)
                {
                    ListViewItem selectedItem = lvOrderMenu.SelectedItems[0];
                    float itemPrice = float.Parse(selectedItem.SubItems[1].Text);
                    lvOrderMenu.Items.Remove(selectedItem);

                    float currentTotal = float.Parse(lbOrderPay.Text);
                    currentTotal -= itemPrice;
                    lbOrderPay.Text = currentTotal.ToString("0");

                    if (tbMemberName.Text != "(ชื่อสมาชิก)")
                    {
                        int currentScore = int.Parse(lbMemberScore.Text);
                        currentScore = Math.Max(0, currentScore - 1);
                        lbMemberScore.Text = currentScore.ToString();
                    }
                }
            }
        }
    }
}
