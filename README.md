
## This project is the basis of a Stock Management Tool.

This basis has:
- Connection to MongoDB
- Seperated BusinessLogic from UI
- Creation of Stock Items
- Checking Stock Items in and out
- Create a transaction history of all those checkin /-outs
- Coming soon: Multilanguage support through settings

# For WiX Installer project to work::
1. WiX v3 Extension for VS: Extensions -> Manage Extensions... -> Browse -> install "WiX v3 - Visual Studio 2022 Extension"
2. Download (and install) WiX Toolset (Min v3.11) from: https://github.com/wixtoolset/wix3/releases
3. Restart Visual Studio (maybe PC).

Getting an Installer as follows:
1. Rebuild the Installer project with the desired configuration (Release x64 for example)
2. Output will be in the following folder: Inside the solution directory -> StockManagement.Installer -> Installer

# Mongo-DB Installer Download
https://www.mongodb.com/try/download/community

# Database config
- `appsettings.json` next to the exe: `ConnectionStrings:Mongo`, `Mongo:DatabaseName`
- Per install: `appsettings.local.json` with the keys to override (kept on upgrade)

# MongoDB replica set (ADR-0006)
- Needed for transactions; standalone `mongod` → sales fail
- `mongod.cfg` (Windows: `C:\Program Files\MongoDB\Server\<version>\bin\mongod.cfg`):
```yaml
replication:
  replSetName: rs0
```
- Restart the `MongoDB` service
- Once: `mongosh --eval "rs.initiate()"`
- Docker: `docker run -d -p 27017:27017 mongo:7 --replSet rs0` + `docker exec <id> mongosh --eval "rs.initiate()"`
