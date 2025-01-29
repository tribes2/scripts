// New function which enables 'isWatchOnly' for clients with the default / secret code.
function serverCmdWatchOnly(%client, %pass){
   if($Host::ObserverOnlyPass $= ""){
      $Host::ObserverOnlyPass = "ImaWatcher";// set a default one if not defined
   }
   if(%pass $= $Host::ObserverOnlyPass){
      %client.isWatchOnly = 1;
   }
}

// Annoying 'user is watching you' message and kick timeout is suppressed for admins
// This edit pretends the user is an admin for two observer mode changes
// This functionality can't be abused to perform admin actions because the engine only lets one function run at a time
// cmdAutoKickObserver automatically changes the verification key for cmdAutoKickObserver, preventing it from kicking isWatchOnly clients.

package SuppressNoisyObserverMessages {
    function cmdAutoKickObserver(%client, %key)
    {
        if (%client.isWatchOnly) %client.okkey = mFloor(getRandom()*1000);
        parent::cmdAutoKickObserver(%client, %key);
    }

    function DefaultGame::forceObserver( %game, %client, %reason )
    {
        if (%client.isWatchOnly) %client.isAdmin = true;
        parent::forceObserver(%game, %client, %reason);
        if (%client.isWatchOnly) %client.isAdmin = false;
    }

    function Observer::onTrigger(%data,%obj,%trigger,%state)
    {
        %client = %obj.getControllingClient();
        if (%client.isWatchOnly) %client.isAdmin = true;
        parent::onTrigger(%data, %obj, %trigger, %state);
        if (%client.isWatchOnly) %client.isAdmin = false;
    }
};

function makeSureSuppressNoisyObserverComesLast() {
    activatePackage(SuppressNoisyObserverMessages);
}

schedule(1000, 0, "makeSureSuppressNoisyObserverComesLast");
