var simpleservice = require("../src/index");

simpleservice.listen(3333, function(err) {
    if (err)
        console.error("failed to listen on port 3333");
    else
        console.log("listening on port 3333");
});