import { Eye, EyeOff } from 'lucide-react'
import { useId, useState, type InputHTMLAttributes } from 'react'
import fieldStyles from './Field.module.css'
import styles from './TextField.module.css'

interface TextFieldProps extends Omit<InputHTMLAttributes<HTMLInputElement>, 'id'> {
  label: string
  error?: string
  hint?: string
}

/** Input com label, dica e erro associados por `aria-describedby`. Senhas ganham botão de mostrar. */
export default function TextField({ label, error, hint, type = 'text', ...props }: TextFieldProps) {
  const id = useId()
  const [revealed, setRevealed] = useState(false)
  const isPassword = type === 'password'
  const describedBy = [hint && `${id}-hint`, error && `${id}-error`].filter(Boolean).join(' ')

  return (
    <div className={fieldStyles.field}>
      <label htmlFor={id} className={fieldStyles.label}>
        {label}
      </label>
      <div className={styles.wrapper}>
        <input
          id={id}
          type={isPassword && revealed ? 'text' : type}
          className={`${fieldStyles.control} ${isPassword ? styles.withToggle : ''}`}
          aria-invalid={error ? true : undefined}
          aria-describedby={describedBy || undefined}
          {...props}
        />
        {isPassword && (
          <button
            type="button"
            className={styles.toggle}
            onClick={() => setRevealed((value) => !value)}
            aria-label="Mostrar senha"
            aria-pressed={revealed}
          >
            {revealed ? (
              <EyeOff size={18} aria-hidden="true" />
            ) : (
              <Eye size={18} aria-hidden="true" />
            )}
          </button>
        )}
      </div>
      {hint && (
        <p id={`${id}-hint`} className={styles.hint}>
          {hint}
        </p>
      )}
      {error && (
        <p id={`${id}-error`} className={styles.error}>
          {error}
        </p>
      )}
    </div>
  )
}
