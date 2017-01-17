/**
     __________ ____
    / ___/ ___// __ \
   \__ \\__ \/ /_/ /
  ___/ /__/ / ____/
 /____/____/_/

 @author Alan Doherty
 @purpose Hosts services for consumers.
 */

var utils = require("../utils");

// export
module.exports = utils.class_("Host", {
    /**
     * @private
     */
    _host: -1,

    /**
     * @private
     */
    _port: -1,



    constructor: function() {
    }
});