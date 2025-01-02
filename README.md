# Currencyinator
Simple .NET web api for downloading currency reates from NBP api. Rates are downloaded once from api and cached in mongo database for faster responses.

### Currently supported actions

- Getting currency buy and sell rate by providing currency code and date. 

## Running project

### Run with docker compose

#### Requirements

- Docker desktop 

#### Steps

1. Pull repository
2. Open solution in Visual Studio
3. Set `docker-compose` project as startup project if needed
4. Start app by hitting `Start without debugging` or Ctrl + F5

**Alternatively you can run project without Visual Studio by executing command in repository root folder:**

```bash
docker compose up -d
```

### Run as console app

#### Requirements

- Mongo database server (optional)
- .Net SDK version 9.0 or higher

#### Steps

1. Pull repository
2. Open solution in Visual Studio
3. In `CurrencyInator.Api/appsettings.json` file set your own *ConnectionString* and *DatabaseName* or change *Enabled* option to *false* if you don't want to use database.
3. Set `CurrencyInator.Api` project as startup project if needed
4. Set `http` launch option 
4. Start app by hitting "Start without debugging" or Ctrl + F5

## Explore API

After succesfull project startup you can:
- go to http://localhost:5231 and explore API with `scalar` UI
- directly request endpoint GET http://localhost:5231/{Currency}/{Date} with postman or browser
- use `CurrencyInator.Api.http` file.

## Tests

Solution contains `unit tests`, `integration tests` and `end to end tests` for main use cases.

*Note: Integration tests and E2E tests to work properly needs docker desktop installed on your machine*
