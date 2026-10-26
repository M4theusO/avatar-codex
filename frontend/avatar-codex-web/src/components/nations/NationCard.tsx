import { Users } from 'lucide-react'
import { Link } from 'react-router'
import type { Nation } from '../../types/content'
import { elementStyle } from '../../utils/labels'
import ElementIcon from '../common/ElementIcon'
import styles from './NationCard.module.css'

export default function NationCard({ nation }: { nation: Nation }) {
  const { id, name, description, symbolImage, element, characterCount } = nation

  return (
    <article className={styles.card} style={elementStyle(element?.color)}>
      <div className={styles.symbol}>
        {symbolImage ? (
          <img src={symbolImage.url} alt="" />
        ) : (
          <ElementIcon icon={element?.icon} size={28} />
        )}
      </div>
      <h2 className={styles.name}>
        <Link to={`/nations/${id}`} className={styles.link}>
          {name}
        </Link>
      </h2>
      <p className={styles.description}>{description}</p>
      <p className={styles.meta}>
        <Users size={16} aria-hidden="true" />
        {characterCount} {characterCount === 1 ? 'personagem' : 'personagens'}
      </p>
    </article>
  )
}
