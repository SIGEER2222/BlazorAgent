using Serilog;
using System.Reflection;
using SqlSugar;

public static class SqlSugarInit {

  public static SqlSugarClient Db(string pgSqlConnection) => new SqlSugarClient(new ConnectionConfig {
    ConnectionString = pgSqlConnection,
    DbType = DbType.PostgreSQL,
    IsAutoCloseConnection = true,
    InitKeyType = InitKeyType.Attribute,
    ConfigureExternalServices = new ConfigureExternalServices {
      EntityService = (c, p) => {
        if (
          new NullabilityInfoContext()
         .Create(c).WriteState is NullabilityState.Nullable) {
          p.IsNullable = true;
        }
      }
    },
    MoreSettings = new ConnMoreSettings() {
      EnableCodeFirstUpdatePrecision = true,
      SqliteCodeFirstEnableDropColumn = true
    },
  },
   db => {
     db.Ado.CommandTimeOut = 15; //设置sql超时时间
     db.Aop.OnLogExecuted = (sql, pars) => {
       var elapsedMs = db.Ado.SqlExecutionTime.TotalMilliseconds;
       if (elapsedMs > 200) {
         Log.Warning("DB Slow {Elapsed}ms | {Sql}", elapsedMs, sql);
       }
     };

     // 记录执行发生错误的sql
     db.Aop.OnError = (exp) => {
       Log.Error(exp, exp.Message);
     };
    #if DEBUG
    db.Aop.OnLogExecuting = (sql, pars) => {
      Log.Information(sql);
    };
    #endif

     // 审计，记录更新前后的数据
     db.Aop.OnDiffLogEvent = it => {
       var editBeforeData = it.BeforeData;
       var editAfterData = it.AfterData;
       var sql = it.Sql;
       var parameter = it.Parameters;
       var data = it.BusinessData;
       var time = it.Time;
       var diffType = it.DiffType;
     };
   });
}
