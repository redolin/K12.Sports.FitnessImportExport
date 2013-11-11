using FISCA.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace K12.Sports.FitnessImportExport.DAO
{
    /// <summary>
    /// 使用 FISCA.Data Query
    /// </summary>
    class FDQuery
    {
        /// <summary>
        /// 取得所有學生學號 key:StudentNumber ; value:StudentID
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, string> GetAllStudenNumberDict()
        {
            Dictionary<string, string> retVal = new Dictionary<string, string>();
            QueryHelper qh = new QueryHelper();
            string strSQL = "select student.student_number,student.id from student where student.status='1';";
            DataTable dt = qh.Select(strSQL);
            foreach (DataRow dr in dt.Rows)
            {
                string key = dr[0].ToString();
                string value = dr[1].ToString();
                if (!retVal.ContainsKey(key))
                    retVal.Add(key, value);
            }

            return retVal;
        }

        /// <summary>
        /// 依學號取得學生ID StudentNumber,id
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, string> GetStudenIdDictByStudentNumber(List<string> StudNumList)
        {
            Dictionary<string, string> retVal = new Dictionary<string, string>();
            QueryHelper qh = new QueryHelper();
            StringBuilder sbQry = new StringBuilder();
            sbQry.Append("'"); sbQry.Append(string.Join("','", StudNumList.ToArray())); sbQry.Append("'");
            string strSQL = "select student.student_number, student.id from student where student_number in(" + sbQry.ToString() + ") and student.status='1';";
            DataTable dt = qh.Select(strSQL);
            
            foreach (DataRow dr in dt.Rows)
            {
                string key = dr[0].ToString();
                string value = dr[1].ToString();
                if (!retVal.ContainsKey(key))
                    retVal.Add(key, value);
            }

            return retVal;
        }

    }
}
