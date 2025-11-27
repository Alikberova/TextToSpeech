import { buildDownloadFilename } from '../home.helpers';

describe('home.helpers - buildDownloadFilename', () => {
  it('replaces extension when present', () => {
    expect(buildDownloadFilename('file.txt', 'mp3')).toBe('file.mp3');
  });

  it('only replaces the last extension segment', () => {
    expect(buildDownloadFilename('archive.tar.gz', 'wav')).toBe('archive.tar.wav');
  });

  it('adds extension when original has none', () => {
    expect(buildDownloadFilename('readme', 'aac')).toBe('readme.aac');
  });
});
