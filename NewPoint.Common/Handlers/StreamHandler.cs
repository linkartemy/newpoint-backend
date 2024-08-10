namespace NewPoint.Common.Handlers;

public static class StreamHandler
{
    public static byte[] StreamToBytes(Stream stream)
    {
        using (var ms = new MemoryStream())
        {
            stream.CopyTo(ms);
            return ms.ToArray();
        }
    }
}