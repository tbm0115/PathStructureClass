/**
 * @template TResult
 */
class JsonRpcResponse {
  /**
   * @param {object} options
   * @param {string|null} options.id
   * @param {TResult} options.result
   * @param {string} [options.jsonrpc]
   */
  constructor({ id, result, jsonrpc = '2.0' }) {
    /** @type {string} */
    this.jsonrpc = jsonrpc;
    /** @type {string|null} */
    this.id = id;
    /** @type {TResult} */
    this.result = result;
  }
}

module.exports = { JsonRpcResponse };
