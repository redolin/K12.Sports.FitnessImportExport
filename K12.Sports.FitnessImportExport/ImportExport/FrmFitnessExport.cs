using K12.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using SmartSchool.API.PlugIn;
using SmartSchool.API.PlugIn.Export;
using Aspose.Cells;
using FISCA.Presentation.Controls;
using System.IO;
using DevComponents.DotNetBar;
using DevComponents.DotNetBar.Rendering;

namespace K12.Sports.FitnessImportExport.ImportExport
{
    public partial class FrmFitnessExport : FISCA.Presentation.Controls.BaseForm, SmartSchool.API.PlugIn.Export.ExportWizard
    {
        private string _Title;
        private int _SchoolYear;
        private int _RowIndex = 0;
        private readonly int _MAX_ROW_COUNT = 65535;

        public FrmFitnessExport(string title, Image img)
        {
            InitializeComponent();
            _Title = this.Text = title;


            #region 加入進階跟HELP按鈕
            _OptionsContainer = new PanelEx();
            _OptionsContainer.Font = this.Font;
            _OptionsContainer.ColorSchemeStyle = eDotNetBarStyle.Office2007;
            _OptionsContainer.Size = new Size(100, 100);
            _OptionsContainer.Style.BackColor1.ColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBackground;
            _OptionsContainer.Style.BackColor2.ColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBackground2;
            _OptionsContainer.Style.BorderColor.ColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBorder;
            _OptionsContainer.Style.ForeColor.ColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelText;
            _OptionsContainer.Style.GradientAngle = 90;
            _Options = new SmartSchool.API.PlugIn.Collections.OptionCollection();
            _Options.ItemsChanged += new EventHandler(_Options_ItemsChanged);

            advContainer = new ControlContainerItem();
            advContainer.AllowItemResize = false;
            advContainer.GlobalItem = false;
            advContainer.MenuVisibility = eMenuVisibility.VisibleAlways;
            advContainer.Control = _OptionsContainer;

            ItemContainer itemContainer2 = new ItemContainer();
            itemContainer2.LayoutOrientation = DevComponents.DotNetBar.eOrientation.Vertical;
            itemContainer2.MinimumSize = new System.Drawing.Size(0, 0);
            itemContainer2.SubItems.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            advContainer});


            advButton = new ButtonX();
            advButton.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            advButton.Text = "    進階";
            advButton.Top = this.wizard1.Controls[1].Controls[0].Top;
            advButton.Left = 5;
            advButton.Size = this.wizard1.Controls[1].Controls[0].Size;
            advButton.Visible = true;
            advButton.SubItems.Add(itemContainer2);
            advButton.PopupSide = ePopupSide.Top;
            advButton.SplitButton = true;
            advButton.Enabled = false;
            advButton.Visible = false; // 暫時不用
            advButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            advButton.AutoExpandOnClick = true;
            advButton.SubItemsExpandWidth = 16;
            advButton.FadeEffect = false;
            advButton.FocusCuesEnabled = false;
            this.wizard1.Controls[1].Controls.Add(advButton);

            helpButton = new LinkLabel();
            helpButton.AutoSize = true;
            helpButton.BackColor = System.Drawing.Color.Transparent;
            helpButton.Location = new System.Drawing.Point(81, 10);
            helpButton.Size = new System.Drawing.Size(69, 17);
            helpButton.TabStop = true;
            helpButton.Text = "Help";
            //helpButton.Top = this.wizard1.Controls[1].Controls[0].Top + this.wizard1.Controls[1].Controls[0].Height - helpButton.Height;
            //helpButton.Left = 150;
            helpButton.Visible = false;
            helpButton.Click += delegate { if (HelpButtonClick != null)HelpButtonClick(this, new EventArgs()); };
            helpButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            this.wizard1.Controls[1].Controls.Add(helpButton);
            #endregion

