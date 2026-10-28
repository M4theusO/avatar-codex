import { Search } from 'lucide-react'
import { useId } from 'react'
import styles from './Field.module.css'

interface SearchInputProps {
  label: string
  value: string
  onChange: (value: string) => void
  placeholder?: string
  autoFocus?: boolean
}

export function SearchInput({ label, value, onChange, placeholder, autoFocus }: SearchInputProps) {
  const id = useId()
  return (
    <div className={styles.field}>
      <label htmlFor={id} className="visually-hidden">
        {label}
      </label>
      <div className={styles.searchWrapper}>
        <Search size={18} className={styles.searchIcon} aria-hidden="true" />
        <input
          id={id}
          type="search"
          className={`${styles.control} ${styles.search}`}
          value={value}
          onChange={(event) => onChange(event.target.value)}
          placeholder={placeholder}
          autoComplete="off"
          autoFocus={autoFocus}
        />
      </div>
    </div>
  )
}

export interface SelectOption {
  value: string
  label: string
}

interface SelectProps {
  label: string
  value: string
  options: SelectOption[]
  onChange: (value: string) => void
  /** Rótulo da opção vazia (sem filtro). Omitir quando sempre houver um valor. */
  emptyLabel?: string
}

export function Select({ label, value, options, onChange, emptyLabel }: SelectProps) {
  const id = useId()
  return (
    <div className={styles.field}>
      <label htmlFor={id} className={styles.label}>
        {label}
      </label>
      <select
        id={id}
        className={styles.control}
        value={value}
        onChange={(event) => onChange(event.target.value)}
      >
        {emptyLabel !== undefined && <option value="">{emptyLabel}</option>}
        {options.map((option) => (
          <option key={option.value} value={option.value}>
            {option.label}
          </option>
        ))}
      </select>
    </div>
  )
}
