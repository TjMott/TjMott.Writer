#!/bin/sh

cd ..

dotnet publish --configuration Release --os linux --output ../linux64

cp ../LICENSE ../linux64/LICENSE
cp ../README.md ../linux64/README.md

cd ../linux64
tar -czvf ../tjm-writer-0.5.2-linux64-portable.tar.gz .
cd ..
