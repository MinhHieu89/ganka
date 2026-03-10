---
created: 2026-03-10T08:56:44.511Z
title: Server side pagination for pharmacy page
area: ui
files:
  - frontend/src/app/pharmacy/*
---

## Problem

The /pharmacy drug catalog page currently loads all drugs client-side. As the catalog grows, this will cause performance issues. The DataTable on /pharmacy should use server-side pagination to handle large datasets efficiently.

## Solution

Implement server-side pagination for the drug catalog DataTable on /pharmacy page, similar to how other paginated tables work in the app. Backend endpoint should accept page/pageSize parameters and return paginated results with total count.
