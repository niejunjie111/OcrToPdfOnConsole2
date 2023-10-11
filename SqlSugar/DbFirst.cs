using SqlSugar;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SqlSugar;
using System.Xml.Linq;

namespace OcrToPdfOnConsole2.SqlSugar
{
    public static class DbFirst
    {
        public static void AddSqlsugarSetup(string configuration)
        {
            SqlSugarClient db = new SqlSugarClient(new ConnectionConfig()
            {
                DbType = DbType.SqlServer,
                ConnectionString = configuration,
                IsAutoCloseConnection = true,
                InitKeyType = InitKeyType.Attribute
            },
                ado =>
                {
                    //单例参数配置，所有上下文生效
                    ado.Aop.OnLogExecuting = (sql, pars) =>
                    {
                        Console.WriteLine(sql);//输出sql
                        Console.WriteLine(string.Join(",", pars?.Select(it => it.ParameterName + ":" + it.Value)));//参数
                    };
                });
        }

    }
}
