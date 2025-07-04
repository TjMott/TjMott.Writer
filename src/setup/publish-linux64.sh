#!/bin/sh

cd ../TjMott.Writer

dotnet publish --configuration Release --os linux --output ../../linux64

cd ..

cp ../LICENSE ../linux64/LICENSE
cp ../README.md ../linux64/README.md

cd ../linux64
tar -czvf ../tjm-writer-0.5.3-linux64-portable.tar.gz .
cd ..
