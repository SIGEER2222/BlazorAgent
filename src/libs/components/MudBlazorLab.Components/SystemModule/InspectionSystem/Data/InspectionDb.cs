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

        try {
            Db.CodeFirst.InitTables(typeof(InspectionDoc), typeof(InspectionObject), typeof(InspectionDetail), typeof(TemplateInfo), typeof(TemplateCheckItem), typeof(ProductionLine), typeof(WorkOrder));
            Db.CodeFirst.InitTables(typeof(InspectionFormTemplate), typeof(InspectionFormTemplateObject), typeof(InspectionFormTemplateObjectItem),
                 typeof(InspectionForm), typeof(InspectionFormObject), typeof(InspectionFormObjectSample), typeof(InspectionFormObjectSampleItem));
        }
        catch (Exception ex) {
            var msg = ex.Message ?? string.Empty;
            var needRecreate = msg.Contains("Sqlite no support alter column primary key", StringComparison.OrdinalIgnoreCase)
                || msg.Contains("Specified method is not supported", StringComparison.OrdinalIgnoreCase)
                || msg.Contains("bind error", StringComparison.OrdinalIgnoreCase);
            if (needRecreate) {
                Db.DbMaintenance.DropTable("InspectionDetail");
                Db.DbMaintenance.DropTable("InspectionObject");
                Db.DbMaintenance.DropTable("InspectionDoc");
                Db.CodeFirst.InitTables(typeof(InspectionDoc), typeof(InspectionObject), typeof(InspectionDetail), typeof(TemplateInfo), typeof(TemplateCheckItem), typeof(ProductionLine), typeof(WorkOrder));
            }
            else throw;
        }
        Db.QueryFilter.AddTableFilter<ProductionLine>(p => p.IsActive == true);
        Db.QueryFilter.AddTableFilter<WorkOrder>(p => p.IsActive == true);
        Db.QueryFilter.AddTableFilter<TemplateInfo>(p => p.IsActive == true);
        Db.QueryFilter.AddTableFilter<TemplateCheckItem>(p => p.IsActive == true);
    }

    public ISugarQueryable<InspectionDoc> Docs => Db.Queryable<InspectionDoc>();
    public ISugarQueryable<InspectionObject> Objects => Db.Queryable<InspectionObject>();
    public ISugarQueryable<InspectionDetail> Details => Db.Queryable<InspectionDetail>();
    public ISugarQueryable<TemplateInfo> Templates => Db.Queryable<TemplateInfo>();
    public ISugarQueryable<TemplateCheckItem> TemplateItems => Db.Queryable<TemplateCheckItem>();
    public ISugarQueryable<ProductionLine> Lines => Db.Queryable<ProductionLine>();
    public ISugarQueryable<WorkOrder> WorkOrders => Db.Queryable<WorkOrder>();


}