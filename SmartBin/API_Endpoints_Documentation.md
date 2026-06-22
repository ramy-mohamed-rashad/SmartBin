# Smart Bin System — Full API Reference

> **Base URL (Production):** `https://smartbinpsu.runasp.net`
> **Base URL (Local Dev):** `http://localhost:5025`

---

## 🔐 Authentication

All **dashboard / protected** endpoints require a **JWT Bearer token** in the `Authorization` header:

```
Authorization: Bearer <your_jwt_token>
```

- **Admin** endpoints require the user to have the `Admin` role.
- **Device (POST report)** endpoints use **custom headers** to identify the device — **no JWT is required**.
  - Surrounding Waste report → `X-Camera-Token` header
  - Crowd Density report     → `X-Camera-Token` + `X-Bin-Token` headers
- **ESP hardware** endpoints (GPS, fill-level) use a plain `Token` header — no JWT.

---

## 📋 Table of Contents

1. [Auth](#1-auth)
2. [Bin (SmartBin)](#2-bin-smartbin)
3. [Bin Sections](#3-bin-sections)
4. [Bin Requests](#4-bin-requests)
5. [Camera](#5-camera)
6. [Camera Requests](#6-camera-requests)
7. [Transactions (AI Classification)](#7-transactions-ai-classification)
8. [Surrounding Waste](#8-surrounding-waste)
9. [Crowd Density](#9-crowd-density)

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
  "userId": "12"
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
  "userId": "12"
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

Assign a bin to the authenticated user using its token.

- **Auth required:** ✅ Any logged-in user
- **Content-Type:** `application/json`

**Request Body:**
```json
{ "token": "BIN-TOKEN-XXXXXXXX-XXXX" }
```

**Response `200 OK`:**
```
"BIN-TOKEN-XXXXXXXX-XXXX"
```

---

### `GET /api/SmartBin`

Get all bins owned by the logged-in user.

- **Auth required:** ✅ Any logged-in user

**Response `200 OK`:**
```json
[
  { "binId": 1, "latitude": 30.0444, "longitude": 31.2357 },
  { "binId": 2, "latitude": 30.1234, "longitude": 31.4567 }
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

- **Auth required:** ❌ None (uses device `Token` header)
- **Content-Type:** `application/json`

**Headers:**
```
Token: BIN-TOKEN-XXXXXXXX-XXXX
```

**Request Body:**
```json
{ "latitude": 30.0500, "longitude": 31.2400 }
```

**Response `200 OK`:**
```json
{ "binId": 1, "latitude": 30.0500, "longitude": 31.2400 }
```

---

### `DELETE /api/SmartBin/Delete`

Delete a bin by its device token.

- **Auth required:** ❌ None (uses device `Token` header)

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

**Response `200 OK`:**
```json
[
  { "materialName": "Plastic", "levelPercentage": 45.5, "weight": 1.2 },
  { "materialName": "Metal",   "levelPercentage": 20.0, "weight": 0.5 }
]
```

---

### `PUT /api/BinSection/update`

Update a bin section's fill level and weight. Called by the ESP device.

- **Auth required:** ❌ None (uses device `Token` header)
- **Content-Type:** `application/json`

**Headers:**
```
Token: BIN-TOKEN-XXXXXXXX-XXXX
```

**Request Body:**
```json
{ "materialName": "Plastic", "levelPercentage": 60.0, "weight": 1.8 }
```

**Response `200 OK`:**
```json
{ "materialName": "Plastic", "levelPercentage": 60.0, "weight": 1.8 }
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

**Response `200 OK`:**
```json
{
  "id": 5,
  "requesterName": "Ahmed Mohamed",
  "latitude": 30.0444,
  "longitude": 31.2357,
  "status": "Pending",
  "createdAt": "2026-06-22T10:00:00Z",
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

**Response `200 OK`:** Array of `BinRequestResultMV`.

---

### `GET /api/BinRequest/my`

Get all bin requests submitted by the logged-in user.

- **Auth required:** ✅ Any logged-in user

**Response `200 OK`:**
```json
[
  {
    "id": 5,
    "requesterName": "Ahmed Mohamed",
    "status": "Accepted",
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
{ "token": "CAM-TOKEN-XXXXXXXX-XXXX" }
```

**Response `200 OK`:**
```
"CAM-TOKEN-XXXXXXXX-XXXX"
```

---

### `GET /api/Camera`

Get all cameras owned by the logged-in user.

- **Auth required:** ✅ Any logged-in user

**Response `200 OK`:**
```json
[
  { "cameraId": 1, "latitude": 30.0444, "longitude": 31.2357 },
  { "cameraId": 2, "latitude": null,    "longitude": null }
]
```

---

### `GET /api/Camera/{id}`

Get details of a single camera.

- **Auth required:** ✅ Any logged-in user

**Response `200 OK`:**
```json
{ "cameraId": 1, "latitude": 30.0444, "longitude": 31.2357 }
```

---

### `PUT /api/Camera/Update`

Update a camera's GPS coordinates. Called by the camera device.

- **Auth required:** ❌ None (uses device `Token` header)

**Headers:**
```
Token: CAM-TOKEN-XXXXXXXX-XXXX
```

**Request Body:**
```json
{ "latitude": 30.0600, "longitude": 31.2500 }
```

**Response `200 OK`:** Updated camera object.

---

### `DELETE /api/Camera/Delete`

Delete a camera by its device token.

- **Auth required:** ❌ None (uses device `Token` header)

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
  "status": "Pending",
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
      "timestamp": "2026-06-22T12:30:00Z",
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

- **Auth required:** ❌ None (uses device `Token` header)
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

> At least one of `MaterialName` or `Image` must be provided.

**Response `200 OK`:**
```json
{
  "transactionId": 43,
  "timestamp": "2026-06-22T13:00:00Z",
  "aiConfidencePercentage": 91.2,
  "imageUrl": "/images/txn_43.jpg",
  "binId": 1,
  "materialName": "Metal"
}
```

---

## 8. Surrounding Waste

> Route prefix: `/api/SurroundingWaste`
> *Records the level of litter/waste detected in the surrounding area of a camera.*

### Workflow Overview

```
External AI service analyses image
        │
        ▼
POST /api/SurroundingWaste/report
  Header: X-Camera-Token
  Body:   AI results (JSON) + base64 image + timestamp
        │
        ▼ (server saves image to disk, persists record)
Dashboard
        │
        ▼
GET /api/SurroundingWaste?CameraId=N
GET /api/SurroundingWaste/history?CameraId=N
```

---

### `POST /api/SurroundingWaste/report`

Accepts AI-analysed surrounding waste data from the **external first-hop AI service**.

- **Auth required:** ❌ None (identified by `X-Camera-Token` header)
- **Content-Type:** `application/json`

**Headers:**
| Header | Required | Description |
|---|---|---|
| `X-Camera-Token` | ✅ | Camera's `IdentificationToken` |

**Request Body:**
```json
{
  "wasteLevel": "High",
  "detectedObjectsCount": 7,
  "aiConfidencePercentage": 91.5,
  "imageBase64": "<base64-encoded image string>",
  "timestamp": "2026-06-22T10:00:00Z"
}
```

| Field | Type | Required | Values / Notes |
|---|---|---|---|
| `wasteLevel` | string | ✅ | `"Low"`, `"Medium"`, `"High"` |
| `detectedObjectsCount` | int | ✅ | Number of waste objects detected |
| `aiConfidencePercentage` | float | ✅ | 0–100 |
| `imageBase64` | string | ✅ | Raw base64 OR `data:image/jpeg;base64,...` prefix accepted |
| `timestamp` | string (ISO 8601) | ✅ | UTC datetime from the AI service |

**Response `200 OK`:**
```json
{ "message": "Surrounding waste recorded successfully." }
```

**Response `400 Bad Request`** — missing `X-Camera-Token` header or invalid body fields.

**Response `404 Not Found`:**
```json
{ "message": "Camera not found." }
```

> Images are decoded and saved to `wwwroot/uploads/waste/`. The saved path is stored as `imageUrl`.

---

### `GET /api/SurroundingWaste`

Get the **latest** surrounding waste reading for a camera or bin. *(Dashboard)*

- **Auth required:** ✅ Any logged-in user

**Query Params (provide one):**
| Param | Type | Description |
|---|---|---|
| `CameraId` | int | Get latest reading for this Camera |
| `BinId` | int | Get latest reading for this Bin |

**Example:**
```
GET /api/SurroundingWaste?CameraId=2
Authorization: Bearer <token>
```

**Response `200 OK`:**
```json
{
  "id": 1,
  "wasteLevel": "High",
  "imageUrl": "/uploads/waste/83b5396b-14f9-47ff-a51a-650d6717ad70.jpg",
  "detectedObjectsCount": 7,
  "aiConfidencePercentage": 91.5,
  "timestamp": "2026-06-22T10:00:00",
  "cameraId": 2
}
```

| Field | Values / Notes |
|---|---|
| `wasteLevel` | `"Low"`, `"Medium"`, `"High"` |
| `detectedObjectsCount` | Integer |
| `aiConfidencePercentage` | Float 0–100 |
| `imageUrl` | Relative path — serve via `<baseUrl><imageUrl>` |
| `cameraId` | ID of the camera that captured the image |

**Response `404 Not Found`** if device doesn't exist or has no data yet.

---

### `GET /api/SurroundingWaste/history`

Get **historical** surrounding waste readings (newest first). *(Dashboard)*

- **Auth required:** ✅ Any logged-in user

**Query Params:**
| Param | Type | Description |
|---|---|---|
| `CameraId` | int | Get history for this Camera |
| `BinId` | int | Get history for this Bin |
| `take` | int | Max records to return (default: `50`) |

**Example:**
```
GET /api/SurroundingWaste/history?CameraId=2&take=20
Authorization: Bearer <token>
```

**Response `200 OK`:** Array of readings, newest first.
```json
[
  {
    "id": 5,
    "wasteLevel": "High",
    "imageUrl": "/uploads/waste/abc123.jpg",
    "detectedObjectsCount": 10,
    "aiConfidencePercentage": 94.5,
    "timestamp": "2026-06-22T12:00:00",
    "cameraId": 2
  },
  {
    "id": 4,
    "wasteLevel": "Medium",
    "imageUrl": "/uploads/waste/def456.jpg",
    "detectedObjectsCount": 4,
    "aiConfidencePercentage": 88.0,
    "timestamp": "2026-06-22T11:00:00",
    "cameraId": 2
  }
]
```

---

## 9. Crowd Density

> Route prefix: `/api/CrowdDensity`
> *Records how many people are detected near a camera and links the reading to the closest bin.*

### Workflow Overview

```
External AI service analyses image
        │
        ▼
POST /api/CrowdDensity/report
  Header: X-Camera-Token   (identifies camera)
  Header: X-Bin-Token      (identifies nearby bin)
  Body:   AI results (JSON) + base64 image + timestamp
        │
        ▼ (server links crowd record to both camera AND bin)
Dashboard
        │
        ▼
GET /api/CrowdDensity?CameraId=N   (or BinId=N)
GET /api/CrowdDensity/history?CameraId=N
```

---

### `POST /api/CrowdDensity/report`

Accepts AI-analysed crowd density data from the **external first-hop AI service**.

- **Auth required:** ❌ None (identified by `X-Camera-Token` + `X-Bin-Token` headers)
- **Content-Type:** `application/json`

**Headers:**
| Header | Required | Description |
|---|---|---|
| `X-Camera-Token` | ✅ | Camera's `IdentificationToken` |
| `X-Bin-Token` | ✅ | Bin's `IdentificationToken` (links crowd reading to a nearby bin) |

> ⚠️ Both headers are **required**. Using the same header name for both is not allowed — they must be `X-Camera-Token` and `X-Bin-Token` respectively.

**Request Body:**
```json
{
  "densityLevel": "Medium",
  "peopleCount": 12,
  "aiConfidencePercentage": 87.3,
  "imageBase64": "<base64-encoded image string>",
  "timestamp": "2026-06-22T10:00:00Z"
}
```

| Field | Type | Required | Values / Notes |
|---|---|---|---|
| `densityLevel` | string | ✅ | `"Low"`, `"Medium"`, `"High"` |
| `peopleCount` | int | ✅ | Number of people detected |
| `aiConfidencePercentage` | float | ✅ | 0–100 |
| `imageBase64` | string | ✅ | Raw base64 OR `data:image/jpeg;base64,...` prefix accepted |
| `timestamp` | string (ISO 8601) | ✅ | UTC datetime from the AI service |

**Response `200 OK`:**
```json
{ "message": "Crowd density recorded successfully." }
```

**Response `400 Bad Request`** — missing headers or invalid body fields.

**Response `404 Not Found`:**
```json
{ "message": "Camera not found." }
```
or
```json
{ "message": "Bin not found." }
```

> Images are decoded and saved to `wwwroot/uploads/crowd/`. The saved path is stored as `imageUrl`.

---

### `GET /api/CrowdDensity`

Get the **latest** crowd density reading for a camera or bin. *(Dashboard)*

- **Auth required:** ✅ Any logged-in user

**Query Params (provide one):**
| Param | Type | Description |
|---|---|---|
| `CameraId` | int | Get latest reading for this Camera |
| `BinId` | int | Get latest reading for this Bin |

**Example:**
```
GET /api/CrowdDensity?CameraId=2
Authorization: Bearer <token>
```

**Response `200 OK`:**
```json
{
  "crowdId": 1,
  "densityLevel": "Medium",
  "imageUrl": "/uploads/crowd/c6181d54-3eec-427c-ad5c-08902448311e.jpg",
  "peopleCount": 12,
  "aiConfidencePercentage": 87.3,
  "timestamp": "2026-06-22T10:00:00",
  "cameraId": 2,
  "binId": 18
}
```

| Field | Values / Notes |
|---|---|
| `densityLevel` | `"Low"`, `"Medium"`, `"High"` |
| `peopleCount` | Integer |
| `aiConfidencePercentage` | Float 0–100 |
| `imageUrl` | Relative path — serve via `<baseUrl><imageUrl>` |
| `cameraId` | ID of the camera that captured the image |
| `binId` | ID of the nearby bin this crowd reading is linked to |

**Response `404 Not Found`** if device doesn't exist or has no data yet.

---

### `GET /api/CrowdDensity/history`

Get **historical** crowd density readings (newest first). *(Dashboard)*

- **Auth required:** ✅ Any logged-in user

**Query Params:**
| Param | Type | Description |
|---|---|---|
| `CameraId` | int | Get history for this Camera |
| `BinId` | int | Get history for this Bin |
| `take` | int | Max records to return (default: `50`) |

**Example:**
```
GET /api/CrowdDensity/history?CameraId=2&take=20
Authorization: Bearer <token>
```

**Response `200 OK`:** Array of readings, newest first.
```json
[
  {
    "crowdId": 3,
    "densityLevel": "High",
    "imageUrl": "/uploads/crowd/xyz789.jpg",
    "peopleCount": 25,
    "aiConfidencePercentage": 92.1,
    "timestamp": "2026-06-22T12:00:00",
    "cameraId": 2,
    "binId": 18
  },
  {
    "crowdId": 1,
    "densityLevel": "Medium",
    "imageUrl": "/uploads/crowd/c6181d54.jpg",
    "peopleCount": 12,
    "aiConfidencePercentage": 87.3,
    "timestamp": "2026-06-22T10:00:00",
    "cameraId": 2,
    "binId": 18
  }
]
```

---

## 🔑 Authentication Flow Summary

```
1. POST /api/Auth/register  →  get JWT token
2. POST /api/Auth/login     →  get JWT token (if already registered)
3. Use token in header:  Authorization: Bearer <token>
```

## 🤖 AI Service Ingest Flow (New Workflow)

```
1. External AI service captures image and runs inference
2. AI service calls one of:

   POST /api/SurroundingWaste/report
     X-Camera-Token: <camera IdentificationToken>
     Body: { wasteLevel, detectedObjectsCount, aiConfidencePercentage, imageBase64, timestamp }

   POST /api/CrowdDensity/report
     X-Camera-Token: <camera IdentificationToken>
     X-Bin-Token:    <bin IdentificationToken>
     Body: { densityLevel, peopleCount, aiConfidencePercentage, imageBase64, timestamp }

3. Our API resolves the device(s), saves the image, and persists the record
4. Dashboard fetches data via the GET endpoints using JWT
```

## 🔩 ESP Device Flow Summary

```
1. Admin creates device:   POST /api/SmartBin/Create  →  gets IdentificationToken
2. Admin approves request: POST /api/BinRequest/{id}/Accept  →  token assigned to user
3. ESP sends data using:   Token: <IdentificationToken>  header (no JWT)

ESP device endpoints:
  POST /api/Transaction          (image → AI classifies material)
  PUT  /api/SmartBin/Update      (update GPS location)
  PUT  /api/BinSection/update    (update fill levels)
  PUT  /api/Camera/Update        (update camera GPS)
```

## 🛑 Common Error Responses

| Status | When |
|---|---|
| `400 Bad Request` | Missing required fields, invalid data, missing required headers |
| `401 Unauthorized` | Missing or invalid JWT token |
| `403 Forbidden` | Logged in but not Admin |
| `404 Not Found` | Resource doesn't exist, device token not found, or resource doesn't belong to user |
| `500 Internal Server Error` | Unexpected server error |
