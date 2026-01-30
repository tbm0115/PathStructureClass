class PathMatchDto {
  /**
   * @param {object} options
   * @param {string} options.name
   * @param {string} options.pattern
   * @param {string} options.matchedValue
   * @param {number} options.matchLength
   * @param {string} options.flavorTextTemplate
   * @param {string|null} options.backgroundColor
   * @param {string|null} options.foregroundColor
   * @param {string|null} options.icon
   * @param {boolean} options.isRequired
   * @param {PathMatchDto[]} [options.childMatches]
   */
  constructor({
    name,
    pattern,
    matchedValue,
    matchLength,
    flavorTextTemplate,
    backgroundColor,
    foregroundColor,
    icon,
    isRequired,
    childMatches = []
  }) {
    /** @type {string} */
    this.name = name;
    /** @type {string} */
    this.pattern = pattern;
    /** @type {string} */
    this.matchedValue = matchedValue;
    /** @type {number} */
    this.matchLength = matchLength;
    /** @type {string} */
    this.flavorTextTemplate = flavorTextTemplate;
    /** @type {string|null} */
    this.backgroundColor = backgroundColor;
    /** @type {string|null} */
    this.foregroundColor = foregroundColor;
    /** @type {string|null} */
    this.icon = icon;
    /** @type {boolean} */
    this.isRequired = isRequired;
    /** @type {PathMatchDto[]} */
    this.childMatches = childMatches;
  }
}

module.exports = { PathMatchDto };
