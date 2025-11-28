namespace HmiInspection.Models;

using SqlSugar;

/// <summary>
/// 检验表单
/// </summary>
[SugarTable("fab_hmi_inspection_form")]
public class InspectionForm
{
  [SugarColumn(IsPrimaryKey = true, ColumnName = "sysid")]
  public Guid Sysid { get; set; }

  /// <summary>
  /// 表单单号
  /// </summary>
  [SugarColumn(Length = 40, ColumnName = "FormNo")]
  public string FormNo { get; set; }

  /// <summary>
  /// 表单模板
  /// </summary>
  [SugarColumn(Length = 40, ColumnName = "FormTemplateName")]
  public string FormTemplateName { get; set; }

  /// <summary>
  /// 模板类型
  /// </summary>
  [SugarColumn(Length = 40, ColumnName = "formType")]
  public string FormType { get; set; }

  /// <summary>
  /// ERP单号
  /// </summary>
  [SugarColumn(Length = 12, ColumnName = "TicketNo", IsNullable = true)]
  public string? TicketNo { get; set; }

  /// <summary>
  /// 产线名称
  /// </summary>
  [SugarColumn(Length = 40, ColumnName = "ProductName")]
  public string ProductName { get; set; }

  /// <summary>
  /// 状态
  /// </summary>
  [SugarColumn(Length = 40, ColumnName = "Status")]
  public string Status { get; set; }

  /// <summary>
  /// 创建日期
  /// </summary>
  [SugarColumn(ColumnName = "TxnTime", ColumnDataType = "timestamp", IsNullable = true)]
  public DateTime? TxnTime { get; set; }

  /// <summary>
  /// 创建人
  /// </summary>
  [SugarColumn(Length = 40, ColumnName = "CreateUser")]
  public string CreateUser { get; set; }

  /// <summary>
  /// 工单单号
  /// </summary>
  [SugarColumn(ColumnDataType = "longtext,text,clob", ColumnName = "WorkCenter")]
  public string WorkCenter { get; set; }
}

/// <summary>
/// 检验表单对象
/// </summary>
[SugarTable("fab_hmi_inspection_form_object")]
public class InspectionFormObject
{
  [SugarColumn(IsPrimaryKey = true, ColumnName = "sysid")]
  public Guid Sysid { get; set; }

  /// <summary>
  /// 表单sysid
  /// </summary>
  [SugarColumn(ColumnName = "formSysid")]
  public Guid FormSysid { get; set; }

  /// <summary>
  /// 对象类型
  /// </summary>
  [SugarColumn(Length = 40, ColumnName = "objectType")]
  public string ObjectType { get; set; }

  /// <summary>
  /// 对象名称
  /// </summary>
  [SugarColumn(Length = 40, ColumnName = "objectName")]
  public string ObjectName { get; set; }

  /// <summary>
  /// 模版定义总量
  /// </summary>
  [SugarColumn(ColumnName = "TemplateSamplingRatio", IsNullable = true)]
  public decimal? TemplateSamplingRatio { get; set; }

  /// <summary>
  /// 总量
  /// </summary>
  [SugarColumn(ColumnName = "TotalQuantity", IsNullable = true)]
  public long? TotalQuantity { get; set; }

  /// <summary>
  /// 抽样数
  /// </summary>
  [SugarColumn(ColumnName = "SampleQuantity", IsNullable = true)]
  public long? SampleQuantity { get; set; }

  /// <summary>
  /// 实际抽样比
  /// </summary>
  [SugarColumn(ColumnName = "actualSamplingRatio", IsNullable = true)]
  public decimal? ActualSamplingRatio { get; set; }

  /// <summary>
  /// 检验结果
  /// </summary>
  [SugarColumn(Length = 40, ColumnName = "CheckResult")]
  public string CheckResult { get; set; }
}

/// <summary>
/// 检验表单对象样品明细（展平）
/// </summary>
[SugarTable("fab_hmi_inspection_form_object_sample_flat")]
public class InspectionFormObjectSampleFlat
{
  [SugarColumn(IsPrimaryKey = true, ColumnName = "sysid")]
  public Guid Sysid { get; set; }

  /// <summary>
  /// 表单对象sysid
  /// </summary>
  [SugarColumn(ColumnName = "formObjectSysid")]
  public Guid FormObjectSysid { get; set; }

  /// <summary>
  /// 批次(可为空)
  /// </summary>
  [SugarColumn(Length = 40, ColumnName = "SampleBatchNo", IsNullable = true)]
  public string? SampleBatchNo { get; set; }

  /// <summary>
  /// 载具名称(可为空)
  /// </summary>
  [SugarColumn(Length = 40, ColumnName = "CarrierName", IsNullable = true)]
  public string? CarrierName { get; set; }

  /// <summary>
  /// 检查项
  /// </summary>
  [SugarColumn(Length = 40, ColumnName = "itemName")]
  public string ItemName { get; set; }

  /// <summary>
  /// 数值
  /// </summary>
  [SugarColumn(ColumnName = "Value", IsNullable = true)]
  public decimal? Value { get; set; }

  /// <summary>
  /// 单位
  /// </summary>
  [SugarColumn(Length = 40, ColumnName = "Unit", IsNullable = true)]
  public string? Unit { get; set; }

  /// <summary>
  /// 检验结果
  /// </summary>
  [SugarColumn(Length = 40, ColumnName = "CheckResult")]
  public string CheckResult { get; set; }

  /// <summary>
  /// 描述
  /// </summary>
  [SugarColumn(ColumnDataType = "longtext,text,clob", ColumnName = "ItemDescription", IsNullable = true)]
  public string? ItemDescription { get; set; }

  [SugarColumn(ColumnName = "BatchIndex", IsNullable = true)]
  public int? BatchIndex { get; set; }
}
