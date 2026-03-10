---
created: 2026-03-10T11:27:16.009Z
title: Textarea auto-expand vertically to fit content
area: ui
files: []
---

## Problem

Textarea fields across the application do not auto-expand vertically to fit their content. When users type long text, they have to scroll within a fixed-height textarea instead of seeing all content at once. Noticed during Phase 06 UAT on supplier form (address field).

## Solution

Apply auto-resize behavior to all `<textarea>` elements (or Shadcn Textarea components). Use a CSS/JS approach: set `overflow: hidden` and dynamically adjust `height` to `scrollHeight` on input. Consider creating a shared `AutoResizeTextarea` wrapper or applying globally via a hook.
