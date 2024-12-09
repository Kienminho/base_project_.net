namespace Model.RequestModel.PermissionManagement;

public class UserPermissionBaseResponse
{
    public Guid? ActivityId { get; set; }
    public string? ActivityName { get; set; }
    public string? Code { get; set; }
    public bool? C { get; set; }
    public bool? R { get; set; }
    public bool? U { get; set; }
    public bool? D { get; set; }
}