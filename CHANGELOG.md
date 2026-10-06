# Changelog

## Unreleased

### Added

- SDK-style .NET 10 CLI, shared Eto GUI, a no-workload Mac64 launcher, native macOS and Windows hosts, and a portable MSTest suite.
- CSV, TXT, XLS, and XLSX import through current Transformalize modules, with SQLite import and paging tests.
- SQL Server, PostgreSQL, and MySQL provider wiring with disposable database integration tests.
- Linux amd64/arm64 CLI image workflow and separate NuGet packaging guidance.

### Changed

- CLI and GUI now share arrangement lookup in the working directory, application directory, and `~/.junkdrawer`, with an explicit override and bundled SQLite example fallback.
- Paths beginning with `~/` or `~\` in arrangements and input paths now expand to the current user's home directory.
- Unset output table names no longer pass Transformalize's `[default]` marker as a GUI view name.
- The CLI now returns nonzero status for invalid arguments, missing input, and import failures.
- The SQLite example uses a portable relative output path. The GUI starts with this SQLite arrangement.
- The Autofac integration targets .NET 10. The core library targets .NET Standard 2.0.

### Removed

- Access and SQL Server Compact support from the modern import path. They have no maintained provider in the current Transformalize set.
- .NET Framework 4.5.2 build targets, legacy package hint paths, and old native copy steps from active projects.

Earlier releases predate this changelog.
