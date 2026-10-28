import type { AttributeComparison, ComparedCharacter } from '../../types/content'
import styles from './Compare.module.css'

interface AttributeRadarProps {
  characters: ComparedCharacter[]
  attributes: AttributeComparison[]
}

// Geometria do SVG (unidades do viewBox). A largura sobra para os rótulos dos eixos.
const WIDTH = 420
const HEIGHT = 330
const CENTER_X = WIDTH / 2
const CENTER_Y = HEIGHT / 2
const RADIUS = 115
const LABEL_OFFSET = 16
const RINGS = [25, 50, 75, 100]

const seriesClass = [styles.seriesA, styles.seriesB]

/** Ponto do eixo `index` (de `count`) para um valor de 0 a 100; o primeiro eixo aponta para cima. */
function point(index: number, count: number, value: number, extra = 0) {
  const angle = (Math.PI * 2 * index) / count - Math.PI / 2
  const distance = (RADIUS * value) / 100 + extra
  return { x: CENTER_X + distance * Math.cos(angle), y: CENTER_Y + distance * Math.sin(angle) }
}

function polygon(values: number[]) {
  return values
    .map((value, index) => point(index, values.length, value))
    .map(({ x, y }) => `${x.toFixed(1)},${y.toFixed(1)}`)
    .join(' ')
}

function anchor(x: number) {
  if (Math.abs(x - CENTER_X) < 1) return 'middle'
  return x > CENTER_X ? 'start' : 'end'
}

/**
 * Visão geral do perfil dos dois personagens. É um resumo visual: os números exatos ficam nas barras
 * (e cada ponto tem uma dica com o valor). Personagem sem estimativas não ganha polígono.
 */
export default function AttributeRadar({ characters, attributes }: AttributeRadarProps) {
  const count = attributes.length
  if (count < 3) return null

  return (
    <svg
      className={styles.radar}
      viewBox={`0 0 ${WIDTH} ${HEIGHT}`}
      role="img"
      aria-label={`Radar dos atributos de ${characters.map((c) => c.name).join(' e ')}. Os valores estão listados nas barras.`}
    >
      <g className={styles.radarGrid}>
        {RINGS.map((ring) => (
          <polygon key={ring} points={polygon(attributes.map(() => ring))} />
        ))}
        {attributes.map((attribute, index) => {
          const end = point(index, count, 100)
          return <line key={attribute.id} x1={CENTER_X} y1={CENTER_Y} x2={end.x} y2={end.y} />
        })}
      </g>

      {characters.map((character, series) => {
        const values = attributes.map((a) => a.values[series])
        if (values.some((v) => v === null)) return null
        const complete = values as number[]
        return (
          <g key={character.id} className={`${styles.radarSeries} ${seriesClass[series]}`}>
            <polygon points={polygon(complete)} />
            {complete.map((value, index) => {
              const { x, y } = point(index, count, value)
              return (
                <circle key={attributes[index].id} cx={x} cy={y} r={4.5}>
                  <title>{`${character.name} · ${attributes[index].name}: ${value}`}</title>
                </circle>
              )
            })}
          </g>
        )
      })}

      {attributes.map((attribute, index) => {
        const { x, y } = point(index, count, 100, LABEL_OFFSET)
        return (
          <text
            key={attribute.id}
            className={styles.radarLabel}
            x={x}
            y={y}
            textAnchor={anchor(x)}
            dominantBaseline="middle"
          >
            {attribute.name}
          </text>
        )
      })}
    </svg>
  )
}
