cd /D %~dp0

cd ..

dotnet publish --configuration Release --os win --output ..\win64

copy ..\LICENSE ..\win64\LICENSE
copy ..\README.md ..\win64\README.md