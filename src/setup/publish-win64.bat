cd /D %~dp0

cd ..\TjMott.Writer

dotnet publish --configuration Release --os win --output ..\..\win64

cd ..

copy ..\LICENSE ..\win64\LICENSE
copy ..\README.md ..\win64\README.md
