namespace Model.ResponseModel.PermissionManagement;

public class SysRoleResponse
{
    public Guid Id { get; set; }
    public Guid RoleId { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedDate { get; set; }
    public string? Creator { get; set; }
}