#!/bin/sh

export VERSION_MAJOR="1"
export VERSION_MINOR="0"
export VERSION_REVISION="0"

export PACKAGE=tjm-writer_${VERSION_MAJOR}.${VERSION_MINOR}.${VERSION_REVISION}-1_amd64

cd ../..

if [ -d ${PACKAGE} ]; then rm -rf ${PACKAGE}; fi

mkdir ${PACKAGE}

# Create package file
mkdir ${PACKAGE}/DEBIAN
touch ${PACKAGE}/DEBIAN/control

echo "Package: tjm-writer" >> ${PACKAGE}/DEBIAN/control
echo "Version: ${VERSION_MAJOR}.${VERSION_MINOR}.${VERSION_REVISION}" >> ${PACKAGE}/DEBIAN/control
echo "Architecture: amd64" >> ${PACKAGE}/DEBIAN/control
echo "Maintainer: TJ Mott <tj@tjmott.com>" >> ${PACKAGE}/DEBIAN/control
echo "Depends: dotnet-sdk-8.0, libx11-dev" >> ${PACKAGE}/DEBIAN/control
echo "HomePage: https://www.tjmott.com" >> ${PACKAGE}/DEBIAN/control
echo "Description: A word processor with useful features for authors." >> ${PACKAGE}/DEBIAN/control

# Create postinstall file to register MIME types
touch ${PACKAGE}/DEBIAN/postinst
chmod 0775 ${PACKAGE}/DEBIAN/postinst
echo "#!/bin/sh" >> ${PACKAGE}/DEBIAN/postinst
echo "update-mime-database /usr/share/mime" >> ${PACKAGE}/DEBIAN/postinst
echo "update-desktop-database /usr/share/applications" >> ${PACKAGE}/DEBIAN/postinst

# Create uninstall pre-removal file (unused right now)
touch ${PACKAGE}/DEBIAN/prerm
chmod 0775 ${PACKAGE}/DEBIAN/prerm

echo "#!/bin/sh" >> ${PACKAGE}/DEBIAN/prerm
echo " " >> ${PACKAGE}/DEBIAN/prerm

# Create uninstall post-removal file to update MIME types
touch ${PACKAGE}/DEBIAN/postrm
chmod 0775 ${PACKAGE}/DEBIAN/postrm
echo "#!/bin/sh" >> ${PACKAGE}/DEBIAN/postrm
echo "update-mime-database /usr/share/mime" >> ${PACKAGE}/DEBIAN/postrm
echo "update-desktop-database /usr/share/applications" >> ${PACKAGE}/DEBIAN/postrm

# Copy program files
mkdir -p ${PACKAGE}/opt/TjMott.Writer
cp -r linux64/* ${PACKAGE}/opt/TjMott.Writer/

# Make sure Cef is executable.
chmod +x ${PACKAGE}/opt/TjMott.Writer/CefGlueBrowserProcess/Xilium.CefGlue.BrowserProcess
chmod +x ${PACKAGE}/opt/TjMott.Writer/TjMott.Writer

# Copy program launcher
mkdir -p ${PACKAGE}/usr/share/applications
cp src/setup/tjm-writer.desktop ${PACKAGE}/usr/share/applications/

# Copy MIME type registration
mkdir -p ${PACKAGE}/usr/share/mime/packages/
cp src/setup/application-tjm-writer.xml ${PACKAGE}/usr/share/mime/packages/

# Create deb
dpkg-deb --build --root-owner-group ${PACKAGE}

# Cleanup
if [ -d ${PACKAGE} ]; then rm -rf ${PACKAGE}; fi
