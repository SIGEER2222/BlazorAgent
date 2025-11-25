namespace HmiInspection.Models;

using SqlSugar;

/// <summary>
/// 检验表单
/// </summary>
[SugarTable("fab_hmi_Inspection_form")]
public class InspectionForm {
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
    [SugarColumn(Length = 12, ColumnName = "TicketNo")]
    public string TicketNo { get; set; }

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
    [SugarColumn(ColumnName = "TxnTime")]
    public DateTime? TxnTime { get; set; }

    /// <summary>
    /// 创建人
    /// </summary>
    [SugarColumn(Length = 40, ColumnName = "CreateUser")]
    public string CreateUser { get; set; }

    /// <summary>
    /// 工单单号
    /// </summary>
    [SugarColumn(Length = 2048, ColumnName = "WorkCenter")]
    public string WorkCenter { get; set; }
}

/// <summary>
/// 检验表单对象
/// </summary>
[SugarTable("fab_hmi_Inspection_form_object")]
public class InspectionFormObject {
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
    [SugarColumn(ColumnName = "TemplateSamplingRatio")]
    public decimal? TemplateSamplingRatio { get; set; }

    /// <summary>
    /// 总量
    /// </summary>
    [SugarColumn(ColumnName = "TotalQuantity")]
    public long? TotalQuantity { get; set; }

    /// <summary>
    /// 抽样数
    /// </summary>
    [SugarColumn(ColumnName = "SampleQuantity")]
    public long? SampleQuantity { get; set; }

    /// <summary>
    /// 实际抽样比
    /// </summary>
    [SugarColumn(ColumnName = "actualSamplingRatio")]
    public decimal? ActualSamplingRatio { get; set; }

    /// <summary>
    /// 检验结果
    /// </summary>
    [SugarColumn(Length = 40, ColumnName = "CheckResult")]
    public string CheckResult { get; set; }
}

/// <summary>
/// 检验表单对象样品
/// </summary>
[SugarTable("fab_hmi_Inspection_form_Object_Sample")]
public class InspectionFormObjectSample {
    [SugarColumn(IsPrimaryKey = true, ColumnName = "sysid")]
    public Guid Sysid { get; set; }

    /// <summary>
    /// 表单对象sysid
    /// </summary>
    [SugarColumn(Length = 40, ColumnName = "formObjectSysid")]
    public Guid FormObjectSysid { get; set; }

    /// <summary>
    /// 批次(可为空)
    /// </summary>
    [SugarColumn(Length = 40, ColumnName = "SampleBatchNo")]
    public string SampleBatchNo { get; set; }

    /// <summary>
    /// 载具名称(可为空)
    /// </summary>
    [SugarColumn(Length = 40, ColumnName = "CarrierName")]
    public string CarrierName { get; set; }

    /// <summary>
    /// 检验结果
    /// </summary>
    [SugarColumn(Length = 40, ColumnName = "CheckResult")]
    public string CheckResult { get; set; }
}

/// <summary>
/// 检验表单对象样品项目
/// </summary>
[SugarTable("fab_hmi_Inspection_form_Object_Sample_Item")]
public class InspectionFormObjectSampleItem {
    [SugarColumn(IsPrimaryKey = true, ColumnName = "sysid")]
    public Guid Sysid { get; set; }

    /// <summary>
    /// 表单对象sysid
    /// </summary>
    [SugarColumn(ColumnName = "formObjectSampleSysid")]
    public Guid FormObjectSampleSysid { get; set; }

    /// <summary>
    /// 检查项
    /// </summary>
    [SugarColumn(Length = 40, ColumnName = "itemName")]
    public string ItemName { get; set; }

    /// <summary>
    /// 数值
    /// </summary>
    [SugarColumn(ColumnName = "Value")]
    public decimal? Value { get; set; }

    /// <summary>
    /// 单位
    /// </summary>
    [SugarColumn(Length = 40, ColumnName = "Unit")]
    public string Unit { get; set; }

    /// <summary>
    /// 检验结果
    /// </summary>
    [SugarColumn(Length = 40, ColumnName = "CheckResult")]
    public string CheckResult { get; set; }

    [SugarColumn(Length = 2048, ColumnName = "itemdesc")]
    public string Itemdesc { get; set; }
}