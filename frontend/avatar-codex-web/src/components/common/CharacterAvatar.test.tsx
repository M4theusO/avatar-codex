import { fireEvent, render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import CharacterAvatar from './CharacterAvatar'

const fire = { id: 2, name: 'Fogo', color: '#B86B35', icon: 'flame' }
const image = { url: 'https://example.com/zuko.webp', sourceUrl: null, credit: null }

describe('CharacterAvatar', () => {
  it('mostra as iniciais e o símbolo do elemento quando não há imagem', () => {
    const { container } = render(
      <CharacterAvatar name="Toph Beifong" image={null} element={fire} />,
    )

    expect(screen.getByText('TB')).toBeInTheDocument()
    expect(container.querySelector('svg')).toBeInTheDocument()
    expect(container.querySelector('img')).not.toBeInTheDocument()
  })

  it('não mostra símbolo para quem não tem elemento', () => {
    const { container } = render(<CharacterAvatar name="Sokka" image={null} element={null} />)

    expect(container.querySelector('svg')).not.toBeInTheDocument()
  })

  it('volta para as iniciais quando a imagem não carrega', () => {
    const { container } = render(<CharacterAvatar name="Zuko" image={image} element={fire} />)
    const img = container.querySelector('img')!
    expect(img).toHaveAttribute('src', image.url)

    fireEvent.error(img)

    expect(container.querySelector('img')).not.toBeInTheDocument()
    expect(screen.getByText('Z')).toBeInTheDocument()
  })
})
