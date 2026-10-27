import {
  Award,
  Brain,
  Compass,
  Eye,
  Footprints,
  Scroll,
  Sparkles,
  type LucideIcon,
} from 'lucide-react'

// O nome do ícone vem do banco (campo `icon` da conquista); desconhecido cai no Award.
const icons: Record<string, LucideIcon> = {
  brain: Brain,
  compass: Compass,
  eye: Eye,
  footprints: Footprints,
  scroll: Scroll,
  sparkles: Sparkles,
}

interface AchievementIconProps {
  icon: string
  size?: number
  className?: string
}

export default function AchievementIcon({ icon, size = 24, className }: AchievementIconProps) {
  const Icon = icons[icon] ?? Award
  return <Icon size={size} className={className} aria-hidden="true" />
}
