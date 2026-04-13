using System.Text.RegularExpressions;

namespace EkycService.Api.Helpers;

public static class MaskingHelper
{
    private static readonly Regex Aadhaar =
        new(@"\b\d{12}\b", RegexOptions.Compiled);

    private static readonly Regex Mobile =
        new(@"\b\d{10}\b", RegexOptions.Compiled);

    private static readonly Regex Email =
        new(@"\b[\w\.-]+@[\w\.-]+\.\w+\b", RegexOptions.Compiled);

    public static string Mask(string? input)
    {
        if (string.IsNullOrEmpty(input))
            return input ?? string.Empty;

        input = Aadhaar.Replace(input, m =>
            $"XXXX-XXXX-{m.Value[^4..]}");

        input = Mobile.Replace(input, m =>
            $"******{m.Value[^4..]}");

        input = Email.Replace(input, m =>
        {
            var parts = m.Value.Split('@');
            return $"{parts[0][0]}***@{parts[1]}";
        });

        return input;
    }
}