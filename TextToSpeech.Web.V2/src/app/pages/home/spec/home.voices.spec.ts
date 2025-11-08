import { createHomeFixture, providerKeyWithModel } from "./home.page.spec-setup";

describe('HomePage - Language dropdown behavior (Narakeet only)', () => {

  it('voice dropdown disabled until provider selected and enables after provider/language as needed', async () => {
    const selector = 'mat-select[name="voice"]';
    const { fixture, component } = await createHomeFixture();
    const voiceSelect = fixture.nativeElement.querySelector(selector);
    expect(voiceSelect.getAttribute('aria-disabled')).toBe('true');
    // No provider -> voicesForProvider empty
    expect(component.voicesForProvider().length).toBe(0);

    // Select provider OpenAI -> enabled and options available
    const providerWithModel = providerKeyWithModel();
    component.onProviderChange(providerWithModel);
    fixture.detectChanges();
    const voiceSelectAfter = fixture.nativeElement.querySelector(selector);
    expect(voiceSelectAfter.getAttribute('aria-disabled')).toBe('false');
    expect(component.voicesForProvider().length).toBeGreaterThan(0);
  });

});
