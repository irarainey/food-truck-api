# San Francisco Food Truck API

To better enable administration for the licensing of the vast array of food trucks in the San Francisco area, the city manage a dataset of food trucks together with information on the type of food they serve, their location, and their licence status.

This dataset is made available to the public, in a range of different formats, for use in third parties applications. This dataset can be found on the [city's data portal](https://data.sfgov.org/Economy-and-Community/Mobile-Food-Facility-Permit/rqzj-sfat/data).

This project utiises that dataset to provide a simple REST API to query the trucks available within a given range of a specified starting point. This data is returned in [GeoJSON](https://datatracker.ietf.org/doc/html/rfc7946) format, which can then easily be consumed other applications to render onto a map.

An example project to render this data onto a map can be found [in this repository](https://github.com/irarainey/food-truck-spa), and hosted at [https://foodtrucks.codeshed.dev](https://foodtrucks.codeshed.dev).

The API is written as an Azure Function App in C# using the .NET 6 framework. It also utilises Azure CosmosDB are a data store for truck data. The API comprises two methods, `ImportTruckData` and `GetTrucks`.

## ImportTruckData

The `ImportTruckData` method runs as a timer-triggered Azure Function which runs once each day at 08:00. This method imports the data from the published public data endpoint and imports it into an Azure CosmosDB database. This is done to allow for the data to be transformed into the standard [GeoJSON](https://datatracker.ietf.org/doc/html/rfc7946) format to make it easier to consume in mapping applications.

By storing the data in a separate database it gives us a persistent source of data to serve to any front-end application, and offers the ability to utilise the [geospatial data](https://docs.microsoft.com/en-us/azure/cosmos-db/sql/sql-query-geospatial-query) query functionality of Azure CosmosDB.

Imported data is stored in the following structure.

```json
{
        "id": "1591820",
        "type": "Feature",
        "properties": {
            "applicant": "Truly Food & More",
            "facilitytype": "Truck",
            "locationdescription": "SANSOME ST: PINE ST to CALIFORNIA ST (200 - 299)",
            "address": "217 SANSOME ST",
            "block": "0260",
            "lot": "004",
            "status": "APPROVED",
            "fooditems": "Latin Food: Tacos: Pupusas: Vegetables: Salad: Waters: Sodas",
            "approved": "2022-02-09T00:00:00",
            "expirationdate": "2022-11-15T00:00:00"
        },
        "geometry": {
            "type": "Point",
            "coordinates": [
                -122.40126969523558,
                37.79238986198323
            ]
        }
    }
```

## GetTrucks

The `GetTrucks` method runs as a http-triggered Azure Function exposed as a `GET` request. The method takes in parameters for the starting latitude and longitude of the query and the range to query for food trucks.

```
https://food-truck-api.azurewebsites.net/api/GetTrucks?lon=-122.4052692&lat=37.7919346&distance=800
```
The API method queries the database using the [geospatial data](https://docs.microsoft.com/en-us/azure/cosmos-db/sql/sql-query-geospatial-query) query functionality and returns an array of objects as specified above, detailing all food trucks found within the range specified in the request. Trucks are returned is their licence status is `APPROVED` and their licence has not expired.
