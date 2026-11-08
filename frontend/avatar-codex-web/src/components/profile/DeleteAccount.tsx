import { AlertCircle, Trash2 } from 'lucide-react'
import { useState, type FormEvent } from 'react'
import { useDeleteAccount } from '../../hooks/useDeleteAccount'
import { getApiError } from '../../utils/apiError'
import { Button } from '../common/Button'
import Modal from '../common/Modal'
import TextField from '../common/TextField'
import styles from './DeleteAccount.module.css'

/** Seção do perfil para excluir a conta, com confirmação pela senha num modal. */
export default function DeleteAccount() {
  const deleteAccount = useDeleteAccount()
  const [open, setOpen] = useState(false)
  const [password, setPassword] = useState('')
  const [fieldError, setFieldError] = useState<string>()
  const [formError, setFormError] = useState<string>()
  const [submitting, setSubmitting] = useState(false)

  const close = () => {
    if (submitting) return
    setOpen(false)
    setPassword('')
    setFieldError(undefined)
    setFormError(undefined)
  }

  const submit = async (event: FormEvent) => {
    event.preventDefault()
    setFieldError(undefined)
    setFormError(undefined)
    setSubmitting(true)
    try {
      await deleteAccount(password)
    } catch (error) {
      const { message, fieldErrors } = getApiError(error)
      if (fieldErrors.password) setFieldError(fieldErrors.password)
      else setFormError(message)
      setSubmitting(false)
    }
  }

  return (
    <div className={styles.zone}>
      <p className={styles.text}>
        Apaga sua conta e todo o seu progresso: XP, conquistas, favoritos, descobertas, quizzes e
        desafios. Não dá para desfazer.
      </p>
      <Button variant="danger" onClick={() => setOpen(true)}>
        <Trash2 size={18} aria-hidden="true" />
        Excluir conta
      </Button>

      <Modal open={open} onClose={close} labelledBy="delete-account-title">
        <form className={styles.form} onSubmit={submit} noValidate>
          <h2 id="delete-account-title" className={styles.title}>
            Excluir conta?
          </h2>
          <p className={styles.text}>
            Todo o seu progresso será apagado para sempre. Digite sua senha para confirmar.
          </p>
          {formError && (
            <div className={styles.alert} role="alert">
              <AlertCircle size={18} aria-hidden="true" />
              {formError}
            </div>
          )}
          <TextField
            label="Senha"
            type="password"
            autoComplete="current-password"
            value={password}
            onChange={(event) => setPassword(event.target.value)}
            error={fieldError}
            required
            autoFocus
          />
          <div className={styles.actions}>
            <Button type="button" variant="ghost" onClick={close} disabled={submitting}>
              Cancelar
            </Button>
            <Button type="submit" variant="danger" disabled={submitting || password === ''}>
              {submitting ? 'Excluindo…' : 'Excluir definitivamente'}
            </Button>
          </div>
        </form>
      </Modal>
    </div>
  )
}
