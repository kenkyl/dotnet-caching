# .NET Cache-Aside Demo
Simple cache aside implementation to demonstrate using Redis as a look-aside cache for MySql. Note that this demo can be run using an alternative Redis or MySql instance by changing the connection information in the _Program_ class.

## Prerequisites 
- [.NET SDK](https://dotnet.microsoft.com/download/dotnet-coreps)
- [Docker](https://www.docker.com/products/docker-desktop)
- NuGet Packages 
  - [StackExchange.Redis](https://www.nuget.org/packages/StackExchange.Redis/) 
  - [MySql.Data](https://www.nuget.org/packages/MySql.Data/) 

## To Run
1. `cd .../dotnet-caching/`
2. `docker-compose up`
3. `dotnet run`

## To Interact
- `get <#>` (1-10) to fetch a user by Id from either the cache or the backend database, e.g. `get 7`
- `quit` to exit the program

## To Stop
1. Quit the program
2. `docker-compose down`
