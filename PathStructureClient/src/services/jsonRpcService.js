const net = require('net');
const { EventEmitter } = require('events');

class JsonRpcService extends EventEmitter {
  constructor({ host, port }) {
    super();
    this.host = host;
    this.port = port;
    this.socket = null;
    this.buffer = '';
    this.nextId = 1;
    this.pending = new Map();
  }

  connect() {
    if (this.socket) {
      return;
    }

    this.socket = net.createConnection({ host: this.host, port: this.port }, () => {
      this.emit('connected');
    });

    this.socket.on('data', (chunk) => this.handleChunk(chunk));
    this.socket.on('close', () => this.handleDisconnect());
    this.socket.on('error', (error) => this.handleError(error));
  }

  isConnected() {
    return Boolean(this.socket && this.socket.readyState === 'open');
  }

  disconnect() {
    if (!this.socket) {
      return;
    }
    this.socket.destroy();
    this.socket = null;
    this.buffer = '';
    this.rejectPending(new Error('Disconnected from JSON-RPC host.'));
  }

  sendRequest(method, params) {
    const id = String(this.nextId++);
    const payload = {
      jsonrpc: '2.0',
      id,
      method,
      params: params ?? {}
    };

    return new Promise((resolve, reject) => {
      if (!this.socket) {
        reject(new Error('JSON-RPC socket is not connected.'));
        return;
      }

      this.pending.set(id, { resolve, reject });
      this.socket.write(`${JSON.stringify(payload)}\n`, 'utf8');
    });
  }

  handleChunk(chunk) {
    this.buffer += chunk.toString();
    const lines = this.buffer.split('\n');
    this.buffer = lines.pop() || '';

    for (const line of lines) {
      if (!line.trim()) {
        continue;
      }
      try {
        const payload = JSON.parse(line);
        this.handlePayload(payload);
      } catch (error) {
        this.emit('parseError', error);
      }
    }
  }

  handlePayload(payload) {
    if (payload?.method) {
      this.emit('notification', payload);
      return;
    }

    if (payload?.id) {
      const pending = this.pending.get(String(payload.id));
      if (!pending) {
        return;
      }
      this.pending.delete(String(payload.id));
      if (payload.error) {
        pending.reject(payload.error);
      } else {
        pending.resolve(payload.result);
      }
    }
  }

  handleDisconnect() {
    if (!this.socket) {
      return;
    }
    this.socket = null;
    this.buffer = '';
    this.rejectPending(new Error('JSON-RPC socket closed.'));
    this.emit('disconnected');
  }

  handleError(error) {
    if (this.socket) {
      this.socket.destroy();
      this.socket = null;
    }
    this.buffer = '';
    this.rejectPending(error);
    this.emit('error', error);
  }

  rejectPending(error) {
    if (this.pending.size === 0) {
      return;
    }
    for (const pending of this.pending.values()) {
      pending.reject(error);
    }
    this.pending.clear();
  }
}

module.exports = { JsonRpcService };
