using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SqlSugar;

namespace InspectionSystem.Models {
    #region 配置数据实体（只读）

    // 配置产线和设备表
    public partial class ProductionLine {
        /// <summary>
        /// 产线名称（如：MA5_Assembly）
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string ProductionLineName { get; set; } = string.Empty;

        /// <summary>
        /// 设备名称（如：MA5_001）
        /// </summary>
        [Required]
        public string DeviceName { get; set; } = string.Empty;

        [Required]
        public bool IsActive { get; set; } = true;
    }

    // 配置工单表
    public partial class WorkOrder {
        /// <summary>
        /// 工单名称（如：MA5_001）
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public bool IsActive { get; set; } = true;
    }


    /// <summary>
    /// 模板检验项配置（模板 + 对象类型 → 检查项、描述、单位）
    /// </summary>
    public class TemplateCheckItem {

        /// <summary>
        /// 模板名称（便于查询）
        /// </summary>
        [MaxLength(200)]
        public string TemplateName { get; set; } = string.Empty;

        /// <summary>
        /// 对象类型（如：设备、工装）
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string ObjectType { get; set; } = string.Empty;

        /// <summary>
        /// 检查对象（如：主设备）
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string CheckObject { get; set; } = string.Empty;

        /// <summary>
        /// 检查项（如：高度）
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string CheckItem { get; set; } = string.Empty;

        /// <summary>
        /// 检查描述（如：高度）
        /// </summary>
        [MaxLength(500)]
        public string CheckDescription { get; set; } = string.Empty;

        /// <summary>
        /// 单位（如：cm）
        /// </summary>
        [MaxLength(50)]
        public string Unit { get; set; } = string.Empty;

        [Required]
        public bool IsActive { get; set; } = true;

    }


    #endregion

    #region 业务数据实体（读写）

    /// <summary>
    /// 状态枚举
    /// </summary>
    public enum InspectionStatus {
        创建 = 0,
        开始 = 1,
        结束 = 2
    }

    /// <summary>
    /// 结果枚举
    /// </summary>
    public enum InspectionResult {
        OK = 0,
        NG = 1
    }

    /// <summary>
    /// 检验表单
    /// </summary>
    public class InspectionDoc {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }

        /// <summary>
        /// 模板名称
        /// </summary>
        [MaxLength(200)]
        public string TemplateName { get; set; } = string.Empty;

        /// <summary>
        /// 表单单号（如：M20251114-0001）
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string DocNumber { get; set; } = string.Empty;


        /// <summary>
        /// 产线名称（如：MA5_Assembly）- 独立下拉选择
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string ProductionLineName { get; set; } = string.Empty;

        /// <summary>
        /// 工单号列表（多个工单逗号分隔）- 独立下拉选择
        /// </summary>
        [Required]
        [MaxLength(1000)]
        public string WorkOrderNumbers { get; set; } = string.Empty;

        /// <summary>
        /// 创建人
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Creator { get; set; } = string.Empty;

        /// <summary>
        /// 创建时间
        /// </summary>
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// 检验对象列表
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public ICollection<InspectionObject> Objects { get; set; } = new List<InspectionObject>();
        [Required]
        public InspectionStatus Status { get; set; } = InspectionStatus.创建;
    }

    /// <summary>
    /// 检验对象列表
    /// </summary>
    public class InspectionObject {

        /// <summary>
        /// 对象 ID，不显示在界面上
        /// </summary>
        [Required]
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        public int ObjectId { get; set; }

        /// <summary>
        /// 模板 ID（用于带出检查项）
        /// </summary>
        [Required]
        public string TemplateName { get; set; }

        /// <summary>
        /// 所属表单单号
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string DocNumber { get; set; } = string.Empty;

        /// <summary>
        /// 对象类型（如：设备、工装）- 独立下拉选择
        /// 与模板组合后带出检查项
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string ObjectType { get; set; } = string.Empty;

        /// <summary>
        /// 对象名称（如：XXX-038）- 用户自由输入
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string ObjectName { get; set; } = string.Empty;

        /// <summary>
        /// 结果（由详情项的 NG 决定）
        /// </summary>
        public InspectionResult Result { get; set; } = InspectionResult.OK;

        /// <summary>
        /// 创建人
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Creator { get; set; } = string.Empty;

        /// <summary>
        /// 创建时间
        /// </summary>
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// 检验详情项
    /// </summary>
    public class InspectionDetail {

        /// <summary>
        /// 所属对象明细 ID，不显示在界面上
        /// </summary>
        [Required] public int ObjectId { get; set; }

        /// <summary>
        /// 检查对象（如：主设备）- 从配置带出
        /// </summary>
        [MaxLength(200)]
        public string CheckObject { get; set; } = string.Empty;

        /// <summary>
        /// 检查项（如：高度）- 从配置带出
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string CheckItem { get; set; } = string.Empty;

        /// <summary>
        /// 检查描述（如：高度）- 从配置带出
        /// </summary>
        [MaxLength(500)]
        public string CheckDescription { get; set; } = string.Empty;

        /// <summary>
        /// 单位（如：cm）- 从配置带出
        /// </summary>
        [MaxLength(50)]
        public string Unit { get; set; } = string.Empty;

        /// <summary>
        /// 数值（用户录入）
        /// </summary>
        [Column(TypeName = "decimal(18,4)")]
        public decimal? NumericValue { get; set; }

        /// <summary>
        /// 结果（OK/NG）
        /// </summary>
        [Required]
        public InspectionResult Result { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// 关联的对象明细
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        [ForeignKey(nameof(ObjectId))]
        public virtual InspectionObject Object { get; set; } = null!;
    }

    #endregion

    #region DTO 定义

    /// <summary>
    /// 新增表单 DTO
    /// </summary>
    public class CreateInspectionDocDto {
        [Required]
        public string DocNumber { get; set; } = string.Empty;

        /// <summary>
        /// 模板 ID（下拉选择）
        /// </summary>
        [Required]
        public int TemplateId { get; set; }

        /// <summary>
        /// 产线名称（下拉选择，独立）
        /// </summary>
        [Required]
        public string ProductionLineName { get; set; } = string.Empty;

        /// <summary>
        /// 工单号数组（多选下拉，独立）
        /// </summary>
        [Required]
        public List<string> WorkOrderNumbers { get; set; } = new();

        [Required]
        public string Creator { get; set; } = string.Empty;

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// 新增对象列表 DTO
    /// </summary>
    public class CreateInspectionObjectDto {


        [Required]
        public int TemplateId { get; set; }

        [Required]
        public string DocNumber { get; set; } = string.Empty;

        /// <summary>
        /// 对象类型（下拉选择，独立）
        /// </summary>
        [Required]
        public string ObjectType { get; set; } = string.Empty;

        /// <summary>
        /// 对象名称（下拉选择，独立）
        /// </summary>
        [Required]
        public string ObjectName { get; set; } = string.Empty;

        public string BatchNumber { get; set; } = string.Empty;

        [Required]
        public int TotalQuantity { get; set; }

        [Required]
        public string Creator { get; set; } = string.Empty;
    }

    /// <summary>
    /// 新增详情项 DTO
    /// </summary>
    public class CreateInspectionDetailDto {
        [Required]
        public int ObjectId { get; set; }

        public string CheckObject { get; set; } = string.Empty;

        [Required]
        public string CheckItem { get; set; } = string.Empty;

        public string CheckDescription { get; set; } = string.Empty;

        public string Unit { get; set; } = string.Empty;

        public decimal? NumericValue { get; set; }

        [Required]
        public InspectionResult Result { get; set; }
    }

    #endregion
}