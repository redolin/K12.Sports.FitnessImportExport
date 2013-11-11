using Aspose.Cells;
using FISCA.Presentation.Controls;
using K12.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace K12.Sports.FitnessImportExport.ImportExport
{
    public partial class FrmFitnessImportBaseForm : BaseForm
    {
        private int _SchoolYear;
        // key: 身分證字號, value: ref_student_id
        private Dictionary<string, string> studentDic = new Dictionary<string, string>();


        public FrmFitnessImportBaseForm()
        {
            InitializeComponent();
        }

        private void FrmFitnessImportBaseForm_Load(object sender, EventArgs e)
        {
            integerInput1.Text = K12.Data.School.DefaultSchoolYear;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            // 讓自己變成不可按
            this.btnImport.Enabled = false;

            OpenFileDialog openFileDiag = new OpenFileDialog();
            openFileDiag.Title = "請選擇檔案";
            openFileDiag.FileName = "*.xls";
            openFileDiag.Filter = "Excel (*.xls)|*.xls|所有檔案 (*.*)|*.*";
            if(openFileDiag.ShowDialog() == DialogResult.OK)
            {

                // 檢查檔案
                if (!File.Exists(openFileDiag.FileName))
                {
                    MsgBox.Show("檔案不存在!!");
                    this.btnImport.Enabled = true;
                    return;
                }
                
                // 新增背景執行緒來處理資料的匯入
                BackgroundWorker BGW = new BackgroundWorker();
                BGW.DoWork += new DoWorkEventHandler(BGW_DoWork);
                BGW.RunWorkerCompleted += new RunWorkerCompletedEventHandler(BGW_RunWorkerCompleted);

                // 把檔案儲存的路徑當作參數傳入
                BGW.RunWorkerAsync(new object[] { openFileDiag.FileName });
            }
            else
            {
                this.btnImport.Enabled = true;
            }
        }

        private void BGW_DoWork(object sender, DoWorkEventArgs e)
        {
            string fileName = (string)((object[])e.Argument)[0];

            _SchoolYear = integerInput1.Value;

            Workbook workBook = new Workbook();
            workBook.Open(fileName);

            Worksheet sheet = workBook.Worksheets[Global._SheetName];

            if(sheet == null)
            {
                _ResultSet result = new _ResultSet();
                result.isOK = false;
                result.errorMsg.Add("找不到名為\""+ Global._SheetName +"\"的sheet");
                e.Result = result;
                return;
            }

            // 找出標題在哪個row
            int dataTitleRow = FindDataTitleRow(sheet);
            if(dataTitleRow == -1)
            {
                _ResultSet result = new _ResultSet();
                result.isOK = false;
                result.errorMsg.Add("在Excel中找不到標題列");
                e.Result = result;
                return;
            }

            // 取得資料

            

        }

        private void BGW_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.btnImport.Enabled = true;
        }

        #region Excel處理
        private int FindDataTitleRow(Worksheet sheet)
        {
            Cells cells = sheet.Cells;
            
            for(int rowIndex=0; rowIndex<cells.MaxRow; rowIndex++)
            {
                int columnIndex = 0;
                bool isFound = true;
                // 欄位名稱以及位置, 必須一致
                foreach(string columnName in Global._ExcelDataTitle)
                {
                    string cellContent = cells[rowIndex, columnIndex++].StringValue;
                    if(columnName != cellContent)
                    {
                        isFound = false;
                        break;
                    }
                }

                if(isFound == true)
                {
                    return rowIndex;
                }
            }

            return -1;
        }

        private _ResultSet ValidateDataFromSheet(Worksheet sheet, int startRow)
        {
            _ResultSet result = new _ResultSet();
            result.isOK = true;
            Cells cells = sheet.Cells;
            StringBuilder sb = new StringBuilder();
                
            for(int rowIndex = startRow; rowIndex<sheet.Cells.MaxRow; rowIndex++)
            {
                bool isOK = true;
                sb.Append("第"+(rowIndex+1)+"筆資料有誤:");

                #region 驗證每個欄位的資料
                for (int columnIndex = 0; columnIndex < Global._ExcelDataTitle.Length; columnIndex++)
                {
                    string cellContent = (cells[rowIndex, columnIndex].StringValue).Trim();
                    //                   0           1           2       3           4            5       6             7       8       9       10            11          12          13
                    // Excel:           "測驗日期", "學校類別", "年級", "班級名稱", "學號/座號", "性別", "身分證字號", "生日", "身高", "體重", "坐姿體前彎", "立定跳遠", "仰臥起坐", "心肺適能"
                    if(cellContent == "")
                    {
                        sb.Append(Global._ExcelDataTitle[columnIndex] + "不可為空白!, ");
                        isOK = false;
                        continue;
                    }

                    switch (columnIndex)
                    {
                        case 0:
                            // "測驗日期
                            
                            break;
                        case 1:
                            // "學校類別"
                            break;
                        case 2:
                            // "年級"
                        case 3:
                            // "班級名稱"
                        case 4:
                            // "學號/座號"
                        case 5:
                            // "性別"
                            break;
                        case 6:
                            // "身分證字號"
                            break;
                        case 7:
                            // "生日"
                            break;
                        case 8:
                            // "身高"
                            break;
                        case 9:
                            // "體重"
                            break;
                        case 10:
                            // "坐姿體前彎"
                            break;
                        case 11:
                            // "立定跳遠"
                            break;
                        case 12:
                            // "仰臥起坐"
                            break;
                        case 13:
                            // "心肺適能"
                            break;
                        default:
                            break;
                    }
                }
                #endregion

                if(isOK == false)
                {
                    result.isOK = false;
                    result.errorMsg.Add(sb.ToString());
                }
            }

            return result;
        }

        private List<DAO.StudentFitnessRecord> GetDataFromSheet(Worksheet sheet, int startRow)
        {
            List<DAO.StudentFitnessRecord> result = new List<DAO.StudentFitnessRecord>();
            Cells cells = sheet.Cells;

            for(int rowIndex = startRow; rowIndex<sheet.Cells.MaxRow; rowIndex++)
            {
                DAO.StudentFitnessRecord rec = new DAO.StudentFitnessRecord();
                rec.SchoolYear = _SchoolYear;
                #region 取得每個欄位的資料
                for(int columnIndex = 0; columnIndex<Global._ExcelDataTitle.Length; columnIndex++)
                {
                    string cellContent = (cells[rowIndex, columnIndex].StringValue).Trim();
                    //                   0           1           2       3           4            5       6             7       8       9       10            11          12          13
                    // Excel:           "測驗日期", "學校類別", "年級", "班級名稱", "學號/座號", "性別", "身分證字號", "生日", "身高", "體重", "坐姿體前彎", "立定跳遠", "仰臥起坐", "心肺適能"
                    // DB:    "學年度", "測驗日期", "學校類別", "學生系統編號",                                                "身高", "體重", "坐姿體前彎", "立定跳遠", "仰臥起坐", "心肺適能"
                    switch(columnIndex)
                    {
                        case 0:
                            // "測驗日期
                            DateTime dateTime = ConvertStringToDataTime(cellContent);
                            rec.TestDate = dateTime;
                            break;
                        case 1:
                            // "學校類別"
                            rec.SchoolCategory = cellContent;
                            break;
                        case 2:
                            // "年級"
                        case 3:
                            // "班級名稱"
                        case 4:
                            // "學號/座號"
                        case 5:
                            // "性別"
                            break;
                        case 6:
                            // "身分證字號"
                            break;
                        case 7:
                            // "生日"
                            break;
                        case 8:
                            // "身高"
                            rec.Height = cellContent;
                            break;
                        case 9:
                            // "體重"
                            rec.Weight = cellContent;
                            break;
                        case 10:
                            // "坐姿體前彎"
                            rec.SitAndReach = cellContent;
                            break;
                        case 11:
                            // "立定跳遠"
                            rec.StandingLongJump = cellContent;
                            break;
                        case 12:
                            // "仰臥起坐"
                            rec.SitUp = cellContent;
                            break;
                         case 13:
                            // "心肺適能"
                            rec.Cardiorespiratory = cellContent;
                            break;
                        default:
                            break;
                    }
                }
                #endregion
                result.Add(rec);
            }

            return result;
        }

        private DateTime ConvertStringToDataTime(string dateString)
        {
            DateTime dateTime = new DateTime();

            return dateTime;
        }
        #endregion

        #region 定義內部類別
        internal class _ResultSet
        {
            public bool isOK { get; set; }
            public List<string> errorMsg { get; set; }
            public Worksheet sheet { get; set; }

            public _ResultSet()
            {
                isOK = false;
                errorMsg = new List<string>();
            }
        }
        #endregion
    }
}
