# azuredigitaltwins-to-kekoapi

This repo will define how Azure Digital Twins (ADT) Platform models and data can be transformed to KEKO APIs. ADT provides query languange which allows quering ADT for digital twins and properties. You can learn more about ADT query language from here https://docs.microsoft.com/en-us/azure/digital-twins/concepts-query-language.

You can find below example queries how KEKO APIs related to Building Information Models (BIM) and Indoor environmental quality. The existing KEKO APIs are here  https://developer.kekoecosystem.com/develop/resources/api-listing/ 

## Building Information Model

/bim/areas/{building_id}, where building_id=Building121

`SELECT Room,Level FROM DIGITALTWINS Room JOIN Level RELATED Room.isPartOf JOIN Building RELATED Level.isPartOf where Building.$dtId = 'Building121' AND IS_OF_MODEL(Room, 'dtmi:digitaltwins:rec_3_3:core:Room;1') AND IS_OF_MODEL(Level, 'dtmi:digitaltwins:rec_3_3:core:Level;1')  AND IS_OF_MODEL(Building, 'dtmi:digitaltwins:rec_3_3:core:Building;1')`

/bim/area_resources/{building_id}/{area_id}, where building_id=Building121 and area_id=Level2

`SELECT Room,Level FROM DIGITALTWINS Room JOIN Level RELATED Room.isPartOf JOIN Level RELATED Room.isLocationOf JOIN Building RELATED Level.isPartOf where Building.$dtId = 'Building121' AND Level.$dtId = 'Level2' AND IS_OF_MODEL(Room, 'dtmi:digitaltwins:rec_3_3:core:Room;1') AND IS_OF_MODEL(Level, 'dtmi:digitaltwins:rec_3_3:core:Level;1')  AND IS_OF_MODEL(Building, 'dtmi:digitaltwins:rec_3_3:core:Building;1')`

/bim/ids

`SELECT Building FROM DIGITALTWINS Building where IS_OF_MODEL(Building, 'dtmi:digitaltwins:rec_3_3:core:Building;1')`

## Indoor environmental quality

/environmental/sensors/{building_id}, where building_id=Building121

`SELECT Room,Sensor, Level FROM DIGITALTWINS Room JOIN Level RELATED Room.isPartOf JOIN Sensor RELATED Room.hasCapability JOIN Building RELATED Level.isPartOf where Building.$dtId = 'Building121' AND IS_OF_MODEL(Room, 'dtmi:digitaltwins:rec_3_3:core:Room;1') AND IS_OF_MODEL(Sensor, 'dtmi:digitaltwins:rec_3_3:core:Sensor;1') AND IS_OF_MODEL(Level, 'dtmi:digitaltwins:rec_3_3:core:Level;1')  AND IS_OF_MODEL(Building, 'dtmi:digitaltwins:rec_3_3:core:Building;1')`

/environmental/humidity/{building_id}/{resource_id}, where building_id=Building121 and resource_id=Humidity1

`SELECT Sensor FROM DIGITALTWINS Sensor where Sensor.$dtId = 'Humidity1' AND IS_OF_MODEL(Sensor, 'dtmi:digitaltwins:rec_3_3:core:HumiditySensor;1')`

Other sensory data can be added similarly.

NExt steps is to have full set of mappings and Digital Twins Desription Models required for implementing KEKO APIs.