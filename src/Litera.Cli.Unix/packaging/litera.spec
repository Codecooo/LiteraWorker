# Managed assemblies aren't ELF objects, so RPM's debuginfo
# extraction produces nothing useful for .NET builds
%global debug_package %{nil}
 
# Single knob for the SDK/runtime stream this package targets
%global dotnet_version 10.0
 
# Upstream project/assembly name (differs from RPM %%{name} casing)
%global upstream_name Litera.Cli.Unix
%global worker_name LiteraWorker.Unix
 
Name:           litera
Version:        1.0.0
Release:        %autorelease
Summary:        A remote print CLI utility to print documents to any printer from the command line
 
# Must be a valid SPDX expression covering the app AND bundled NuGet deps
License:        GPL-3.0
URL:            https://github.com/Codecooo/LiteraWorker
Source0:        %{name}-%{version}.tar.gz
 
# %%{dotnet_arches} is provided by the Fedora dotnet packages
# (currently x86_64 aarch64 ppc64le s390x); .NET is not built elsewhere
ExclusiveArch:  %{dotnet_arches}
 
BuildRequires:  dotnet-sdk-%{dotnet_version}
BuildRequires:  systemd-rpm-macros
BuildRequires:  selinux-policy-devel
BuildRequires:  make

# Framework-dependent deployment: the system runtime executes the app
Requires:       dotnet-runtime-%{dotnet_version}
 
# Bundling guidelines: every vendored NuGet package must be declared,
# one virtual Provides per package, with exact version
# Provides:       bundled(nuget(Newtonsoft.Json)) = 13.0.3
# Provides:       bundled(nuget(Spectre.Console)) = 0.49.1
 
%description
A remote print CLI utility to print documents to any printer from the command line.
 
%prep
%autosetup -n %{name}-%{version}
 
%build
# Fedora's SDK ships with telemetry disabled, but be explicit
export DOTNET_CLI_TELEMETRY_OPTOUT=1

# Framework-dependent (NOT self-contained, NOT single-file): the app
# must run on the distro runtime so it picks up security updates
dotnet publish \
    --configuration Release \
    --no-self-contained \
    -p:VersionPrefix=%{version} \
    --output publish-cli \
    src/%{upstream_name}/%{upstream_name}.csproj

dotnet publish \
    --configuration Release \
    --no-restore \
    --no-self-contained \
    --output publish-worker \
    src/%{worker_name}/%{worker_name}.csproj

# Build an SELinux policy module
checkmodule -M -m -o \
    src/%{upstream_name}/packaging/litera_worker.mod \
    src/%{upstream_name}/packaging/selinux/litera_worker.te

semodule_package -o \
    src/%{upstream_name}/packaging/litera_worker.pp \
    -m src/%{upstream_name}/packaging/litera_worker.mod
 
%install
# Arch-specific payload (apphost launcher) => %%{_libdir}, not %%{_datadir}
install -d -m 0755 %{buildroot}%{_libdir}/%{name}
cp -a publish-cli/* %{buildroot}%{_libdir}/%{name}/
cp -a src/%{upstream_name}/packaging/paper_sizes.json \
      %{buildroot}%{_libdir}/%{name}/
 
# Entry point: symlink the native apphost into %%{_bindir}.
# If upstream disables the apphost, install a 2-line wrapper script
# invoking 'exec /usr/bin/dotnet %%{_libdir}/%%{name}/%%{upstream_name}.dll "$@"' instead.
mkdir -p %{buildroot}%{_libdir}/%{name}/worker
cp -a publish-worker/* %{buildroot}%{_libdir}/%{name}/worker/

install -d %{buildroot}%{_bindir}
ln -sr \
    %{buildroot}%{_libdir}/%{name}/Litera.Cli.Unix \
    %{buildroot}%{_bindir}/litera

install -D -m0644 \
    src/%{upstream_name}/packaging/litera-worker.service \
    %{buildroot}%{_unitdir}/litera-worker.service

# install the compiled module into the package payload so the RPM can
# install it at %post time. The build requires selinux-policy-devel.
install -D -m0644 \
    src/%{upstream_name}/packaging/litera_worker.pp \
    %{buildroot}%{_datadir}/%{name}/selinux/litera_worker.pp

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
%{_unitdir}/litera-worker.service
%{_datadir}/%{name}/selinux/

%post
# If selinux is enabled and a compiled module was packaged, load it now.
if selinuxenabled; then
    %{_sbindir}/semodule -i \
        %{_datadir}/%{name}/selinux/litera_worker.pp || :
fi
%systemd_post litera-worker.service

%preun
if [ $1 -eq 0 ] && selinuxenabled; then
    %{_sbindir}/semodule -r litera_worker || :
fi
%systemd_preun litera-worker.service

%postun
%systemd_postun_with_restart litera-worker.service

%changelog
%autochangelog