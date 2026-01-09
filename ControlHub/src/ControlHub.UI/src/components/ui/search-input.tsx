import { useState, useEffect } from "react"
import { Input } from "@/components/ui/input"
import { Button } from "@/components/ui/button"
import { Search, X } from "lucide-react"
import { useDebounce } from "@/hooks/use-debounce"

interface SearchInputProps {
  placeholder?: string
  onSearch: (value: string) => void
  debounceDelay?: number
  className?: string
}

export function SearchInput({
  placeholder = "Search...",
  onSearch,
  debounceDelay = 500,
  className,
}: SearchInputProps) {
  const [value, setValue] = useState("")
  const debouncedValue = useDebounce(value, debounceDelay)

  useEffect(() => {
    onSearch(debouncedValue)
  }, [debouncedValue, onSearch])

  const handleClear = () => {
    setValue("")
  }

  return (
    <div className="relative">
      <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
      <Input
        type="search"
        placeholder={placeholder}
        value={value}
        onChange={(e) => setValue(e.target.value)}
        className={`pl-10 pr-10 ${className}`}
      />
      {value && (
        <Button
          type="button"
          variant="ghost"
          size="sm"
          className="absolute right-1 top-1/2 -translate-y-1/2 h-7 w-7 p-0"
          onClick={handleClear}
        >
          <X className="h-4 w-4" />
        </Button>
      )}
    </div>
  )
}
