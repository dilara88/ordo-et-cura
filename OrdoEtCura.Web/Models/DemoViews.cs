namespace OrdoEtCura.Web.Models;
public sealed record WorkspaceView(OnboardingProject Project, bool CanApprove,
    IReadOnlyList<OnboardingProject> Projects, IReadOnlyList<ProjectEvent> History);
