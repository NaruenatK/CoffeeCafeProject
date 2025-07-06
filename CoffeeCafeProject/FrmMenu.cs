using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace CoffeeCafeProject
{
    public partial class FrmMenu : Form
    {
        byte[] menuImage;
        public FrmMenu()
        {
            InitializeComponent();

        }

        private Image convertByteArrayToImage(byte[] byteArrayIn)
        {
            if (byteArrayIn == null || byteArrayIn.Length == 0)
            {
                return null;
            }
            try
            {
                using (MemoryStream ms = new MemoryStream(byteArrayIn))
                {
                    return Image.FromStream(ms);
                }
            }
            catch (ArgumentException ex)
            {
                // อาจเกิดขึ้นถ้า byte array ไม่ใช่ข้อมูลรูปภาพที่ถูกต้อง
                Console.WriteLine("Error converting byte array to image: " + ex.Message);
                return null;
            }
        }



        private void getAllMenuToListView()
        {
            string connectionString = @"Server=DESKTOP-9U4FO0V\SQLEXPRESS;Database=coffee_cafe_db;Trusted_Connection=True;";

            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                try
                {
                    sqlConnection.Open();

                    string strSQL = "SELECT menuId, menuName, menuPrice, menuImage FROM menu_tb";

                    using (SqlDataAdapter dataAdapter = new SqlDataAdapter(strSQL, sqlConnection))
                    {
                        DataTable dataTable = new DataTable();
                        dataAdapter.Fill(dataTable);

                        lvShowAllMenu.Items.Clear();
                        lvShowAllMenu.Columns.Clear();
                        lvShowAllMenu.FullRowSelect = true;
                        lvShowAllMenu.View = View.Details;

                        if (lvShowAllMenu.SmallImageList == null)
                        {
                            lvShowAllMenu.SmallImageList = new ImageList();
                            lvShowAllMenu.SmallImageList.ImageSize = new Size(50, 50);
                            lvShowAllMenu.SmallImageList.ColorDepth = ColorDepth.Depth32Bit;
                        }
                        lvShowAllMenu.SmallImageList.Images.Clear();

                        lvShowAllMenu.Columns.Add("รูปเมนู", 80, HorizontalAlignment.Left);
                        lvShowAllMenu.Columns.Add("รหัสเมนู", 80, HorizontalAlignment.Left);
                        lvShowAllMenu.Columns.Add("ชื้อเมนู", 150, HorizontalAlignment.Left);
                        lvShowAllMenu.Columns.Add("ราคาเมนู", 80, HorizontalAlignment.Left);

                        foreach (DataRow dataRow in dataTable.Rows)
                        {
                            ListViewItem item = new ListViewItem();
                            Image menuImage = null;
                            if (dataRow["menuImage"] != DBNull.Value)
                            {
                                byte[] imgByte = (byte[])dataRow["menuImage"];
                                menuImage = convertByteArrayToImage(imgByte);
                            }
                            string imageKey = null;
                            if (menuImage != null)
                            {
                                imageKey = $"pro_{dataRow["menuId"]}";
                                lvShowAllMenu.SmallImageList.Images.Add(imageKey, menuImage);
                                item.ImageKey = imageKey;
                            }
                            else
                            {
                                item.ImageIndex = -1;
                            }

                            item.SubItems.Add(dataRow["menuId"].ToString());
                            item.SubItems.Add(dataRow["menuName"].ToString());
                            item.SubItems.Add(dataRow["menuPrice"].ToString());


                            lvShowAllMenu.Items.Add(item);

                        }
                    }
                }

                catch (Exception ex)
                {
                    MessageBox.Show("พบข้อผิดพลาด กรูณาลองใหม่หรือติดต่อ IT : " + ex.Message);
                }

            }
        }

        private void FrmMenu_Load(object sender, EventArgs e)
        {
            getAllMenuToListView();
            pbMenuImage.Image = null;
            tbMenuId.Clear();
            tbMenuName.Clear();
            tbMenuPrice.Clear();
            btSave.Enabled = true;
            btUpdate.Enabled = false;
            btDelete.Enabled = false;
        }

        private byte[] convertImageToByteArray(Image image, ImageFormat imageFormat)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, imageFormat);
                return ms.ToArray();
            }
        }
        private void btSelectMenuImage_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = @"C:\";
            openFileDialog.Filter = "Image File (*.jpg;*.png)|*.jpg;*.png";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                pbMenuImage.Image = Image.FromFile(openFileDialog.FileName);

                if (pbMenuImage.Image.RawFormat == ImageFormat.Jpeg)
                {
                    menuImage = convertImageToByteArray(pbMenuImage.Image, ImageFormat.Jpeg);
                }
                else
                {
                    menuImage = convertImageToByteArray(pbMenuImage.Image, ImageFormat.Png);
                }
            }
        }

        private void showWarningMSG(string msg)
        {
            MessageBox.Show(msg, "คำเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void btSave_Click(object sender, EventArgs e)
        {
            if (menuImage == null)
            {
                showWarningMSG("เลือกรูปเมนูด้วย");
            }
            else if (tbMenuName.Text.Length == 0)
            {
                showWarningMSG("ป้อนชื่อสินค้าด้วย");
            }
            else if (tbMenuPrice.Text.Length == 0)
            {
                showWarningMSG("ป้อนราคาสินค้าด้วย");
            }
            else
            {
                string connectionString = @"Server=DESKTOP-9U4FO0V\SQLEXPRESS;Database=coffee_cafe_db;Trusted_Connection=True;";

                using (SqlConnection sqlConnection = new SqlConnection(connectionString))
                {
                    try
                    {
                        sqlConnection.Open();

                        SqlTransaction sqlTransaction = sqlConnection.BeginTransaction();

                        string countSQL = "SELECT COUNT(*) FROM menu_tb";
                        using (SqlCommand countCommand = new SqlCommand(countSQL, sqlConnection, sqlTransaction))
                        {
                            int rowcount = (int)countCommand.ExecuteScalar();
                            if (rowcount >= 10)
                            {
                                showWarningMSG("ไม่สามารถเพิ่มเมนูได้เกิน 10 รายการ หากต้องการเพิ่มให้ลบของเก่าออกก่อน");
                                return;
                            }
                        }

                        string strSQL = "INSERT INTO menu_tb ( menuName, menuPrice, menuImage) " +
                                        "VALUES (  @menuName, @menuPrice, @menuImage)";

                        using (SqlCommand sqlCommand = new SqlCommand(strSQL, sqlConnection, sqlTransaction))
                        {
                            sqlCommand.Parameters.Add("@menuName", SqlDbType.NVarChar, 100).Value = tbMenuName.Text;
                            sqlCommand.Parameters.Add("@menuPrice", SqlDbType.Float).Value = float.Parse(tbMenuPrice.Text);
                            sqlCommand.Parameters.Add("@menuImage", SqlDbType.Image).Value = menuImage;

                            sqlCommand.ExecuteNonQuery();
                            sqlTransaction.Commit();

                            MessageBox.Show("บันทึกเรียบร้อย", "ผลการทำงาน", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            getAllMenuToListView();
                            menuImage = null;
                            pbMenuImage.Image = null;
                            tbMenuId.Clear();
                            tbMenuName.Clear();
                            tbMenuPrice.Clear();
                        }

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("พบข้อผิดพลาด กรูณาลองใหม่หรือติดต่อ IT : " + ex.Message);
                    }
                }
            }
        }

        private void tbMenuPrice_KeyPress(object sender, KeyPressEventArgs e)
        {
            // อนุญาตให้กดปุ่ม backspace ได้
            if (char.IsControl(e.KeyChar))
            {
                return;
            }

            // อนุญาตให้พิมพ์ตัวเลขได้
            if (char.IsDigit(e.KeyChar))
            {
                return;
            }

            // อนุญาตให้พิมพ์จุดทศนิยมได้ แต่เฉพาะครั้งเดียว
            TextBox tb = sender as TextBox;
            if (e.KeyChar == '.' && !tb.Text.Contains("."))
            {
                return;
            }

            // ไม่ใช่ตัวที่อนุญาต: ปิดการพิมพ์
            e.Handled = true;
        }

        private void lvShowAllMenu_ItemActivate(object sender, EventArgs e)
        {
            tbMenuId.Text = lvShowAllMenu.SelectedItems[0].SubItems[1].Text;
            tbMenuName.Text = lvShowAllMenu.SelectedItems[0].SubItems[2].Text;
            tbMenuPrice.Text = lvShowAllMenu.SelectedItems[0].SubItems[3].Text;

            var item = lvShowAllMenu.SelectedItems[0];
            if (item.ImageKey != null && lvShowAllMenu.SmallImageList.Images.ContainsKey(item.ImageKey))
            {
                pbMenuImage.Image = lvShowAllMenu.SmallImageList.Images[item.ImageKey];

            }
            else
            {
                pbMenuImage.Image = null;

            }

            btSave.Enabled = false;
            btUpdate.Enabled = true;
            btDelete.Enabled = true;
        }

        private void btDelete_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("คุณต้องการลบเมนูนี้ใช่หรือไม่?", "ยืนยันการลบ", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                string connectionString = @"Server=DESKTOP-9U4FO0V\SQLEXPRESS;Database=coffee_cafe_db;Trusted_Connection=True;";
                using (SqlConnection sqlConnection = new SqlConnection(connectionString))
                {
                    try
                    {
                        sqlConnection.Open();
                        string strSQL = "DELETE FROM menu_tb WHERE menuId = @menuId";
                        using (SqlCommand sqlCommand = new SqlCommand(strSQL, sqlConnection))
                        {
                            sqlCommand.Parameters.Add("@menuId", SqlDbType.Int).Value = int.Parse(tbMenuId.Text);
                            sqlCommand.ExecuteNonQuery();
                            MessageBox.Show("ลบเมนูเรียบร้อย", "ผลการทำงาน", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            getAllMenuToListView();
                            pbMenuImage.Image = null;
                            tbMenuId.Clear();
                            tbMenuName.Clear();
                            tbMenuPrice.Clear();
                            btSave.Enabled = true;
                            btUpdate.Enabled = false;
                            btDelete.Enabled = false;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("พบข้อผิดพลาด กรูณาลองใหม่หรือติดต่อ IT : " + ex.Message);
                    }
                }
            }
        }

        private void btUpdate_Click(object sender, EventArgs e)
        {
            if (tbMenuName == null)
            {
                showWarningMSG("ป้อนชื่อสินค้าด้วย");
            }
            else if (tbMenuPrice.Text.Length == 0)
            {
                showWarningMSG("ป้อนราคาสินค้าด้วย");
            }
            else
            {
                string connectionString = @"Server=DESKTOP-9U4FO0V\SQLEXPRESS;Database=coffee_cafe_db;Trusted_Connection=True;";
                using (SqlConnection sqlConnection = new SqlConnection(connectionString))
                {
                    try
                    {
                        sqlConnection.Open();

                        SqlTransaction sqlTransaction = sqlConnection.BeginTransaction();

                        string strSQL = "";

                        if (menuImage != null)
                        {
                            strSQL = "UPDATE menu_tb SET menuName = @menuName, menuPrice = @menuPrice, menuImage = @menuImage WHERE menuId = @menuId";
                        }
                        else
                        {
                            strSQL = "UPDATE menu_tb SET menuName = @menuName, menuPrice = @menuPrice WHERE menuId = @menuId";
                        }

                        using (SqlCommand sqlCommand = new SqlCommand(strSQL, sqlConnection, sqlTransaction))
                        {
                            sqlCommand.Parameters.Add("@menuId", SqlDbType.Int).Value = int.Parse(tbMenuId.Text);
                            sqlCommand.Parameters.Add("@menuName", SqlDbType.NVarChar, 100).Value = tbMenuName.Text;
                            sqlCommand.Parameters.Add("@menuPrice", SqlDbType.Float).Value = float.Parse(tbMenuPrice.Text);
                            if (menuImage != null)
                            {
                                sqlCommand.Parameters.Add("@menuImage", SqlDbType.Image).Value = menuImage;
                            }
                            sqlCommand.ExecuteNonQuery();
                            sqlTransaction.Commit();
                            MessageBox.Show("อัพเดทเรียบร้อย", "ผลการทำงาน", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            getAllMenuToListView();
                            menuImage = null;
                            pbMenuImage.Image = null;
                            tbMenuId.Clear();
                            tbMenuName.Clear();
                            tbMenuPrice.Clear();
                            btSave.Enabled = true;
                            btUpdate.Enabled = false;
                            btDelete.Enabled = false;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("พบข้อผิดพลาด กรูณาลองใหม่หรือติดต่อ IT : " + ex.Message);
                    }
                }
            }
        }

        private void btCancel_Click(object sender, EventArgs e)
        {
            getAllMenuToListView();
            pbMenuImage.Image = null;
            tbMenuName.Clear();
            tbMenuId.Clear();
            tbMenuPrice.Clear();
            menuImage = null;
            btSave.Enabled = true;
            btUpdate.Enabled = false;
            btDelete.Enabled = false;
            btCancel.Enabled = false;

        }

        private void btClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}

