using SqlSugar;

namespace HmiInspection.Models;
/// <summary>
/// 检验表单模板
/// </summary>
[SugarTable("fab_hmi_inspection_form_template")]
public class InspectionFormTemplate {
  [SugarColumn(IsPrimaryKey = true, ColumnName = "sysid")]
  public Guid Sysid { get; set; }

  [SugarColumn(Length = 40, ColumnName = "formTemplateName")]
  public string FormTemplateName { get; set; }

  [SugarColumn(Length = 40, ColumnName = "formType")]
  public string FormType { get; set; }

  [SugarColumn(ColumnName = "createTime")]
  public DateTime? CreateTime { get; set; }

  [SugarColumn(ColumnName = "updateTime")]
  public DateTime? UpdateTime { get; set; }
}

/// <summary>
/// 检验表单模板对象
/// </summary>
[SugarTable("fab_hmi_inspection_form_template_object")]
public class InspectionFormTemplateObject {
  [SugarColumn(IsPrimaryKey = true, ColumnName = "sysid")]
  public Guid Sysid { get; set; }

  [SugarColumn(ColumnName = "FormTemplateSysid")]
  public Guid FormTemplateSysid { get; set; }

  /// <summary>
  /// 物料类型
  /// </summary>
  [SugarColumn(Length = 40, ColumnName = "objectType")]
  public string ObjectType { get; set; }

  /// <summary>
  /// 物料名称
  /// </summary>
  [SugarColumn(Length = 40, ColumnName = "objectName")]
  public string ObjectName { get; set; }

  /// <summary>
  /// 是否需要物料
  /// </summary>
  [SugarColumn(ColumnName = "IsRequiredSN")]
  public bool? IsRequiredSN { get; set; }

  /// <summary>
  /// 抽样率
  /// </summary>
  [SugarColumn(ColumnName = "samplingRatio")]
  public decimal? SamplingRatio { get; set; }
}

/// <summary>
/// 检验表单模板对象项目
/// </summary>
[SugarTable("fab_hmi_inspection_form_template_object_item")]
public class InspectionFormTemplateObjectItem {
  [SugarColumn(IsPrimaryKey = true, ColumnName = "sysid")]
  public Guid Sysid { get; set; }

  [SugarColumn(ColumnName = "formTemplateObjectSysid")]
  public Guid FormTemplateObjectSysid { get; set; }

  [SugarColumn(Length = 40, ColumnName = "itemName")]
  public string ItemName { get; set; }

  [SugarColumn(Length = 40, ColumnName = "Unit")]
  public string Unit { get; set; }

  [SugarColumn(ColumnDataType = "longtext,text,clob", ColumnName = "ItemDescription")]
  public string ItemDescription { get; set; }
}


