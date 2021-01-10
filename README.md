# azuredigitaltwins-to-kekoapi

This repo will define how Azure Digital Twins (ADT) Platform models and data can be transformed to KEKO APIs. ADT provides query languange which allows quering ADT for digital twins and properties. You can learn more about ADT query language from here https://docs.microsoft.com/en-us/azure/digital-twins/concepts-query-language.

You can find below example queries how KEKO APIs related to Building Information Models (BIM) and Indoor environmental quality. The existing KEKO APIs are here  https://developer.kekoecosystem.com/develop/resources/api-listing/ 

## Example case and demo

The example case for which also the demo implementation is included is based on [the original example of the DTDL use of RealEstateCore](https://github.com/Azure/opendigitaltwins-building#using-realestatecore-ontology).

![Example building graph](https://drive.google.com/uc?export=view&id=1eeNp_BzVsgpMcYXhRVNfAB-aO3piO4pC)

Example responses from the demo are included in below.

## Building Information Model

GET /bim/areas/{building_id}, where building_id=Building121

Azure Digital Twin query:
`SELECT Room,Level FROM DIGITALTWINS Room JOIN Level RELATED Room.isPartOf JOIN Building RELATED Level.isPartOf where Building.$dtId = 'Building121' AND IS_OF_MODEL(Room, 'dtmi:digitaltwins:rec_3_3:core:Room;1') AND IS_OF_MODEL(Level, 'dtmi:digitaltwins:rec_3_3:core:Level;1')  AND IS_OF_MODEL(Building, 'dtmi:digitaltwins:rec_3_3:core:Building;1')`

Demo example response:
```
[
  {
    "name": "Level name 1",
    "area_id": "Level1",
    "building_id": "Building121",
    "area_type": "dtmi:digitaltwins:rec_3_3:core:Level;1"
  },
  {
    "name": "Level name 2",
    "area_id": "Level2",
    "building_id": "Building121",
    "area_type": "dtmi:digitaltwins:rec_3_3:core:Level;1"
  }
]
```

GET /bim/area_resources/{building_id}/{area_id}, where building_id=Building121 and area_id=Level2

Azure Digital Twin query:
`SELECT Room,Level FROM DIGITALTWINS Room JOIN Level RELATED Room.isPartOf JOIN Level RELATED Room.isLocationOf JOIN Building RELATED Level.isPartOf where Building.$dtId = 'Building121' AND Level.$dtId = 'Level2' AND IS_OF_MODEL(Room, 'dtmi:digitaltwins:rec_3_3:core:Room;1') AND IS_OF_MODEL(Level, 'dtmi:digitaltwins:rec_3_3:core:Level;1')  AND IS_OF_MODEL(Building, 'dtmi:digitaltwins:rec_3_3:core:Building;1')`

GET /bim/ids

Azure Digital Twin query:
`SELECT Building FROM DIGITALTWINS Building where IS_OF_MODEL(Building, 'dtmi:digitaltwins:rec_3_3:core:Building;1')`

## Indoor environmental quality

GET /environmental/sensors/{building_id}, where building_id=Building121

Azure Digital Twin query:
`SELECT Room,Sensor, Level FROM DIGITALTWINS Room JOIN Level RELATED Room.isPartOf JOIN Sensor RELATED Room.hasCapability JOIN Building RELATED Level.isPartOf where Building.$dtId = 'Building121' AND IS_OF_MODEL(Room, 'dtmi:digitaltwins:rec_3_3:core:Room;1') AND IS_OF_MODEL(Sensor, 'dtmi:digitaltwins:rec_3_3:core:Sensor;1') AND IS_OF_MODEL(Level, 'dtmi:digitaltwins:rec_3_3:core:Level;1')  AND IS_OF_MODEL(Building, 'dtmi:digitaltwins:rec_3_3:core:Building;1')`

Demo example response:
```
[
  {
   resource_id: "TemperatureR",
   type: "dtmi:digitaltwins:rec_3_3:core:TemperatureSensor;1",
   area_id: "Level1",
   building_id: "Building121"
  },
  {
   resource_id: "TempSensor1",
   type: "dtmi:digitaltwins:rec_3_3:core:AirTemperatureSensor;1",
   area_id: "Level1",
   building_id: "Building121"
  },
  {
   resource_id: "Humidity1",
   type: "dtmi:digitaltwins:rec_3_3:core:HumiditySensor;1",
   area_id: "Level1",
   building_id: "Building121"
  }
]
```

GET /environmental/humidity/{building_id}/{resource_id}, where building_id=Building121 and resource_id=Humidity1

Azure Digital Twin query:
`SELECT Sensor FROM DIGITALTWINS Sensor where Sensor.$dtId = 'Humidity1' AND IS_OF_MODEL(Sensor, 'dtmi:digitaltwins:rec_3_3:core:HumiditySensor;1')`

Other sensory data can be added similarly.

Next steps is to have full set of mappings and Digital Twins Desription Models required for implementing KEKO APIs.

