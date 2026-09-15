// Staj.Application/Common/Helpers/SlugHelper.cs
using System.Globalization;
using System.Text;

namespace Staj.Application.Common.Helpers;

// Metinden URL-dostu slug üreten yardımcı
public static class SlugHelper
{
    public static string Generate(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        var normalized = input.Trim().ToLowerInvariant();

        // Programlama dili ve teknoloji adlarındaki özel karakterleri anlamlı sözcüklere çevir
        // (C# -> c-sharp, C++ -> c-plus-plus gibi)
        normalized = normalized
            .Replace("#", "-sharp")
            .Replace("++", "-plus-plus");

        // Türkçe karakter dönüşümü
        normalized = normalized
            .Replace("ı", "i").Replace("ğ", "g").Replace("ü", "u")
            .Replace("ş", "s").Replace("ö", "o").Replace("ç", "c");

        // Aksan kaldır
        var formD = normalized.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (var c in formD)
        {
            var uc = CharUnicodeInfo.GetUnicodeCategory(c);
            if (uc != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }
        normalized = sb.ToString().Normalize(NormalizationForm.FormC);

        // Alfa-numerik dışını tire ile değiştir
        var result = new StringBuilder();
        foreach (var c in normalized)
        {
            if (char.IsLetterOrDigit(c))
                result.Append(c);
            else if (c == ' ' || c == '-' || c == '_' || c == '.' || c == '+' || c == '/')
                result.Append('-');
        }

        var slug = result.ToString();

        // Ardışık tireleri tekilleştir
        while (slug.Contains("--"))
            slug = slug.Replace("--", "-");

        return slug.Trim('-');
    }
}