import { Users } from 'lucide-react'
import { Link } from 'react-router'
import type { AbilityListItem } from '../../types/content'
import { characterCountLabel } from '../../utils/abilities'
import { elementStyle } from '../../utils/labels'
import ElementIcon from '../common/ElementIcon'
import styles from './AbilityCard.module.css'

export default function AbilityCard({ ability }: { ability: AbilityListItem }) {
  const { id, name, description, element, characterCount } = ability

  return (
    <article className={styles.card} style={elementStyle(element?.color)}>
      <h3 className={styles.name}>
        <ElementIcon icon={element?.icon} size={20} className={styles.icon} />
        <Link to={`/abilities/${id}`} className={styles.link}>
          {name}
        </Link>
      </h3>
      <p className={styles.description}>{description}</p>
      <p className={styles.meta}>
        <Users size={16} aria-hidden="true" />
        {characterCountLabel(characterCount)}
      </p>
    </article>
  )
}
