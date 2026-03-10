---
created: 2026-03-10T08:35:00.000Z
title: Patient name link to detail page from visit
area: ui
files:
  - frontend/src/**/visit/**
---

## Problem

On the visit detail page, the patient name is displayed but not clickable. Users cannot navigate directly to the patient's detail page from the visit detail page. This forces them to go back to the patient list and search for the patient manually.

## Solution

Make the patient name on the visit detail page a clickable link that opens the patient detail page in a new browser tab (`target="_blank"`).
