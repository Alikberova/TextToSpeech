import { SignalRService } from './signalr.service';
import * as signalR from '@microsoft/signalr';
import { createHubWithOnSpy, setSignalRHub } from '../../../testing/signalr-test-utils';
import { AudioStatusCallback } from './audio-status-callback';
import { describe, expect, it, vi } from 'vitest';

describe('SignalRService', () => {

    const fileId = 'id-123';

    it('registers AudioStatusUpdated listener and normalizes progress', () => {
        const service = new SignalRService();
        let registeredCallback: AudioStatusCallback | undefined;
        const onSpy = vi.fn().mockImplementation((evt: string, cb: AudioStatusCallback) => {
            if (evt === 'AudioStatusUpdated') {
                registeredCallback = cb;
            }
        });

        const partialHub = createHubWithOnSpy(onSpy);
        setSignalRHub(service, partialHub);

        const calls: {
            id: string;
            status: string;
            progress: number | null;
            error?: string;
        }[] = [];
        service.addAudioStatusListener((id, st, pr, err) => calls.push({ id, status: st, progress: pr, error: err }));

        expect(onSpy).toHaveBeenCalledWith('AudioStatusUpdated', expect.any(Function));

        // Simulate hub events
        registeredCallback!(fileId, 'Processing', 42, undefined);
        registeredCallback!('id-2', 'Processing', undefined as unknown as number, 'bad');

        expect(calls.length).toBe(2);
        expect(calls[0]).toEqual({ id: fileId, status: 'Processing', progress: 42, error: undefined });
        expect(calls[1]).toEqual({ id: 'id-2', status: 'Processing', progress: null, error: 'bad' });
    });

    it('invokes CancelProcessing on cancel()', async () => {
        const service = new SignalRService();
        const fakeHub = {
            invoke: vi.fn().mockReturnValue(Promise.resolve())
        } as unknown as signalR.HubConnection;
        setSignalRHub(service, fakeHub);

        service.cancelProcessing(fileId);
        expect(fakeHub.invoke).toHaveBeenCalledWith('CancelProcessing', fileId);
    });

    it('stops and clears hub on stopConnection()', async () => {
        const service = new SignalRService();
        const fakeHub = {
            stop: vi.fn().mockReturnValue(Promise.resolve())
        } as unknown as signalR.HubConnection;
        setSignalRHub(service, fakeHub);

        service.stopConnection();
        expect(fakeHub.stop).toHaveBeenCalled();
        expect((service as unknown as {
            hub?: signalR.HubConnection;
        }).hub).toBeUndefined();
    });

    it('cancel() is a no-op when hub is not started', () => {
        const service = new SignalRService();
        expect(() => service.cancelProcessing(fileId)).not.toThrow();
    });

    it('supports multiple listeners', () => {
        const service = new SignalRService();
        const callbacks: AudioStatusCallback[] = [];
        const multipleHub: Partial<signalR.HubConnection> = {
            on: ((_event: string, fn: AudioStatusCallback) => {
                callbacks.push(fn);
            }) as unknown as signalR.HubConnection['on']
        };
        setSignalRHub(service, multipleHub);

        const firstListener = vi.fn();
        const secondListener = vi.fn();

        service.addAudioStatusListener(firstListener);
        service.addAudioStatusListener(secondListener);

        callbacks.forEach(cb => cb(fileId, 'Processing', 1, undefined));

        expect(firstListener).toHaveBeenCalledWith(fileId, 'Processing', 1, undefined);
        expect(secondListener).toHaveBeenCalledWith(fileId, 'Processing', 1, undefined);
    });
});
