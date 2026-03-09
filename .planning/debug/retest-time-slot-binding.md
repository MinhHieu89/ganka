---
status: diagnosed
trigger: "When clicking a time slot on the FullCalendar in /appointments, the date binds correctly in the booking dialog but the time field remains empty"
created: 2026-03-09T00:00:00+07:00
updated: 2026-03-09T00:00:00+07:00
---

## Current Focus

hypothesis: CONFIRMED - getUTCHours() returns actual UTC hour (not wall-clock) because momentTimezonePlugin converts the Date from UTC-coerced marker to real UTC Date
test: Traced FullCalendar core source code + ran numerical simulation
expecting: getUTCHours() produces value outside 08-19 range, causing no match in Select
next_action: Apply fix - switch from getUTCHours/getUTCMinutes to getHours/getMinutes (or use moment-timezone)

## Symptoms

expected: Clicking a time slot (e.g., 09:00) on the calendar should pre-populate the time dropdown in the booking dialog with "09:00"
actual: The time dropdown remains empty (shows placeholder "selectTime") while the date field binds correctly
errors: No runtime errors; the Select component silently shows nothing when its value doesn't match any option
reproduction: Click any time slot on the FullCalendar in /appointments page -> observe booking dialog time field is empty
started: After previous "fix" changed getHours() to getUTCHours() -- the fix was correct for UTC-coercion mode but wrong with momentTimezonePlugin loaded

## Eliminated

- hypothesis: "FullCalendar still uses UTC-coercion even with momentTimezonePlugin loaded"
  evidence: >
    Read FullCalendar core source at internal-common.js line 2184-2185.
    The toDate() method for named timezones WITH the plugin does:
    `new Date(m.valueOf() - this.namedTimeZoneImpl.offsetForArray(dateToUtcArray(m)) * 1000 * 60)`
    This subtracts the timezone offset from the UTC-coerced marker, producing a real UTC Date.
    Verified numerically: marker UTC 09:00 minus 420 min offset = UTC 02:00.
  timestamp: 2026-03-09

## Evidence

- timestamp: 2026-03-09
  checked: FullCalendar calendar config in AppointmentCalendar.tsx
  found: >
    timeZone="Asia/Ho_Chi_Minh" with momentTimezonePlugin loaded.
    Both @fullcalendar/moment-timezone and moment-timezone packages are installed (v6.1.20 / v0.5.48).
  implication: FullCalendar uses real timezone conversion, not UTC-coercion.

- timestamp: 2026-03-09
  checked: Data flow from slot click to dialog
  found: >
    AppointmentsPage.handleSlotClick receives DateSelectArg, stores info.start in selectedSlotStart state.
    This Date is passed to AppointmentBookingDialog as defaultStartTime prop.
    Dialog extracts time via getUTCHours()/getUTCMinutes() at lines 110 and 129.
  implication: The extraction method determines what value the Select receives.

- timestamp: 2026-03-09
  checked: FullCalendar core source - toDate() method (internal-common.js:2174-2186)
  found: >
    When namedTimeZoneImpl exists (momentTimezonePlugin is loaded), toDate() converts the
    internal UTC-coerced marker to a real UTC Date by subtracting the timezone offset:
    `new Date(m.valueOf() - offset * 60000)`
    For 09:00 HCM: marker has UTC hours=9, offset=420min, result has UTC hours=2.
  implication: DateSelectArg.start.getUTCHours() returns 2 (real UTC), not 9 (wall-clock).

- timestamp: 2026-03-09
  checked: FullCalendar core source - select callback (internal-common.js:4573-4574)
  found: >
    The select callback builds DateSelectArg with:
    `start: dateEnv.toDate(range.start)`
    Confirming the Date goes through toDate() conversion.
  implication: DateSelectArg.start IS the toDate()-converted value, not the raw marker.

- timestamp: 2026-03-09
  checked: Numerical simulation of the complete flow
  found: >
    For 09:00 HCM slot click:
    1. Internal marker: Date.UTC(2026,2,10,9,0,0) -> getUTCHours()=9
    2. toDate() subtracts 420 min -> Date.UTC(2026,2,10,2,0,0) -> getUTCHours()=2
    3. Dialog formats: "02:00"
    4. generateTimeSlots() produces ["08:00","08:30",...,"19:30"]
    5. "02:00" is NOT in the list -> Select shows no match -> empty/placeholder
  implication: This is the exact root cause. The time value computed by the dialog does not match any Select option.

- timestamp: 2026-03-09
  checked: generateTimeSlots() function in AppointmentBookingDialog.tsx
  found: >
    Generates slots from 08:00 to 19:30 in 30-minute increments.
    getUTCHours() will produce values 1-12 for HCM wall-clock hours 8-19.
    None of these UTC values (01:00 through 12:00) fall within the 08:00-19:30 range,
    EXCEPT if the wall-clock time is 15:00-19:30 HCM (which maps to UTC 08:00-12:30).
    But those would match the wrong slot (e.g., 15:00 HCM -> UTC 08:00 -> shows "08:00").
  implication: The bug affects ALL morning/early-afternoon slots. Late afternoon slots would bind to the WRONG time.

## Resolution

root_cause: >
  The previous fix changed getHours() to getUTCHours()/getUTCMinutes() to handle FullCalendar's
  UTC-coercion behavior. However, the calendar also has momentTimezonePlugin loaded, which causes
  FullCalendar's toDate() method to convert the UTC-coerced internal marker into a REAL UTC Date
  (by subtracting the timezone offset). As a result:

  - DateSelectArg.start is a real UTC Date (e.g., 02:00 UTC for 09:00 HCM)
  - getUTCHours() returns the actual UTC hour (2), not the wall-clock hour (9)
  - The formatted time string "02:00" does not match any option in the Select dropdown
    (which contains "08:00" through "19:30")
  - The Select component shows nothing (empty/placeholder)

  The fix should EITHER:
  (a) Use getHours()/getMinutes() which works because the system timezone matches the calendar timezone (Asia/Ho_Chi_Minh)
  (b) Use moment-timezone to convert: moment.tz(date.valueOf(), 'Asia/Ho_Chi_Minh').hours()/.minutes()
  (c) Remove the momentTimezonePlugin and rely on UTC-coercion (then getUTCHours would be correct)

  Option (a) is simplest and correct for this application (Vietnam clinic, always running in Vietnam timezone).
  Option (c) risks display issues in the calendar itself.

fix: ""
verification: ""
files_changed:
  - frontend/src/features/scheduling/components/AppointmentBookingDialog.tsx (lines 110, 129)
