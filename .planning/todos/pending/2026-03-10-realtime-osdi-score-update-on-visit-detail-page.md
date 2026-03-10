---
created: 2026-03-10T08:29:10.431Z
title: Realtime OSDI score update on visit detail page
area: ui
files:
  - frontend/src/**/osdi/**
  - frontend/src/**/dry-eye/**
---

## Problem

After a patient submits the OSDI questionnaire on the public page (`/osdi/{token}`), the OSDI score on the visit detail page does not update in realtime. The doctor has to manually refresh the page to see the newly submitted score. This creates a poor workflow experience when the doctor generates the OSDI link and waits for the patient to complete it.

## Solution

Implement realtime updates via SignalR or polling so that when an OSDI submission is recorded on the backend, the visit detail page automatically reflects the new OSDI score and severity badge without a manual refresh.
