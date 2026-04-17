# TF2HUD.Editor

## Project Structure

- `src/HUDEditor/` — Avalonia .NET app (TargetFramework: net10.0)
- Root `package.json` / `docusaurus.config.js` — Docs site only (separate from the app)

## Dev Commands

**App (from repo root):**
- `dotnet build src/HUDEditor/HUDEditor.csproj` — build
- `dotnet publish -c Release -r <rid> --self-contained true -p:PublishSingleFile=true` — publish (rid: win-x64, linux-x64)
- `dotnet watch run --project src/HUDEditor/HUDEditor.csproj` — live reload

**Docs (from repo root):**
- `pnpm install` — install docs deps
- `pnpm build-docs` — build Docusaurus site (outputs to `build/`)
- `pnpm start-docs` — dev server with hot reload

## CI

- `build.yml` — restores, builds Debug for win-x64 and linux-x64
- `package.yml` — publishes Release self-contained single-file executables
- `docs.yml` — builds and deploys Docusaurus to GitHub Pages on master push

## Architecture Notes

- App uses MVVM with CommunityToolkit.Mvvm and ReactiveUI.Avalonia
- JSON HUD presets live in `src/HUDEditor/JSON/` and are copied to output on build
- Embedded tools: `vtex2` and `vtfview` for VTF texture conversion
- `Assets/Resources/Resources.resx` generates `Resources.Designer.cs` via PublicResXFileCodeGenerator
- Logging via log4net (`log4net.config` copied to output)
- `publish/` is gitignored but used by CI for artifacts

## Editor Settings

- `.vscode/` config: C# format-on-save, spaces (no tabs), JSON tabSize 2
- `omnisharp.json`: SpaceAfterCast enabled
- C# formatter uses `modifications` mode on save
