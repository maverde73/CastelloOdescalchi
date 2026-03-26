# AGENTS.md

## Scope & routing

- Solution: `Gestione_Visite_Castello_Odescalchi.sln` — C# .NET (WPF + WinForms), no .NET Core.
- Not a git repository — no git commands will work.
- No CI/CD, no automated tests, no linter config.

## Non-discoverable conventions

- DB connection string key is `SCV_DEV_DBConn` (in App.config files via `ConfigurationManager.ConnectionStrings`).
- DAL layer (`Scv_Dal/`) mixes Entity Framework context usage with commented-out raw `SqlConnection` blocks — the active pattern is EF, do not reintroduce raw ADO.NET.
- `Scv_Entities/classes/` contains both EF entity classes and view models (e.g. `V_Movimento`) — naming prefix `V_` means database view mapping, not "ViewModel".
- `Scv_Presentation/viewmodels/` contains actual MVVM ViewModels for the WPF layer.
- `Scv_Presentation/CustomControls/` subfolders with `/classes/` dirs are empty placeholder directories — do not expect code there.
- `Scv_Printer/` handles both Boca thermal printers and serial printers — printer type is determined at runtime, not compile-time.

## Landmines

- `packages/` contains loose `.dll` files alongside NuGet packages — these are manually managed vendor dependencies (Telerik, custom controls). Do not delete or reorganize.
- `GestioneTornello/` (without REA suffix) is a legacy/stub project — `GestioneTornelloREA/` is the active turnstile management app.
- `GestioneVisite_MailService` vs `Scv_MailService` — two separate mail projects. `GestioneVisite_MailService` is the Windows Service installer; `Scv_MailService` is the reusable mail logic library.
- `.vspscc` and `.vssscc` files are Visual SourceSafe artifacts (legacy VCS) — ignore, do not delete.
- `BuildProcessTemplates/` contains TFS/XAML build definitions — historical only, not active.
