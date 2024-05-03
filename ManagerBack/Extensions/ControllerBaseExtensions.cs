using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace ManagerBack.Extensions;

/// <summary>
/// Extension class for ControllerBase objects
/// </summary>
public static class ControllerBaseExtensions {
    /// <summary>
    /// Extracts claim value from the HTTP context claims
    /// </summary>
    /// <typeparam name="T">Value type</typeparam>
    /// <param name="controller">HTTP controller</param>
    /// <param name="type">Claim type</param>
    /// <returns>Claim value</returns>
    public static string ExtractClaim<T> (this T controller, string type) where T : ControllerBase {
        var result = controller.HttpContext.User.Claims.FirstOrDefault(c => c.Type == type);
        return result!.Value;
    }
}