#!/bin/sh

export SETUP_DIR=$(pwd)

cd ../..

export REPO_ROOT=$(pwd)

export VERSION_MAJOR="0"
export VERSION_MINOR="5"
export VERSION_REVISION="2"

export RPM_BUILD_ROOT=~/rpmbuild
export RPM_TMP=${RPM_BUILD_ROOT}/BUILDROOT/tjm-writer-${VERSION_MAJOR}.${VERSION_MINOR}.${VERSION_REVISION}-1.x86_64

if [ -d ${RPM_BUILD_ROOT} ]; then rm -rf ${RPM_BUILD_ROOT}; fi

mkdir ${RPM_BUILD_ROOT}
mkdir ${RPM_BUILD_ROOT}/RPMS
mkdir ${RPM_BUILD_ROOT}/RPMS/x86_64
mkdir ${RPM_BUILD_ROOT}/SRPMS
mkdir ${RPM_BUILD_ROOT}/BUILD
mkdir ${RPM_BUILD_ROOT}/SOURCES
mkdir ${RPM_BUILD_ROOT}/SPECS

mkdir -p ${RPM_TMP}

# Copy program files
mkdir -p ${RPM_TMP}/opt/TjMott.Writer
cp -r linux64/* ${RPM_TMP}/opt/TjMott.Writer/
if [ -f ${RPM_TMP}/opt/TjMott.Writer/TjMott.Writer.pdb ]; then rm ${RPM_TMP}/opt/TjMott.Writer/TjMott.Writer.pdb; fi

# Copy program launcher and MIME type
mkdir -p ${RPM_TMP}/usr/share/applications
mkdir -p ${RPM_TMP}/usr/share/mime/packages
cp ${SETUP_DIR}/tjm-writer.desktop ${RPM_TMP}/usr/share/applications/
cp ${SETUP_DIR}/application-tjm-writer.xml ${RPM_TMP}/usr/share/mime/packages/

# Copy policykit policy to allow elevation when installing CEF
mkdir -p ${RPM_TMP}/usr/share/polkit-1/actions/
cp ${SETUP_DIR}/com.tjmott.tjm-writer.policy ${RPM_TMP}/usr/share/polkit-1/actions/


# Build RPM spec file
cat <<EOF > ${RPM_BUILD_ROOT}/SPECS/tjm-writer.spec
Summary: A word processor with useful features for authors
Name: tjm-writer
Version: ${VERSION_MAJOR}.${VERSION_MINOR}.${VERSION_REVISION}
Release: 1
License: BSD 3-Clause
Requires: dotnet-sdk-6.0
Requires: libX11-devel
Group: Office
URL: https://www.tjmott.com
BuildRoot: ${RPM_TMP}

%description
%{summary}

%files
%attr(0755, root, root) /opt/TjMott.Writer/TjMott.Writer
%attr(0755, root, root) /opt/TjMott.Writer/TjMott.Writer.ico
%attr(0644, root, root) /opt/TjMott.Writer/*.dll
%attr(0644, root, root) /opt/TjMott.Writer/*.so
%attr(0644, root, root) /opt/TjMott.Writer/*.json
%attr(0644, root, root) /opt/TjMott.Writer/LICENSE
%attr(0644, root, root) /opt/TjMott.Writer/README.md
%attr(0644, root, root) /opt/TjMott.Writer/Assets/*
%attr(0644, root, root) /opt/TjMott.Writer/WordTemplates/*
%attr(0644, root, root) /usr/share/polkit-1/actions/com.tjmott.tjm-writer.policy
%attr(0644, root, root) /usr/share/applications/tjm-writer.desktop
%attr(0644, root, root) /usr/share/mime/packages/application-tjm-writer.xml

%post
# Register MIME type and application
update-mime-database /usr/share/mime
update-desktop-database /usr/share/applications
exit

%postun
# Clean up installation folder
rm -rf /opt/TjMott.Writer
# Clean up MIME types and application
update-mime-database /usr/share/mime
update-desktop-database /usr/share/applications
exit

%clean
rm -rf ${RPM_BUILD_ROOT}/opt
rm -rf ${RPM_BUILD_ROOT}/usr

EOF
# End of spec file

# Now build actual RPM
cd ${RPM_BUILD_ROOT}/SPECS
rpmbuild --target x86_64 -bb tjm-writer.spec

# Copy RPM to repository root, and do cleanup.
cp ${RPM_BUILD_ROOT}/RPMS/x86_64/*.rpm ${REPO_ROOT}/

if [ -d ${RPM_BUILD_ROOT} ]; then rm -rf ${RPM_BUILD_ROOT}; fi
