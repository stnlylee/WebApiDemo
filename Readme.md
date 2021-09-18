# Implementation Notes

## Overview

In this demo I will show you how to build a complete solution using ASP.NET Core WebAPI. For simplicity I use some XML movie data as data source, and I will build some Restful API endpoints based on this. The functions are very simple. With these endpoints you can:

- List all movies
- Create and update movies
- Search movies

To run this demo:

- Directly launch in Visual Studio / Rider as a Console app, NET Core 5 is required for running locally
- Or run in docker - in CMD, locate to solution folder, run `docker-compose up` to spin up a new docker container
- Navigate to `http://localhost:5000/swagger/index.html` in any browser to use **Swagger**
- You can also use **Postman** to hit endpoints
- Use *Basic Authentication* to access endpoints - user name: **user**, passwords: **123** ;), or it will return *401 Unauthorized* response

![web api running](https://github.com/stnlylee/WebApiDemo/blob/main/ReadmeImages/web%20api%20running.png)



## RESTful API Design

5 endpoints are implemented for this demo

- [GET] /v1/Movies - return all movies, default cache 30 secs, it can be changed in config
- [POST] /v1/Movie - create new movie
- [PUT] /v1/Movie - update an existing movie
- [GET] /v1/Movie/Search - search the movie for all fields, where all fields can be presented ad query stings. It will use cached result too
- [GET] /HealthCheck - it's for Kubernetes to ping this endpoint in production for liveness check

```
Note: If set cache expiration too long the result for /v1/Movies and /v1/Movie/Search could be incorrect after adding or updating because they are still getting data from cache
```

Some design principals are followed:

- KISS (Keep It Simple Stupid) - only *nouns* are used alone with *HTTP verbs* for *Resource* access, no such things like GetMovie, SearchMovie etc
- Search is other than resource CRUD so query strings are used for meta data (search parameters)
- API versioning



## Application Layers

From top to bottom

- Controllers - handle incoming requests and route data to right place, and receive processed data to return to client
- Services - process core business logic, though in this demo there isn't much logic
- Repository - data access layer on top of domain models for easy CRUD access to serve the service layer
- Domain - core business models
- Persistence - simply for database connection



## Search / Filtering Design

It's using a class called `SearchOption`, it will accept all string fields as it is because all incoming data are basically strings. But for number fields, it has strong logic to validate against them. For example, for string field `Rating`, it actually became `MinRating` and `MaxRating` and it is converted to `MinRatingInt` and `MaxRatingInt` as number for calculation. These number fields only have private setters so it won't be messed up by other logic.



## Design Patterns

Seems excessive code were implemented for these simple features? Nope, that's essential to maximize the possibility to extend the system in the future. Following design patterns are used:

**MVC** - ASP.NET Core Web API is MVC based, this is well known I don't explain to much here.

**Repository Pattern** and **Abstract Factory Pattern** - it's to achieve maximum flexibility for future extension. Eg. we need to add TvShow in the future so we can have a new `ITvShowRepository` and `TvShowRepository`, and they can have their own methods in addition to methods inherited from `IRepository` and `RepositoryBase`

![repository pattern](https://github.com/stnlylee/WebApiDemo/blob/main/ReadmeImages/repository%20pattern.png)

**DTO** and **Request/Response** objects - to achieve flexibility on the View layer. Eg, you need to add one more field for searching but don't want to change model class



## Error Handling and Logging

Error handling and Logging are very important in Microservice, in real life we use **Elastic Logstash / Kibana**  or **NewRelic** to catch production bugs so we can debug. It is part of ongoing application support.

### Server side error handling

In this demo my design is as following:

- **Serilog** (https://serilog.net/) is a powerful logging library used for server side logging
- A custom `ErrorHandlingMiddleware` is used to catch all exception at top level. In each layer of this application, it uses try/catch block to catch the error and do proper logging, then rethrow to upper layer, and finally caught by `ErrorHandlingMiddleware`, it will only show limited information to outside, but we keep every bit of details for inside to investigate via logging.

![error handling](https://github.com/stnlylee/WebApiDemo/blob/main/ReadmeImages/error%20handling.png)

### Client side error handling

**FluentValidation** (https://fluentvalidation.net/) is used for client side validation so it will check the incoming data from request to report errors in a 400 BadRequest in response. We can right complex logic for validation using this library. My implementation in this demo is `AddMovieRequestMovieValidator`

![client validation](https://github.com/stnlylee/WebApiDemo/blob/main/ReadmeImages/client%20validation.png)



## API Security

For simplicity, **Basic Authentication** is implemented, the idea is  it will intercept every HTTP request and check if there is a Authorization header with a value of Base64 encoded string for username:passwords.

A better way is **JWT**, JWT token is signed so it can make sure the data is from sender, it's better to use it with HTTPS because it doesn't handle encryption. The idea is client sends an initial authentication request to server with its credentials. Server issues a temporary token as Bearer token. Client needs to attach this token in the header for every request for accessing resources until the token expires.

If more security mechanism needed, we can go **OAuth 2** or **OpenId**, it's more complex but it can handle senario like multi-tenant or privileges etc.



## Caching

Caching is an essential part of modern application. My implementation of caching in this demo is **Distributed Caching**. Well, it's not a real distributed cache for this demo purpose. `services.AddDistributedMemoryCache()` is used in `Startup` class and it will inject an instance of `DistributedMemeoryCache` for `IDistributedCache` interface. It works pretty much same as `InMemeoryCache` in a single server but it can be very easy to change to Redis because they share the interface. Refer to https://docs.microsoft.com/en-us/aspnet/core/performance/caching/distributed?view=aspnetcore-5.0

I chose to place caching as close as data access because I think it directly connect to data access. But caching can be place on different places and it can be multiple. 

In this demo, for `/v1/Movies` and `/v1/Movie/Search` endpoints, you will notice first load is taking 2 seconds (because of the `Thread.Sleep(2000)` in data layer) but after that it will be much faster to get data. The cache expiration is 30 seconds. It can be changed in config for different environment.



## API Rate Limiting

Rate limiting is another important aspect for APIs exposed to outside. The idea is to control access from some source like IP address or  user, then limit their number of accesses with a period. If quota exceeds, it will return *429 Too Many Request* response. It can prevent from *DOS attack* to protect the API. I implemented **AspNetCoreRateLimit** (https://github.com/stefanprodan/AspNetCoreRateLimit) in this demo. For all v1 movie endpoints, I set <u>5 requests with 1 minute</u> for testing purpose. It can be easily changed in config file. Don't be shocked if you saw this, no tolerance to abuse this API :).

![api rate limiting](https://github.com/stnlylee/WebApiDemo/blob/main/ReadmeImages/api%20rate%20limiting.png)



## Data Mapping

**AutoMapper** (https://docs.automapper.org/) is a common way for data mapping in .NET project. It is very handy to write profiles for different purpose, eg. you can do post processing after data conversion. My implementation in demo is `MovieMappingProfile` It's just straight forward to do conversion with requests / responses / models / search parameters etc.



## Application Configuration

It's common sense to not hard coded settings in the code. For "production ready" code we need one appsettings.json as base, and appsettings.[env].json for settings in that environment. There is an environmental variable called `ASPNETCORE_ENVIRONMENT`, which can be retrieved from CI pipeline or docker etc. So it will get proper settings.

One more thing for settings is it's better to use some secret store service like **Azure Key Vault** or **AWS KMS** but not directly store passwords in the config file.



## Testing

### Unit Tests

Unit tests were done in this demo to cover most of code. According to `coverlet`, the line coverage is 72.9% and branch coverage (meaning if, switch or all other possible code execution path) is 91.5%.

![unit tests code coverage](https://github.com/stnlylee/WebApiDemo/blob/main/ReadmeImages/unit%20tests%20code%20coverage.png)

For more drill-down details, you can see it's still can be improved later. I left that for demo purpose (what an excuse!)

![unit tests code coverage 2](https://github.com/stnlylee/WebApiDemo/blob/main/ReadmeImages/unit%20tests%20code%20coverage%202.png)

To generate testing coverage report:

Run this command to generate `coverage.cobertura.xml` in the project folder, it's a common format that other software or SAAS can recognize, eg. if you want to integrate **SonarQube** https://www.sonarqube.org/ to CI/CD pipeline

`dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura`

To generate a local report you need to install a tool called `reportgenerator`

`dotnet tool install -g dotnet-reportgenerator-globaltool`

Then you can use this command to generate pretty HTML report for the xml report

`reportgenerator -reports:"\PathToProject\CommSec.Movie.UnitTests\coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:Html`



### Integration Tests

Unit tests are more specific to individual to components it can have thousands in an application and they are very fast. Integration tests normally targeting small part of application that is actually used as same way the end users use. It can be slow. Integration testing was hard before, but now it's possible since Microsoft has provided us a feasible way to do that in memory.

In my demo, `CustomWebApplicationFactory<TStartup>` is created, which is basically a in-memory application server, it will do the same thing as the real application server will do. It can even create in-memory database. I tested all 4 endpoints using this.

Also some times integration tests can behave strangely because all things come together. In this demo, cache may effect the testing result so I used **Test priority for XUnit** (https://github.com/asherber/Xunit.Priority) to let tests comes with a certain order so it will not mess up.



## Application Deploy / Release

It is almost 100% an application is in a CI/CD pipeline in a development house for now. And it's very common that Docker and Kubernetes are used in the pipeline. In this demo I enabled Docker support by writing `Dockerfile` and `docker-compose.yml` in the solution folder. This image can be pushed to container registry like **Azure ACR** and **AWS ECR**. Then deployed to K8s service like **Azure AKS** or **AWS ECS**.

Run `docker-compose up`

![docker running](https://github.com/stnlylee/WebApiDemo/blob/main/ReadmeImages/docker%20running.png)

Please note the `Dockerfile` is optimized for production use:

-  Use `aspnet:5.0-alpine` base images for small footprint in CI/CD pipeline, so the docker image will be small (60Mb) it can reduce the building time
-  No multiple `dotnet restore` it can reduce time further
-  Optimization of `dot publish` so it will the artifact will be fit for production use
-  `dotnetuser` created for running app but not directly using root user for better security
-  Patched the image during building process for minimum known vulnerabilities

Some other efforts needed like writing Shell scripting to integrate application into CI/CD pipeline eg. **Teamcity**, **Gitlab**, **Bitbucket**, **Azure DevOps**. I placed a `CI.yaml` for demo purpose.

Or use **Terraform** to automate this.



## Improvements

We can make some improvements based on this demo.

### Searching

For searching it's normally not a good idea to grab all data in one go. So pagination is needed in this case. Also sometimes performance for relational database like SQL Server is not good if there are too many join, we can create an indexer to index data into some NoSQL database like Elasticsearch, which is a very good choice for searching.



### Caching

As mentioned it is very easy to upgrade the caching to use Redis. Also need to invalidate cache after adding or updating.



### CQRS

With **MediatR** (https://github.com/jbogard/MediatR) it is easy to implement CQRS pattern. Main benefit is to separate the read (Query) and write (Command) logic so it can be scale independently.



### More Security

Something to think about - OWASP10 (https://owasp.org/www-project-top-ten/)

Implement Anti Forgery Token for CSRF / XSRF attack.  The idea is to include an `X-XSRF-TOKEN` header (for AJAX calls) or a `__RequestVerificationToken` in the form postback, that must be the same as the original server issued.



### Secret Management

It should use managed secrete service like **Azure Key Vault** or **AWS KMS** to store passwords but not stored directly in config files



### Load/Performance Testing

Lot of options - Apache JMeter, K6 etc



### Endpoint health check

It will be better to test database for health check



## Summary

When I was doing this demo following references helped me a lot:

- This blog helped me on Integration Tests - https://timdeschryver.dev/blog/how-to-test-your-csharp-web-api
- More read on CQRS - https://docs.microsoft.com/en-us/azure/architecture/patterns/cqrs
- This helped me on building the production ready docker - https://www.thorsten-hans.com/how-to-build-smaller-and-secure-docker-images-for-net5/
- This helped me to have more understanding on JWT - https://anil-pace.medium.com/json-web-tokens-vs-oauth-2-0-85dd0b32057d
- This helped me on troubleshooting on building a docker but no access to host, it just need to add some binding in `Program.cs` - https://newbedev.com/how-do-i-get-the-kestrel-web-server-to-listen-to-non-localhost-requests
- This helped me on test coverage - https://docs.microsoft.com/en-us/dotnet/core/testing/unit-testing-code-coverage?tabs=windows
- This helped me on rate limiting - https://github.com/stefanprodan/AspNetCoreRateLimit/wiki/IpRateLimitMiddleware#setup
- For Anti Forgery Token - https://www.blinkingcaret.com/2018/11/29/asp-net-core-web-api-antiforgery/

Finally, this demo is crafted by my passion and ♥ to technologies. Star it if you like.



## About Myself

My name is Stanley Li. I am a Senior Developer living in Sydney with my wife and son. I have more than 15 years experiences in .NET, Clouding, enterprise software design and management. I worked in many large companies and governments as a developer or consultant. My LinkedIn profile is https://www.linkedin.com/in/stanley-li-12158414/.

![myself](https://github.com/stnlylee/WebApiDemo/blob/main/ReadmeImages/myself.png) 

