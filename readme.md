Work in progress .net photbooth app based on opencv.

Planned features:
* Easy to configure user selectable print layouts
* Saving collage and source images to API to give users access via qr code
* Printing on photo printers like Canon Selphy CP1500
* Smart home lighting integration (deConz paired with TRÅDFRI lights) 
* Webcam and PTP camera integration for image acqusition

## Web

Configuration from appsettings.json can be overrided by enveironment variables:

raBooth_ConnectionStrings__MySql=\<ConnectionString>
raBooth_FilesystemPhotoStorageConfiguration__DirectoryPath=\<Path>
raBooth_ApiKeysConfiguration__ApiKeys__\<Index>=\<ApiKey>

To run application in docker:
* Bind port 8080
* Configure MySql connection string to bridge network IP
* Mount volume to ~\rabooth-storage or path configured by FilesystemPhotoStorageConfiguration:DirectoryPath configuration key