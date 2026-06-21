# Smart Bin System — Full API Reference

> **Base URL:** `https://smartbinpsu.runasp.net`  
> **Local Dev URL:** `http://localhost:5025`

---

## 🔐 Authentication

All protected endpoints require a **JWT Bearer token** in the `Authorization` header.

```
Authorization: Bearer <your_jwt_token>
```

- **Admin** endpoints additionally require the user to have the `Admin` role.
- **Device** endpoints (POST data from ESP hardware) use a **device Token** sent in a custom `Token` header — **not** the JWT.

---

## 📋 Table of Contents

1. [Auth](#1-auth)
2. [Bin (SmartBin)](#2-bin-smartbin)
3. [Bin Sections](#3-bin-sections)
4. [Bin Requests](#4-bin-requests)
5. [Camera](#5-camera)
6. [Camera Requests](#6-camera-requests)
7. [Transactions (AI Classification)](#7-transactions-ai-classification)
8. [Crowd Density](#8-crowd-density)
9. [Surrounding Waste](#9-surrounding-waste)

---

## 1. Auth

### `POST /api/Auth/register`

Register a new user account.

- **Auth required:** ❌ None
- **Content-Type:** `application/json`

**Request Body:**
```json
{
  "email": "user@example.com",
  "password": "Aa12345678@",
  "fullName": "Ahmed Mohamed"
}
```

| Field | Type | Required | Notes |
|---|---|---|---|
| `email` | string | ✅ | Valid email format |
| `password` | string | ✅ | Minimum 6 characters |
| `fullName` | string | ✅ | Display name |

**Response `200 OK`:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "email": "user@example.com",
  "userId": "abc123",
  "role": "User"
}
```

**Response `400 Bad Request`:**
```json
{ "message": "Email is already registered." }
```

---

### `POST /api/Auth/login`

Login and receive a JWT token.

- **Auth required:** ❌ None
- **Content-Type:** `application/json`

**Request Body:**
```json
{
  "email": "user@example.com",
  "password": "Aa12345678@"
}
```

**Response `200 OK`:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "email": "user@example.com",
  "userId": "abc123",
  "role": "User"
}
```

> Save the `token` and send it as `Authorization: Bearer <token>` on all protected requests.

**Response `401 Unauthorized`:**
```json
{ "message": "Invalid email or password." }
```

---

## 2. Bin (SmartBin)

> Route prefix: `/api/SmartBin`

---

### `POST /api/SmartBin/Create`

Create a new unassigned bin and get its identification token.

- **Auth required:** ✅ Admin only

**Request:** No body needed.

**Response `200 OK`:**
```
"BIN-TOKEN-XXXXXXXX-XXXX"
```
> This token is the device's **IdentificationToken** — flash it to the ESP hardware.

---

### `POST /api/SmartBin/Assign`

Assign a bin to the authenticated user using its token. *(Used internally by the admin approval flow — you normally don't call this manually.)*

- **Auth required:** ✅ Any logged-in user
- **Content-Type:** `application/json`

**Request Body:**
```json
{
  "token": "BIN-TOKEN-XXXXXXXX-XXXX"
}
```

**Response `200 OK`:**
```
"Bin assigned successfully"
```

**Response `400 Bad Request`:**
```
"Bin not found or already assigned"
```

---

### `GET /api/SmartBin`

Get all bins owned by the logged-in user.

- **Auth required:** ✅ Any logged-in user

**Response `200 OK`:**
```json
[
  {
    "binId": 1,
    "latitude": 30.0444,
    "longitude": 31.2357
  },
  {
    "binId": 2,
    "latitude": 30.1234,
    "longitude": 31.4567
  }
]
```

---

### `GET /api/SmartBin/{id}`

Get details of a single bin (including sections).

- **Auth required:** ✅ Any logged-in user

**URL Params:**
| Param | Type | Description |
|---|---|---|
| `id` | int | The Bin ID |

**Response `200 OK`:**
```json
{
  "binId": 1,
  "latitude": 30.0444,
  "longitude": 31.2357,
  "sections": [
    { "materialName": "Plastic", "levelPercentage": 45.5, "weight": 1.2 },
    { "materialName": "Metal",   "levelPercentage": 20.0, "weight": 0.5 },
    { "materialName": "Paper",   "levelPercentage": 10.0, "weight": 0.3 }
  ]
}
```

**Response `404 Not Found`** if the bin doesn't belong to the user.

---

### `PUT /api/SmartBin/Update`

Update a bin's GPS coordinates. Called by the ESP device.

- **Auth required:** ❌ None (uses device Token header)
- **Content-Type:** `application/json`

**Headers:**
```
Token: BIN-TOKEN-XXXXXXXX-XXXX
```

**Request Body:**
```json
{
  "latitude": 30.0500,
  "longitude": 31.2400
}
```

**Response `200 OK`:**
```json
{
  "binId": 1,
  "latitude": 30.0500,
  "longitude": 31.2400
}
```

**Response `404 Not Found`** if the token is invalid.

---

### `DELETE /api/SmartBin/Delete`

Delete a bin by its device token.

- **Auth required:** ❌ None (uses device Token header)

**Headers:**
```
Token: BIN-TOKEN-XXXXXXXX-XXXX
```

**Response `200 OK`:**
```
"Bin deleted successfully"
```

---

## 3. Bin Sections

> Route prefix: `/api/BinSection`

---

### `GET /api/BinSection/SmartBin/{Id}`

Get all sections (compartments) for a specific bin.

- **Auth required:** ✅ Any logged-in user

**URL Params:**
| Param | Type | Description |
|---|---|---|
| `Id` | int | The Bin ID |

**Response `200 OK`:**
```json
[
  { "materialName": "Plastic", "levelPercentage": 45.5, "weight": 1.2 },
  { "materialName": "Metal",   "levelPercentage": 20.0, "weight": 0.5 },
  { "materialName": "Paper",   "levelPercentage": 10.0, "weight": 0.3 }
]
```

---

### `PUT /api/BinSection/update`

Update a bin section's fill level and weight. Called by the ESP device.

- **Auth required:** ❌ None (uses device Token header)
- **Content-Type:** `application/json`

**Headers:**
```
Token: BIN-TOKEN-XXXXXXXX-XXXX
```

**Request Body:**
```json
{
  "materialName": "Plastic",
  "levelPercentage": 60.0,
  "weight": 1.8
}
```

**Response `200 OK`:**
```json
{
  "materialName": "Plastic",
  "levelPercentage": 60.0,
  "weight": 1.8
}
```

---

## 4. Bin Requests

> Route prefix: `/api/BinRequest`

---

### `POST /api/BinRequest`

Submit a request to purchase/get a new Smart Bin.

- **Auth required:** ✅ Any logged-in user
- **Content-Type:** `application/json`

**Request Body:**
```json
{
  "requesterName": "Ahmed Mohamed",
  "latitude": 30.0444,
  "longitude": 31.2357
}
```

| Field | Type | Required | Notes |
|---|---|---|---|
| `requesterName` | string | ✅ | Full name |
| `latitude` | double | ✅ | Must not be 0 |
| `longitude` | double | ✅ | Must not be 0 |

**Response `200 OK`:**
```json
{
  "id": 5,
  "requesterName": "Ahmed Mohamed",
  "latitude": 30.0444,
  "longitude": 31.2357,
  "status": "Pending",
  "createdAt": "2025-06-20T14:00:00Z",
  "userEmail": "user@example.com",
  "userFullName": "Ahmed Mohamed",
  "userId": 12,
  "binId": null,
  "identificationToken": null
}
```

---

### `GET /api/BinRequest`

Get all bin requests. *(Admin panel)*

- **Auth required:** ✅ Admin only

**Response `200 OK`:** Array of `BinRequestResultMV` (same shape as above, all users).

---

### `GET /api/BinRequest/my`

Get all bin requests submitted by the logged-in user.

- **Auth required:** ✅ Any logged-in user

**Response `200 OK`:** Array of `BinRequestResultMV`.

```json
[
  {
    "id": 5,
    "requesterName": "Ahmed Mohamed",
    "latitude": 30.0444,
    "longitude": 31.2357,
    "status": "Accepted",
    "createdAt": "2025-06-20T14:00:00Z",
    "userEmail": "user@example.com",
    "userFullName": "Ahmed Mohamed",
    "userId": 12,
    "binId": 3,
    "identificationToken": "BIN-TOKEN-XXXXXXXX-XXXX"
  }
]
```

> After the admin accepts, `status` becomes `"Accepted"`, `binId` is set, and the `identificationToken` is revealed (for the ESP device).

---

### `POST /api/BinRequest/{id}/Accept`

Admin accepts a pending bin request → creates a bin with a token and assigns it.

- **Auth required:** ✅ Admin only

**URL Params:**
| Param | Type | Description |
|---|---|---|
| `id` | int | The request ID |

**Request:** No body needed.

**Response `200 OK`:** Updated `BinRequestResultMV` with `status: "Accepted"` and the new `identificationToken`.

**Response `400 Bad Request`:**
```json
{ "message": "Request not found or already processed." }
```

---

## 5. Camera

> Route prefix: `/api/Camera`

---

### `POST /api/Camera/Create`

Create a new unassigned camera and get its identification token.

- **Auth required:** ✅ Admin only

**Request:** No body needed.

**Response `200 OK`:**
```
"CAM-TOKEN-XXXXXXXX-XXXX"
```

---

### `POST /api/Camera/Assign`

Assign a camera to the authenticated user using its token.

- **Auth required:** ✅ Any logged-in user
- **Content-Type:** `application/json`

**Request Body:**
```json
{
  "token": "CAM-TOKEN-XXXXXXXX-XXXX"
}
```

**Response `200 OK`:**
```
"Camera assigned successfully"
```

---

### `GET /api/Camera`

Get all cameras owned by the logged-in user.

- **Auth required:** ✅ Any logged-in user

**Response `200 OK`:**
```json
[
  { "cameraId": 1, "latitude": 30.0444, "longitude": 31.2357 },
  { "cameraId": 2, "latitude": 30.1500, "longitude": 31.3000 }
]
```

---

### `GET /api/Camera/{id}`

Get details of a single camera.

- **Auth required:** ✅ Any logged-in user

**Response `200 OK`:**
```json
{
  "cameraId": 1,
  "latitude": 30.0444,
  "longitude": 31.2357
}
```

---

### `PUT /api/Camera/Update`

Update a camera's GPS coordinates. Called by the ESP device.

- **Auth required:** ❌ None (uses device Token header)

**Headers:**
```
Token: CAM-TOKEN-XXXXXXXX-XXXX
```

**Request Body:**
```json
{
  "latitude": 30.0600,
  "longitude": 31.2500
}
```

**Response `200 OK`:** Updated camera object.

---

### `DELETE /api/Camera/Delete`

Delete a camera by its device token.

- **Auth required:** ❌ None (uses device Token header)

**Headers:**
```
Token: CAM-TOKEN-XXXXXXXX-XXXX
```

**Response `200 OK`:**
```
"Camera deleted successfully"
```

---

## 6. Camera Requests

> Route prefix: `/api/CameraRequest`

*Identical workflow to Bin Requests but for cameras.*

---

### `POST /api/CameraRequest`

Submit a request for a new camera.

- **Auth required:** ✅ Any logged-in user
- **Content-Type:** `application/json`

**Request Body:**
```json
{
  "requesterName": "Sara Ali",
  "latitude": 30.0600,
  "longitude": 31.2600
}
```

**Response `200 OK`:**
```json
{
  "id": 3,
  "requesterName": "Sara Ali",
  "latitude": 30.0600,
  "longitude": 31.2600,
  "status": "Pending",
  "createdAt": "2025-06-20T15:00:00Z",
  "userEmail": "sara@example.com",
  "userFullName": "Sara Ali",
  "userId": 7,
  "cameraId": null,
  "identificationToken": null
}
```

---

### `GET /api/CameraRequest`

Get all camera requests. *(Admin panel)*

- **Auth required:** ✅ Admin only

---

### `GET /api/CameraRequest/my`

Get the logged-in user's camera requests.

- **Auth required:** ✅ Any logged-in user

---

### `POST /api/CameraRequest/{id}/Accept`

Admin accepts a camera request → creates and assigns a camera token.

- **Auth required:** ✅ Admin only

**URL Params:**
| Param | Type | Description |
|---|---|---|
| `id` | int | The camera request ID |

**Response `200 OK`:** Updated `CameraRequestResultMV` with `status: "Accepted"` and `identificationToken`.

---

## 7. Transactions (AI Classification)

> Route prefix: `/api/Transaction`  
> *Transactions record each material item classified by the bin's AI camera.*

---

### `GET /api/Transaction/SmartBin/{BinId}/page/{PageNumber}`

Get paginated classification history for a bin.

- **Auth required:** ✅ Any logged-in user

**URL Params:**
| Param | Type | Description |
|---|---|---|
| `BinId` | int | The Bin ID |
| `PageNumber` | int | Page number (starts at 1) |

**Query Params:**
| Param | Type | Default | Description |
|---|---|---|---|
| `PageSize` | int | `10` | Number of items per page |

**Example:**
```
GET /api/Transaction/SmartBin/1/page/1?PageSize=20
Authorization: Bearer <token>
```

**Response `200 OK`:**
```json
{
  "items": [
    {
      "transactionId": 42,
      "timestamp": "2025-06-20T12:30:00Z",
      "aiConfidencePercentage": 95.4,
      "imageUrl": "/images/txn_42.jpg",
      "binId": 1,
      "materialName": "Plastic"
    }
  ],
  "totalPages": 5,
  "page": 1,
  "pageSize": 10
}
```

---

### `POST /api/Transaction`

Add a new material classification event. **Called by the bin's ESP device.**

- **Auth required:** ❌ None (uses device Token header)
- **Content-Type:** `multipart/form-data`

**Headers:**
```
Token: BIN-TOKEN-XXXXXXXX-XXXX
```

**Form Fields:**
| Field | Type | Required | Notes |
|---|---|---|---|
| `MaterialName` | string | ⚠️ | Required if no Image |
| `Image` | file | ⚠️ | Required if no MaterialName |

> At least one of `MaterialName` or `Image` must be provided. If an image is sent, the AI model classifies the material automatically.

**Response `200 OK`:**
```json
{
  "transactionId": 43,
  "timestamp": "2025-06-20T13:00:00Z",
  "aiConfidencePercentage": 91.2,
  "imageUrl": "/images/txn_43.jpg",
  "binId": 1,
  "materialName": "Metal"
}
```

**cURL Example:**
```bash
curl -X POST https://smartbinpsu.runasp.net/api/Transaction \
  -H "Token: BIN-TOKEN-XXXXXXXX-XXXX" \
  -F "Image=@/path/to/photo.jpg"
```

---

## 8. Crowd Density

> Route prefix: `/api/CrowdDensity`  
> *Records how many people are detected near a bin or camera.*

---

### `GET /api/CrowdDensity`

Get the **latest** crowd density reading for a bin or camera.

- **Auth required:** ✅ Any logged-in user

**Query Params (one required):**
| Param | Type | Description |
|---|---|---|
| `BinId` | int | Get latest reading for this Bin |
| `CameraId` | int | Get latest reading for this Camera |

**Example:**
```
GET /api/CrowdDensity?BinId=1
GET /api/CrowdDensity?CameraId=2
```

**Response `200 OK`:**
```json
{
  "densityLevel": "Medium",
  "peopleCount": 14,
  "aiConfidencePercentage": 88.5,
  "timestamp": "2025-06-20T13:45:00Z"
}
```

| Field | Values |
|---|---|
| `densityLevel` | `"Low"`, `"Medium"`, `"High"` |
| `peopleCount` | Integer |
| `aiConfidencePercentage` | Float 0–100 |

**Response `404 Not Found`** if device doesn't exist or has no data yet.

---

### `GET /api/CrowdDensity/history`

Get **historical** crowd density readings for a bin or camera (newest first).

- **Auth required:** ✅ Any logged-in user

**Query Params:**
| Param | Type | Description |
|---|---|---|
| `BinId` | int | Get history for this Bin |
| `CameraId` | int | Get history for this Camera |
| `take` | int | Max records to return (default: `50`) |

**Example:**
```
GET /api/CrowdDensity/history?BinId=1&take=30
GET /api/CrowdDensity/history?CameraId=2&take=20
```

**Response `200 OK`:**
```json
[
  {
    "densityLevel": "High",
    "peopleCount": 22,
    "aiConfidencePercentage": 92.1,
    "timestamp": "2025-06-20T14:00:00Z"
  },
  {
    "densityLevel": "Medium",
    "peopleCount": 11,
    "aiConfidencePercentage": 87.3,
    "timestamp": "2025-06-20T13:00:00Z"
  }
]
```

---

### `POST /api/CrowdDensity`

Add a crowd density reading. **Called by the ESP device.**

- **Auth required:** ❌ None (uses device Token header)
- **Content-Type:** `multipart/form-data`

**Headers:**
```
Token: BIN-TOKEN-XXXXXXXX-XXXX
  — OR —
Token: CAM-TOKEN-XXXXXXXX-XXXX
```

**Form Fields:**
| Field | Type | Required | Notes |
|---|---|---|---|
| `Image` | file | ✅ | Image used for AI crowd counting |

**Response `200 OK`:**
```json
{ "message": "Crowd density added for Bin" }
```
or
```json
{ "message": "Crowd density added for Camera" }
```

**cURL Example:**
```bash
curl -X POST https://smartbinpsu.runasp.net/api/CrowdDensity \
  -H "Token: CAM-TOKEN-XXXXXXXX-XXXX" \
  -F "Image=@/path/to/crowd_photo.jpg"
```

---

## 9. Surrounding Waste

> Route prefix: `/api/SurroundingWaste`  
> *Records the level of litter/waste detected in the surrounding area.*

---

### `GET /api/SurroundingWaste`

Get the **latest** surrounding waste reading for a bin or camera.

- **Auth required:** ✅ Any logged-in user

**Query Params (one required):**
| Param | Type | Description |
|---|---|---|
| `BinId` | int | Get latest for this Bin |
| `CameraId` | int | Get latest for this Camera |

**Response `200 OK`:**
```json
{
  "id": 7,
  "wasteLevel": "Low",
  "imageUrl": "/images/waste_7.jpg",
  "detectedObjectsCount": 3,
  "aiConfidencePercentage": 90.0,
  "timestamp": "2025-06-20T14:10:00Z"
}
```

| Field | Values |
|---|---|
| `wasteLevel` | `"Low"`, `"Medium"`, `"High"` |
| `detectedObjectsCount` | Integer — number of waste objects detected |
| `imageUrl` | Relative path or full URL to captured image |

**Response `404 Not Found`** if device doesn't exist or has no data yet.

---

### `GET /api/SurroundingWaste/history`

Get **historical** surrounding waste readings (newest first).

- **Auth required:** ✅ Any logged-in user

**Query Params:**
| Param | Type | Description |
|---|---|---|
| `BinId` | int | Get history for this Bin |
| `CameraId` | int | Get history for this Camera |
| `take` | int | Max records to return (default: `50`) |

**Example:**
```
GET /api/SurroundingWaste/history?CameraId=1&take=30
```

**Response `200 OK`:**
```json
[
  {
    "id": 10,
    "wasteLevel": "High",
    "imageUrl": "/images/waste_10.jpg",
    "detectedObjectsCount": 12,
    "aiConfidencePercentage": 94.5,
    "timestamp": "2025-06-20T14:30:00Z"
  },
  {
    "id": 9,
    "wasteLevel": "Medium",
    "imageUrl": "/images/waste_9.jpg",
    "detectedObjectsCount": 5,
    "aiConfidencePercentage": 88.0,
    "timestamp": "2025-06-20T13:30:00Z"
  }
]
```

---

### `POST /api/SurroundingWaste`

Add a surrounding waste reading. **Called by the ESP device.**

- **Auth required:** ❌ None (uses device Token header)
- **Content-Type:** `multipart/form-data`

**Headers:**
```
Token: BIN-TOKEN-XXXXXXXX-XXXX
  — OR —
Token: CAM-TOKEN-XXXXXXXX-XXXX
```

**Form Fields:**
| Field | Type | Required | Notes |
|---|---|---|---|
| `Image` | file | ✅ | Image used for AI waste detection |

**Response `200 OK`:**
```json
{ "message": "Surrounding waste added for Bin" }
```

**cURL Example:**
```bash
curl -X POST https://smartbinpsu.runasp.net/api/SurroundingWaste \
  -H "Token: BIN-TOKEN-XXXXXXXX-XXXX" \
  -F "Image=@/path/to/area_photo.jpg"
```

---

## 🔑 Authentication Flow Summary

```
1. POST /api/Auth/register  →  get JWT token
2. POST /api/Auth/login     →  get JWT token (if already registered)
3. Use token in header: Authorization: Bearer <token>
```

## 🤖 ESP Device Flow Summary

```
1. Admin creates device:   POST /api/SmartBin/Create  →  gets IdentificationToken
2. Admin approves request: POST /api/BinRequest/{id}/Accept  →  token assigned to user
3. ESP sends data using:   Token: <IdentificationToken>  header (no JWT)

Device endpoints:
  POST /api/Transaction           (image → AI classifies material)
  POST /api/CrowdDensity          (image → AI counts people)
  POST /api/SurroundingWaste      (image → AI detects waste)
  PUT  /api/SmartBin/Update       (update GPS location)
  PUT  /api/BinSection/update     (update fill levels)
```

## 🛑 Common Error Responses

| Status | When |
|---|---|
| `400 Bad Request` | Missing required fields, invalid data |
| `401 Unauthorized` | Missing or invalid JWT token |
| `403 Forbidden` | Logged in but not Admin |
| `404 Not Found` | Resource doesn't exist or doesn't belong to user |