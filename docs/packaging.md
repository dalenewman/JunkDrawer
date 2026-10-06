# NuGet packaging

Only the reusable libraries are packed:

| Package ID | Project | Framework |
| --- | --- | --- |
| `JunkDrawer` | `JunkDrawer/JunkDrawer.csproj` | `netstandard2.0` |
| `JunkDrawer.Autofac` | `JunkDrawer.Autofac/JunkDrawer.Autofac.csproj` | `net10.0` |

The CLI, GUI, test, and spike projects have `IsPackable=false`. The Autofac package depends on the core package and current Transformalize provider packages. Project files hold the default version (`2.0.0-preview.1`); pass `--package-version` to set the version for both packages in one build. Choose a new version deliberately before publishing and update `CHANGELOG.md`.

From the repository root:

```sh
./scripts/nuget-pack.sh --dry-run
./scripts/nuget-pack.sh --package-version 2.0.0-preview.1
```

The default output is `artifacts/nuget/`; `--output-dir PATH` changes it. Inspect package contents and dependency metadata before using them:

```sh
unzip -l artifacts/nuget/JunkDrawer.2.0.0-preview.1.nupkg
unzip -p artifacts/nuget/JunkDrawer.Autofac.2.0.0-preview.1.nupkg JunkDrawer.Autofac.nuspec
```

CI packs the libraries and uploads `.nupkg` files as artifacts. Normal builds and pushes never publish to NuGet. The separate `NuGet Publish` workflow can be started manually after review; it requires `NUGET_API_KEY` in repository secrets and an explicit publish input. A maintainer can also publish an inspected artifact manually:

```sh
dotnet nuget push artifacts/nuget/JunkDrawer.2.0.0-preview.1.nupkg --source https://api.nuget.org/v3/index.json --api-key "$NUGET_API_KEY"
dotnet nuget push artifacts/nuget/JunkDrawer.Autofac.2.0.0-preview.1.nupkg --source https://api.nuget.org/v3/index.json --api-key "$NUGET_API_KEY"
```

Publish the core package before the Autofac package so its dependency is available. NuGet publication is irreversible for a package version; verify both packages and the changelog first. This follows the separate pack/push strategy in the neighboring Transformalize repository.
