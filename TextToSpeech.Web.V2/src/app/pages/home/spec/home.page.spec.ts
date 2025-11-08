import { createHomeFixture } from './home.page.spec-setup';

describe('HomePage (smoke)', () => {
  it('creates component', async () => {
    const { component } = await createHomeFixture();
    expect(component).toBeTruthy();
  });
});
