import { describe, expect, it } from 'vitest';

import { AppComponent } from './app.component';

describe('AppComponent', () => {
  it('can be instantiated', () => {
    expect(new AppComponent()).toBeInstanceOf(AppComponent);
  });
});