            #region 設定Wizard會跟著Style跑
            //this.wizard1.FooterStyle.ApplyStyle(( GlobalManager.Renderer as Office2007Renderer ).ColorTable.GetClass(ElementStyleClassKeys.RibbonFileMenuBottomContainerKey));
            this.wizard1.HeaderStyle.ApplyStyle((GlobalManager.Renderer as Office2007Renderer).ColorTable.GetClass(ElementStyleClassKeys.RibbonFileMenuBottomContainerKey));
            this.wizard1.FooterStyle.BackColorGradientAngle = -90;
            this.wizard1.FooterStyle.BackColorGradientType = eGradientType.Linear;
            this.wizard1.FooterStyle.BackColor = (GlobalManager.Renderer as Office2007Renderer).ColorTable.RibbonBar.Default.TopBackground.Start;
            this.wizard1.FooterStyle.BackColor2 = (GlobalManager.Renderer as Office2007Renderer).ColorTable.RibbonBar.Default.TopBackground.End;
            this.wizard1.BackColor = (GlobalManager.Renderer as Office2007Renderer).ColorTable.RibbonBar.Default.TopBackground.Start;
            this.wizard1.BackgroundImage = null;
            for (int i = 0; i < 6; i++)
            {
                (this.wizard1.Controls[1].Controls[i] as ButtonX).ColorTable = eButtonColor.OrangeWithBackground;
            }
            (this.wizard1.Controls[0].Controls[1] as System.Windows.Forms.Label).ForeColor = (GlobalManager.Renderer as Office2007Renderer).ColorTable.RibbonBar.MouseOver.TitleText;
            (this.wizard1.Controls[0].Controls[2] as System.Windows.Forms.Label).ForeColor = (GlobalManager.Renderer as Office2007Renderer).ColorTable.RibbonBar.Default.TitleText;
            #endregion



            _ExportableFields = new SmartSchool.API.PlugIn.Collections.FieldsCollection();
            _SelectedFields = new SmartSchool.API.PlugIn.Collections.FieldsCollection();

