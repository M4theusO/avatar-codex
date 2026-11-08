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

  it('vai do personagem para a habilidade e vê quem a domina', () => {
    // Com login (Painel + área do usuário), a navbar completa cabe em 1280px sem rolagem lateral.
    cy.visit('/abilities')
    cy.findByRole('link', { name: /meu perfil/ }).should('be.visible')
    cy.document().then((doc) => {
      expect(doc.documentElement.scrollWidth).to.be.at.most(doc.documentElement.clientWidth)
    })
    cy.findByRole('region', { name: 'Fogo' }).should('be.visible')

    cy.characterId('Zuko').then((id) => cy.visit(`/characters/${id}`))
    cy.dismissLevelUp()
    cy.findByRole('link', { name: 'Redirecionamento de relâmpago' }).click()

    cy.findByRole('heading', { level: 1, name: 'Redirecionamento de relâmpago' }).should(
      'be.visible',
    )
    cy.findByRole('heading', { name: /Quem domina/ }).should('be.visible')
    cy.findByRole('link', { name: /Iroh/ }).should('be.visible')
  })
})
