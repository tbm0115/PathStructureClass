class WatcherAbortedNotificationParams {
  /**
   * @param {object} options
   * @param {string} options.message
   * @param {string} [options.error]
   * @param {string} options.timestamp
   */
  constructor({ message, error, timestamp }) {
    /** @type {string} */
    this.message = message;
    /** @type {string|undefined} */
    this.error = error;
    /** @type {string} */
    this.timestamp = timestamp;
  }
}

module.exports = { WatcherAbortedNotificationParams };
