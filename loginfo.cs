using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using SCJ.Common;
using SCJ.DataHelper;
using System.IO;

namespace SCJ.Log
{
    /// <summary>
    /// 日志信息
    /// </summary>
    /// <author>gsc</author>
    /// <date>2012-11-22</date>
    public class LogInfo
    {
        #region 成员方法
        /// <summary>
        /// 增加一条数据
        /// </summary>
        /// <example>
        /// <code>
        ///LogInfo_Add(new LogInfoModel()
        ///    {
        ///        ObjectID = 0,//用户ID
        ///        Platform = (int)LogInfoModel.PlatformValue.Base,//所属平台
        ///        Status = (int)LogInfoModel.StatusValue.Normal,//日志类型
        ///        Remark = String.Empty,//日志内容
        ///        Source = (int)LogInfoModel.SourceValue.User,//来源
        ///        LogType = (int)LogInfoModel.LogTypeValue.Operate,//日志类型
        ///        ObjectValue = String.Empty,
        ///        OperatorType = 0,//预留
        ///    });
        /// </code>
        /// </example>
        /// <author>gsc</author>
        /// <date>2012-11-22</date>
        public static Int32 LogInfo_Add(LogInfoModel _LogInfo)
        {
            SqlParameter[] myParams =
            {
               
                new SqlParameter("@LogType",SqlDbType.Int ,4),
                new SqlParameter("@Platform",SqlDbType.Int ,4),
                new SqlParameter("@Source",SqlDbType.Int ,4),
                new SqlParameter("@Status",SqlDbType.Int ,4),
                new SqlParameter("@OperatorType",SqlDbType.Int ,4),
                new SqlParameter("@ObjectID",SqlDbType.Int ,4),
                new SqlParameter("@ObjectValue",SqlDbType.VarChar ,64),
                new SqlParameter("@Remark",SqlDbType.NVarChar ,3072),
                new SqlParameter("@CreateTime",SqlDbType.DateTime ,8),
                new SqlParameter("@Ret",SqlDbType.Int)
            };

           
            myParams[0].Value = _LogInfo.LogType;
            myParams[1].Value = _LogInfo.Platform;
            myParams[2].Value = _LogInfo.Source;
            myParams[3].Value = _LogInfo.Status;
            myParams[4].Value = _LogInfo.OperatorType;
            myParams[5].Value = _LogInfo.ObjectID;
            myParams[6].Value = _LogInfo.ObjectValue;
            myParams[7].Value = _LogInfo.Remark;
            myParams[8].Value = _LogInfo.CreateTime;
            myParams[9].Direction = ParameterDirection.ReturnValue;

            SqlHelper.ExecuteNonQuery(SCJ.SystemConfig.Common.SCJ_LogConnectionString, CommandType.StoredProcedure, "LogInfo_Add", myParams);
            //异常错误发送邮件
            if (_LogInfo.Status == (int)LogInfoModel.StatusValue.Exception)
            {
                string[] arryEmail = BaseCommon.GetConfigurationSettings("ErrorReport_Email").Split(',');
                //string[] arryMobile = BaseCommon.GetConfigurationSettings("ErrorReport_Mobile").Split(',');
                for (int i = 0; i < arryEmail.Length; i++)
                {
                    if (arryEmail[i] != string.Empty)
                    {
                        BaseCommon.SendEmail(arryEmail[i], "SCJ错误报告", _LogInfo.Remark);

                    }
                }
            }
            return ConvertUtility.ToInt32(myParams[9].Value);
        }
        #endregion

