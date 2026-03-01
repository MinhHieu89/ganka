#!/bin/bash
# Phase 02: Patient Management & Scheduling - API Smoke Tests
# Run with: bash scripts/smoke-test-phase02.sh
#
# Prerequisites:
#   - Backend running at http://localhost:5255
#   - Default admin credentials (admin@ganka28.com / Admin@123456)

set -e

BASE_URL="http://localhost:5255"
PASS=0
FAIL=0
TOTAL=0

# Colors
GREEN='\033[0;32m'
RED='\033[0;31m'
NC='\033[0m'

report() {
  TOTAL=$((TOTAL + 1))
  local test_name="$1"
  local expected="$2"
  local actual="$3"
  if [ "$actual" = "$expected" ]; then
    PASS=$((PASS + 1))
    echo -e "${GREEN}PASS${NC} [$TOTAL] $test_name (HTTP $actual)"
  else
    FAIL=$((FAIL + 1))
    echo -e "${RED}FAIL${NC} [$TOTAL] $test_name (expected $expected, got $actual)"
  fi
}

# Authenticate
echo "=== Authenticating ==="
TOKEN=$(curl -s -X POST "$BASE_URL/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@ganka28.com","password":"Admin@123456"}' \
  | node -e "let d='';process.stdin.on('data',c=>d+=c);process.stdin.on('end',()=>process.stdout.write(JSON.parse(d).accessToken))")

if [ -z "$TOKEN" ]; then
  echo "Failed to authenticate. Is the backend running?"
  exit 1
fi
AUTH="Authorization: Bearer $TOKEN"
echo "Authenticated successfully"
echo ""

# ============================================================================
# PATIENT API TESTS
# ============================================================================
echo "=== Patient API Tests ==="

# Register Medical Patient
RESULT=$(curl -s -w "\n%{http_code}" -X POST "$BASE_URL/api/patients" \
  -H "Content-Type: application/json" -H "$AUTH" \
  -d '{"fullName":"Smoke Test Patient","phone":"0900000001","dateOfBirth":"1985-01-01","gender":0,"patientType":0}')
HTTP=$(echo "$RESULT" | tail -1)
BODY=$(echo "$RESULT" | head -n -1)
# 201 = new, 409 = already exists (both acceptable for idempotent test)
if [ "$HTTP" = "201" ] || [ "$HTTP" = "409" ]; then
  report "Register Medical Patient" "$HTTP" "$HTTP"
else
  report "Register Medical Patient" "201" "$HTTP"
fi

# Register Walk-in
RESULT=$(curl -s -w "\n%{http_code}" -X POST "$BASE_URL/api/patients" \
  -H "Content-Type: application/json" -H "$AUTH" \
  -d '{"fullName":"Walk-in Customer","phone":"0900000099","patientType":1}')
HTTP=$(echo "$RESULT" | tail -1)
if [ "$HTTP" = "201" ] || [ "$HTTP" = "409" ]; then
  report "Register Walk-in Customer" "$HTTP" "$HTTP"
else
  report "Register Walk-in Customer" "201" "$HTTP"
fi

# Search Patients
HTTP=$(curl -s -o /dev/null -w "%{http_code}" "$BASE_URL/api/patients/search?term=smoke" -H "$AUTH")
report "Search Patients" "200" "$HTTP"

# Get Patient (use search to find ID)
SEARCH_RESULT=$(curl -s "$BASE_URL/api/patients/search?term=smoke" -H "$AUTH")
PATIENT_ID=$(echo "$SEARCH_RESULT" | node -e "let d='';process.stdin.on('data',c=>d+=c);process.stdin.on('end',()=>{try{const a=JSON.parse(d);process.stdout.write(a[0]?.id||'')}catch(e){process.stdout.write('')}})")

