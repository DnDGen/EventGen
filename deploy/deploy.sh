 
echo "Deploying EventGen to NuGet"

ApiKey=$1
Source=$2

echo "Nuget Source is $Source"
echo "Nuget API Key is $ApiKey (should be secure)"

echo "Listing bin directory"
for entry in "./EventGen/bin"/*
do
  echo "$entry"
done

echo "Packing EventGen"
nuget pack ./EventGen/EventGen.nuspec -Verbosity detailed

echo "Pushing EventGen"
nuget push ./EventGen.*.nupkg -Verbosity detailed -ApiKey $ApiKey -Source $Source