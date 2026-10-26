import styles from './Skeleton.module.css'

interface SkeletonProps {
  width?: string
  height?: string
  round?: boolean
}

export default function Skeleton({
  width = '100%',
  height = '1rem',
  round = false,
}: SkeletonProps) {
  return (
    <span
      className={`${styles.skeleton} ${round ? styles.round : ''}`}
      style={{ width, height }}
      aria-hidden="true"
    />
  )
}
