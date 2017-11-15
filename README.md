Lidarr Update Server [![AppVeyor](https://img.shields.io/appveyor/ci/lidarr/lidarrapi-update/master.svg?maxAge=60&style=flat-square)](https://ci.appveyor.com/project/lidarr/lidarrapi-update)
===================

This is the update API of [https://github.com/Lidarr/Lidarr](https://github.com/Lidarr/Lidarr). The API is forked from [Radarr's update server](https://github.com/Radarr/RadarrAPI.Update)

## Development

If you want to work on **LidarrAPI.Update**, make sure you have [.NET Core 2.0 SDK](https://www.microsoft.com/net/download/core) installed and [Visual Studio 2017 RC](https://www.visualstudio.com/vs/visual-studio-2017-rc/).

## Using Docker

If you would like to use the docker setup we have for this project, follow these directions:
- Setup Environment Variables
	- In the `docker-services/mysql/Dockerfile` 
		- Make sure to set your ROOT password and the Database if you want to change it
	- In the `docker-compose.yml`
		- Make sure to update the `environment:` section to add your ApiKey, and your mysql root password
		
The most important thing is the `ApiKey`, the rest can be used **AS-IS**, but if the ApiKey is not set, fetching updates from AppVeyor and Github will not function correctly.