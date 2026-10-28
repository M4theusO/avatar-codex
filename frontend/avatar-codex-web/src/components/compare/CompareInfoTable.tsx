import type { ReactNode } from 'react'
import { Link } from 'react-router'
import type { ComparedCharacter } from '../../types/content'
import { statusLabels } from '../../utils/labels'
import { ElementBadge } from '../common/Badge'
import styles from './Compare.module.css'

function kind(character: ComparedCharacter) {
  if (character.isAvatar) return 'Avatar'
  return character.isBender ? 'Dobrador' : 'Não dobrador'
}

function names(items: { id: number; name: string }[]) {
  return items.length === 0 ? (
    <span className={styles.none}>Nenhum</span>
  ) : (
    <ul className={styles.cellList}>
      {items.map((item) => (
        <li key={item.id}>{item.name}</li>
      ))}
    </ul>
  )
}

const rows: { label: string; render: (c: ComparedCharacter) => ReactNode }[] = [
  {
    label: 'Elemento',
    render: (c) => (c.element ? <ElementBadge element={c.element} /> : 'Nenhum'),
  },
  {
    label: 'Nação',
    render: (c) =>
      c.nation ? (
        <Link to={`/nations/${c.nation.id}`} className={styles.cellLink}>
          {c.nation.name}
        </Link>
      ) : (
        <span className={styles.none}>Sem nação</span>
      ),
  },
  { label: 'Tipo', render: kind },
  { label: 'Situação', render: (c) => statusLabels[c.status] },
  {
    label: 'Primeira aparição',
    render: (c) => c.firstAppearance ?? <span className={styles.none}>Não informada</span>,
  },
  { label: 'Séries', render: (c) => names(c.series) },
  { label: 'Subtipos de dobra', render: (c) => names(c.bendingSubtypes) },
  { label: 'Habilidades', render: (c) => names(c.abilities) },
]

/** Informações lado a lado, uma linha por característica. */
export default function CompareInfoTable({ characters }: { characters: ComparedCharacter[] }) {
  return (
    <div className={styles.tableWrapper}>
      <table className={styles.table}>
        <caption className="visually-hidden">Informações dos personagens lado a lado</caption>
        <thead>
          <tr>
            <td />
            {characters.map((character) => (
              <th key={character.id} scope="col">
                <Link to={`/characters/${character.id}`} className={styles.cellLink}>
                  {character.name}
                </Link>
              </th>
            ))}
          </tr>
        </thead>
        <tbody>
          {rows.map((row) => (
            <tr key={row.label}>
              <th scope="row">{row.label}</th>
              {characters.map((character) => (
                <td key={character.id}>{row.render(character)}</td>
              ))}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}
