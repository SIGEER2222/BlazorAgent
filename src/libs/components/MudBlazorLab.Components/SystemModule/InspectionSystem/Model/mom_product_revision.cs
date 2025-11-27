using SqlSugar;

[SugarTable("mom_product_revision")]
public partial class mom_product_revision {

  [SugarColumn(IsPrimaryKey = true)]
  public Guid SysId { get; set; }


  public Guid? PrimaryPlanSysId { get; set; }


  public bool IsComponent { get; set; }


  public string Name { get; set; }


  public string ConcurrencyStamp { get; set; } = "";


  public int Revision { get; set; }


  public Guid MasterSysId { get; set; }


  public string RevisionState { get; set; }


  public string? Description { get; set; }

}
