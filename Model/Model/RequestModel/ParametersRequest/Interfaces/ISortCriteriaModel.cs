namespace Model.RequestModel.ParametersRequest.Interface
{
    public interface ISortCriteriaModel
    {
        public string? Field { get; set; }
        public bool IsDescending { get; set; }
    }
}
