import * as React from "react"
import { cn } from "@/lib/utils"
import { ChevronDown } from "lucide-react"

interface CollapsibleContextValue {
    open: boolean
    setOpen: (open: boolean) => void
}

const CollapsibleContext = React.createContext<CollapsibleContextValue | undefined>(undefined)

interface CollapsibleProps {
    children: React.ReactNode
    defaultOpen?: boolean
    open?: boolean
    onOpenChange?: (open: boolean) => void
    className?: string
}

const Collapsible = React.forwardRef<HTMLDivElement, CollapsibleProps>(
    ({ children, defaultOpen = false, open: controlledOpen, onOpenChange, className, ...props }, ref) => {
        const [uncontrolledOpen, setUncontrolledOpen] = React.useState(defaultOpen)

        const isControlled = controlledOpen !== undefined
        const open = isControlled ? controlledOpen : uncontrolledOpen

        const setOpen = React.useCallback((value: boolean) => {
            if (!isControlled) {
                setUncontrolledOpen(value)
            }
            onOpenChange?.(value)
        }, [isControlled, onOpenChange])

        return (
            <CollapsibleContext.Provider value={{ open, setOpen }}>
                <div ref={ref} className={cn("", className)} {...props}>
                    {children}
                </div>
            </CollapsibleContext.Provider>
        )
    }
)
Collapsible.displayName = "Collapsible"

interface CollapsibleTriggerProps extends React.ButtonHTMLAttributes<HTMLButtonElement> {
    asChild?: boolean
}

const CollapsibleTrigger = React.forwardRef<HTMLButtonElement, CollapsibleTriggerProps>(
    ({ className, children, asChild, ...props }, ref) => {
        const context = React.useContext(CollapsibleContext)

        if (!context) {
            throw new Error("CollapsibleTrigger must be used within a Collapsible")
        }

        const { open, setOpen } = context

        return (
            <button
                ref={ref}
                type="button"
                className={cn("flex items-center justify-between w-full", className)}
                onClick={() => setOpen(!open)}
                aria-expanded={open}
                {...props}
            >
                {children}
                <ChevronDown
                    className={cn(
                        "h-4 w-4 transition-transform duration-200",
                        open && "rotate-180"
                    )}
                />
            </button>
        )
    }
)
CollapsibleTrigger.displayName = "CollapsibleTrigger"

interface CollapsibleContentProps extends React.HTMLAttributes<HTMLDivElement> { }

const CollapsibleContent = React.forwardRef<HTMLDivElement, CollapsibleContentProps>(
    ({ className, children, ...props }, ref) => {
        const context = React.useContext(CollapsibleContext)

        if (!context) {
            throw new Error("CollapsibleContent must be used within a Collapsible")
        }

        const { open } = context

        if (!open) return null

        return (
            <div
                ref={ref}
                className={cn("overflow-hidden", className)}
                {...props}
            >
                {children}
            </div>
        )
    }
)
CollapsibleContent.displayName = "CollapsibleContent"

export { Collapsible, CollapsibleTrigger, CollapsibleContent }
