# Trusted DCR CLI
NET Core CLI for Trusted DCR.

## Building
To build the solution the following tools are required:
* .NET SDK
* Google Protocol Buffers

Further `dotnet` and `protoc` are expected to available in PATH. The following commands are used to build the entire solution. *Note: If first time building, you may have to build twice.*

Make sure the `tdcr-proto` submodule is included by executing:
```
git submodule init
git submodule update
```

* **Debug:** `tdcr>dotnet build`
* **Release:** `tdcr>dotnet build -c Release`

## Running
After following the build instructions, the binaries can be run with `dotnet run`.
For ease of use [tdcr.bat](https://github.com/trusted-dcr/tdcr/tdcr.bat) is provided to run the correct binaries depending on build target.

Usage: `tdcr>tdcr [command] [arguments]`

Please see `tdcr>tdcr --help` for a command and argument list.

### Pre-made experiments
A collection of pre-made experiments are available via [exp.bat](https://github.com/trusted-dcr/tdcr/exp.bat).

See `tdcr>exp --help` for a list of experiments.
