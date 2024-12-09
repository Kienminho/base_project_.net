using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Model.RequestModel;
using Model.RequestModel.Common;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Common.Authorization
{
    public class CustomHeaderParameter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            operation.Parameters ??= new List<OpenApiParameter>();
            var customHeaderParameter = new OpenApiParameter
            {
                Name = APIResourceRequest.X_DEVICE_UDID,
                In = ParameterLocation.Header,
                Required = false,
                Schema = new OpenApiSchema
                {
                    Type = "string",
                    Default = new OpenApiString("00000000-0000-0000-0000-000000000000")
                }
            };

            operation.Parameters.Insert(0, customHeaderParameter);
        }
    }
}
