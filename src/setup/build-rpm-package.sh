#!/bin/sh

export SETUP_DIR=$(pwd)
echo "SETUP_DIR: $SETUP_DIR"

cd ../..

export REPO_ROOT=$(pwd)
echo "REPO_ROOT: $REPO_ROOT"

export PUBLISH_ROOT="${REPO_ROOT}/linux64"

export VERSION_MAJOR="1"
export VERSION_MINOR="0"
export VERSION_REVISION="1"

echo "Using version $VERSION_MAJOR.$VERSION_MINOR.$VERSION_REVISION"

echo "Building RPM specfile at ${SETUP_DIR}/tjm-writer.spec..."
# Build RPM spec file
cat <<EOF > ${SETUP_DIR}/tjm-writer.spec
Summary: An open source word processor with useful features for authors
Name: tjm-writer
Version: ${VERSION_MAJOR}.${VERSION_MINOR}.${VERSION_REVISION}
Release: 1
License: BSD 3-Clause
Requires: dotnet-sdk-8.0
Group: Office
URL: https://www.tjmott.com
AutoReqProv: no

%description
%{summary}

%prep
mkdir -p %{buildroot}/usr/share/applications/
mkdir -p %{buildroot}/usr/share/mime/packages/
mkdir -p %{buildroot}/opt/TjMott.Writer/

rsync -av --exclude='*.pdb' ${PUBLISH_ROOT}/ %{buildroot}/opt/TjMott.Writer/
cp ${SETUP_DIR}/tjm-writer.desktop %{buildroot}/usr/share/applications/
cp ${SETUP_DIR}/application-tjm-writer.xml %{buildroot}/usr/share/mime/packages/

%files
%attr(0755, root, root) /opt/TjMott.Writer/TjMott.Writer
%attr(0755, root, root) /opt/TjMott.Writer/TjMott.Writer.ico
%attr(0644, root, root) /opt/TjMott.Writer/*.dll
%attr(0644, root, root) /opt/TjMott.Writer/*.so
%attr(0644, root, root) /opt/TjMott.Writer/*.json
%attr(0644, root, root) /opt/TjMott.Writer/LICENSE
%attr(0644, root, root) /opt/TjMott.Writer/README.md
%attr(0644, root, root) /opt/TjMott.Writer/Assets/editor.html
%attr(0644, root, root) /opt/TjMott.Writer/Assets/quilljs/*
%attr(0644, root, root) /opt/TjMott.Writer/WordTemplates/*
%attr(0755, root, root) /opt/TjMott.Writer/CefGlueBrowserProcess/Xilium.CefGlue.BrowserProcess
%attr(0755, root, root) /opt/TjMott.Writer/CefGlueBrowserProcess/createdump
%attr(0644, root, root) /opt/TjMott.Writer/CefGlueBrowserProcess/*.dll
%attr(0644, root, root) /opt/TjMott.Writer/CefGlueBrowserProcess/*.so*
%attr(0644, root, root) /opt/TjMott.Writer/CefGlueBrowserProcess/*.json
%attr(0644, root, root) /opt/TjMott.Writer/CefGlueBrowserProcess/*.pak
%attr(0644, root, root) /opt/TjMott.Writer/CefGlueBrowserProcess/*.dat
%attr(0644, root, root) /opt/TjMott.Writer/CefGlueBrowserProcess/*.bin
%attr(0644, root, root) /opt/TjMott.Writer/CefGlueBrowserProcess/locales/*.pak
%attr(0644, root, root) /usr/share/applications/tjm-writer.desktop
%attr(0644, root, root) /usr/share/mime/packages/application-tjm-writer.xml

%post
# Register MIME type and application
update-mime-database /usr/share/mime
update-desktop-database /usr/share/applications
exit

%postun
# Clean up MIME types and application
update-mime-database /usr/share/mime
update-desktop-database /usr/share/applications
# I wouldn't expect this to be necessary, but some junk remains...
rm -rf /opt/TjMott.Writer
exit

%clean
rm -rf %{buildroot}

EOF
# End of spec file

echo "RPM specfile written. Contents:"
cat ${SETUP_DIR}/tjm-writer.spec

if [ -d ~/rpmbuild ]; then rm -rf ~/rpmbuild; fi

echo "Calling rpmbuild..."
# Now build actual RPM
cd ${SETUP_DIR}
rpmbuild --target x86_64 -bb tjm-writer.spec

# Copy RPM to repository root, and do cleanup.
cp ~/rpmbuild/RPMS/x86_64/*.rpm ${REPO_ROOT}/

if [ -d ~/rpmbuild ]; then rm -rf ~/rpmbuild; fi
rm ${SETUP_DIR}/tjm-writer.spec
