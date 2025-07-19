using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                Console.WriteLine("Error converting byte array to image: " + ex.Message);
                return null;
            }
        }

        private byte[] convertImageToByteArray(Image image, ImageFormat imageFormat)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, imageFormat);
                return ms.ToArray();
            }
        }

        private void getAllMenuToListView()
        {

            string connectionString = @"LAPTOP-NNRSHB5L\SQLEXPRESS;Database=coffee_cafe_db;Trusted_Connection=True";

            using (SqlConnection sqlConnection = new SqlConnection(ShareResource.connectionString))
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
                            lvShowAllMenu.SmallImageList.ImageSize = new Size(40, 40); 
                            lvShowAllMenu.SmallImageList.ColorDepth = ColorDepth.Depth32Bit; 
                        }
                        lvShowAllMenu.SmallImageList.Images.Clear();

                        lvShowAllMenu.Columns.Add("รูปเมนู", 80, HorizontalAlignment.Left);
                        lvShowAllMenu.Columns.Add("รหัสเมนู", 60, HorizontalAlignment.Left); 
                        lvShowAllMenu.Columns.Add("ชื่อเมนู", 150, HorizontalAlignment.Left); 
                        lvShowAllMenu.Columns.Add("ราคาเมนู", 80, HorizontalAlignment.Left); 

                        foreach (DataRow dataRow in dataTable.Rows)
                        {
                            ListViewItem item = new ListViewItem();

                            Image menuImage = null;
                            if (dataRow["menuImage"] != DBNull.Value)
                            {
                                byte[] imgByte = (byte[])dataRow["menuImage"];
                                menuImage = convertByteArrayToImage(imgByte); 

                            string imagekey = null;
                            if (menuImage != null)
                            {
                                imagekey = $"menu_{dataRow["menuId"]}"; 
                                lvShowAllMenu.SmallImageList.Images.Add(imagekey, menuImage);
                                item.ImageKey = imagekey; 
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
                    MessageBox.Show("ไม่สามารถเชื่อมต่อฐานข้อมูลได้ กรุณาลองใหม่หรือติดต่อ IT\n" + ex.Message);
                }
            }
        }

        private void FrmMenu_Load(object sender, EventArgs e)
        {
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

        private void btSelectMenuImage_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = @"C:\\";
            openFileDialog.Filter = "Image Files|*.jpg;*.png;";

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
        private void showWarningMessage(string message)
        {
            MessageBox.Show(message, "คำเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void btSave_Click(object sender, EventArgs e)
        {
            if (menuImage == null)
            {
                showWarningMessage("กรุณาเลือกรูปภาพเมนู");
            }
            else if (tbMenuName.Text.Length == 0)
            {
                showWarningMessage("กรุณากรอกชื่อเมนู");
            }
            else if (tbMenuPrice.Text.Length == 0)
            {
                showWarningMessage("กรุณากรอกราคาเมนู");
            }
            else
            {
                //string connectionString = @"LAPTOP-NNRSHB5L\SQLEXPRESS;Database=coffee_cafe_db;Trusted_Connection=True";

                using (SqlConnection sqlConnection = new SqlConnection(ShareResource.connectionString))
                {
                    try
                    {
                        sqlConnection.Open(); 

                        String countSQL = "SELECT COUNT(*) FROM menu_tb";
                        using (SqlCommand conutCommand = new SqlCommand(countSQL, sqlConnection))
                        {
                            int rowCount = (int)conutCommand.ExecuteScalar(); 
                            if (rowCount == 10)
                            {
                                showWarningMessage("มีเมนูได้แค่ 10 รายการเท่านั้น กรุณาลบรายการที่ไม่ต้องการออกก่อน");
                                return;
                            }
                        }

                        SqlTransaction sqlTransaction = sqlConnection.BeginTransaction();

                        string strSQL = "INSERT INTO menu_tb (menuName, menuPrice, menuImage) " +
                                        "VALUES (@menuName, @menuPrice, @menuImage)";

                        using (SqlCommand sqlCommand = new SqlCommand(strSQL, sqlConnection, sqlTransaction))
                        {
                            sqlCommand.Parameters.Add("@menuName", SqlDbType.NVarChar, 100).Value = tbMenuName.Text;
                            sqlCommand.Parameters.Add("@menuPrice", SqlDbType.Float).Value = float.Parse(tbMenuPrice.Text);
                            sqlCommand.Parameters.Add("@menuImage", SqlDbType.Image).Value = menuImage;

                            sqlCommand.ExecuteNonQuery();
                            sqlTransaction.Commit();

                            MessageBox.Show(Text = "บันทึกข้อมูลสินค้าเรียบร้อยแล้ว", "ผลการทำงาน", MessageBoxButtons.OK, MessageBoxIcon.Information);

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
                        MessageBox.Show("ไม่สามารถเชื่อมต่อฐานข้อมูลได้ กรุณาลองใหม่หรือติดต่อ IT\n" + ex.Message);
                        return;
                    }
                }
            }
        }

        private void tbMenuPrice_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsDigit(e.KeyChar) && e.KeyChar != '.' && e.KeyChar != '\b')
            {
                e.Handled = true;
            }

            if (e.KeyChar == '.' && (sender as TextBox).Text.Contains('.'))
            {
                e.Handled = true;
            }
        }

        private void lvShowAllMenu_ItemActivate(object sender, EventArgs e)
        {
            tbMenuId.Text = lvShowAllMenu.SelectedItems[0].SubItems[1].Text;
            tbMenuName.Text = lvShowAllMenu.SelectedItems[0].SubItems[2].Text;
            tbMenuPrice.Text = lvShowAllMenu.SelectedItems[0].SubItems[3].Text;

            var item = lvShowAllMenu.SelectedItems[0];
            if (!string.IsNullOrEmpty(item.ImageKey) && lvShowAllMenu.SmallImageList.Images.ContainsKey(item.ImageKey))
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
            if (MessageBox.Show("ต้องการลบเมนูหรือไม่", "ยืนยัน", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                //string connectionString = @"LAPTOP-NNRSHB5L\SQLEXPRESS;Database=coffee_cafe_db;Trusted_Connection=True";
                using (SqlConnection sqlConnection = new SqlConnection(ShareResource.connectionString))
                {
                    try
                    {
                        sqlConnection.Open();
                        string strSQL = "DELETE FROM menu_tb WHERE menuId = @menuId";
                        using (SqlCommand sqlCommand = new SqlCommand(strSQL, sqlConnection))
                        {
                            sqlCommand.Parameters.Add("@menuId", SqlDbType.Int).Value = int.Parse(tbMenuId.Text);
                            sqlCommand.ExecuteNonQuery();

                            MessageBox.Show("ลบเมนูเรียบร้อยแล้ว", "ผลการทำงาน", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            getAllMenuToListView();
                            menuImage = null;
                            pbMenuImage.Image = null; 
                            tbMenuId.Clear(); 
                            tbMenuName.Clear(); 
                            tbMenuPrice.Clear();
                            btUpdate.Enabled = false;
                            btDelete.Enabled = false;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("ไม่สามารถเชื่อมต่อฐานข้อมูลได้ กรุณาลองใหม่หรือติดต่อ IT\n" + ex.Message);
                    }
                }
            }
        }

        private void btUpdate_Click(object sender, EventArgs e)
        {
            if (tbMenuName.Text.Length == 0)
            {
                showWarningMessage("กรุณากรอกชื่อเมนู");
            }
            else if (tbMenuPrice.Text.Length == 0)
            {
                showWarningMessage("กรุณากรอกราคาเมนู");
            }
            else
            {
         
                //string connectionString = @"LAPTOP-NNRSHB5L\SQLEXPRESS;Database=coffee_cafe_db;Trusted_Connection=True";

                using (SqlConnection sqlConnection = new SqlConnection(ShareResource.connectionString))
                {
                    try
                    {
                        sqlConnection.Open(); 

                        SqlTransaction sqlTransaction = sqlConnection.BeginTransaction();

                        string strSQL = "";
                        if (menuImage == null)
                        {
                            strSQL = "UPDATE menu_tb SET menuName = @menuName, menuPrice = @menuPrice " +
                                    "WHERE menuId = @menuId";
                        }
                        else
                        {
                            strSQL = "UPDATE menu_tb SET menuName = @menuName, menuPrice = @menuPrice, menuImage = @menuImage " +
                                    "WHERE menuId = @menuId";
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

                            MessageBox.Show(Text = "แก้ไขเมนูเรียบร้อยแล้ว", "ผลการทำงาน", MessageBoxButtons.OK, MessageBoxIcon.Information);

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
                        MessageBox.Show("ไม่สามารถเชื่อมต่อฐานข้อมูลได้ กรุณาลองใหม่หรือติดต่อ IT\n" + ex.Message);
                        return;
                    }
                }
            }
        }

        private void btCancel_Click(object sender, EventArgs e)
        {
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

        private void btClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
