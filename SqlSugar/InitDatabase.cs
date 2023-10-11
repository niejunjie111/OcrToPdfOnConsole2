using SqlSugar;
using System;
using System.Configuration;

namespace OcrToPdfOnConsole2.SqlSugar
{
    public class InitDatabase
    {
        public static string RefreshEntity(string directoryPath, string nameSpace = "OcrToPdfOnConsole2.TableModels")
        {
            try
            {
                SqlSugarClient db = new SqlSugarClient(new ConnectionConfig()
                {
                    ConnectionString = ConfigurationSettings.AppSettings["SqlConnection"],//连接符字串
                    DbType = DbType.SqlServer,
                    IsAutoCloseConnection = true,
                    InitKeyType = InitKeyType.Attribute//从特性读取主键自增信息
                });
                foreach (var item in db.DbMaintenance.GetTableInfoList())
                {
                    string entityName = item.Name;/*实体名大写*/
                    db.MappingTables.Add(entityName, item.Name);
                    foreach (var col in db.DbMaintenance.GetColumnInfosByTableName(item.Name))
                    {
                        db.MappingColumns.Add(col.DbColumnName /*类的属性大写*/, col.DbColumnName, entityName);
                    }
                }
                db.DbFirst.CreateClassFile(directoryPath, nameSpace);
            }
            catch (Exception exp)
            {

            }
            return "Success";

        }
    }
}
