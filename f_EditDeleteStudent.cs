using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace QuanLySinhVien
{
    public partial class f_EditDeleteStudent : Form
    {
        private byte[] studentImage = null;

        private readonly string namePattern = @"^[\p{L}\s]+$";
        private readonly string emailPattern = @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$";
        private readonly string phonePattern = @"^\d{9,11}$";

        public string TargetMSSV { get; set; }

        // ✅ Constructor mặc định
        public f_EditDeleteStudent()
        {
            InitializeComponent();
            RegisterRealTimeValidationEvents();
        }

        // ✅ Constructor nhận MSSV
        public f_EditDeleteStudent(string mssv)
        {
            InitializeComponent();
            TargetMSSV = mssv;
            RegisterRealTimeValidationEvents();
        }

        private void RegisterRealTimeValidationEvents()
        {
            this.Load += f_EditDeleteStudent_Load;
            this.FormClosed += f_EditStudent_FormClosed;

            txb_EditFname.TextChanged += txb_EditFname_TextChanged;
            txb_EditLname.TextChanged += txb_EditLname_TextChanged;
            txb_EditPhone.TextChanged += txb_EditPhone_TextChanged;
            txb_EditEmail.TextChanged += txb_EditEmail_TextChanged;

            // ✅ Click dòng trên bảng → tự động đổ dữ liệu lên form
            dgvStudents.SelectionChanged += dgvStudents_SelectionChanged;
        }

        // =========================================================================
        // LOAD FORM
        // =========================================================================

        private void f_EditDeleteStudent_Load(object sender, EventArgs e)
        {
            LoadStudentData(); // ✅ Load bảng danh sách trước

            if (!string.IsNullOrEmpty(TargetMSSV))
            {
                FillFormByMSSV(TargetMSSV); // ✅ Đổ form theo MSSV được truyền vào
            }
        }

        /// <summary>
        /// Load toàn bộ danh sách sinh viên vào dgvStudents
        /// </summary>
        private void LoadStudentData()
        {
            My_DB db = new My_DB();
            string query = "SELECT MSSV AS [Mã SV], Fname AS [Họ], Lname AS [Tên], " +
                           "Dob AS [Ngày sinh], Gder AS [Giới tính], Phone AS [SĐT], Email " +
                           "FROM Student";

            using (SqlConnection conn = db.getConnection)
            using (SqlCommand cmd = new SqlCommand(query, conn))
            using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
            {
                DataTable table = new DataTable();
                try
                {
                    conn.Open();
                    adapter.Fill(table);
                    dgvStudents.DataSource = table;

                    dgvStudents.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                    dgvStudents.RowHeadersVisible = false;
                    dgvStudents.AllowUserToAddRows = false;
                    dgvStudents.ReadOnly = true;
                    dgvStudents.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                    dgvStudents.MultiSelect = false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Không thể nạp danh sách sinh viên: " + ex.Message,
                        "Lỗi SQL", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// Đổ thông tin sinh viên lên form theo MSSV + highlight dòng trên bảng
        /// </summary>
        private void FillFormByMSSV(string mssv)
        {
            Student sv = Student.GetStudentByID(mssv);

            if (sv != null)
            {
                txtMSSV.Text = sv.MSSV;
                txtMSSV.Enabled = false;

                txb_EditFname.Text = sv.Fname;
                txb_EditLname.Text = sv.Lname;
                dtpEditDob.Value = sv.Dob;
                cboEditGender.Text = sv.Gender;
                txb_EditPhone.Text = sv.Phone;
                txb_EditAddress.Text = sv.Address;
                txb_EditHometown.Text = sv.Hometown;
                txb_EditEmail.Text = sv.Email;

                studentImage = sv.Picture;

                if (sv.Picture != null && sv.Picture.Length > 0)
                {
                    using (MemoryStream ms = new MemoryStream(sv.Picture))
                    {
                        if (picEditStudent.Image != null) picEditStudent.Image.Dispose();
                        picEditStudent.Image = Image.FromStream(ms);
                    }
                }
                else
                {
                    picEditStudent.Image = null;
                }

                // ✅ Highlight đúng dòng trên bảng
                foreach (DataGridViewRow row in dgvStudents.Rows)
                {
                    if (row.Cells["Mã SV"].Value?.ToString() == mssv)
                    {
                        dgvStudents.ClearSelection();
                        row.Selected = true;
                        dgvStudents.FirstDisplayedScrollingRowIndex = row.Index;
                        break;
                    }
                }
            }
            else
            {
                MessageBox.Show("Không tìm thấy dữ liệu sinh viên hợp lệ!",
                    "Lỗi hệ thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }
        }

        /// <summary>
        /// Click chọn dòng khác trên bảng → tự động đổ dữ liệu lên form
        /// </summary>
        private void dgvStudents_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvStudents.CurrentRow == null) return;

            string mssv = dgvStudents.CurrentRow.Cells["Mã SV"].Value?.ToString();
            if (!string.IsNullOrEmpty(mssv) && mssv != TargetMSSV)
            {
                TargetMSSV = mssv;
                FillFormByMSSV(mssv);
            }
        }

        // =========================================================================
        // VALIDATION THỜI GIAN THỰC
        // =========================================================================

        private void txb_EditFname_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txb_EditFname.Text))
                errorProvider1.SetError(txb_EditFname, "Họ và tên đệm không được để trống!");
            else if (!Regex.IsMatch(txb_EditFname.Text.Trim(), namePattern))
                errorProvider1.SetError(txb_EditFname, "Họ tên không được chứa số hoặc ký tự đặc biệt!");
            else
                errorProvider1.SetError(txb_EditFname, null);
        }

        private void txb_EditLname_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txb_EditLname.Text))
                errorProvider1.SetError(txb_EditLname, "Tên không được để trống!");
            else if (!Regex.IsMatch(txb_EditLname.Text.Trim(), namePattern))
                errorProvider1.SetError(txb_EditLname, "Tên không được chứa số hoặc ký tự đặc biệt!");
            else
                errorProvider1.SetError(txb_EditLname, null);
        }

        private void txb_EditPhone_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txb_EditPhone.Text))
                errorProvider1.SetError(txb_EditPhone, "Số điện thoại không được để trống!");
            else if (!Regex.IsMatch(txb_EditPhone.Text.Trim(), phonePattern))
                errorProvider1.SetError(txb_EditPhone, "Số điện thoại phải là chữ số dài từ 9 đến 11 ký tự!");
            else
                errorProvider1.SetError(txb_EditPhone, null);
        }

        private void txb_EditEmail_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txb_EditEmail.Text))
                errorProvider1.SetError(txb_EditEmail, "Địa chỉ Email không được để trống!");
            else if (!Regex.IsMatch(txb_EditEmail.Text.Trim(), emailPattern))
                errorProvider1.SetError(txb_EditEmail, "Email không đúng định dạng chuẩn (Ví dụ: abc@gmail.com)!");
            else
                errorProvider1.SetError(txb_EditEmail, null);
        }

        // =========================================================================
        // CẬP NHẬT / XÓA
        // =========================================================================

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            txb_EditFname_TextChanged(null, null);
            txb_EditLname_TextChanged(null, null);
            txb_EditPhone_TextChanged(null, null);
            txb_EditEmail_TextChanged(null, null);

            if (!string.IsNullOrEmpty(errorProvider1.GetError(txb_EditFname)) ||
                !string.IsNullOrEmpty(errorProvider1.GetError(txb_EditLname)) ||
                !string.IsNullOrEmpty(errorProvider1.GetError(txb_EditPhone)) ||
                !string.IsNullOrEmpty(errorProvider1.GetError(txb_EditEmail)))
            {
                MessageBox.Show("Vui lòng sửa các trường thông tin đang báo lỗi trước khi lưu!",
                    "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DateTime birthDate = dtpEditDob.Value;
            DateTime today = DateTime.Today;
            int age = today.Year - birthDate.Year;
            if (birthDate.Date > today.AddYears(-age)) age--;

            if (birthDate >= today || age < 17 || age > 60)
            {
                MessageBox.Show($"Độ tuổi ({age} tuổi) không hợp lệ (yêu cầu 17 - 60 tuổi)!",
                    "Lỗi tuổi học viên", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Student sv = new Student(
                TargetMSSV,
                txb_EditFname.Text.Trim(),
                txb_EditLname.Text.Trim(),
                dtpEditDob.Value,
                cboEditGender.Text,
                txb_EditPhone.Text.Trim(),
                txb_EditAddress.Text.Trim(),
                txb_EditHometown.Text.Trim(),
                txb_EditEmail.Text.Trim(),
                studentImage
            );

            if (sv.EditStudent())
            {
                MessageBox.Show("Cập nhật thông tin sinh viên thành công!",
                    "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadStudentData();          // ✅ Reload lại bảng sau khi cập nhật
                FillFormByMSSV(TargetMSSV); // ✅ Giữ nguyên dữ liệu form
            }
            else
            {
                MessageBox.Show("Cập nhật thất bại! Lỗi kết nối cơ sở dữ liệu.",
                    "Lỗi CSDL", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(TargetMSSV)) return;

            if (HasScoreRecords(TargetMSSV))
            {
                MessageBox.Show($"Không thể xóa! Sinh viên {TargetMSSV} đã có bảng điểm trong hệ thống.",
                    "Vi phạm ràng buộc dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return;
            }

            DialogResult confirm = MessageBox.Show(
                $"Bạn có chắc muốn xóa vĩnh viễn sinh viên {TargetMSSV}?",
                "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (confirm == DialogResult.Yes)
            {
                Student sv = new Student();
                sv.MSSV = TargetMSSV;

                if (sv.DeleteStudent())
                {
                    MessageBox.Show("Xóa sinh viên thành công!",
                        "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Xóa thất bại! Lỗi kết nối SQL Server.",
                        "Lỗi thực thi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private bool HasScoreRecords(string mssv)
        {
            My_DB myDb = new My_DB();
            string query = "SELECT COUNT(*) FROM Score WHERE MSSV = @mssv";

            using (SqlConnection conn = myDb.getConnection)
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.Add("@mssv", SqlDbType.NVarChar, 20).Value = mssv.Trim();
                try
                {
                    conn.Open();
                    return (int)cmd.ExecuteScalar() > 0;
                }
                catch { return false; }
            }
        }

        // =========================================================================
        // GIAO DIỆN PHỤ TRỢ
        // =========================================================================

        private void btnChooseImage_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    if (picEditStudent.Image != null) picEditStudent.Image.Dispose();
                    picEditStudent.Image = Image.FromFile(ofd.FileName);

                    using (MemoryStream ms = new MemoryStream())
                    {
                        picEditStudent.Image.Save(ms, picEditStudent.Image.RawFormat);
                        studentImage = ms.ToArray();
                    }
                }
            }
        }

        // ✅ Nút X đóng form
        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btn_Cancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void f_EditStudent_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (picEditStudent.Image != null)
                picEditStudent.Image.Dispose();
        }

        private void btn_Verify_Click(object sender, EventArgs e)
        {
            btnUpdate_Click(sender, e);
        }
    }
}