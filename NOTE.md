dotnet ef migrations add InitIdentityOpenIddict --project VibeCode.IdentityServer

dotnet ef migrations remove --project VibeCode.IdentityServer

dotnet ef database update --project VibeCode.IdentityServer

dotnet ef database update 0 --project VibeCode.IdentityServer

dotnet ef database drop --project VibeCode.IdentityServer

dotnet publish -c Release

DOTNET_URLS="https://localhost:7225;http://localhost:5019" dotnet VibeCode.IdentityServer.dll

dotnet tool update --global dotnet-ef

```
dotnet clean && dotnet build
```
