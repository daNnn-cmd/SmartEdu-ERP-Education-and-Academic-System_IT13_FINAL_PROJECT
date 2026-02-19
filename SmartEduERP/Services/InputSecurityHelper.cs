using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using SmartEduERP.Data.Models;

namespace SmartEduERP.Services;

public static class InputSecurityHelper
{
    private const string PasswordAllowedPattern = @"^[A-Za-z0-9!@#$%^&*()\-_=+\[\]{};:,./?]+$";

    public static void ValidateUserForCreate(UserAccount user, string password)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password is required.");

        if (password.Length < 12 || password.Length > 255)
            throw new ArgumentException("Password must be between 12 and 255 characters.");

        if (!IsPasswordAllowed(password))
            throw new ArgumentException("Password contains invalid characters.");

        ValidateUserCommon(user);
    }

    public static void ValidateUserForUpdate(UserAccount user, string? newPassword)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        ValidateUserCommon(user);

        if (!string.IsNullOrEmpty(newPassword))
        {
            if (newPassword.Length < 12 || newPassword.Length > 255)
                throw new ArgumentException("Password must be between 12 and 255 characters.");

            if (!IsPasswordAllowed(newPassword))
                throw new ArgumentException("Password contains invalid characters.");
        }
    }

    private static void ValidateUserCommon(UserAccount user)
    {
        user.FirstName = user.FirstName?.Trim() ?? string.Empty;
        user.LastName = user.LastName?.Trim() ?? string.Empty;
        user.Username = user.Username?.Trim() ?? string.Empty;
        user.Email = user.Email?.Trim() ?? string.Empty;
        user.Role = user.Role?.Trim() ?? string.Empty;

        if (ContainsUnsafeText(user.FirstName) ||
            ContainsUnsafeText(user.LastName) ||
            ContainsUnsafeText(user.Username) ||
            ContainsUnsafeText(user.Email) ||
            ContainsUnsafeText(user.Role))
        {
            throw new ArgumentException("Input contains invalid or unsafe characters.");
        }

        user.Role = NormalizeRole(user.Role);

        var context = new ValidationContext(user);
        var results = new List<ValidationResult>();

        Validator.TryValidateObject(user, context, results, validateAllProperties: true);

        var nonPasswordErrors = results
            .Where(r => !r.MemberNames.Contains(nameof(UserAccount.Password)))
            .Select(r => r.ErrorMessage)
            .Where(m => !string.IsNullOrWhiteSpace(m))
            .ToList();

        if (nonPasswordErrors.Count > 0)
        {
            var message = string.Join(" ", nonPasswordErrors);
            throw new ArgumentException(message);
        }
    }

    public static bool ContainsUnsafeText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        var v = value;

        if (v.Contains('<') || v.Contains('>'))
            return true;

        var lower = v.ToLowerInvariant();

        if (lower.Contains("<script") || lower.Contains("</script") ||
            lower.Contains("&lt;script") || lower.Contains("&gt;script") ||
            lower.Contains("&#x3c;script") || lower.Contains("&#60;script"))
            return true;

        if (lower.Contains("javascript:"))
            return true;

        if (lower.Contains("' or '") || lower.Contains("\" or \""))
            return true;

        if (lower.Contains(" or 1=1") || lower.Contains(" or '1'='1"))
            return true;

        if (lower.Contains(";--") || lower.Contains("-- "))
            return true;

        if (lower.Contains("/*") || lower.Contains("*/"))
            return true;

        if (lower.Contains(" drop ") || lower.Contains(" delete ") ||
            lower.Contains(" insert ") || lower.Contains(" update "))
            return true;

        return false;
    }

    public static bool IsPasswordAllowed(string password)
    {
        return Regex.IsMatch(password, PasswordAllowedPattern);
    }

    public static string NormalizeRole(string role)
    {
        if (string.IsNullOrWhiteSpace(role))
            throw new ArgumentException("Role is required.");

        var trimmed = role.Trim();

        if (trimmed.Equals("admin", StringComparison.OrdinalIgnoreCase))
            return "Admin";

        if (trimmed.Equals("teacher", StringComparison.OrdinalIgnoreCase))
            return "Teacher";

        if (trimmed.Equals("student", StringComparison.OrdinalIgnoreCase))
            return "Student";

        if (trimmed.Equals("hr", StringComparison.OrdinalIgnoreCase))
            return "HR";

        if (trimmed.Equals("accounting", StringComparison.OrdinalIgnoreCase))
            return "Accounting";

        throw new ArgumentException("Invalid role specified.");
    }
}
