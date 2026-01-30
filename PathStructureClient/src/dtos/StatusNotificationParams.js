class StatusNotificationParams {
  /**
   * @param {object} options
   * @param {string} options.message
   * @param {string} options.state
   * @param {string} options.timestamp
   */
  constructor({ message, state, timestamp }) {
    /** @type {string} */
    this.message = message;
    /** @type {string} */
    this.state = state;
    /** @type {string} */
    this.timestamp = timestamp;
  }
}

module.exports = { StatusNotificationParams };
