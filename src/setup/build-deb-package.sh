#!/bin/sh

export VERSION_MAJOR="0"
export VERSION_MINOR="5"
export VERSION_REVISION="0"

export PACKAGE=tjm-writer_${VERSION_MAJOR}.${VERSION_MINOR}.${VERSION_REVISION}-1_amd64

if [ -d ${PACKAGE} ]; then rm -rf ${PACKAGE}; fi

mkdir ${PACKAGE}

# Create package file
mkdir ${PACKAGE}/DEBIAN
touch ${PACKAGE}/DEBIAN/control

echo "Package: tjm-writer" >> ${PACKAGE}/DEBIAN/control
echo "Version: ${VERSION_MAJOR}.${VERSION_MINOR}.${VERSION_REVISION}" >> ${PACKAGE}/DEBIAN/control
echo "Architecture: amd64" >> ${PACKAGE}/DEBIAN/control
echo "Maintainer: TJ Mott <tj@tjmott.com>" >> ${PACKAGE}/DEBIAN/control
echo "Depends: dotnet-sdk-6.0, libx11-dev" >> ${PACKAGE}/DEBIAN/control
echo "HomePage: https://github.com/TjMott/TjMott.Writer" >> ${PACKAGE}/DEBIAN/control
echo "Description: A word processor with useful features for authors." >> ${PACKAGE}/DEBIAN/control

# Create uninstall pre-removal file
touch ${PACKAGE}/DEBIAN/prerm
chmod 0775 ${PACKAGE}/DEBIAN/prerm

echo "#!/bin/sh" >> ${PACKAGE}/DEBIAN/prerm
echo "rm -rf /opt/TjMott.Writer/Assets" >> ${PACKAGE}/DEBIAN/prerm
echo "rm -rf /opt/TjMott.Writer/GPUCache" >> ${PACKAGE}/DEBIAN/prerm
echo "if [ -f /opt/TjMott.Writer/cefinstalled ]; then rm /opt/TjMott.Writer/cefinstalled; fi" >> ${PACKAGE}/DEBIAN/prerm

# Copy program files
mkdir -p ${PACKAGE}/opt/TjMott.Writer
cp -r ../linux64/* ${PACKAGE}/opt/TjMott.Writer/
chmod 0775 ${PACKAGE}/opt/TjMott.Writer/elevate-install-cef.sh

# Copy program launcher
mkdir -p ${PACKAGE}/usr/share/applications
cp tjm-writer.desktop ${PACKAGE}/usr/share/applications/

# Create deb
dpkg-deb --build --root-owner-group ${PACKAGE}

