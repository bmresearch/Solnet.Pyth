#!/bin/bash

PREV="5.0.7"

FILES="README.md
SharedBuildProperties.props
Solnet.Pyth/Solnet.Pyth.csproj
Solnet.Pyth.Examples/Solnet.Pyth.Examples.csproj
Solnet.Pyth.Test/Solnet.Pyth.Test.csproj
chver.sh"

for f in $FILES
do
    echo $f
    sed -i "s/$PREV/$1/g" $f
done