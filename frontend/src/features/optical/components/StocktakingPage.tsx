import { useState } from "react"
import { format } from "date-fns"
import {
  IconClipboardList,
  IconPlus,
  IconCheck,
  IconX,
  IconEye,
  IconAlertTriangle,
} from "@tabler/icons-react"
import { Card, CardContent, CardHeader, CardTitle } from "@/shared/components/Card"
import { Button } from "@/shared/components/Button"
import { Badge } from "@/shared/components/Badge"
import { Skeleton } from "@/shared/components/Skeleton"
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/shared/components/Dialog"
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from "@/shared/components/AlertDialog"
import { Input } from "@/shared/components/Input"
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/shared/components/Table"
import {
  useStocktakingSessions,
  useStartStocktakingSession,
  useCompleteStocktaking,
} from "../api/optical-queries"
import type { StocktakingSessionDto } from "../api/optical-api"
import { STOCKTAKING_STATUS_MAP } from "../api/optical-api"
import { StocktakingScanner } from "./StocktakingScanner"
import { DiscrepancyReport } from "./DiscrepancyReport"

// Status values matching backend StocktakingStatus enum
const STATUS_IN_PROGRESS = 0
const STATUS_COMPLETED = 1
// const STATUS_CANCELLED = 2  // Available if needed later

type PageMode = "list" | "active" | "report"

