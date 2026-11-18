## MVC demo in Visual Studio

Open `OrdoEtCura.Web/OrdoEtCura.Web.slnx`, select **Customer website and back office** in the startup dropdown, and press **F5**. Visual Studio starts both apps and opens their addresses in your browser.

- Customer website: https://localhost:7061
- Staff back office: https://localhost:7062

Each app has its own sign-in session. Both use the same SQL Server database and local User Secrets configuration.

 The MVC demo uses ASP.NET Core Identity and SQL Server. Its local database is `OrdoEtCuraDemo`, which you can inspect in SSMS with Windows Authentication.

For a fresh setup, configure `ConnectionStrings:OrdoEtCura` and the five `Demo:Passwords` entries using **Manage User Secrets** in Visual Studio. Use these keys under `Demo:Passwords`: `collaborator`, `approver`, `outsider`, `aster-approver`, and `admin`. Choose a different strong password for each. For the local development server, the connection string is `Server=DILARAM\MSSQLSERVER1;Database=OrdoEtCuraDemo;Integrated Security=True;Encrypt=True;TrustServerCertificate=True`.

From the project directory, run `dotnet run --launch-profile https -- --initialize-demo` to apply the migration and seed the fictional accounts. Run the app normally after that. You can rerun initialization without resetting an existing approval or replacing account passwords.

- `collaborator@demo.example` can view the Transitus workspace.
- `approver@demo.example` can approve configuration and unblock training.
- `outsider@demo.example` opens Aster Launch on the Core plan.
- `aster-approver@demo.example` approves configuration for Aster Launch.
- `admin@demo.example` manages users, company membership, roles, and assigned approvers in the back office, and inspects project details and approval history.

The read-only endpoints are `GET /api/projects/P-B01/approval` (Transitus) and `GET /api/projects/P-A01/approval` (Aster Launch). Customer access is scoped to the signed-in user's current company. Deactivation, membership edits, and role edits revoke existing sessions on their next request. Saving a user without changing their email, company, or role leaves their sessions alone, and administrators stay signed in after changing their own email. Reassign a user's projects before removing their approver role, changing their company, or deactivating them. A change of assigned approver resets approval to Pending and is recorded in project history.

The lookup currently uses the MVC sign-in session. When connecting Intercom, add service authentication and verify the caller's project access before returning data.
