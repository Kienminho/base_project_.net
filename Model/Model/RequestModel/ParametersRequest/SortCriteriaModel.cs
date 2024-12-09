using Model.RequestModel.ParametersRequest.Interface;

namespace Model.RequestModel.ParametersRequest
{
    public class SortCriteriaModel : ISortCriteriaModel
    {
        public string? Field { get; set; }
        public bool IsDescending { get; set; } = false;
    }
}
