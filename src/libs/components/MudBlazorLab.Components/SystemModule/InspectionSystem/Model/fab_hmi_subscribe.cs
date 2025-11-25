public class fab_hmi_subscribe {
    public string Subscribe_Id { get; set; }
    public Guid SysId { get; set; }
}

public class fab_hmi_subscribe_item {
    public Guid Subscribe_Sysid { get; set; }
    public string Object_Type { get; set; }
    public string Object_Name { get; set; }
}
