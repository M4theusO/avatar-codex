import { useEffect, useRef, type ReactNode } from 'react'
import styles from './Modal.module.css'

interface ModalProps {
  open: boolean
  onClose: () => void
  /** Id do título dentro do modal (aria-labelledby). */
  labelledBy: string
  children: ReactNode
}

/**
 * `<dialog>` nativo com `showModal()`: o navegador cuida do foco preso dentro do modal,
 * do Esc para fechar e de deixar o resto da página inerte.
 */
export default function Modal({ open, onClose, labelledBy, children }: ModalProps) {
  const ref = useRef<HTMLDialogElement>(null)

  useEffect(() => {
    const dialog = ref.current
    if (!dialog) return
    if (open && !dialog.open) dialog.showModal()
    if (!open && dialog.open) dialog.close()
  }, [open])

  return (
    <dialog
      ref={ref}
      className={styles.dialog}
      aria-labelledby={labelledBy}
      onClose={onClose}
      // Clique no fundo escurecido (fora da caixa) também fecha.
      onClick={(event) => event.target === event.currentTarget && onClose()}
    >
      <div className={styles.content}>{children}</div>
    </dialog>
  )
}
