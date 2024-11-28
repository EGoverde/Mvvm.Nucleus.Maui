dotnet clean Mvvm.Nucleus.Maui/Mvvm.Nucleus.Maui.csproj
dotnet pack Mvvm.Nucleus.Maui/Mvvm.Nucleus.Maui.csproj /p:ContinuousIntegrationBuild=true
rm -f *.nupkg
rm -f *.snupkg
mv Mvvm.Nucleus.Maui/bin/Release/*.nupkg .
mv Mvvm.Nucleus.Maui/bin/Release/*.snupkg .