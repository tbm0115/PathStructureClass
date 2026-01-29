/**
 * @template TParams
 */
class JsonRpcNotification {
  /**
   * @param {object} options
   * @param {string} options.method
   * @param {TParams} options.params
   * @param {string} [options.jsonrpc]
   */
  constructor({ method, params, jsonrpc = '2.0' }) {
    /** @type {string} */
    this.jsonrpc = jsonrpc;
    /** @type {string} */
    this.method = method;
    /** @type {TParams} */
    this.params = params;
  }
}

module.exports = { JsonRpcNotification };
