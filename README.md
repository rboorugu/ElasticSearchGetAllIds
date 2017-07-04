# ElasticSearchGetAllIds
project build in .net core which will scan elasticsearch uisng scroll API to get all IDs and save them to text file comma delimited
before you run the project update the appsettings.json  with 
savePathFolder: where the end file will be saved
elasticsearch:endpoints: list of all the endpoint to scan for, this should include end point/index/type
