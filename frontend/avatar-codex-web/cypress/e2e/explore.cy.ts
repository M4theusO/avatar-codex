// Fluxos 3, 4, 5 e 7: pesquisar, abrir e favoritar um personagem, ganhando XP pela descoberta.
describe('Explorar personagens', () => {
  beforeEach(() => {
    cy.newUser().then((user) => cy.registerViaApi(user))
  })

  it('pesquisa, abre o personagem, ganha XP pela descoberta e favorita', () => {
    cy.visit('/characters')
    cy.findByLabelText('Buscar personagem').type('zuko')
    cy.location('search').should('include', 'search=zuko')
    cy.findByText('1 personagem encontrado').should('be.visible')

    cy.findByRole('link', { name: 'Zuko' }).click()
    cy.findByRole('heading', { level: 1, name: 'Zuko' }).should('be.visible')

    // Primeira visita: +10 XP decidido pelo backend.
    cy.findByText(/Nova descoberta — Você descobriu Zuko! \+10 XP/).should('be.visible')
    cy.dismissLevelUp()

    cy.findByRole('button', { name: 'Favoritar' }).click()
    cy.findByRole('button', { name: 'Favoritado' }).should('have.attr', 'aria-pressed', 'true')

    cy.visit('/favorites')
    cy.findByRole('link', { name: 'Zuko' }).should('be.visible')
  })

  it('a segunda visita ao mesmo personagem não dá XP de novo', () => {
    cy.characterId('Katara').then((id) => {
      cy.visit(`/characters/${id}`)
      cy.findByText(/Você descobriu Katara! \+10 XP/).should('be.visible')

      cy.reload()
      cy.findByRole('heading', { level: 1, name: 'Katara' }).should('be.visible')
      cy.findByText('Descoberto').should('be.visible')
      cy.findByText(/Você descobriu Katara/).should('not.exist')
    })
  })
})
