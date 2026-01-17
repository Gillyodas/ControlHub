"use client"

import { Outlet } from "react-router-dom"

import { Header } from "@/components/dashboard/header"
import { Sidebar } from "@/components/dashboard/Sidebar"
import { cn } from "@/lib/utils"
import { useState } from "react"
import { useTokenRefresh } from "@/hooks/use-token-refresh"

export function MainLayout() {
  const [collapsed, setCollapsed] = useState(false)
  useTokenRefresh() // Auto-refresh tokens

  return (
    <div className="flex h-screen overflow-hidden bg-background selection:bg-sidebar-primary/30">
      <Sidebar collapsed={collapsed} onToggle={() => setCollapsed(!collapsed)} />
      <div className="flex flex-col flex-1 overflow-hidden relative">
        {/* Ambient background glow */}
        <div className="absolute top-0 right-0 w-[500px] h-[500px] bg-sidebar-primary/5 blur-[120px] rounded-full pointer-events-none -z-10 translate-x-1/2 -translate-y-1/2" />
        <div className="absolute bottom-0 left-0 w-[500px] h-[500px] bg-vibrant-purple/5 blur-[120px] rounded-full pointer-events-none -z-10 -translate-x-1/2 translate-y-1/2" />

        <Header />
        <main className={cn("flex-1 overflow-auto p-8 custom-scrollbar")}>
          <div className="max-w-7xl mx-auto min-h-full">
            <Outlet />
          </div>
        </main>
      </div>
    </div>
  )
}
