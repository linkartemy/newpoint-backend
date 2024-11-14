using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using Google.Rpc;
using Newtonsoft.Json;

namespace NewPoint.Common.Models;

public class GrpcError
{
    [JsonProperty("message")]
    public string Message { get; set; }

    [JsonProperty("code")]
    public Code Code { get; set; }

    [JsonProperty("details")]
    public RepeatedField<Any> Details { get; set; }

    public GrpcError(string message, Code code, IEnumerable<BadRequest.Types.FieldViolation> fieldViolations)
    {
        Message = message;
        Code = code;
        Details = new RepeatedField<Any>
        {
            Any.Pack(new BadRequest
            {
                FieldViolations = { fieldViolations }
            })
        };
    }

    public GrpcError(string message, Code code)
    {
        Message = message;
        Code = code;
    }

    public GrpcError(string message)
    {
        Message = message;
    }

    public GrpcError()
    {
    }
}