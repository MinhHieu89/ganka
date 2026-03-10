---
created: 2026-03-10T09:35:00.000Z
title: Implement clinic logo upload endpoint
area: api
files:
  - backend/src/Shared/Shared.Presentation/SettingsApiEndpoints.cs
  - backend/src/Shared/Shared.Infrastructure/Entities/ClinicSettings.cs
  - frontend/src/features/admin/api/clinic-settings-api.ts
---

## Problem

Frontend calls `POST /api/settings/clinic/logo` to upload clinic logo, but this endpoint doesn't exist in the backend. Only `GET /clinic` and `PUT /clinic` are implemented in SettingsApiEndpoints.cs. The ClinicSettings entity has `LogoBlobUrl` field and an update method, but no file upload infrastructure (blob storage or local file storage) is wired up.

## Solution

1. Add `POST /api/settings/clinic/logo` endpoint to SettingsApiEndpoints.cs
2. Implement file upload handling (local storage to wwwroot/uploads or a blob storage service)
3. Save uploaded file, update ClinicSettings.LogoBlobUrl with the URL
4. Return the logo URL in the response
