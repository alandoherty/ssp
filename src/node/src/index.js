/**
     __________ ____
    / ___/ ___// __ \
   \__ \\__ \/ /_/ /
  ___/ /__/ / ____/
 /____/____/_/

 @author Alan Doherty
 @purpose Exports the module.
 */

var Consumer = require("./classes/Consumer"),
    Host = require("./classes/Host");

var ssp = {
    Consumer: Consumer,
    Host: Host,

    /**
     * Creates a new host and connects to the specified port.
     * @param {number} port
     * @param {function} callback
     */
    listen: function(port, callback) {

    },

    /**
     * Creates a new consumer and connects to the specified host and port.
     * @param {string} host
     * @param {number} port
     * @param {function} callback
     */
    connect: function(host, port, callback) {

    }
};

// export
module.exports = ssp;