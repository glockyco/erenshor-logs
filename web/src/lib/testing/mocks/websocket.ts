/**
 * WebSocket mock for testing. Separates mock instance from test control.
 * Follows YAGNI principle - minimal but sufficient implementation.
 */

export interface WebSocketMock {
  /** The mock WebSocket instance to pass to code under test */
  socket: WebSocket;
  /** Trigger events for testing (generic handler for all event types) */
  trigger: (event: string, data?: unknown) => void;
  /** Access sent messages for assertions */
  sent: string[];
}

/**
 * Create a mock WebSocket with separate control interface.
 *
 * @example
 * const { socket, trigger, sent } = createMockWebSocket();
 *
 * // Pass socket to code under test
 * const client = new WebSocketClient(socket);
 *
 * // Simulate server events
 * trigger('open');
 * trigger('message', { data: '{"type":"handshake"}' });
 * trigger('close', { code: 1000, wasClean: true });
 *
 * // Assert on sent messages
 * expect(sent).toHaveLength(1);
 */
export function createMockWebSocket(): WebSocketMock {
  type EventHandler = (event: Event) => void;
  const listeners = new Map<string, Set<EventHandler>>();
  const sent: string[] = [];

  const socket = {
    addEventListener(event: string, handler: EventHandler) {
      if (!listeners.has(event)) {
        listeners.set(event, new Set());
      }
      listeners.get(event)!.add(handler);
    },
    removeEventListener(event: string, handler: EventHandler) {
      listeners.get(event)?.delete(handler);
    },
    send(data: string) {
      sent.push(data);
    },
    close() {
      // Implemented via trigger('close')
    },
    readyState: WebSocket.OPEN,
    url: "ws://localhost:38729",
    protocol: "",
    bufferedAmount: 0,
    extensions: "",
    binaryType: "blob" as BinaryType,
    onopen: null,
    onmessage: null,
    onerror: null,
    onclose: null,
    dispatchEvent: () => false,
    CONNECTING: WebSocket.CONNECTING,
    OPEN: WebSocket.OPEN,
    CLOSING: WebSocket.CLOSING,
    CLOSED: WebSocket.CLOSED,
  } as WebSocket;

  const trigger = (event: string, data?: unknown) => {
    const eventData = (data as Event) || ({ type: event } as Event);
    listeners.get(event)?.forEach((handler) => handler(eventData));

    // Also trigger on* properties if set
    switch (event) {
      case "open":
        socket.onopen?.(eventData);
        break;
      case "message":
        socket.onmessage?.(eventData as MessageEvent);
        break;
      case "error":
        socket.onerror?.(eventData);
        break;
      case "close":
        (socket as unknown as { readyState: number }).readyState = WebSocket.CLOSED;
        socket.onclose?.(eventData as CloseEvent);
        break;
    }
  };

  return { socket, trigger, sent };
}
