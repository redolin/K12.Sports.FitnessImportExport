using Campus.DocumentValidator;
using Campus.Import;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace K12.Sports.FitnessImportExport
{
    class Utility
    {
        /// <summary>
        /// 透過輸入的欄位名稱, 取的匯入資料的值
        /// </summary>
        /// <param name="row"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetIRowValueString(IRowStream row, string name)
        {
            if(row.Contains(name))
            {
                if(string.IsNullOrEmpty(row.GetValue(name)))
                    return "";

                return row.GetValue(name).Trim();
            }
            else
                return "";
        }

        /// <summary>
        /// 透過輸入的欄位名稱, 取的匯入資料的值, 並轉成DateTime
        /// </summary>
        /// <param name="row"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static DateTime? GetIRowValueDateTime(IRowStream row, string name)
        {
            if (row.Contains(name))
            {
                if (string.IsNullOrEmpty(row.GetValue(name)))
                    return null;

                DateTime dt;
                if (DateTime.TryParse(row.GetValue(name).Trim(), out dt))
                    return dt;
                else
                    return null;
            }
            else
                return null;
        }

        /// <summary>
        /// 透過輸入的欄位名稱, 取的匯入資料的值, 並轉成int
        /// </summary>
        /// <param name="row"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static int? GetIRowValueInt(IRowStream row, string name)
        {
            if (row.Contains(name))
            {
                if (string.IsNullOrEmpty(row.GetValue(name)))
                    return null;
                int retVal;
                if (int.TryParse(row.GetValue(name).Trim(), out retVal))
                    return retVal;
                else
                    return null;
            }
            else
                return null;
        }

        /// <summary>
        ///  把DateTime轉成民國年, 格式: yyyMMdd, 月跟日會補0到兩碼
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static string ConvertDateTimeToChineseDateTime(DateTime? dt)
        {
            
            if(dt.HasValue)
            {
                DateTime tmp = DateTime.Parse(dt.Value.ToString());

                int year = (tmp.Year) - 1911;
                int month = tmp.Month;
                int day = tmp.Day;

                string result = "" + year + (month.ToString()).PadLeft(2, '0') + (day.ToString()).PadLeft(2, '0');

                return result;
            }
            else
            {
                return "";
            }
            
            
        }

        /// <summary>
        /// 把文字轉成DateTime
        /// </summary>
        /// <param name="strDate"></param>
        /// <returns></returns>
        public static DateTime? ConvertStringToDateTime(string strDate)
        {
            DateTime dt;

            if(string.IsNullOrEmpty(strDate))
            {
                return null;
            }
            else
            {
                strDate = strDate.Trim();
            }

            if(DateTime.TryParse(strDate, out dt))
            {
                return dt;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 把DB的性別數字轉成輸出的數字
        /// </summary>
        /// <param name="strDbGender"></param>
        /// <returns></returns>
        public static string ConvertDBGenderToOutGender(string strDbGender)
        {
            // 男
            if(strDbGender == "男")
            {
                return "1";
            }
            //女
            else if (strDbGender == "女")
            {
                return "2";
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// 把輸入的性別數字轉成DB的數字
        /// </summary>
        /// <param name="strDbGender"></param>
        /// <returns></returns>
        public static string ConvertOutGenderToDBGender(string strInGender)
        {
            // 男
            if (strInGender == "男")
            {
                return "1";
            }
            // 女
            else if (strInGender == "女")
            {
                return "0";
            }
            else
            {
                return "";
            }
        }
    }
}
