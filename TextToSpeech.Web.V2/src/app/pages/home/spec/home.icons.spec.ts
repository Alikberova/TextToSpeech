import { createHomeFixture } from './home.page.spec-setup';

describe('HomePage - Icon mapping', () => {
  it('progressIcon maps statuses to material icons', async () => {
    const { component } = await createHomeFixture();
    const cases: { status: string; icon: string }[] = [
      { status: 'Idle', icon: 'info' },
      { status: 'Created', icon: 'schedule' },
      { status: 'Processing', icon: 'autorenew' },
      { status: 'Completed', icon: 'check_circle' },
      { status: 'Failed', icon: 'error' },
      { status: 'Canceled', icon: 'cancel' },
    ];
    for (const c of cases) {
      // @ts-expect-error test narrowing
      component.status.set(c.status);
      expect(component.progressIcon()).toBe(c.icon);
    }
  });
});

