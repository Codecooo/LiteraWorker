#!/bin/bash

name="litera"
version="1.0.0"

echo "Building RPM for $name version $version"

# Setup the RPM build environment. This is a no-op if the rpmbuild tree already exists.
rm -rf rpmbuild

mkdir -p rpmbuild/{BUILD,RPMS,SOURCES,SPECS,SRPMS}

# Build a source tarball from the Git HEAD.
git archive \
    --format=tar.gz \
    --prefix=$name-$version/ \
    -o rpmbuild/SOURCES/$name-$version.tar.gz \
    HEAD

cp $name.spec rpmbuild/SPECS/

sudo dnf builddep -y rpmbuild/SPECS/$name.spec

rpmbuild --define "_topdir $PWD/rpmbuild" -ba $name.spec

echo "RPM build complete. The resulting RPMs can be found in $PWD/rpmbuild/RPMS/ and the source RPM in $PWD/rpmbuild/SRPMS/"