using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace K12.Sports.FitnessImportExport.ImportExport
{
    /// <summary>
    /// 匯出學生體適能
    /// </summary>
    class ExportStudentFitness : SmartSchool.API.PlugIn.Export.Exporter
    {
        // 可勾選選項
        //List<string> ExportItemList;

        public ExportStudentFitness()
        {
            this.Image = null;
            this.Text = "匯出學生體適能";
            //ExportItemList = new List<string>();
            //ExportItemList.Add("");

        }

        public override void InitializeExport(SmartSchool.API.PlugIn.Export.ExportWizard wizard)
        {
            //wizard.ExportableFields.AddRange(ExportItemList);
            //wizard.ExportPackage += delegate(object sender, SmartSchool.API.PlugIn.Export.ExportPackageEventArgs e)
            //{
            //    RowData row = new RowData();

            //    foreach (string field in e.ExportFields)
            //    {
            //        if (wizard.ExportableFields.Contains(field))
            //        {
            //            switch (field)
            //            {
            //                case "": break;
            //            }
            //        }
            //    }
            //    e.Items.Add(row);

            //    // 寫 Log
            //    // ApplicationLog.Log();                
            //};
        }
    }
}