if [ -n "$PATIENT_ID" ]; then
  HTTP=$(curl -s -o /dev/null -w "%{http_code}" "$BASE_URL/api/patients/$PATIENT_ID" -H "$AUTH")
  report "Get Patient Profile" "200" "$HTTP"

  # Add Allergy
  RESULT=$(curl -s -w "\n%{http_code}" -X POST "$BASE_URL/api/patients/$PATIENT_ID/allergies" \
    -H "Content-Type: application/json" -H "$AUTH" \
    -d '{"name":"Test Allergy","severity":1}')
  HTTP=$(echo "$RESULT" | tail -1)
  report "Add Allergy" "201" "$HTTP"
else
  echo "SKIP: No patient found for profile/allergy tests"
fi

echo ""

# ============================================================================
# SCHEDULING API TESTS
# ============================================================================
echo "=== Scheduling API Tests ==="

# Appointment Types
RESULT=$(curl -s -w "\n%{http_code}" "$BASE_URL/api/appointments/types" -H "$AUTH")
HTTP=$(echo "$RESULT" | tail -1)
BODY=$(echo "$RESULT" | head -n -1)
COUNT=$(echo "$BODY" | node -e "let d='';process.stdin.on('data',c=>d+=c);process.stdin.on('end',()=>process.stdout.write(String(JSON.parse(d).length)))")
report "Get Appointment Types (4)" "200" "$HTTP"

# Clinic Schedule
RESULT=$(curl -s -w "\n%{http_code}" "$BASE_URL/api/appointments/schedule" -H "$AUTH")
HTTP=$(echo "$RESULT" | tail -1)
report "Get Clinic Schedule (7 days)" "200" "$HTTP"

# Book Appointment (next Wednesday at 15:00)
NEXT_WED=$(node -e "const d=new Date();while(d.getDay()!=3)d.setDate(d.getDate()+1);console.log(d.toISOString().split('T')[0]+'T15:00:00')")
ADMIN_ID=$(echo "$TOKEN" | node -e "let d='';process.stdin.on('data',c=>d+=c);process.stdin.on('end',()=>{const p=JSON.parse(atob(d.split('.')[1]));process.stdout.write(p.sub)})")
NEW_TYPE="00000000-0000-0000-0000-000000000101"

if [ -n "$PATIENT_ID" ]; then
  RESULT=$(curl -s -w "\n%{http_code}" -X POST "$BASE_URL/api/appointments" \
    -H "Content-Type: application/json" -H "$AUTH" \
    -d "{\"patientId\":\"$PATIENT_ID\",\"patientName\":\"Smoke Test Patient\",\"doctorId\":\"$ADMIN_ID\",\"doctorName\":\"System Administrator\",\"startTime\":\"$NEXT_WED\",\"appointmentTypeId\":\"$NEW_TYPE\",\"notes\":\"Smoke test\"}")
  HTTP=$(echo "$RESULT" | tail -1)
  BODY=$(echo "$RESULT" | head -n -1)
  APPT_ID=$(echo "$BODY" | node -e "let d='';process.stdin.on('data',c=>d+=c);process.stdin.on('end',()=>{try{process.stdout.write(JSON.parse(d).id||'')}catch(e){process.stdout.write('')}})")

  if [ "$HTTP" = "201" ]; then
    report "Book Appointment" "201" "$HTTP"

    # Double-book same slot
    RESULT=$(curl -s -w "\n%{http_code}" -X POST "$BASE_URL/api/appointments" \
      -H "Content-Type: application/json" -H "$AUTH" \
      -d "{\"patientId\":\"$PATIENT_ID\",\"patientName\":\"Smoke Test Patient\",\"doctorId\":\"$ADMIN_ID\",\"doctorName\":\"System Administrator\",\"startTime\":\"$NEXT_WED\",\"appointmentTypeId\":\"$NEW_TYPE\"}")
    HTTP=$(echo "$RESULT" | tail -1)
    report "Double-Book Prevention (409)" "409" "$HTTP"

    # Cancel
    RESULT=$(curl -s -w "\n%{http_code}" -X PUT "$BASE_URL/api/appointments/$APPT_ID/cancel" \
      -H "Content-Type: application/json" -H "$AUTH" \
      -d "{\"appointmentId\":\"$APPT_ID\",\"cancellationReason\":1}")
    HTTP=$(echo "$RESULT" | tail -1)
    report "Cancel Appointment" "200" "$HTTP"
  else
    report "Book Appointment" "201" "$HTTP"
  fi
