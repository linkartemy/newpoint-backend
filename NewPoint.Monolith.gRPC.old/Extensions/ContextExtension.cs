using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using NewPoint.Models;

namespace NewPoint.Extensions;

public static class ContextExtension
{
    public static User RetrieveUser(this ServerCallContext context)
    {
        return (User)context.UserState["user"];
    }

    public static string RetrieveToken(this ServerCallContext context)
    {
        return (string)context.UserState["token"];
    }
}