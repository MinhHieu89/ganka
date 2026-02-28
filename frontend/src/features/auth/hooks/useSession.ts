import { useCallback, useEffect, useRef, useState } from "react"
import { useAuth } from "./useAuth"

const SESSION_TIMEOUT_MS = 30 * 60 * 1000 // 30 minutes
const WARNING_THRESHOLD_MS = 2 * 60 * 1000 // 2 minutes before timeout
const ACTIVITY_EVENTS = ["mousemove", "keydown", "click", "touchstart", "scroll"]
const ACTIVITY_THROTTLE_MS = 30_000 // Only reset timer at most every 30s of activity

export function useSession() {
  const { logout, isAuthenticated } = useAuth()
  const [showWarning, setShowWarning] = useState(false)
  const [remainingSeconds, setRemainingSeconds] = useState(0)
  const [isTimedOut, setIsTimedOut] = useState(false)

  const lastActivityRef = useRef(Date.now())
  const warningTimerRef = useRef<ReturnType<typeof setTimeout> | null>(null)
  const timeoutTimerRef = useRef<ReturnType<typeof setTimeout> | null>(null)
  const countdownRef = useRef<ReturnType<typeof setInterval> | null>(null)
  const throttleRef = useRef<number>(0)

  const clearAllTimers = useCallback(() => {
    if (warningTimerRef.current) clearTimeout(warningTimerRef.current)
    if (timeoutTimerRef.current) clearTimeout(timeoutTimerRef.current)
    if (countdownRef.current) clearInterval(countdownRef.current)
    warningTimerRef.current = null
    timeoutTimerRef.current = null
    countdownRef.current = null
  }, [])

  const startCountdown = useCallback(() => {
    const warningDuration = WARNING_THRESHOLD_MS / 1000
    setRemainingSeconds(warningDuration)

    countdownRef.current = setInterval(() => {
      setRemainingSeconds((prev) => {
        if (prev <= 1) {
          return 0
        }
        return prev - 1
      })
    }, 1000)
  }, [])

  const resetTimers = useCallback(() => {
    clearAllTimers()
    setShowWarning(false)
    setIsTimedOut(false)
    lastActivityRef.current = Date.now()

    // Set warning timer (fires 2 minutes before timeout)
    warningTimerRef.current = setTimeout(() => {
      setShowWarning(true)
      startCountdown()
    }, SESSION_TIMEOUT_MS - WARNING_THRESHOLD_MS)

    // Set timeout timer
    timeoutTimerRef.current = setTimeout(() => {
      setIsTimedOut(true)
      setShowWarning(false)
      clearAllTimers()
      logout()
    }, SESSION_TIMEOUT_MS)
  }, [clearAllTimers, startCountdown, logout])

  const extendSession = useCallback(() => {
    setShowWarning(false)
    setRemainingSeconds(0)
    resetTimers()
  }, [resetTimers])

  // Activity tracking
  useEffect(() => {
    if (!isAuthenticated) return

    const handleActivity = () => {
      const now = Date.now()
      // Throttle: only reset if enough time has passed since last reset
      if (now - throttleRef.current < ACTIVITY_THROTTLE_MS) return
      throttleRef.current = now

      // Only reset timers if warning is NOT showing
      // (Don't reset on activity while the warning modal is displayed)
      if (!showWarning) {
        resetTimers()
      }
    }

    for (const event of ACTIVITY_EVENTS) {
      window.addEventListener(event, handleActivity, { passive: true })
    }

    // Start initial timers
    resetTimers()

    return () => {
      for (const event of ACTIVITY_EVENTS) {
        window.removeEventListener(event, handleActivity)
      }
      clearAllTimers()
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [isAuthenticated])

  return {
    showWarning,
    remainingSeconds,
    extendSession,
    isTimedOut,
  }
}