        /// <summary>
        /// 获取日志列表
        /// </summary>
        /// <param name="iLogType">日志类型（枚举：系统日志、操作日志）</param>
        /// <param name="iPlatform">所属平台（枚举：系统、投资商、运营商、物业）</param>
        /// <param name="iSource">来源（枚举：用户操作、管理员操作、SQL作业、后台服务、系统操作）</param>
        /// <param name="iStatus">状态（枚举：正常、异常）</param>
        /// <param name="iOperatorType">操作类型（预留）</param>
        /// <param name="iObjectID">操作员ID（各个平台的员工ID）</param>
        /// <param name="strObjectValue">相关对象</param>
        /// <param name="strRemark">备注</param>
        /// <param name="dCreateTime">创建时间（必填）  根据时间找到日志所在表</param>
        /// <param name="iPageIndex">第n页，从1开始</param>
        /// <param name="iPageSize">每页记录数</param>
        /// <param name="iTotalCount">总共条数</param>
        /// <returns></returns>
        /// <author>gsc</author>
        /// <date>2012-11-22</date>
        public static List<LogInfoModel> GetLogInfoList(Int32 iLogType, Int32 iPlatform, Int32 iSource, Int32 iStatus, Int32 iOperatorType, Int32 iObjectID, String strObjectValue, String strRemark, DateTime dCreateTimeBegin, DateTime dCreateTimeEnd, Int32 iPageIndex, Int32 iPageSize, out Int32 iTotalCount)
        {
            SqlParameter[] myParams =
                {
                    new SqlParameter("@LogType",SqlDbType.Int ,4),
                    new SqlParameter("@Platform",SqlDbType.Int ,4),
                    new SqlParameter("@Source",SqlDbType.Int ,4),
                    new SqlParameter("@Status",SqlDbType.Int ,4),
                    new SqlParameter("@OperatorType",SqlDbType.Int ,4),
                    new SqlParameter("@ObjectID",SqlDbType.Int ,4),
                    new SqlParameter("@ObjectValue",SqlDbType.VarChar ,64),
                    new SqlParameter("@Remark",SqlDbType.NVarChar ,3072),
                    new SqlParameter("@CreateTimeBegin",SqlDbType.DateTime),
                    new SqlParameter("@CreateTimeEnd",SqlDbType.DateTime),
                    new SqlParameter("PageIndex",SqlDbType.Int ,4), 
                    new SqlParameter("PageSize",SqlDbType.Int ,4), 
                    new SqlParameter("TotalCount",SqlDbType.Int ,4), 
                };


            myParams[0].Value = iLogType;
            myParams[1].Value = iPlatform;
            myParams[2].Value = iSource;
            myParams[3].Value = iStatus;
            myParams[4].Value = iOperatorType;
            myParams[5].Value = iObjectID;
            myParams[6].Value = strObjectValue;
            myParams[7].Value = strRemark;
            myParams[8].Value = dCreateTimeBegin;
            myParams[9].Value = dCreateTimeEnd;
            myParams[10].Value = iPageIndex;
            myParams[11].Value = iPageSize;
            myParams[12].Direction = ParameterDirection.Output;

            DataSet ds = SqlHelper.ExecuteDataset(SCJ.SystemConfig.Common.SCJ_LogConnectionString, CommandType.StoredProcedure, "LogInfo_GetListByPageIndex", myParams);
            iTotalCount = ConvertUtility.ToInt32(myParams[12].Value);
            if (ds.Tables.Count > 0)
            {
                return ConvertUtility.ToList<LogInfoModel>(ds.Tables[0]);
            }
            else
            {
                return new List<LogInfoModel>();
            }

        }

        #region 打印文本日志
        /// <summary>
        /// 打印文本日志
        /// </summary>
        /// <param name="strLogPath">日志存放目录（相对目录，如Log，如果不存在会在应用程序下建一个Log的目录）</param>
        /// <param name="strContent">文本内容</param>
        /// <returns>执行结果</returns>
        /// <Author>Hukl</Author>
        /// <Date>2012-2-24</Date>
        public static Boolean PrintTextLog(String strLogPath, String strContent)
        {
            try
            {
                String LogDir = String.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, strLogPath);
                if (!Directory.Exists(LogDir))
                {
                    Directory.CreateDirectory(LogDir);
                }
                StreamWriter sw = System.IO.File.AppendText(String.Format("{0}\\{1}.log", LogDir, DateTime.Now.ToString("yyyy-MM-dd")));
                sw.WriteLine("{0} {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss ffff"), strContent);
                sw.Close();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        /// <summary>
        /// 打印文本日志（默认放在程序根目录的Log目录下，如果不存在会在应用程序下建一个Log的目录）
        /// </summary>
        /// <param name="strContent">文本内容</param>
        /// <returns>执行结果</returns>
        /// <Author>Hukl</Author>
        /// <Date>2012-12-11</Date>
        public static Boolean PrintTextLog(String strContent)
        {
            return PrintTextLog("Log", strContent);
        }
        #endregion

    }




}
