export const validators = {
  email: (value: string): boolean => {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/
    return emailRegex.test(value)
  },

  phone: (value: string): boolean => {
    const phoneRegex = /^\+?[1-9]\d{1,14}$/
    return phoneRegex.test(value.replace(/\s/g, ''))
  },

  password: (value: string): { isValid: boolean; errors: string[] } => {
    const errors: string[] = []

    if (value.length < 8) {
      errors.push('Password must be at least 8 characters long')
    }
    if (!/[A-Z]/.test(value)) {
      errors.push('Password must contain at least one uppercase letter')
    }
    if (!/[a-z]/.test(value)) {
      errors.push('Password must contain at least one lowercase letter')
    }
    if (!/[0-9]/.test(value)) {
      errors.push('Password must contain at least one number')
    }
    if (!/[!@#$%^&*(),.?":{}|<>]/.test(value)) {
      errors.push('Password must contain at least one special character')
    }

    return {
      isValid: errors.length === 0,
      errors,
    }
  },

  username: (value: string): { isValid: boolean; error?: string } => {
    if (value.length < 3) {
      return { isValid: false, error: 'Username must be at least 3 characters long' }
    }
    if (value.length > 30) {
      return { isValid: false, error: 'Username must not exceed 30 characters' }
    }
    if (!/^[a-zA-Z0-9_-]+$/.test(value)) {
      return { isValid: false, error: 'Username can only contain letters, numbers, hyphens, and underscores' }
    }
    return { isValid: true }
  },

  required: (value: string | null | undefined): boolean => {
    return value !== null && value !== undefined && value.trim() !== ''
  },

  minLength: (value: string, min: number): boolean => {
    return value.length >= min
  },

  maxLength: (value: string, max: number): boolean => {
    return value.length <= max
  },

  isNumeric: (value: string): boolean => {
    return /^\d+$/.test(value)
  },

  isAlphanumeric: (value: string): boolean => {
    return /^[a-zA-Z0-9]+$/.test(value)
  },

  url: (value: string): boolean => {
    try {
      new URL(value)
      return true
    } catch {
      return false
    }
  },
}

export type ValidationResult = {
  isValid: boolean
  errors?: string[]
  error?: string
}

export const validateForm = <T extends Record<string, unknown>>(
  values: T,
  rules: Partial<Record<keyof T, (value: unknown) => ValidationResult>>
): { isValid: boolean; errors: Partial<Record<keyof T, string>> } => {
  const errors: Partial<Record<keyof T, string>> = {}
  let isValid = true

  for (const key in rules) {
    const validator = rules[key]
    if (validator) {
      const result = validator(values[key])
      if (!result.isValid) {
        isValid = false
        errors[key] = result.error || result.errors?.[0] || 'Invalid value'
      }
    }
  }

  return { isValid, errors }
}
