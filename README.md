# HA.Common
Home Automation Common Library



## Build (cake)
``
dotnet cake --target Clean
dotnet cake --target Test
dotnet cake --target Build
dotnet cake --target Build --buildversion 1.0.3
dotnet cake --target Pack
dotnet cake --target Pack --buildversion 1.0.3
..\setApiKey_common.bat
dotnet cake --target PushNuget --buildversion 1.0.3
``

### Install cake
``
dotnet new tool-manifest
dotnet tool install Cake.Tool --version 4.0.0
``