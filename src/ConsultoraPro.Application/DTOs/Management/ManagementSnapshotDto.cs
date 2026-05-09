namespace ConsultoraPro.Application.DTOs.Management;

public class ManagementSnapshotDto
{
    public string GeneratedAt { get; set; } = string.Empty;
    public string PeriodLabel { get; set; } = string.Empty;
    public ExecutiveOverviewDto Executive { get; set; } = new();
    public List<ManagementClientDto> Clients { get; set; } = new();
    public List<ManagementProjectDto> Projects { get; set; } = new();
    public List<TipoSolucionDto> TiposSolucion { get; set; } = new();
    public InfrastructureOverviewDto Infrastructure { get; set; } = new();
    public TeamOverviewDto Team { get; set; } = new();
}

public class TipoSolucionDto
{
    public string Id { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
}

public class ExecutiveOverviewDto
{
    public List<MetricDto> Metrics { get; set; } = new();
    public List<AlertMessageDto> Alerts { get; set; } = new();
    public List<ManagementProjectDto> SpotlightProjects { get; set; } = new();
    public List<GanttItemDto> Gantt { get; set; } = new();
    public List<AlertMessageDto> Milestones { get; set; } = new();
}

public class MetricDto
{
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Detail { get; set; } = string.Empty;
    public string Tone { get; set; } = "blue";
    public string? DetailTone { get; set; }
}

public class AlertMessageDto
{
    public string Tone { get; set; } = "info";
    public string Text { get; set; } = string.Empty;
}

public class ManagementClientDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Initials { get; set; } = string.Empty;
    public int ProjectsCount { get; set; }
    public string Sector { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string StatusTone { get; set; } = "gray";
    public string LogoTone { get; set; } = "blue";
}

public class ManagementProjectDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public string Stage { get; set; } = string.Empty;
    public string StageTone { get; set; } = "blue";
    public LeadDto Lead { get; set; } = new();
    public int Progress { get; set; }
    public string ProgressTone { get; set; } = "blue";
    public string StartDate { get; set; } = string.Empty;
    public string EndDate { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string StatusTone { get; set; } = "blue";
    public int TeamSize { get; set; }
}

public class LeadDto
{
    public string Initials { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Tone { get; set; } = "blue";
}

public class GanttItemDto
{
    public string Label { get; set; } = string.Empty;
    public string Caption { get; set; } = string.Empty;
    public int Start { get; set; }
    public int Width { get; set; }
    public string Tone { get; set; } = "blue";
}

public class InfrastructureOverviewDto
{
    public List<EnvironmentGroupDto> EnvironmentGroups { get; set; } = new();
    public List<DeploymentDto> Deployments { get; set; } = new();
    public List<RepositoryHealthDto> Repositories { get; set; } = new();
    public List<CredentialDto> Credentials { get; set; } = new();
}

public class EnvironmentGroupDto
{
    public string ProjectName { get; set; } = string.Empty;
    public List<EnvironmentItemDto> Items { get; set; } = new();
}

public class EnvironmentItemDto
{
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Stack { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string StateTone { get; set; } = "gray";
    public string? Availability { get; set; }
}

public class DeploymentDto
{
    public string ProjectName { get; set; } = string.Empty;
    public string Target { get; set; } = string.Empty;
    public string When { get; set; } = string.Empty;
    public string Actor { get; set; } = string.Empty;
    public string Duration { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Tone { get; set; } = "gray";
}

public class RepositoryHealthDto
{
    public string Name { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string Branch { get; set; } = string.Empty;
    public string Stack { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Tone { get; set; } = "gray";
}

public class CredentialDto
{
    public string Service { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
    public string EnvironmentTone { get; set; } = "gray";
    public string Kind { get; set; } = string.Empty;
    public string ExpiresIn { get; set; } = string.Empty;
    public string Tone { get; set; } = "gray";
}

public class TeamOverviewDto
{
    public List<TeamMemberDto> Members { get; set; } = new();
    public List<RoleDto> Roles { get; set; } = new();
    public List<ProjectPreviewDto> Previews { get; set; } = new();
}

public class TeamMemberDto
{
    public string Initials { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Projects { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string RoleTone { get; set; } = "gray";
    public string AvatarTone { get; set; } = "blue";
}

public class RoleDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string UsersLabel { get; set; } = string.Empty;
    public string Tone { get; set; } = "blue";
    public List<PermissionDto> Permissions { get; set; } = new();
}

public class PermissionDto
{
    public string Label { get; set; } = string.Empty;
    public bool Granted { get; set; }
}

public class ProjectPreviewDto
{
    public string Title { get; set; } = string.Empty;
    public string Kind { get; set; } = "desktop";
    public string Tone { get; set; } = "blue";
}
