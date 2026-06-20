# ?? API Endpoints Documentation

Welcome to the **SmartBin API Documentation**. This document covers all the endpoints provided by the application, clearly outlining the request and response structures including headers, bodies, routes, and expected codes. 

---

## ?? AuthController
Base Route: `/api/Auth`

### Register
Create a new user account.
- **Endpoint**: `POST /api/Auth/register`
- **Body** `(JSON)`:
  ```json
  {
    "Email": "user@example.com", // Required, Valid Email
    "Password": "password123", // Required, Min length 6
    "FullName": "John Doe" // Required
  }
  ```
- **Response**: 
  - `200 OK` on success.
  - `400 BadRequest` on error.

### Login
Authenticate a user and receive a token.
- **Endpoint**: `POST /api/Auth/login`
- **Body** `(JSON)`:
  ```json
  {
    "Email": "user@example.com", // Required, Valid Email
    "Password": "password123" // Required
  }
  ```
- **Response**: 
  - `200 OK` on success.
  - `401 Unauthorized` on error.

---

## ??? BinController
Base Route: `/api/SmartBin`

### Create Bin
Initialize a new blank bin system logic.
- **Endpoint**: `POST /api/SmartBin/Create`
- **Response**: `200 OK` with a newly created token string.

### Assign Bin
Assign a previously created bin via its token to the authenticated user.
- **Endpoint**: `POST /api/SmartBin/Assign`
- **Authorization**: Required (Bearer Token)
- **Body** `(JSON)`:
  ```json
  {
    "token": "string"
  }
  ```
- **Response**: 
  - `200 OK` on success.
  - `400 BadRequest` if assignment fails.

### Get All Bins
Retrieve a list of all assigned bins to the current user.
- **Endpoint**: `GET /api/SmartBin`
- **Authorization**: Required (Bearer Token)
- **Response**: `200 OK` with a list of `BinMV`:
  ```json
  [
    {
      "BinId": 1,
      "Latitude": 0.0,
      "Longitude": 0.0
    }
  ]
  ```

### Get Bin Details
Retrieve details for a specific bin, including its sections, if owned by the user.
- **Endpoint**: `GET /api/SmartBin/{Id}`
- **Authorization**: Required (Bearer Token)
- **Route Parameter**: `int Id` (Bin Id)
- **Response**: 
  - `200 OK` with `BinDetailsMV`
  - `404 NotFound` if bin is not found.

### Update Bin
Update geographical coordinates or properties of a bin.
- **Endpoint**: `PUT /api/SmartBin/Update`
- **Header**: `Token` (hardware IdentificationToken)
- **Body** `(JSON)`:
  ```json
  {
    "Latitude": 12.345,
    "Longitude": 67.890
  }
  ```
- **Response**: 
  - `200 OK` with updated `BinMV`.
  - `404 NotFound` if the bin is not found.

### Delete Bin
Delete an existing bin.
- **Endpoint**: `DELETE /api/SmartBin/Delete`
- **Header**: `Token` (hardware IdentificationToken)
- **Response**: 
  - `200 OK` on success.
  - `404 NotFound` if the bin is not found.

---

## ??? BinSectionController
Base Route: `/api/BinSection`

### Update Bin Section
Updates the capacities inside a bin's subsection (e.g., Plastic, Metal).
- **Endpoint**: `PUT /api/BinSection/update`
- **Header**: `Token` (hardware token)
- **Body** `(JSON)`:
  ```json
  {
    "MaterialName": "Plastic",
    "LevelPercentage": 75.5,
    "Weight": 12.2
  }
  ```
- **Response**: 
  - `200 OK` with updated `BinSectionMV`.
  - `404 NotFound` if no mapping or matching bin/section is found.

### Get All Bin Sections
Retrieves subsections of a given bin ID.
- **Endpoint**: `GET /api/BinSection/SmartBin/{Id}`
- **Authorization**: Required (Bearer Token)
- **Route Parameter**: `int Id` (Bin Id)
- **Response**: 
  - `200 OK` with a list of sections.
  - `404 NotFound` if the bin is not found.

---

## ?? CrowdDensityController
Base Route: `/api/CrowdDensity`

### Get Last State
Retrieves the most recent crowd density logged by a specific bin.
- **Endpoint**: `GET /api/CrowdDensity`
- **Query Parameter**: `int BinId`
- **Response**: 
  - `200 OK` with Crowd Density Details.
  - `404 NotFound` if no data available.

### Add Crowd Density
Upload a new image showing current crowd density from the bin's camera constraints.
- **Endpoint**: `POST /api/CrowdDensity`
- **Header**: `Token` (hardware token)
- **Body** `(multipart/form-data)`:
  - `Image`: `(file)` image byte map 
- **Response**: 
  - `200 OK` on success.
  - `400 BadRequest` on validation failure.
  - `404 NotFound` if the bin is not found.

---

## ?? SurroundingWasteController
Base Route: `/api/SurroundingWaste`

### Get Last State
Retrieves the most recent record of surrounding wastes reported around a given bin.
- **Endpoint**: `GET /api/SurroundingWaste`
- **Query Parameter**: `int BinId`
- **Response**: 
  - `200 OK` with Surrounding Waste Details.
  - `404 NotFound` if no data available.

### Add Surrounding Waste
Publish data and images for new surrounding waste issues detected by the bin's hardware.
- **Endpoint**: `POST /api/SurroundingWaste`
- **Header**: `Token` (hardware token)
- **Body** `(multipart/form-data)`:
  - `Image`: `(file, optional)`
- **Response**: 
  - `200 OK` on success.
  - `400 BadRequest` on validation failure.
  - `404 NotFound` if the bin is not found.

---

## ?? TransactionController
Base Route: `/api/Transaction`

### Get All Transactions For A Bin
Fetches page-indexed throwing transactions logs related to a user's bin.
- **Endpoint**: `GET /api/Transaction/SmartBin/{BinId}/page/{PageNumber}`
- **Authorization**: Required (Bearer Token)
- **Route Parameters**: 
  - `int BinId` 
  - `int PageNumber`
- **Query Parameter**: `int PageSize` (Optional, defaults to 10)
- **Response**: 
  - `200 OK` with a list of `TransactionResultMV`.
  - `401 Unauthorized` if not authorized or valid ownership fails.

### Add Transaction
Submit a throwing activity transaction from the bin hardware.
- **Endpoint**: `POST /api/Transaction`
- **Header**: `Token` (hardware token)
- **Body** `(multipart/form-data)`:
  - `MaterialName`: `(string, optional)`
  - `Image`: `(file, optional)`
  - `weight`: `(in gram) optional`
  *(Note: You must provide either a MaterialName or an Image. Both cannot be empty.)*
- **Response**: 
  - `200 OK` on success.
  - `400 BadRequest` on validation failure or missing mandatory requirements.