export function StocktakingPage() {
  const [mode, setMode] = useState<PageMode>("list")
  const [activeSessionId, setActiveSessionId] = useState<string | null>(null)
  const [reportSessionId, setReportSessionId] = useState<string | null>(null)

  // Dialog state
  const [startDialogOpen, setStartDialogOpen] = useState(false)
  const [newSessionName, setNewSessionName] = useState("")
  const [completeDialogOpen, setCompleteDialogOpen] = useState(false)

  // Stats for active session
  const [scannedCount, setScannedCount] = useState(0)
  const [discrepancyCount, setDiscrepancyCount] = useState(0)

  const { data: sessionsResult, isLoading } = useStocktakingSessions({ page: 1, pageSize: 50 })
  const startSession = useStartStocktakingSession()
  const completeSession = useCompleteStocktaking()

  const sessions = sessionsResult?.items ?? []
  const inProgressSession = sessions.find((s) => s.status === STATUS_IN_PROGRESS)

  const handleStartSession = async () => {
    if (!newSessionName.trim()) return
    try {
      const result = await startSession.mutateAsync({ name: newSessionName.trim() })
      setActiveSessionId(result.id)
      setScannedCount(0)
      setDiscrepancyCount(0)
      setMode("active")
      setStartDialogOpen(false)
      setNewSessionName("")
    } catch {
      // Error handled by mutation's onError toast
    }
  }

  const handleResumeSession = (session: StocktakingSessionDto) => {
    setActiveSessionId(session.id)
    setScannedCount(session.itemCount ?? 0)
    setDiscrepancyCount(0)
    setMode("active")
  }

  const handleCompleteSession = async () => {
    if (!activeSessionId) return
    try {
      await completeSession.mutateAsync(activeSessionId)
      setReportSessionId(activeSessionId)
      setActiveSessionId(null)
      setMode("report")
      setCompleteDialogOpen(false)
    } catch {
      // Error handled by mutation's onError toast
    }
  }

  const handleViewReport = (session: StocktakingSessionDto) => {
    setReportSessionId(session.id)
    setMode("report")
  }

  const handleBackToList = () => {
    setMode("list")
    setActiveSessionId(null)
    setReportSessionId(null)
  }

  const handleItemRecorded = (itemDiscrepancy: number) => {
    setScannedCount((prev) => prev + 1)
    if (itemDiscrepancy !== 0) {
      setDiscrepancyCount((prev) => prev + 1)
    }
  }

  const getStatusBadgeVariant = (status: number) => {
    switch (status) {
      case STATUS_IN_PROGRESS:
        return "secondary" as const
      case STATUS_COMPLETED:
        return "default" as const
      default:
        return "outline" as const
    }
  }

  // Active Session View
  if (mode === "active" && activeSessionId) {
    const session = inProgressSession ?? sessions.find((s) => s.id === activeSessionId)

    return (
      <div className="space-y-4">
        <div className="flex items-start justify-between gap-4">
          <div>
            <h1 className="text-xl font-bold">{session?.name ?? "Active Stocktaking"}</h1>
            <p className="text-muted-foreground mt-0.5 text-sm">
              {scannedCount} items scanned
              {discrepancyCount > 0 && (
                <span className="ml-2 text-yellow-600">
                  <IconAlertTriangle className="inline h-3.5 w-3.5" /> {discrepancyCount} discrepancies
                </span>
              )}
            </p>
          </div>
          <div className="flex gap-2 shrink-0">
            <Button
              variant="outline"
              size="sm"
              onClick={handleBackToList}
            >
              <IconX className="mr-1.5 h-4 w-4" />
              Cancel
            </Button>
            <Button
              size="sm"
              onClick={() => setCompleteDialogOpen(true)}
              disabled={scannedCount === 0}
            >
              <IconCheck className="mr-1.5 h-4 w-4" />
              Complete
            </Button>
          </div>
        </div>

        <StocktakingScanner
          sessionId={activeSessionId}
          onItemRecorded={(item) => handleItemRecorded(item.discrepancy)}
        />

        {/* Complete confirmation dialog */}
        <AlertDialog open={completeDialogOpen} onOpenChange={setCompleteDialogOpen}>
          <AlertDialogContent>
            <AlertDialogHeader>
              <AlertDialogTitle>Complete Stocktaking Session?</AlertDialogTitle>
              <AlertDialogDescription>
                This will finalize the stocktaking session and generate a discrepancy report.
                You have scanned {scannedCount} items with {discrepancyCount} discrepancies.
                This action cannot be undone.
              </AlertDialogDescription>
            </AlertDialogHeader>
            <AlertDialogFooter>
              <AlertDialogCancel>Continue Scanning</AlertDialogCancel>
              <AlertDialogAction
                onClick={handleCompleteSession}
                disabled={completeSession.isPending}
              >
                {completeSession.isPending ? "Completing..." : "Complete Session"}
              </AlertDialogAction>
            </AlertDialogFooter>
          </AlertDialogContent>
        </AlertDialog>
      </div>
    )
  }

  // Report View
  if (mode === "report" && reportSessionId) {
    const session = sessions.find((s) => s.id === reportSessionId)

    return (
      <div className="space-y-4">
        <div className="flex items-center justify-between gap-4">
          <div>
            <h1 className="text-xl font-bold">Discrepancy Report</h1>
            {session && (
              <p className="text-muted-foreground mt-0.5 text-sm">
                {session.name}
                {session.completedAt && (
                  <span className="ml-2">
                    — Completed {format(new Date(session.completedAt), "dd/MM/yyyy HH:mm")}
                  </span>
                )}
              </p>
            )}
          </div>
          <Button variant="outline" size="sm" onClick={handleBackToList}>
            Back to Sessions
          </Button>
        </div>

        <DiscrepancyReport sessionId={reportSessionId} />
      </div>
    )
  }

  // Session List View
  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold">Stocktaking</h1>
          <p className="text-muted-foreground mt-0.5 text-sm">
            Barcode-based physical inventory count
          </p>
        </div>
        <Button
          onClick={() => setStartDialogOpen(true)}
          disabled={!!inProgressSession}
          title={inProgressSession ? "A session is already in progress" : undefined}
        >
          <IconPlus className="mr-1.5 h-4 w-4" />
          Start New Stocktaking
        </Button>
      </div>

      {/* In-progress session banner */}
      {inProgressSession && (
        <Card className="border-yellow-300 bg-yellow-50 dark:bg-yellow-950/20">
          <CardContent className="flex items-center justify-between py-4">
            <div className="flex items-center gap-3">
              <IconAlertTriangle className="h-5 w-5 text-yellow-600" />
              <div>
                <p className="font-medium text-yellow-800 dark:text-yellow-200">
                  Session in progress: {inProgressSession.name}
                </p>
                <p className="text-xs text-yellow-700 dark:text-yellow-300">
                  Started {format(new Date(inProgressSession.startedAt), "dd/MM/yyyy HH:mm")}
                  {" — "}
                  {inProgressSession.itemCount} items scanned
                </p>
              </div>
            </div>
            <Button
              size="sm"
              variant="outline"
              className="border-yellow-400 text-yellow-800 hover:bg-yellow-100 dark:text-yellow-200"
              onClick={() => handleResumeSession(inProgressSession)}
            >
              Resume
            </Button>
          </CardContent>
        </Card>
      )}

      {/* Sessions table */}
      <Card>
        <CardHeader className="pb-3">
          <CardTitle className="flex items-center gap-2 text-base">
            <IconClipboardList className="h-5 w-5" />
            Sessions History
          </CardTitle>
        </CardHeader>
        <CardContent className="p-0">
          {isLoading ? (
            <div className="space-y-2 p-4">
              {Array.from({ length: 4 }).map((_, i) => (
                <Skeleton key={i} className="h-12 rounded" />
              ))}
            </div>
          ) : sessions.length === 0 ? (
            <div className="text-muted-foreground py-12 text-center text-sm">
              <IconClipboardList className="mx-auto mb-3 h-10 w-10 opacity-20" />
              No stocktaking sessions yet. Start a new session to begin counting inventory.
            </div>
          ) : (
            <div className="overflow-x-auto">
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Session Name</TableHead>
                    <TableHead>Status</TableHead>
                    <TableHead>Started By</TableHead>
                    <TableHead>Date</TableHead>
                    <TableHead className="text-right">Items</TableHead>
                    <TableHead className="text-right">Actions</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {sessions.map((session) => (
                    <TableRow key={session.id}>
                      <TableCell className="font-medium">{session.name}</TableCell>
                      <TableCell>
                        <Badge variant={getStatusBadgeVariant(session.status)}>
                          {STOCKTAKING_STATUS_MAP[session.status] ?? "Unknown"}
                        </Badge>
                      </TableCell>
                      <TableCell className="text-muted-foreground text-sm">
                        {"—"}
                      </TableCell>
                      <TableCell className="text-sm">
                        {format(new Date(session.startedAt), "dd/MM/yyyy HH:mm")}
                      </TableCell>
                      <TableCell className="text-right text-sm">
                        {session.itemCount ?? 0}
                      </TableCell>
                      <TableCell className="text-right">
                        {session.status === STATUS_IN_PROGRESS ? (
                          <Button
                            size="sm"
                            variant="outline"
                            onClick={() => handleResumeSession(session)}
                          >
                            Resume
                          </Button>
                        ) : session.status === STATUS_COMPLETED ? (
                          <Button
                            size="sm"
                            variant="ghost"
                            onClick={() => handleViewReport(session)}
                          >
                            <IconEye className="mr-1 h-4 w-4" />
                            View Report
                          </Button>
                        ) : null}
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </div>
          )}
        </CardContent>
      </Card>

      {/* Start new session dialog */}
      <Dialog open={startDialogOpen} onOpenChange={setStartDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Start New Stocktaking Session</DialogTitle>
            <DialogDescription>
              Enter a name for this stocktaking session. Once started, you can scan barcodes to
              record physical inventory counts.
            </DialogDescription>
          </DialogHeader>
          <div className="py-2">
            <label className="mb-1.5 block text-sm font-medium">Session Name</label>
            <Input
              value={newSessionName}
              onChange={(e) => setNewSessionName(e.target.value)}
              onKeyDown={(e) => {
                if (e.key === "Enter") handleStartSession()
              }}
              autoFocus
            />
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setStartDialogOpen(false)}>
              Cancel
            </Button>
            <Button
              onClick={handleStartSession}
              disabled={!newSessionName.trim() || startSession.isPending}
            >
              {startSession.isPending ? "Starting..." : "Start Session"}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  )
}
