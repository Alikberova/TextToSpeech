import { createHomeFixture, providerKeyWithModel, SELECTOR_VOICE_SELECT, ATTR_ARIA_DISABLED } from "./home.page.spec-setup";

describe('HomePage - Language dropdown behavior (Narakeet only)', () => {

  it('voice dropdown disabled until provider selected and enables after provider/language as needed', async () => {
    const { fixture, component } = await createHomeFixture();
    const voiceSelect = fixture.nativeElement.querySelector(SELECTOR_VOICE_SELECT);
    expect(voiceSelect.getAttribute(ATTR_ARIA_DISABLED)).toBe('true');
    // No provider -> voicesForProvider empty
    expect(component.voicesForProvider().length).toBe(0);

    // Select provider OpenAI -> enabled and options available
    const providerWithModel = providerKeyWithModel();
    component.onProviderChange(providerWithModel);
    fixture.detectChanges();
    const voiceSelectAfter = fixture.nativeElement.querySelector(SELECTOR_VOICE_SELECT);
    expect(voiceSelectAfter.getAttribute(ATTR_ARIA_DISABLED)).toBe('false');
    expect(component.voicesForProvider().length).toBeGreaterThan(0);
  });

});
