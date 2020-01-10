 
echo "Deploying EventGen to NuGet"

ApiKey=$1
Source=$2

echo "Nuget Source is $Source"
echo "Nuget API Key is $ApiKey (should be secure)"

echo "Pushing EventGen"
nuget push ./EventGen/bin/Release/EventGen.*.nupkg -Verbosity detailed -ApiKey $ApiKey -Source $Source