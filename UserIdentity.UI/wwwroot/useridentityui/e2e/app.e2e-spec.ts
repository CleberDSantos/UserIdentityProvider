///<reference path="../node_modules/@types/jasmine/index.d.ts"/>

import { AppPage } from './app.po';

describe('useridentityui App', () => {
  let page: AppPage;

  beforeEach(() => {
    page = new AppPage();
  });

  it('should display welcome message', () => {
    page.navigateTo();
    expect(page.getParagraphText()).toEqual('Welcome to app!');
  });
});
