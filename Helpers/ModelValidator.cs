using System.ComponentModel.DataAnnotations;

namespace wt_lab7_7postgres_efcore_kleshchenok.Helpers;

/// <summary>
/// В Minimal API атрибуты DataAnnotations НЕ проверяются автоматически (в отличие от Razor Pages,
/// где работает ModelState.IsValid). Поэтому проверяем модель вручную и возвращаем 400 Bad Request.
/// </summary>
public static class ModelValidator
{
    public static Dictionary<string, string[]>? Validate<T>(T model) where T : notnull
    {
        var results = new List<ValidationResult>();
        if (Validator.TryValidateObject(model, new ValidationContext(model), results, validateAllProperties: true))
            return null;

        return results
            .SelectMany(r => (r.MemberNames.Any() ? r.MemberNames : new[] { "" })
                .Select(m => (Member: m, Message: r.ErrorMessage ?? "Некорректное значение")))
            .GroupBy(x => x.Member)
            .ToDictionary(g => g.Key, g => g.Select(x => x.Message).ToArray());
    }
}
