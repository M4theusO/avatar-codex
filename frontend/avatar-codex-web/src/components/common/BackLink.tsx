import { ArrowLeft } from 'lucide-react'
import { Link } from 'react-router'
import styles from './BackLink.module.css'

export default function BackLink({ to, label }: { to: string; label: string }) {
  return (
    <Link to={to} className={styles.link}>
      <ArrowLeft size={16} aria-hidden="true" />
      {label}
    </Link>
  )
}
