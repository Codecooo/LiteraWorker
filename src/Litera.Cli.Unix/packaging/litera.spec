# Managed assemblies aren't ELF objects, so RPM's debuginfo
# extraction produces nothing useful for .NET builds
%global debug_package %{nil}
 
# Single knob for the SDK/runtime stream this package targets
%global dotnet_version 10.0
 
# Upstream project/assembly name (differs from RPM %%{name} casing)
%global upstream_name Litera.Cli.Unix
%global worker_name Litera.Worker.Unix
 
Name:           litera
Version:        1.0.0
Release:        %autorelease
Summary:        A remote print CLI utility to print documents to any printer from the command line
 
# Must be a valid SPDX expression covering the app AND bundled NuGet deps
License:        GPL-3.0
URL:            https://github.com/Codecooo/LiteraWorker
Source0:        %{url}/archive/v%{version}/%{name}-%{version}.tar.gz
# Vendored NuGet dependencies for offline build (Koji has no network).
# Regenerate with something like:
dotnet restore --packages ./nuget-deps && tar cJf %%{name}-%%{version}-nuget-deps.tar.xz -C nuget-deps .
Source1:        %{name}-%{version}-nuget-deps.tar.xz
 
# %%{dotnet_arches} is provided by the Fedora dotnet packages
# (currently x86_64 aarch64 ppc64le s390x); .NET is not built elsewhere
ExclusiveArch:  %{dotnet_arches}
 
BuildRequires:  dotnet-sdk-%{dotnet_version}
 
# Framework-dependent deployment: the system runtime executes the app
Requires:       dotnet-runtime-%{dotnet_version}
 
# Bundling guidelines: every vendored NuGet package must be declared,
# one virtual Provides per package, with exact version
# Provides:       bundled(nuget(Newtonsoft.Json)) = 13.0.3
# Provides:       bundled(nuget(Spectre.Console)) = 0.49.1
 
%description
A remote print CLI utility to print documents to any printer from the command line.
 
%prep
%autosetup -p1 -n %{name}-%{version}
# Lay out the vendored NuGet packages as a local restore source
mkdir -p nuget-sources
tar -C nuget-sources -xf %{SOURCE1}
# Ensure restore can only see the vendored source, never nuget.org
cat > nuget.config <<'EOF'
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="local" value="./nuget-sources" />
  </packageSources>
</configuration>
EOF
 
%build
# Fedora's SDK ships with telemetry disabled, but be explicit
export DOTNET_CLI_TELEMETRY_OPTOUT=1
 
dotnet restore src/%{upstream_name}/%{upstream_name}.csproj
 
# Framework-dependent (NOT self-contained, NOT single-file): the app
# must run on the distro runtime so it picks up security updates
dotnet publish \
    --no-restore \
    --configuration Release \
    --no-self-contained \
    -p:VersionPrefix=%{version} \
    --output publish-cli \
    src/%{upstream_name}/%{upstream_name}.csproj

dotnet publish \
    --configuration Release \
    --no-restore \
    --no-self-contained \
    -o publish-worker \
    src/%{worker_name}/%{worker_name}.csproj
 
%install
# Arch-specific payload (apphost launcher) => %%{_libdir}, not %%{_datadir}
install -d -m 0755 %{buildroot}%{_libdir}/%{name}
cp -a publish-cli/* %{buildroot}%{_libdir}/%{name}/
cp -a paper_sizes.json %{buildroot}%{_libdir}/%{name}/
 
# Entry point: symlink the native apphost into %%{_bindir}.
# If upstream disables the apphost, install a 2-line wrapper script
# invoking 'exec /usr/bin/dotnet %%{_libdir}/%%{name}/%%{upstream_name}.dll "$@"' instead.
mkdir -p %{buildroot}%{_libdir}/%{name}/worker
cp -a publish-worker/* %{buildroot}%{_libdir}/%{name}/worker/

install -d %{buildroot}%{_bindir}
ln -s %{_libdir}/%{name}/Litera.Cli.Unix \
      %{buildroot}%{_bindir}/litera

install -D -m0644 litera-worker.service \
      %{buildroot}%{_unitdir}/litera-worker.service
 
# %check
# dotnet test \
#     --no-restore \
#     --configuration Release \
#     tests/%{upstream_name}.Tests/%{upstream_name}.Tests.csproj
 
%files
%license LICENSE
%doc README.md
%{_bindir}/%{name}
%{_libdir}/%{name}/
 
%changelog
%autochangelog