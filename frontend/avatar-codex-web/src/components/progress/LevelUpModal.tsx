import { Sparkles } from 'lucide-react'
import { Button } from '../common/Button'
import Modal from '../common/Modal'
import styles from './LevelUpModal.module.css'

interface LevelUpModalProps {
  level: number | undefined
  onClose: () => void
}

export default function LevelUpModal({ level, onClose }: LevelUpModalProps) {
  return (
    <Modal open={level !== undefined} onClose={onClose} labelledBy="level-up-title">
      <div className={styles.body}>
        <span className={styles.burst} aria-hidden="true">
          <Sparkles size={36} />
        </span>
        <h2 id="level-up-title" className={styles.title}>
          Novo nível!
        </h2>
        <p className={styles.level}>
          Você alcançou o <strong>nível {level}</strong>.
        </p>
        <p className={styles.text}>Continue sua jornada!</p>
        <Button variant="primary" onClick={onClose} autoFocus>
          Continuar
        </Button>
      </div>
    </Modal>
  )
}
