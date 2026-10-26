import type { Ability } from '../../types/content'
import { elementStyle } from '../../utils/labels'
import ElementIcon from '../common/ElementIcon'
import styles from './DetailLists.module.css'

export default function AbilityList({ abilities }: { abilities: Ability[] }) {
  return (
    <ul className={styles.cardGrid}>
      {abilities.map((ability) => (
        <li
          key={ability.id}
          className={styles.ability}
          style={elementStyle(ability.element?.color)}
        >
          <h3 className={styles.abilityName}>
            <ElementIcon icon={ability.element?.icon} size={18} className={styles.abilityIcon} />
            {ability.name}
          </h3>
          <p className={styles.muted}>{ability.description}</p>
        </li>
      ))}
    </ul>
  )
}
