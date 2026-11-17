namespace OrdoEtCura.Web.Models;

public sealed record DemoAccountRow(string Email, string Company, string Role);
public sealed record DemoProjectRow(string Workspace, string Company, string Plan, string Approver, DateTimeOffset? ApprovedAt);
public sealed record BackOfficeView(IReadOnlyList<DemoAccountRow> Accounts, IReadOnlyList<DemoProjectRow> Projects);
