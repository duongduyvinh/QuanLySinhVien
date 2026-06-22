using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace QuanLySinhVien
{
    public partial class f_ListStudent : Form
    {
        public f_ListStudent()
        {
            InitializeComponent();
            RegisterRealTimeEvents();
        }

        private void RegisterRealTimeEvents()
        {
            this.Load += f_ListStudent_Load;
            this.txtSearch.TextChanged += txtSearch_SelectionChanged;
            this.cboGioiTinh.SelectedIndexChanged += txtSearch_SelectionChanged;
        }

        private void f_ListStudent_Load(object sender, EventArgs e)
        {
            if (cboGioiTinh.Items.Count > 0)
                cboGioiTinh.SelectedIndex = 0;

            ApplyRolePermissions();
            FormatDataGridView();
            LoadStudentData();
        }

        /// <summary>
        /// Phân quyền dựa theo Globals.GlobalUserRole
        /// 0 = Admin → Hiện cả 2 nút
        /// 1 = Sinh viên → Ẩn cả 2 nút
        /// 2 = HR → Hiện cả 2 nút
        /// </summary>
        private void ApplyRolePermissions()
        {
            switch (Globals.GlobalUserRole)
            {
                case 0: // Admin
                    btnAdd.Visible = true;
                    btnUpdate.Visible = true;
                    break;

                case 1: // Sinh viên
                    btnAdd.Visible = false;
                    btnUpdate.Visible = false;
                    break;

                case 2: // HR
                    btnAdd.Visible = true;
                    btnUpdate.Visible = true;
                    break;

                default:
                    btnAdd.Visible = false;
                    btnUpdate.Visible = false;
                    break;
            }
        }

        private void FormatDataGridView()
        {
            dgvStudents.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvStudents.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dgvStudents.RowHeadersVisible = false;
            dgvStudents.AllowUserToAddRows = false;
            dgvStudents.AllowUserToDeleteRows = false;
            dgvStudents.ReadOnly = true;
            dgvStudents.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvStudents.MultiSelect = false;
        }

        /// <summary>
        /// Bộ lọc nâng cao: Kết hợp tìm kiếm ký tự và phân loại giới tính realtime
        /// </summary>
        private void txtSearch_SelectionChanged(object sender, EventArgs e)
        {
            My_DB db = new My_DB();
            string keyword = txtSearch.Text.Trim();
            string genderFilter = cboGioiTinh.Text;

            string query = "SELECT MSSV AS [Mã SV], Fname AS [Họ], Lname AS [Tên], Dob AS [Ngày sinh], " +
                           "Gder AS [Giới tính], Phone AS [SĐT], Email " +
                           "FROM Student WHERE (MSSV LIKE @key OR Fname LIKE @key OR Lname LIKE @key)";

            if (genderFilter != "Tất cả")
                query += " AND Gder = @gender";

            using (SqlConnection conn = db.getConnection)
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.Add("@key", SqlDbType.NVarChar, 100).Value = "%" + keyword + "%";
                if (genderFilter != "Tất cả")
                    cmd.Parameters.Add("@gender", SqlDbType.NVarChar, 10).Value = genderFilter;

                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                {
                    DataTable table = new DataTable();
                    try
                    {
                        conn.Open();
                        adapter.Fill(table);
                        dgvStudents.DataSource = table;
                        UpdateSummaryCard();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Lỗi lọc tìm kiếm dữ liệu: " + ex.Message,
                            "Lỗi thực thi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        /// <summary>
        /// Sự kiện bấm nút Làm mới
        /// </summary>
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            txtSearch.Clear();
            if (cboGioiTinh.Items.Count > 0)
                cboGioiTinh.SelectedIndex = 0;
            LoadStudentData();
        }

        /// <summary>
        /// Nạp toàn bộ danh sách sinh viên lên DataGridView
        /// </summary>
        public void LoadStudentData()
        {
            My_DB db = new My_DB();
            string query = "SELECT MSSV AS [Mã SV], Fname AS [Họ], Lname AS [Tên], Dob AS [Ngày sinh], " +
                           "Gder AS [Giới tính], Phone AS [SĐT], Email FROM Student";

            using (SqlConnection conn = db.getConnection)
            using (SqlCommand cmd = new SqlCommand(query, conn))
            using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
            {
                DataTable table = new DataTable();
                try
                {
                    conn.Open();
                    table.Clear();
                    adapter.Fill(table);
                    dgvStudents.DataSource = table;
                    UpdateSummaryCard();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Không thể nạp danh sách dữ liệu sinh viên: " + ex.Message,
                        "Lỗi SQL", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// Đếm và hiển thị tổng số sinh viên thời gian thực
        /// </summary>
        private void UpdateSummaryCard()
        {
            if (dgvStudents.Rows != null)
            {
                int totalStudents = dgvStudents.Rows.Count;
                if (dgvStudents.AllowUserToAddRows && dgvStudents.Rows.Count > 0)
                    totalStudents--;
                // lblTotalStudents.Text = $"Tổng số sinh viên: {totalStudents}";
            }
        }

        /// <summary>
        /// Mở form Thêm sinh viên
        /// </summary>
        private void btnAdd_Click(object sender, EventArgs e)
        {
            f_AddStudent formAdd = new f_AddStudent();
            formAdd.ShowDialog();
            LoadStudentData();
        }

        /// <summary>
        /// Mở form Sửa/Xoá sinh viên — truyền MSSV của dòng đang chọn
        /// </summary>
        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (dgvStudents.CurrentRow == null)
            {
                MessageBox.Show("Vui lòng chọn một sinh viên cần cập nhật.",
                    "Chưa chọn sinh viên", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string mssv = dgvStudents.CurrentRow.Cells["Mã SV"].Value?.ToString();

            f_EditDeleteStudent formEdit = new f_EditDeleteStudent();
            formEdit.ShowDialog();
            LoadStudentData();
        }
    }
}