using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;
using NHSOds.MCP.Services;

namespace NHSOds.MCP.Tools;

/// <summary>
/// MCP tools that expose NHS Organisation Data Service over the Model Context Protocol.
/// Dependencies (OdsApiClient) are resolved from the DI container per-call.
/// </summary>
[McpServerToolType]
public sealed class NHSOdsTools
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    // ── Tool 1: GP Practices ─────────────────────────────────────────────────

    [McpServerTool, Description(
        "Search for NHS GP practices by name or town. " +
        "Returns up to 10 matches with name, ODS code, postcode, and status. " +
        "Optionally narrow results with a postcode.")]
    public static async Task<string> FindGpPractice(
        OdsApiClient client,
        [Description("Name of the practice or a town/city (e.g. 'North Leeds Medical', 'Harrogate')")] string query,
        [Description("Optional postcode prefix to narrow results (e.g. 'LS17')")] string? postcode = null,
        CancellationToken cancellationToken = default)
    {
        var result = await client.FindGpPracticesAsync(query, postcode, cancellationToken);

        if (result?.Organisations is not { Count: > 0 })
            return $"No GP practices found matching '{query}'.";

        var matches = result.Organisations.Take(10).Select(o => new
        {
            o.Name,
            OdsCode   = o.OrgId,
            o.PostCode,
            o.Status,
            Role      = o.PrimaryRoleDescription
        });

        return JsonSerializer.Serialize(matches, JsonOptions);
    }

    // ── Tool 2: Pharmacies ───────────────────────────────────────────────────

    [McpServerTool, Description(
        "Search for NHS pharmacies by name or town. " +
        "Returns up to 10 matches with name, ODS code, postcode, and status.")]
    public static async Task<string> FindPharmacy(
        OdsApiClient client,
        [Description("Pharmacy name or town/city (e.g. 'Boots', 'Manchester')")] string query,
        [Description("Optional postcode prefix to narrow results")] string? postcode = null,
        CancellationToken cancellationToken = default)
    {
        var result = await client.FindPharmaciesAsync(query, postcode, cancellationToken);

        if (result?.Organisations is not { Count: > 0 })
            return $"No pharmacies found matching '{query}'.";

        var matches = result.Organisations.Take(10).Select(o => new
        {
            o.Name,
            OdsCode  = o.OrgId,
            o.PostCode,
            o.Status
        });

        return JsonSerializer.Serialize(matches, JsonOptions);
    }

    // ── Tool 3: NHS Trusts ───────────────────────────────────────────────────

    [McpServerTool, Description(
        "Search for NHS trusts or hospital sites by name or region. " +
        "Returns up to 10 matches. Use GetOrganisation with the ODS code for full address details.")]
    public static async Task<string> FindNhsTrust(
        OdsApiClient client,
        [Description("Trust or hospital name, or a region (e.g. 'Harrogate', 'Leeds Teaching', 'Royal Free')")] string query,
        CancellationToken cancellationToken = default)
    {
        var result = await client.FindNhsTrustsAsync(query, cancellationToken);

        if (result?.Organisations is not { Count: > 0 })
            return $"No NHS trusts found matching '{query}'.";

        var matches = result.Organisations.Take(10).Select(o => new
        {
            o.Name,
            OdsCode  = o.OrgId,
            o.PostCode,
            o.Status,
            Role     = o.PrimaryRoleDescription
        });

        return JsonSerializer.Serialize(matches, JsonOptions);
    }

    // ── Tool 4: Organisation detail by ODS code ──────────────────────────────

    [McpServerTool, Description(
        "Get full details for any NHS organisation using its ODS code. " +
        "Returns name, address, phone number, operational status, and last-change date. " +
        "ODS codes are returned by the other search tools (e.g. 'B86013', 'RX1').")]
    public static async Task<string> GetOrganisation(
        OdsApiClient client,
        [Description("The ODS code of the organisation (e.g. 'B86013' for a GP practice, 'RX1' for Nottingham University Hospitals Trust)")] string odsCode,
        CancellationToken cancellationToken = default)
    {
        var result = await client.GetOrganisationAsync(odsCode, cancellationToken);

        if (result?.Organisation is null)
            return $"No organisation found with ODS code '{odsCode}'.";

        var org      = result.Organisation;
        var loc      = org.GeoLoc?.Location;
        var phone    = org.Contacts?.Contact?.FirstOrDefault(c => c.Type == "tel")?.Value;

        var detail = new
        {
            org.Name,
            OdsCode = org.OrgId.Extension,
            org.Status,
            Address = loc is null ? null : new
            {
                loc.AddrLn1,
                loc.AddrLn2,
                loc.Town,
                loc.County,
                loc.PostCode,
                loc.Country
            },
            Phone           = phone,
            LastChanged     = org.LastChangeDate
        };

        return JsonSerializer.Serialize(detail, JsonOptions);
    }
}
