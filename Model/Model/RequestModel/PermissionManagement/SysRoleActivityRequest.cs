namespace Model.RequestModel.PermissionManagement;

public class SysRoleActivityRequest
{
    public Guid Id { get; set; }
    public bool C { get; set; } = false;
    public bool R { get; set; } = false;
    public bool U { get; set; } = false;
    public bool D { get; set; } = false;
}