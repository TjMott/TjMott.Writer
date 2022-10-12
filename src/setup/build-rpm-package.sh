#!/bin/sh

export VERSION_MAJOR="0"
export VERSION_MINOR="5"
export VERSION_REVISION="0"

export SETUP_DIR=$(pwd)

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
cp -r ../linux64/* ${RPM_TMP}/opt/TjMott.Writer/
if [ -f ${RPM_TMP}/opt/TjMott.Writer/TjMott.Writer.pdb ]; then rm ${RPM_TMP}/opt/TjMott.Writer/TjMott.Writer.pdb; fi

# Copy program launcher
mkdir -p ${RPM_TMP}/usr/share/applications
cp tjm-writer.desktop ${RPM_TMP}/usr/share/applications/

# Copy policykit policy to allow elevation when installing CEF
mkdir -p ${RPM_TMP}/usr/share/polkit-1/actions/
cp com.tjmott.tjm-writer.policy ${RPM_TMP}/usr/share/polkit-1/actions/



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
%attr(0644, root, root) /opt/TjMott.Writer/*.dll
%attr(0644, root, root) /opt/TjMott.Writer/*.so
%attr(0644, root, root) /opt/TjMott.Writer/*.json
%attr(0644, root, root) /opt/TjMott.Writer/LICENSE
%attr(0644, root, root) /opt/TjMott.Writer/README.md
%attr(0644, root, root) /opt/TjMott.Writer/Assets/*
%attr(0644, root, root) /opt/TjMott.Writer/WordTemplates/*
%attr(0644, root, root) /usr/share/polkit-1/actions/com.tjmott.tjm-writer.policy
%attr(0644, root, root) /usr/share/applications/tjm-writer.desktop

%postun
rm -rf /opt/TjMott.Writer
exit

%clean
rm -rf ${RPM_BUILD_ROOT}/opt
rm -rf ${RPM_BUILD_ROOT}/usr

EOF


cd ${RPM_BUILD_ROOT}/SPECS
rpmbuild --target x86_64 -bb tjm-writer.spec

cp ${RPM_BUILD_ROOT}/RPMS/x86_64/*.rpm ${SETUP_DIR}/

#if [ -d ${RPM_BUILD_ROOT} ]; then rm -rf ${RPM_BUILD_ROOT}; fi
