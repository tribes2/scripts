// #autoload
// #name = ignore-server
// #date = October 14th, 2025
// #author = loop
// #warrior = looop
// #email = loop@tribesforever.com
// #description = Prevent Tribes 2 from querying a server based on a partial IP match. Compatible with RC2a and QoL

if (isObject($ignoredServer) || $ignoredServer !$= "0.0.0.0") {
    return;
}
$ignoredServer = "0.0.0.0"; // Replace me

package ServerQueryIgnore {
    function TNbite::onLine(%this, %line) {
        if (strpos(%line, $ignoredServer) != -1) {
            echo("Suppressing query for " @ $ignoredServer);
            return;
        }
        parent::onLine(%this, %line);
    }

    function ServerList::onLine(%this, %line) {
        if (strpos(%line, $ignoredServer) != -1) {
            echo("Suppressing query for " @ $ignoredServer);
            return;
        }
        parent::onLine(%this, %line);
    }
};

echo("Ignoring server " @ $ignoredServer);
activatePackage(ServerQueryIgnore);
