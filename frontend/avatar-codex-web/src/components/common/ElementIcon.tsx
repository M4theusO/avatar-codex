import { Droplets, Flame, Mountain, Sparkles, Wind, type LucideIcon } from 'lucide-react'

// O nome do ícone vem do banco (campo `icon` do elemento).
const icons: Record<string, LucideIcon> = {
  droplets: Droplets,
  flame: Flame,
  mountain: Mountain,
  wind: Wind,
}

interface ElementIconProps {
  icon: string | null | undefined
  size?: number
  className?: string
}

export default function ElementIcon({ icon, size = 16, className }: ElementIconProps) {
  const Icon = (icon && icons[icon]) || Sparkles
  return <Icon size={size} className={className} aria-hidden="true" />
}
