import { Suspense, useEffect, useState } from "react"
import type { ReactNode } from "react"
import { Outlet, createRootRoute, HeadContent, Scripts } from "@tanstack/react-router"
import { QueryClient, QueryClientProvider } from "@tanstack/react-query"
import { Toaster } from "@/shared/components/Sonner"
import { TooltipProvider } from "@/shared/components/Tooltip"
import { useAuthStore } from "@/shared/stores/authStore"
import globalsCss from "@/styles/globals.css?url"
import "@/shared/i18n/i18n"

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 1000 * 60 * 5,
      retry: 1,
    },
  },
})

export const Route = createRootRoute({
  head: () => ({
    meta: [
      { charSet: "utf-8" },
      { name: "viewport", content: "width=device-width, initial-scale=1" },
      { title: "Ganka28 - Clinic Management" },
    ],
    links: [
      { rel: "preconnect", href: "https://fonts.googleapis.com" },
      { rel: "preconnect", href: "https://fonts.gstatic.com", crossOrigin: "anonymous" },
      { rel: "stylesheet", href: "https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600&family=Manrope:wght@400;500;600;700;800&family=JetBrains+Mono:wght@400;500&display=swap" },
      { rel: "stylesheet", href: globalsCss },
    ],
  }),
  component: RootComponent,
})

function RootComponent() {
  const isInitializing = useAuthStore((s) => s.isInitializing)
  const [initStarted, setInitStarted] = useState(false)

  useEffect(() => {
    if (!initStarted) {
      setInitStarted(true)
      useAuthStore.getState().initialize()
    }
  }, [initStarted])

  return (
    <RootDocument>
      <QueryClientProvider client={queryClient}>
        <TooltipProvider delayDuration={0}>
          {isInitializing ? (
            <div className="flex items-center justify-center min-h-screen">Loading...</div>
          ) : (
            <Suspense fallback={<div className="flex items-center justify-center min-h-screen">Loading...</div>}>
              <Outlet />
            </Suspense>
          )}
          <Toaster />
        </TooltipProvider>
      </QueryClientProvider>
    </RootDocument>
  )
}

function RootDocument({ children }: Readonly<{ children: ReactNode }>) {
  return (
    <html lang="vi">
      <head>
        <HeadContent />
      </head>
      <body>
        {children}
        <Scripts />
      </body>
    </html>
  )
}
