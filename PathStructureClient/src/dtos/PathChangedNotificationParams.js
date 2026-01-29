const { PathMatchDto } = require('./PathMatchDto');

class PathChangedNotificationParams {
  /**
   * @param {object} options
   * @param {string} options.message
   * @param {string} options.path
   * @param {PathMatchDto|null} options.currentMatch
   * @param {Record<string, string>} options.variables
   * @param {PathMatchDto[]} options.matches
   * @param {PathMatchDto[]} options.immediateChildMatches
   * @param {string} options.timestamp
   */
  constructor({
    message,
    path,
    currentMatch,
    variables,
    matches,
    immediateChildMatches,
    timestamp
  }) {
    /** @type {string} */
    this.message = message;
    /** @type {string} */
    this.path = path;
    /** @type {PathMatchDto|null} */
    this.currentMatch = currentMatch;
    /** @type {Record<string, string>} */
    this.variables = variables;
    /** @type {PathMatchDto[]} */
    this.matches = matches;
    /** @type {PathMatchDto[]} */
    this.immediateChildMatches = immediateChildMatches;
    /** @type {string} */
    this.timestamp = timestamp;
  }
}

module.exports = { PathChangedNotificationParams };
