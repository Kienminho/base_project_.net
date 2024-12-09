namespace Model.RequestModel.ParametersRequest.Interface
{
    public interface IPaginationModel
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
    }
}
