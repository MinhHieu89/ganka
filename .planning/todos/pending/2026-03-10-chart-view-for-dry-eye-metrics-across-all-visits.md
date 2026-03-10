---
created: 2026-03-10T08:40:00.000Z
title: Chart view for dry eye metrics across all visits
area: ui
files:
  - frontend/src/**/dry-eye/**
  - frontend/src/**/patient/**
---

## Problem

On the patient detail page, the dry eye cross-visit comparison only supports comparing 2 visits side-by-side. There is no way to visualize dry eye metric trends (TBUT, Schirmer, Meibomian, Tear Meniscus, Staining) across all visits over a period of time. Doctors need to see the progression of these metrics over time to evaluate treatment effectiveness.

## Solution

Add a chart view (line chart or multi-series chart) on the patient detail page Dry Eye tab that plots all dry eye metrics across all visits within a selectable time period. Similar to the existing OSDI trend chart but for individual dry eye metrics (TBUT, Schirmer, etc.) with OD/OS series differentiation.