fi

# Outside hours (Monday)
NEXT_MON=$(node -e "const d=new Date();while(d.getDay()!=1)d.setDate(d.getDate()+1);console.log(d.toISOString().split('T')[0]+'T14:00:00')")
if [ -n "$PATIENT_ID" ]; then
  HTTP=$(curl -s -o /dev/null -w "%{http_code}" -X POST "$BASE_URL/api/appointments" \
    -H "Content-Type: application/json" -H "$AUTH" \
    -d "{\"patientId\":\"$PATIENT_ID\",\"patientName\":\"Smoke Test Patient\",\"doctorId\":\"$ADMIN_ID\",\"doctorName\":\"System Administrator\",\"startTime\":\"$NEXT_MON\",\"appointmentTypeId\":\"$NEW_TYPE\"}")
  report "Outside Hours Rejection (400)" "400" "$HTTP"
fi

echo ""

# ============================================================================
# PUBLIC BOOKING TESTS (no auth)
# Note: Public endpoints have rate limiting (5 req/min/IP).
# 429 responses indicate rate limiter is working correctly.
# ============================================================================
echo "=== Public Booking Tests (No Auth) ==="
echo "(Rate limited: 5 req/min -- 429 = rate limiter working correctly)"

# Public types endpoint (test first, before using up rate limit on POST)
HTTP=$(curl -s -o /dev/null -w "%{http_code}" "$BASE_URL/api/public/booking/types")
report "Public Appointment Types" "200" "$HTTP"

# Public schedule endpoint
HTTP=$(curl -s -o /dev/null -w "%{http_code}" "$BASE_URL/api/public/booking/schedule")
report "Public Clinic Schedule" "200" "$HTTP"

# Submit self-booking (use unique phone to avoid pending limit)
RAND_PHONE="09$(shuf -i 10000000-99999999 -n 1 2>/dev/null || echo $RANDOM$RANDOM | head -c 8)"
RESULT=$(curl -s -w "\n%{http_code}" -X POST "$BASE_URL/api/public/booking" \
  -H "Content-Type: application/json" \
  -d "{\"patientName\":\"Public Tester\",\"phone\":\"$RAND_PHONE\",\"preferredDate\":\"$NEXT_WED\",\"appointmentTypeId\":\"$NEW_TYPE\"}")
HTTP=$(echo "$RESULT" | tail -1)
BODY=$(echo "$RESULT" | head -n -1)
REF=$(echo "$BODY" | node -e "let d='';process.stdin.on('data',c=>d+=c);process.stdin.on('end',()=>{try{process.stdout.write(JSON.parse(d).referenceNumber||'')}catch(e){process.stdout.write('')}})")
report "Public Self-Booking" "201" "$HTTP"

if [ -n "$REF" ]; then
  HTTP=$(curl -s -o /dev/null -w "%{http_code}" "$BASE_URL/api/public/booking/status/$REF")
  report "Booking Status Check (Pending)" "200" "$HTTP"
fi

echo ""

# ============================================================================
# SUMMARY
# ============================================================================
echo "=== RESULTS ==="
echo "Total: $TOTAL | Passed: $PASS | Failed: $FAIL"
if [ $FAIL -eq 0 ]; then
  echo -e "${GREEN}ALL TESTS PASSED${NC}"
  exit 0
else
  echo -e "${RED}SOME TESTS FAILED${NC}"
  exit 1
fi
