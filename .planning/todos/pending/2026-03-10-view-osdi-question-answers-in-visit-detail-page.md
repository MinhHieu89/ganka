---
created: 2026-03-10T08:32:00.000Z
title: View OSDI question answers in visit detail page
area: ui
files:
  - frontend/src/**/dry-eye/**
  - frontend/src/**/osdi/**
---

## Problem

In the visit detail page, the doctor can only see the overall OSDI score and severity badge. There is no way to view the individual answers to all 12 OSDI questions. Doctors need to see which specific areas (vision, eye symptoms, environmental triggers) are problematic for the patient to make informed treatment decisions.

## Solution

Add an expandable section or modal in the Dry Eye section that displays all 12 OSDI question responses with their individual scores when clicked. Could be a "View Details" button next to the OSDI score that shows a breakdown of each question and the patient's answer.
