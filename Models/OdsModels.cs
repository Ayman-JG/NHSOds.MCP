using System.Text.Json.Serialization;

namespace NHSOds.MCP.Models;

// ── Search response ──────────────────────────────────────────────────────────
// OrgId is a plain string (e.g. "B86013") in the list endpoint.

public record OdsSearchResponse(
    [property: JsonPropertyName("Organisations")] List<OdsSummary> Organisations
);

public record OdsSummary(
    [property: JsonPropertyName("Name")] string Name,
    [property: JsonPropertyName("OrgId")] string OrgId,
    [property: JsonPropertyName("Status")] string Status,
    [property: JsonPropertyName("PostCode")] string? PostCode,
    [property: JsonPropertyName("LastChangeDate")] string? LastChangeDate,
    [property: JsonPropertyName("PrimaryRoleId")] string? PrimaryRoleId,
    [property: JsonPropertyName("PrimaryRoleDescription")] string? PrimaryRoleDescription,
    [property: JsonPropertyName("OrgLink")] string? OrgLink
);

// ── Detail response ──────────────────────────────────────────────────────────
// OrgId becomes an object with an "extension" field in the detail endpoint.

public record OdsDetailResponse(
    [property: JsonPropertyName("Organisation")] OdsOrganisation Organisation
);

public record OdsOrganisation(
    [property: JsonPropertyName("Name")] string Name,
    [property: JsonPropertyName("Status")] string Status,
    [property: JsonPropertyName("LastChangeDate")] string? LastChangeDate,
    [property: JsonPropertyName("OrgId")] OdsOrgId OrgId,
    [property: JsonPropertyName("GeoLoc")] GeoLoc? GeoLoc,
    [property: JsonPropertyName("Contacts")] OdsContacts? Contacts
);

public record OdsOrgId(
    [property: JsonPropertyName("extension")] string Extension,
    [property: JsonPropertyName("root")] string? Root,
    [property: JsonPropertyName("assigningAuthorityName")] string? AssigningAuthorityName
);

public record GeoLoc(
    [property: JsonPropertyName("Location")] OdsLocation? Location
);

public record OdsLocation(
    [property: JsonPropertyName("AddrLn1")] string? AddrLn1,
    [property: JsonPropertyName("AddrLn2")] string? AddrLn2,
    [property: JsonPropertyName("Town")] string? Town,
    [property: JsonPropertyName("County")] string? County,
    [property: JsonPropertyName("PostCode")] string? PostCode,
    [property: JsonPropertyName("Country")] string? Country
);

public record OdsContacts(
    [property: JsonPropertyName("Contact")] List<OdsContact>? Contact
);

public record OdsContact(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("value")] string Value
);
