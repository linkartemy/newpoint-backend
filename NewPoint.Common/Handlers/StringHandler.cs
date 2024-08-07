namespace NewPoint.Common.Handlers;

public static class StringHandler
{
    private const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
    public static string GenerateString(int length)
    {
        var random = new Random();
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}