            // 因為匯出欄位是固定的, 所以不需要讓使用者選取
            foreach(string field in Global._ExcelDataTitle)
            {
                _ExportableFields.Add(field);
                _SelectedFields.Add(field);
            }

        }


        private void wizard1_FinishButtonClick(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Title = "另存新檔";
            saveFileDialog1.FileName = "" + _Title + ".xls";
            saveFileDialog1.Filter = "Excel (*.xls)|*.xls|所有檔案 (*.*)|*.*";
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                // 新增背景執行緒來處理資料的匯出
                BackgroundWorker BGW = new BackgroundWorker();
                BGW.DoWork += new DoWorkEventHandler(BGW_DoWork);
                BGW.RunWorkerCompleted += new RunWorkerCompletedEventHandler(BGW_RunWorkerCompleted);

                // 把檔案儲存的路徑當作參數傳入
                BGW.RunWorkerAsync(new object[] { saveFileDialog1.FileName });
            }
        }

        // 主要邏輯區塊
        void BGW_DoWork(object sender, DoWorkEventArgs e)
        {
            string fileName = (string)((object[])e.Argument)[0];

            _SchoolYear = integerInput1.Value;

            #region 取得需要的資料
            // 取得選取的學生ID
            List<string> studentIDList = K12.Presentation.NLDPanels.Student.SelectedSource;

            // 取得學生的基本資料
            List<StudentRecord> studentRecords = K12.Data.Student.SelectByIDs(studentIDList);

            // 取得班級ID
            List<string> classIDList = GetClassID(studentRecords);

            // 取得班級資料
            List<ClassRecord> classRecords = K12.Data.Class.SelectByIDs(classIDList);

            // 取得學生的體適能資料
            List<DAO.StudentFitnessRecord> FitnessRecords = DAO.StudentFitness.SelectByStudentIDListAndSchoolYear(studentIDList, _SchoolYear);
            #endregion

            // 所有資料得集合
            List<_ClassObj> studentFitnessRecords = new List<_ClassObj>();

            #region 把資料組合起來
            // 組合班級
            foreach (ClassRecord aClass in classRecords)
            {
                _ClassObj classObj = new _ClassObj();
                classObj.classRecord = aClass;

                // 學生
                foreach(StudentRecord student in studentRecords)
                {
                    if(student.RefClassID == aClass.ID)
                    {
                        _StudentObj studentObj = new _StudentObj();
                        studentObj.studentRecord = student;
                        
                        // 體適能
                        foreach(DAO.StudentFitnessRecord fitnessRecord in FitnessRecords)
                        {
                            if(fitnessRecord.StudentID == student.ID)
                            {
                                studentObj.fitnessRecord = fitnessRecord;
                                break;
                            }
                        }

                        classObj.studentList.Add(studentObj);
                        break;
                    }
                }

                studentFitnessRecords.Add(classObj);
            }

            
            #endregion

            // 針對班級排序
            studentFitnessRecords.Sort(SortClass);

            Workbook report = new Workbook();
            Worksheet sheet = report.Worksheets[0];
            sheet.Name = _Title;

            int columnIndex = 0;
            // 填入表頭
            foreach(string column in _ExportableFields)
            {
                sheet.Cells[_RowIndex++, columnIndex++].PutValue(column);
            }

            //填入資料
            foreach(_ClassObj aClass in studentFitnessRecords)
            {
                if( SetDataIntoSheet(sheet, aClass) == false)
                {
                    // 資料可能超過_MAX_ROW_COUNT(65535)
                    break;
                }
            }

            // 設定欄位寬度
            sheet.AutoFitColumns();

            // 凍結欄位
            sheet.FreezePanes(1, 0, 1, _ExportableFields.Count);

            e.Result = new object[] { report, fileName, _RowIndex > _MAX_ROW_COUNT };

        }

        // 當背景程式結束, 就會呼叫method
        void BGW_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if(e.Error == null)
            {
                Workbook report = (Workbook)((object[])e.Result)[0];
                bool overLimit = (bool)((object[])e.Result)[2];

                #region 儲存 Excel
                string path = (string)((object[])e.Result)[1];

                if (File.Exists(path))
                {
                    bool needCount = true;
                    try
                    {
                        File.Delete(path);
                        needCount = false;
                    }
                    catch { }
                    int i = 1;
                    while (needCount)
                    {
                        string newPath = Path.GetDirectoryName(path) + "\\" + Path.GetFileNameWithoutExtension(path) + (i++) + Path.GetExtension(path);
                        if (!File.Exists(newPath))
                        {
                            path = newPath;
                            break;
                        }
                        else
                        {
                            try
                            {
                                File.Delete(newPath);
                                path = newPath;
                                break;
                            }
                            catch { }
                        }
                    }
                }
                try
                {
                    File.Create(path).Close();
                }
                catch
                {
                    SaveFileDialog sd = new SaveFileDialog();
                    sd.Title = "另存新檔";
                    sd.FileName = Path.GetFileNameWithoutExtension(path) + ".xls";
                    sd.Filter = "Excel檔案 (*.xls)|*.xls|所有檔案 (*.*)|*.*";
                    if (sd.ShowDialog() == DialogResult.OK)
                    {
                        try
                        {
                            File.Create(sd.FileName);
                            path = sd.FileName;
                        }
                        catch
                        {
                            FISCA.Presentation.Controls.MsgBox.Show("指定路徑無法存取。", "建立檔案失敗", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }
                }
                report.Save(path, FileFormatType.Excel2003);
                #endregion
                if (overLimit)
                    MsgBox.Show("匯出資料已經超過Excel的極限(65536筆)。\n超出的資料無法被匯出。\n\n請減少選取學生人數。");
                System.Diagnostics.Process.Start(path);
            }
            else
            {
                // SmartSchool.Customization.PlugIn.Global.SetStatusBarMessage(_Title + "發生未預期錯誤。");
                MsgBox.Show(_Title + "發生未預期錯誤。\n" + e.Error.Message);
            }
        }

        private bool SetDataIntoSheet(Worksheet sheet, _ClassObj aClass)
        {
            
            // 針對學生排序
            aClass.studentList.Sort(SortStudent);

            foreach (_StudentObj student in aClass.studentList)
            {
                _RowIndex++;
                if(_RowIndex > _MAX_ROW_COUNT)
                {
                    return false;
                }

                #region 輸出每個欄位的資料
                int columnIndex = 0;
                foreach (string column in _ExportableFields)
                {
                    switch (column)
                    {
                        case "測驗日期":
                            sheet.Cells[_RowIndex, columnIndex++].PutValue(student.fitnessRecord.TestDate);
                            break;
                        case "學校類別":
                            sheet.Cells[_RowIndex, columnIndex++].PutValue(student.fitnessRecord.SchoolCategory);
                            break;
                        case "年級":
                            sheet.Cells[_RowIndex, columnIndex++].PutValue(aClass.classRecord.GradeYear);
                            break;
                        case "班級名稱":
                            sheet.Cells[_RowIndex, columnIndex++].PutValue(aClass.classRecord.Name);
                            break;
                        case "學號/座號":
                            sheet.Cells[_RowIndex, columnIndex++].PutValue(student.studentRecord.StudentNumber);
                            break;
                        case "性別":
                            string gender = "";
                            if (student.studentRecord.Gender == "1")
                            {
                                gender = "男";
                            }
                            else if (student.studentRecord.Gender == "0")
                            {
                                gender = "女";
                            }
                            sheet.Cells[_RowIndex, columnIndex++].PutValue(gender);
                            break;
                        case "身分證字號":
                            sheet.Cells[_RowIndex, columnIndex++].PutValue(student.studentRecord.IDNumber);
                            break;
                        case "生日":
                            sheet.Cells[_RowIndex, columnIndex++].PutValue(student.studentRecord.Birthday);
                            break;
                        case "身高":
                            sheet.Cells[_RowIndex, columnIndex++].PutValue(student.fitnessRecord.Height);
                            break;
                        case "體重":
                            sheet.Cells[_RowIndex, columnIndex++].PutValue(student.fitnessRecord.Weight);
                            break;
                        case "坐姿體前彎":
                            sheet.Cells[_RowIndex, columnIndex++].PutValue(student.fitnessRecord.SitAndReach);
                            break;
                        case "立定跳遠":
                            sheet.Cells[_RowIndex, columnIndex++].PutValue(student.fitnessRecord.StandingLongJump);
                            break;
                        case "仰臥起坐":
                            sheet.Cells[_RowIndex, columnIndex++].PutValue(student.fitnessRecord.SitUp);
                            break;
                        case "心肺適能":
                            sheet.Cells[_RowIndex, columnIndex++].PutValue(student.fitnessRecord.Cardiorespiratory);
                            break;
                        default:
                            break;
                    }
                }
                #endregion
            }

            return true;
        }

        private List<string> GetClassID(List<StudentRecord> studentList)
        {
            return studentList.Select(x => x.RefClassID).ToList();
        }

        /// <summary>
        /// 排序:座號/姓名
        /// </summary>
        private int SortStudent(_StudentObj obj1, _StudentObj obj2)
        {

            string seatno1 = (obj1.studentRecord.SeatNo).ToString().PadLeft(3, '0');
            seatno1 += obj1.studentRecord.Name.PadLeft(10, '0');

            string seatno2 = (obj2.studentRecord.SeatNo).ToString().PadLeft(3, '0');
            seatno2 += obj2.studentRecord.Name.PadLeft(10, '0');

            return seatno1.CompareTo(seatno2);
        }
        
        /// <summary>
        /// 排序:年級/班級序號/班級名稱
        /// </summary>
        private int SortClass(_ClassObj obj1, _ClassObj obj2)
        {
            //年級
            string seatno1 = (obj1.classRecord.GradeYear).ToString().PadLeft(1, '0');
            seatno1 += obj1.classRecord.DisplayOrder.PadLeft(3, '0');
            seatno1 += obj1.classRecord.Name.PadLeft(20, '0');

            string seatno2 = (obj2.classRecord.GradeYear).ToString().PadLeft(1, '0');
            seatno2 += obj2.classRecord.DisplayOrder.PadLeft(3, '0');
            seatno2 += obj2.classRecord.Name.PadLeft(20, '0');

            return seatno1.CompareTo(seatno2);
        }

        private void wizard1_CancelButtonClick(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.Close();
        }
        private void FrmFitnessExport_Load(object sender, EventArgs e)
        {
            integerInput1.Text = K12.Data.School.DefaultSchoolYear;
        }

        private SmartSchool.API.PlugIn.Collections.OptionCollection _Options;
        private SmartSchool.API.PlugIn.Collections.FieldsCollection _SelectedFields;
        private SmartSchool.API.PlugIn.Collections.FieldsCollection _ExportableFields;
        private LinkLabel helpButton;
        private int _PackageLimint = 280;
        private ButtonX advButton;
        private PanelEx _OptionsContainer;
        private ControlContainerItem advContainer;

        Dictionary<VirtualCheckBox, DevComponents.DotNetBar.Controls.CheckBoxX> _CheckBoxs = new Dictionary<VirtualCheckBox, DevComponents.DotNetBar.Controls.CheckBoxX>();
        Dictionary<VirtualRadioButton, System.Windows.Forms.RadioButton> _RadioButtons = new Dictionary<VirtualRadioButton, System.Windows.Forms.RadioButton>();
        List<VirtualCheckItem> _AllItems = new List<VirtualCheckItem>();

        private void _Options_ItemsChanged(object sender, EventArgs e)
        {
            _CheckBoxs.Clear();
            _RadioButtons.Clear();
            int width = 0;
            int Y = 1;
            int speace = 1;
            int visibleCount = 0;
            foreach (Control control in _OptionsContainer.Controls)
            {
                control.Dispose();
            }
            _OptionsContainer.Controls.Clear();
            foreach (VirtualCheckItem item in _Options)
            {
                if (!_AllItems.Contains(item))
                {
                    _AllItems.Add(item);
                    item.VisibleChanged += new EventHandler(item_VisibleChanged);
                }
                if (item.Visible)
                {
                    visibleCount++;
                    if (item is VirtualCheckBox)
                    {
                        #region 加入CheckBox
                        DevComponents.DotNetBar.Controls.CheckBoxX checkbox = new DevComponents.DotNetBar.Controls.CheckBoxX();
                        checkbox.TabIndex = 0;
                        checkbox.TabStop = true;
                        checkbox.AutoSize = true;
                        checkbox.Text = item.Text;
                        checkbox.Checked = item.Checked;
                        checkbox.Enabled = item.Enabled;
                        checkbox.Tag = item;
                        checkbox.CheckedChanged += new EventHandler(checkbox_CheckedChanged);
                        item.CheckedChanged += new EventHandler(syncCheckBox);
                        item.TextChanged += new EventHandler(syncCheckBox);
                        item.EnabledChanged += new EventHandler(syncCheckBox);
                        checkbox.Location = new Point(9, Y);
                        _OptionsContainer.Controls.Add(checkbox);//要先加入Panel後抓Size才準
                        Y += checkbox.Height + speace;
                        if (checkbox.PreferredSize.Width + 25 > width)
                            width = checkbox.PreferredSize.Width + 25;
                        _CheckBoxs.Add(item as VirtualCheckBox, checkbox);
                        #endregion
                    }
                    if (item is VirtualRadioButton)
                    {
                        #region 加入RadioButton
                        System.Windows.Forms.RadioButton radioButton = new System.Windows.Forms.RadioButton();
                        radioButton.TabIndex = 0;
                        radioButton.TabStop = true;
                        radioButton.AutoSize = true;
                        radioButton.Text = item.Text;
                        radioButton.Checked = item.Checked;
                        radioButton.Enabled = item.Enabled; ;
                        radioButton.Tag = item;
                        radioButton.CheckedChanged += new EventHandler(radioButton_CheckedChanged);
                        item.CheckedChanged += new EventHandler(syncRadioButton);
                        item.TextChanged += new EventHandler(syncRadioButton);
                        item.EnabledChanged += new EventHandler(syncRadioButton);
                        radioButton.Location = new Point(12, Y);
                        _OptionsContainer.Controls.Add(radioButton);
                        //radioButton.Invalidate();
                        //radioButton.PerformLayout();
                        Y += radioButton.Height + speace;
                        if (radioButton.PreferredSize.Width + 25 > width)
                            width = radioButton.PreferredSize.Width + 25;
                        _RadioButtons.Add(item as VirtualRadioButton, radioButton);
                        #endregion
                    }
                }
            }
            _OptionsContainer.Size = new Size(width, Y);
            advButton.Enabled = visibleCount > 0;
            advContainer.RecalcSize();
            SetForeColor(_OptionsContainer);
        }

        private void checkbox_CheckedChanged(object sender, EventArgs e)
        {
            DevComponents.DotNetBar.Controls.CheckBoxX checkbox = (DevComponents.DotNetBar.Controls.CheckBoxX)sender;
            VirtualCheckItem item = (VirtualCheckItem)checkbox.Tag;
            if (item.Checked != checkbox.Checked)
                item.Checked = checkbox.Checked;
        }

        private void syncCheckBox(object sender, EventArgs e)
        {
            VirtualCheckBox item = (VirtualCheckBox)sender;
            if (!_CheckBoxs.ContainsKey(item)) return;
            DevComponents.DotNetBar.Controls.CheckBoxX checkbox = _CheckBoxs[item];
            checkbox.Text = item.Text;
            checkbox.Enabled = item.Enabled;
            if (item.Checked != checkbox.Checked)
                checkbox.Checked = item.Checked;
            if (checkbox.PreferredSize.Width + 25 > _OptionsContainer.Width)
                _OptionsContainer.Width = checkbox.PreferredSize.Width + 25;
        }

        private void syncRadioButton(object sender, EventArgs e)
        {
            VirtualRadioButton item = (VirtualRadioButton)sender;
            if (!_RadioButtons.ContainsKey(item)) return;
            System.Windows.Forms.RadioButton radioButton = _RadioButtons[item];
            radioButton.Text = item.Text;
            radioButton.Enabled = item.Enabled;
            if (item.Checked != radioButton.Checked)
                radioButton.Checked = item.Checked;
            if (radioButton.PreferredSize.Width + 25 > _OptionsContainer.Width)
                _OptionsContainer.Width = radioButton.PreferredSize.Width + 25;
        }

        private void radioButton_CheckedChanged(object sender, EventArgs e)
        {
            System.Windows.Forms.RadioButton radioButton = (System.Windows.Forms.RadioButton)sender;
            VirtualCheckItem item = (VirtualCheckItem)radioButton.Tag;
            if (item.Checked != radioButton.Checked)
                item.Checked = radioButton.Checked;
        }

        private void SetForeColor(Control parent)
        {
            foreach (Control var in parent.Controls)
            {
                if (var is System.Windows.Forms.RadioButton)
                    var.ForeColor = ((Office2007Renderer)GlobalManager.Renderer).ColorTable.CheckBoxItem.Default.Text;
                SetForeColor(var);
            }
        }

        private void item_VisibleChanged(object sender, EventArgs e)
        {
            _Options_ItemsChanged(null, null);
        }

        #region ExportWizard 成員
        public event EventHandler ControlPanelClose;
        public event EventHandler ControlPanelOpen;
        public event EventHandler HelpButtonClick;
        public event EventHandler<SmartSchool.API.PlugIn.Export.ExportPackageEventArgs> ExportPackage;
        public SmartSchool.API.PlugIn.Collections.OptionCollection Options
        {
            get { return _Options; }
        }
        public SmartSchool.API.PlugIn.Collections.FieldsCollection ExportableFields
        {
            get { return _ExportableFields; }
        }
        public SmartSchool.API.PlugIn.Collections.FieldsCollection SelectedFields
        {
            get
            {
                return _SelectedFields;
            }
        }
        public bool HelpButtonVisible
        {
            get
            {
                return helpButton.Visible;
            }
            set
            {
                helpButton.Visible = value;
            }
        }
        public int PackageLimit
        {
            get
            {
                return _PackageLimint;
            }
            set
            {
                _PackageLimint = value;
            }
        }
        #endregion


        #region Inner Class
        internal class _ClassObj
        {
            public ClassRecord classRecord = new ClassRecord();
            public List<_StudentObj> studentList = new List<_StudentObj>();
        }
        internal class _StudentObj
        {
            public StudentRecord studentRecord = new StudentRecord();
            public DAO.StudentFitnessRecord fitnessRecord = new DAO.StudentFitnessRecord();
        }
        #endregion
    }
}
