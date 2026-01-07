## Running the Backend

To build the backend, navigate to the `src` folder and run:  
```sh
dotnet build
```

To run all tests:  
```sh
dotnet test
```

To start the main API:  
```sh
cd Fundo.WebApi  
dotnet run
```

The following endpoint should return **200 OK**:  
```http
GET -> https://localhost:5001/loan
```

## Notes  

Feel free to modify the code as needed, but try to **respect and extend the current architecture**, as this is intended to be a replica of the Fundo codebase.
