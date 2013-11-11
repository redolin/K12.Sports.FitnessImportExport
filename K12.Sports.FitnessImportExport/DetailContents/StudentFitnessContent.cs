using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FISCA.Permission;
using Campus.Windows;
using FISCA.Presentation;

namespace K12.Sports.FitnessImportExport.DetailContents
{
    [FeatureCode("K12.Sports.Fitness.Content.cs", "體適能")]
    public partial class StudentFitnessContent : DetailContent
    {
        // 背景處理
        private BackgroundWorker _bgWorker;
        bool _isBusy = false;

        // 資料變動檢查
        ChangeListener _ChangeListener;

        public StudentFitnessContent()
        {
            InitializeComponent();

            // 資料項目名稱
            this.Group = Global._ModuleName;

            _bgWorker = new BackgroundWorker();
            _ChangeListener = new ChangeListener();
            _bgWorker.DoWork += new DoWorkEventHandler(_bgWorker_DoWork);
            _bgWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_bgWorker_RunWorkerCompleted);
            _ChangeListener.StatusChanged += new EventHandler<ChangeEventArgs>(_ChangeListener_StatusChanged);
        }

        // 假如資料有變動, 就會呼叫本method
        void _ChangeListener_StatusChanged(object sender, ChangeEventArgs e)
        {
            CancelButtonVisible = (e.Status == ValueStatus.Dirty);
            SaveButtonVisible = (e.Status == ValueStatus.Dirty);
        }

        // 假如背景處理結束, 就會呼叫本method
        void _bgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (_isBusy)
            {
                _isBusy = false;
                _bgWorker.RunWorkerAsync();
                return;
            }

            // 載入資料至畫面

        }

        // 開始背景處理
        void _bgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            // 資料處理
        }

        // 切換學生時
        protected override void OnPrimaryKeyChanged(EventArgs e)
        {

        }

        // 點 儲存 按鈕
        protected override void OnSaveButtonClick(EventArgs e)
        {

        }

        // 點 取消 按鈕
        protected override void OnCancelButtonClick(EventArgs e)
        {

        }

    }
}
