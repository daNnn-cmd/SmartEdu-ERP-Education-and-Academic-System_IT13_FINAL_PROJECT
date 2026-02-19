using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace SmartEduERP.Services
{
    /// <summary>
    /// Central helper for sanitizing and validating input models before they are saved
    /// or sent to sync services. This enforces DataAnnotations and blocks script / SQL-like input.
    /// </summary>
    public static class ValidationHelper
    {
        private static readonly Regex ScriptLikeRegex = new(
            pattern: @"(<\s*script\b|onerror\s*=|onload\s*=|javascript:|data:|eval\(|alert\(|SELECT\s+.+\s+FROM|INSERT\s+INTO|UPDATE\s+\w+\s+SET|DELETE\s+FROM|DROP\s+TABLE)",
            options: RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex MultiWhitespaceRegex = new(
            pattern: @"\s+",
            options: RegexOptions.Compiled);

        /// <summary>
        /// Sanitize all string properties on the model and then run DataAnnotations validation.
        /// Throws <see cref="ValidationException"/> when validation fails or unsafe content is detected.
        /// </summary>
        public static void SanitizeAndValidateModel(object instance)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));

            SanitizeStringProperties(instance);
            RunDataAnnotationsValidation(instance);
        }

        /// <summary>
        /// Basic name sanitization helper for use in UI code before assigning to models.
        /// Trims, collapses whitespace, and removes characters outside letters, spaces, hyphens and apostrophes.
        /// Also blocks script/SQL-like input.
        /// </summary>
        public static string SanitizeName(string? value, int maxLength = 100)
        {
            if (string.IsNullOrWhiteSpace(value)) return string.Empty;

            value = value.Trim();
            value = Regex.Replace(value, @"[^A-Za-z\s\-']", string.Empty);
            value = CollapseWhitespace(value);

            if (value.Length > maxLength)
                value = value.Substring(0, maxLength);

            // Simple title casing per word
            var words = value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < words.Length; i++)
            {
                var w = words[i];
                if (w.Length == 0) continue;
                words[i] = char.ToUpperInvariant(w[0]) + (w.Length > 1 ? w[1..].ToLowerInvariant() : string.Empty);
            }

            value = string.Join(' ', words);

            EnsureNoScriptOrSql(value, "Name");
            return value;
        }

        /// <summary>
        /// Trim and normalize email, removing spaces and blocking unsafe patterns.
        /// </summary>
        public static string SanitizeEmail(string? value, int maxLength = 255)
        {
            if (string.IsNullOrWhiteSpace(value)) return string.Empty;

            value = value.Trim();
            value = value.Replace(" ", string.Empty);

            if (value.Length > maxLength)
                value = value.Substring(0, maxLength);

            EnsureNoScriptOrSql(value, "Email");
            return value;
        }

        /// <summary>
        /// Keep only digits for phone numbers and enforce a maximum length.
        /// </summary>
        public static string SanitizePhoneNumber(string? value, int maxLength = 20)
        {
            if (string.IsNullOrWhiteSpace(value)) return string.Empty;

            var digits = new string(value.Where(char.IsDigit).ToArray());
            if (digits.Length > maxLength)
                digits = digits.Substring(0, maxLength);

            EnsureNoScriptOrSql(digits, "Contact number");
            return digits;
        }

        /// <summary>
        /// Generic free-text sanitization: strips HTML tags, normalizes whitespace,
        /// enforces max length and blocks script / SQL-like patterns.
        /// </summary>
        public static string SanitizeFreeText(string? value, int maxLength = 255)
        {
            if (string.IsNullOrWhiteSpace(value)) return string.Empty;

            // Strip HTML tags
            value = Regex.Replace(value, "<.*?>", string.Empty);
            value = CollapseWhitespace(value.Trim());

            if (value.Length > maxLength)
                value = value.Substring(0, maxLength);

            EnsureNoScriptOrSql(value, "Text");
            return value;
        }

        /// <summary>
        /// Throws ValidationException if the supplied value contains script or SQL-like patterns.
        /// </summary>
        public static void EnsureNoScriptOrSql(string? value, string fieldName)
        {
            if (string.IsNullOrEmpty(value)) return;

            if (ScriptLikeRegex.IsMatch(value))
            {
                throw new ValidationException($"{fieldName} contains invalid or potentially dangerous content.");
            }
        }

        private static void SanitizeStringProperties(object instance)
        {
            var type = instance.GetType();
            var properties = type
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead && p.CanWrite && p.PropertyType == typeof(string));

            foreach (var prop in properties)
            {
                var current = (string?)prop.GetValue(instance);
                if (string.IsNullOrWhiteSpace(current))
                    continue;

                var trimmed = CollapseWhitespace(current.Trim());

                // Enforce MaxLength / StringLength attributes if present
                int? maxLength = null;
                var maxLengthAttr = prop.GetCustomAttribute<MaxLengthAttribute>();
                if (maxLengthAttr != null)
                {
                    maxLength = maxLengthAttr.Length;
                }
                else
                {
                    var stringLengthAttr = prop.GetCustomAttribute<StringLengthAttribute>();
                    if (stringLengthAttr != null)
                    {
                        maxLength = stringLengthAttr.MaximumLength;
                    }
                }

                if (maxLength.HasValue && trimmed.Length > maxLength.Value)
                {
                    trimmed = trimmed.Substring(0, maxLength.Value);
                }

                EnsureNoScriptOrSql(trimmed, prop.Name);

                if (!string.Equals(trimmed, current, StringComparison.Ordinal))
                {
                    prop.SetValue(instance, trimmed);
                }
            }
        }

        private static void RunDataAnnotationsValidation(object instance)
        {
            var context = new ValidationContext(instance);
            var results = new List<ValidationResult>();

            if (!Validator.TryValidateObject(instance, context, results, validateAllProperties: true))
            {
                var errorMessage = string.Join(" ", results
                    .Select(r => r.ErrorMessage)
                    .Where(m => !string.IsNullOrWhiteSpace(m)));

                if (string.IsNullOrWhiteSpace(errorMessage))
                {
                    errorMessage = "One or more fields are invalid. Please review your input.";
                }

                throw new ValidationException(errorMessage);
            }
        }

        private static string CollapseWhitespace(string value)
        {
            return MultiWhitespaceRegex.Replace(value, " ");
        }
    }
}
