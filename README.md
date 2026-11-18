# Ordo et Cura

Ordo et Cura is a portfolio project about customer onboarding and support. It uses a fictional B2B platform to show how teams and their customers can coordinate tasks, responsibilities, and approvals from kickoff to handover.

The repository has two parts: a static landing page and an ASP.NET Core MVC demo with a customer website and a staff back office.

## Static landing page

Open `index.html` in a web browser. If you use Visual Studio, open the repository folder to edit the page, then refresh the browser after saving. The page uses HTML and CSS and requires no installation or build step.

The page introduces Ordo et Cura and includes three expandable support examples based on the project baseline:

- What to do when an invitation expires
- How to troubleshoot missing project access
- Why training can stay blocked after configuration is complete

## MVC demo

The MVC demo is a working version of the onboarding flow. It runs on .NET 10 with ASP.NET Core Identity and SQL Server, and has three projects:

- `OrdoEtCura.Web`: the customer website (https://localhost:7061). Customers sign in and see their company's onboarding projects. The assigned approver approves configuration so training can start. The site also serves a read-only approval status endpoint at `GET /api/projects/{id}/approval`.
- `OrdoEtCura.BackOffice`: the staff back office (https://localhost:7062). Administrators manage users, company membership, and roles, assign approvers to projects, and review approval history.
- `OrdoEtCura.Data`: the shared data model and database migrations.

To run it, open `OrdoEtCura.Web/OrdoEtCura.Web.slnx` in Visual Studio. See [`OrdoEtCura.Web/README.md`](OrdoEtCura.Web/README.md) for setup, the demo accounts, and how access rules work.

## Project status

The project is in its early stages. The broader portfolio explores how AI and human support can work together during customer onboarding.
