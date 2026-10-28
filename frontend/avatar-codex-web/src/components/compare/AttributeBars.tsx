import { Crown } from 'lucide-react'
import type { AttributeComparison, ComparedCharacter } from '../../types/content'
import styles from './Compare.module.css'

interface AttributeBarsProps {
  characters: ComparedCharacter[]
  attributes: AttributeComparison[]
}

const seriesClass = [styles.seriesA, styles.seriesB]

/** Barras pareadas por atributo, com o valor escrito e o maior valor destacado (decidido pela API). */
export default function AttributeBars({ characters, attributes }: AttributeBarsProps) {
  return (
    <ul className={styles.bars}>
      {attributes.map((attribute) => (
        <li key={attribute.id} className={styles.attribute}>
          <div className={styles.attributeHeader}>
            <h3 className={styles.attributeName}>{attribute.name}</h3>
            <p className={styles.attributeDescription}>{attribute.description}</p>
          </div>
          {characters.map((character, index) => {
            const value = attribute.values[index]
            const leads = attribute.leaderId === character.id
            return (
              <div key={character.id} className={`${styles.barRow} ${seriesClass[index]}`}>
                <span className={styles.barLabel}>{character.name}</span>
                <div className={styles.track} aria-hidden="true">
                  {value !== null && <div className={styles.fill} style={{ width: `${value}%` }} />}
                </div>
                <span className={`${styles.barValue} ${leads ? styles.leader : ''}`}>
                  {value === null ? (
                    'Sem estimativa'
                  ) : (
                    <>
                      {leads && <Crown size={14} aria-hidden="true" />}
                      {value}
                      {leads && <span className="visually-hidden"> (maior valor)</span>}
                    </>
                  )}
                </span>
              </div>
            )
          })}
        </li>
      ))}
    </ul>
  )
}
