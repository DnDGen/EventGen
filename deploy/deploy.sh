 
echo "Deploying DnDGen.EventGen to NuGet"

ApiKey=$1
Source=$2

echo "Nuget Source is $Source"
echo "Nuget API Key is $ApiKey (should be secure)"

echo "Pushing DnDGen.EventGen"
dotnet nuget push ./DnDGen.EventGen/bin/Release/DnDGen.EventGen.*.nupkg -v normal --api-key $ApiKey --source $Source --skip-duplicate