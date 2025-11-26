using HmiInspection.Models;
using InspectionSystem.Models;
using SqlSugar;

namespace InspectionSystem.Data;

public class InspectionDb {
  public SqlSugarClient Db { get; }

  public InspectionDb(string connectionString) {
    Db = SqlSugarInit.Db(connectionString);
  }
}
