class JsonRpcErrorDetails {
  /**
   * @param {object} options
   * @param {number} options.code
   * @param {string} options.message
   * @param {unknown} [options.data]
   */
  constructor({ code, message, data }) {
    /** @type {number} */
    this.code = code;
    /** @type {string} */
    this.message = message;
    /** @type {unknown} */
    this.data = data;
  }
}

class JsonRpcErrorResponse {
  /**
   * @param {object} options
   * @param {string|null} options.id
   * @param {JsonRpcErrorDetails} options.error
   * @param {string} [options.jsonrpc]
   */
  constructor({ id, error, jsonrpc = '2.0' }) {
    /** @type {string} */
    this.jsonrpc = jsonrpc;
    /** @type {string|null} */
    this.id = id;
    /** @type {JsonRpcErrorDetails} */
    this.error = error;
  }
}

module.exports = { JsonRpcErrorResponse, JsonRpcErrorDetails };
