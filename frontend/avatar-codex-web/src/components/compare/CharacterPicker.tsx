import { useState } from 'react'
import { useCharacter, useCharacters } from '../../hooks/useContent'
import { useDebouncedValue } from '../../hooks/useDebouncedValue'
import { Button } from '../common/Button'
import CharacterAvatar from '../common/CharacterAvatar'
import { SearchInput } from '../common/Field'
import Skeleton from '../common/Skeleton'
import styles from './Compare.module.css'

const SEARCH_DEBOUNCE_MS = 300
const RESULT_COUNT = 6

interface CharacterPickerProps {
  /** Posição na comparação: define a cor da série (A = Água, B = Fogo). */
  slot: 'a' | 'b'
  label: string
  selectedId?: number
  /** Personagem já escolhido do outro lado: não aparece nos resultados. */
  excludeId?: number
  onSelect: (id: number) => void
  onClear: () => void
}

export default function CharacterPicker({
  slot,
  label,
  selectedId,
  excludeId,
  onSelect,
  onClear,
}: CharacterPickerProps) {
  // Só foca a busca quando a pessoa pediu para trocar, nunca ao abrir a página.
  const [focusSearch, setFocusSearch] = useState(false)

  return (
    <fieldset className={`${styles.picker} ${slot === 'a' ? styles.seriesA : styles.seriesB}`}>
      <legend className={styles.pickerLegend}>
        <span className={styles.swatch} aria-hidden="true" />
        {label}
      </legend>
      {selectedId !== undefined ? (
        <SelectedCharacter
          id={selectedId}
          label={label}
          onClear={() => {
            setFocusSearch(true)
            onClear()
          }}
        />
      ) : (
        <CharacterSearch
          label={label}
          excludeId={excludeId}
          autoFocus={focusSearch}
          onSelect={onSelect}
        />
      )}
    </fieldset>
  )
}

function SelectedCharacter({
  id,
  label,
  onClear,
}: {
  id: number
  label: string
  onClear: () => void
}) {
  const { data, isPending, isError } = useCharacter(id)

  return (
    <div className={styles.selected}>
      {isPending ? (
        <Skeleton height="3rem" width="70%" />
      ) : isError ? (
        <p className={styles.pickerMessage}>Personagem não encontrado.</p>
      ) : (
        <div className={styles.selectedInfo}>
          <CharacterAvatar name={data.name} image={data.image} element={data.element} />
          <span className={styles.selectedName}>{data.name}</span>
        </div>
      )}
      <Button variant="ghost" onClick={onClear} aria-label={`Trocar ${label.toLowerCase()}`}>
        Trocar
      </Button>
    </div>
  )
}

function CharacterSearch({
  label,
  excludeId,
  autoFocus,
  onSelect,
}: {
  label: string
  excludeId?: number
  autoFocus: boolean
  onSelect: (id: number) => void
}) {
  const [text, setText] = useState('')
  const search = useDebouncedValue(text.trim(), SEARCH_DEBOUNCE_MS)
  // Um a mais que o exibido, para ainda sobrarem RESULT_COUNT sem o personagem do outro lado.
  const { data, isPending, isError, refetch } = useCharacters({
    search: search || undefined,
    pageSize: RESULT_COUNT + 1,
  })
  const results = (data?.items ?? []).filter((c) => c.id !== excludeId).slice(0, RESULT_COUNT)

  return (
    <div className={styles.search}>
      <SearchInput
        label={`Buscar ${label.toLowerCase()}`}
        value={text}
        onChange={setText}
        placeholder="Buscar pelo nome"
        autoFocus={autoFocus}
      />
      {isPending ? (
        <p className={styles.pickerMessage} role="status">
          Carregando personagens…
        </p>
      ) : isError ? (
        <p className={styles.pickerMessage} role="alert">
          Não foi possível buscar.{' '}
          <button type="button" className={styles.inlineButton} onClick={() => refetch()}>
            Tentar novamente
          </button>
        </p>
      ) : results.length === 0 ? (
        <p className={styles.pickerMessage} role="status">
          Nenhum personagem encontrado.
        </p>
      ) : (
        <ul className={styles.results} aria-label={`Resultados para ${label.toLowerCase()}`}>
          {results.map((character) => (
            <li key={character.id}>
              <button
                type="button"
                className={styles.result}
                onClick={() => onSelect(character.id)}
              >
                <CharacterAvatar
                  name={character.name}
                  image={character.image}
                  element={character.element}
                  size="sm"
                />
                <span className={styles.resultName}>{character.name}</span>
                <span className={styles.resultMeta}>
                  {character.element?.name ?? 'Não dobrador'}
                </span>
              </button>
            </li>
          ))}
        </ul>
      )}
    </div>
  )
}
