
namespace InspectionSystem.Models {

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


    #endregion
}