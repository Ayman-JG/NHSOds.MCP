using System.Net.Http.Json;
using NHSOds.MCP.Models;

namespace NHSOds.MCP.Services;

/// <summary>
/// Typed HTTP client for the NHS Organisation Data Service (ODS).
/// Base URL: https://directory.spineservices.nhs.uk/ORD/2-0-0/
/// No authentication required — completely open public API.
/// </summary>
public sealed class OdsApiClient(HttpClient http)
{
    // ODS role codes for the organisation types we care about.
    private const string RoleGpPractice = "RO76";
    private const string RolePharmacy   = "RO182";
    private const string RoleNhsTrust   = "RO197";

    public Task<OdsSearchResponse?> FindGpPracticesAsync(string query, string? postcode = null, CancellationToken ct = default)
        => SearchAsync(name: query, postcode: postcode, role: RoleGpPractice, ct);

    public Task<OdsSearchResponse?> FindPharmaciesAsync(string query, string? postcode = null, CancellationToken ct = default)
        => SearchAsync(name: query, postcode: postcode, role: RolePharmacy, ct);

    public Task<OdsSearchResponse?> FindNhsTrustsAsync(string query, CancellationToken ct = default)
        => SearchAsync(name: query, role: RoleNhsTrust, ct);

    public Task<OdsDetailResponse?> GetOrganisationAsync(string odsCode, CancellationToken ct = default)
        => http.GetFromJsonAsync<OdsDetailResponse>($"organisations/{odsCode.ToUpperInvariant()}", ct);

    // ── private ──────────────────────────────────────────────────────────────

    private Task<OdsSearchResponse?> SearchAsync(
        string? name     = null,
        string? postcode = null,
        string? role     = null,
        CancellationToken ct = default)
    {
        var qs = BuildQueryString(name, postcode, role);
        return http.GetFromJsonAsync<OdsSearchResponse>($"organisations?{qs}&_format=json", ct);
    }

    private static string BuildQueryString(string? name, string? postcode, string? role)
    {
        var parts = new List<string>(3);
        if (!string.IsNullOrWhiteSpace(name))     parts.Add($"Name={Uri.EscapeDataString(name)}");
        if (!string.IsNullOrWhiteSpace(postcode)) parts.Add($"PostCode={Uri.EscapeDataString(postcode)}");
        if (!string.IsNullOrWhiteSpace(role))     parts.Add($"Roles={role}");
        return string.Join("&", parts);
    }
}
