namespace Model.ResponseModel.PermissionManagement
{
    public class SysRoleActivitiesResponse
    {
        public Guid Id { get; set; }
        public Guid ActivityId { get; set; }
        public string? ActivityName { get; set; }
        public bool? C { get; set; }
        public bool? R { get; set; }
        public bool? U { get; set; }
        public bool? D { get; set; }
    }
}
