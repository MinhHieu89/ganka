import { Suspense } from "react"
import type { ReactNode } from "react"
import { Outlet, createRootRoute, HeadContent, Scripts } from "@tanstack/react-router"
import { QueryClient, QueryClientProvider } from "@tanstack/react-query"
import { Toaster } from "@/shared/components/ui/sonner"
import { TooltipProvider } from "@/shared/components/ui/tooltip"
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
      { title: "Ganka28 - Ophthalmology Clinic Management" },
    ],
    links: [
      { rel: "stylesheet", href: globalsCss },
    ],
  }),
  component: RootComponent,
})

function RootComponent() {
  return (
    <RootDocument>
      <QueryClientProvider client={queryClient}>
        <TooltipProvider delayDuration={0}>
          <Suspense fallback={<div className="flex items-center justify-center min-h-screen">Loading...</div>}>
            <Outlet />
          </Suspense>
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
