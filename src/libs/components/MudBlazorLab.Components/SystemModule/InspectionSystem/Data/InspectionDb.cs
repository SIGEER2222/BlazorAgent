using SqlSugar;
using InspectionSystem.Models;
using HmiInspection.Models;

namespace InspectionSystem.Data;

public class InspectionDb {
    public SqlSugarClient Db { get; }

    public InspectionDb(string connectionString) {
        Db = new SqlSugarClient(new ConnectionConfig {
            ConnectionString = connectionString,
            DbType = DbType.PostgreSQL,
            IsAutoCloseConnection = true,
        });

        Db.CodeFirst.InitTables(typeof(InspectionFormTemplate), typeof(InspectionFormTemplateObject), typeof(InspectionFormTemplateObjectItem),
             typeof(InspectionForm), typeof(InspectionFormObject), typeof(InspectionFormObjectSample), typeof(InspectionFormObjectSampleItem));
    }
}
