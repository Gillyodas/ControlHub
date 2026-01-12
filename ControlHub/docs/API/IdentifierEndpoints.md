# Identifier Configuration API Endpoints

## Overview
The Identifier Configuration API provides endpoints for managing identifier validation rules and configurations in the ControlHub system.

## Base URL
```
/api/Identifier
```

## Authentication
All endpoints require authentication with a valid JWT token.

## Endpoints

### 1. Get All Identifier Configurations
Retrieves all active identifier configurations with their validation rules.

**Endpoint:** `GET /api/Identifier`

**Response:**
```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "name": "Email",
    "description": "Email address validation",
    "isActive": true,
    "rules": [
      {
        "type": "Required",
        "parameters": {},
        "errorMessage": null,
        "order": 0
      },
      {
        "type": "Email",
        "parameters": {},
        "errorMessage": "Invalid email format",
        "order": 1
      }
    ]
  }
]
```

**Status Codes:**
- `200 OK` - Successfully retrieved configurations
- `401 Unauthorized` - Authentication required
- `500 Internal Server Error` - Server error occurred

---

### 2. Create Identifier Configuration
Creates a new identifier configuration with validation rules.

**Endpoint:** `POST /api/Identifier`

**Request Body:**
```json
{
  "name": "EmployeeID",
  "description": "Employee ID validation",
  "rules": [
    {
      "type": "Required",
      "parameters": {},
      "errorMessage": "Employee ID is required",
      "order": 0
    },
    {
      "type": "MinLength",
      "parameters": {
        "length": 5
      },
      "errorMessage": "Employee ID must be at least 5 characters",
      "order": 1
    },
    {
      "type": "MaxLength",
      "parameters": {
        "length": 10
      },
      "errorMessage": "Employee ID must not exceed 10 characters",
      "order": 2
    },
    {
      "type": "Pattern",
      "parameters": {
        "pattern": "^EMP\\d{4,9}$",
        "options": 0
      },
      "errorMessage": "Employee ID must start with EMP followed by 4-9 digits",
      "order": 3
    }
  ]
}
```

**Response:**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

**Status Codes:**
- `201 Created` - Configuration created successfully
- `400 Bad Request` - Invalid request data or validation rule parameters
- `401 Unauthorized` - Authentication required
- `409 Conflict` - Identifier name already exists
- `500 Internal Server Error` - Server error occurred

## Validation Rule Types

### Available Rule Types:
1. **Required** - Field must not be empty
2. **MinLength** - Minimum length validation
3. **MaxLength** - Maximum length validation
4. **Pattern** - Regex pattern validation
5. **Email** - Email format validation
6. **Phone** - Phone number validation
7. **Range** - Numeric range validation
8. **Custom** - Custom validation logic

### Rule Parameters:

#### Required
- No parameters required

#### MinLength
- `length` (number, required) - Minimum length

#### MaxLength
- `length` (number, required) - Maximum length

#### Pattern
- `pattern` (string, required) - Regex pattern
- `options` (number, optional) - RegexOptions enum value

#### Email
- No parameters required

#### Phone
- `pattern` (string, optional) - Custom phone regex pattern
- `allowInternational` (boolean, optional, default: true) - Allow international numbers

#### Range
- `min` (number, required) - Minimum value
- `max` (number, required) - Maximum value

#### Custom
- `customLogic` (string, required) - Type of custom validation:
  - `"uppercase"` - Only uppercase letters
  - `"lowercase"` - Only lowercase letters
  - `"alphanumeric"` - Letters and numbers only
  - `"numeric"` - Numbers only
  - `"letters"` - Letters only

## Example Usage

### Create Email Validation Config
```json
{
  "name": "Email",
  "description": "Email address validation",
  "rules": [
    {
      "type": "Required",
      "parameters": {},
      "order": 0
    },
    {
      "type": "Email",
      "parameters": {},
      "errorMessage": "Please enter a valid email address",
      "order": 1
    }
  ]
}
```

### Create Phone Validation Config
```json
{
  "name": "Phone",
  "description": "Phone number validation",
  "rules": [
    {
      "type": "Required",
      "parameters": {},
      "order": 0
    },
    {
      "type": "Phone",
      "parameters": {
        "pattern": "^(\\+?\\d{1,3}[-.\\s]?)?\\(?\\d{3}\\)?[-.\\s]?\\d{3}[-.\\s]?\\d{4}$",
        "allowInternational": true
      },
      "errorMessage": "Please enter a valid phone number",
      "order": 1
    }
  ]
}
```

### Create Age Validation Config
```json
{
  "name": "Age",
  "description": "Age validation",
  "rules": [
    {
      "type": "Required",
      "parameters": {},
      "order": 0
    },
    {
      "type": "Range",
      "parameters": {
        "min": 18,
        "max": 65
      },
      "errorMessage": "Age must be between 18 and 65",
      "order": 1
    }
  ]
}
```
