// #autoload
// #name = auto-record
// #date = January 27th, 2025
// #author = loop
// #warrior = looop
// #email = loop@tribesforever.com
// #description = Allows you to automatically record your perspective for each match you play in. Recordings are stopped on exit or debrief.
// #warning = You should have support.vl2! Without it, your demo files will be missing useful metadata (your name, server name, time of recording) and tribesforever.com won't show it.

// config
$minimumRecordingLengthSecs = 60; // set to zero for no-minimum-length
$minimumRecordingPlayerCount = 3; // set to zero for no-minimum-players
$silenceAutoRecordHudChat = false;
// If your demos don't record because servers have funky names (or you exceed Windows MAX_PATH), turn this off again!
$filenameIncludesServerName = false;


// internal vars, don't change 'em
$onlineCaptureRunning = false;
$shouldCaptureNextPlayWake = false;
$recordingPlayerCountOkay = false;
$recordingStartTime = 0;
$lastRecordingFilename = "";

package OnlineDemoCaptureShim {
    function DebriefGui::onWake( %this ) {
        $shouldCaptureNextPlayWake = true;
        parent::onWake(%this);
    }
    function LoadingGui::onWake(%this) {
        $shouldCaptureNextPlayWake = true;
        parent::onWake(%this);
    }
    function PlayGui::onWake(%this) {
        parent::onWake(%this);
        if ($shouldCaptureNextPlayWake) {
            $shouldCaptureNextPlayWake = false;
            delayedStartOnlineCapture();
        }
    }

    function demoRecordComplete() {
        if ($onlineCaptureRunning && !$silenceAutoRecordHudChat) {
            addMessageHudLine( "\c4Auto-capture stopped.");
        }
        $onlineCaptureRunning = false;
        parent::demoRecordComplete();
        // Check if this demo should be allowed to persist
        if ($minimumRecordingLengthSecs != 0 && $recordingStartTime != 0) {
            %totalRecordingTime = getRealTime() - $recordingStartTime;
            if (%totalRecordingTime / 1000 < $minimumRecordingLengthSecs) {
                deleteLastRecording("too short");
            }
            $recordingStartTime = 0;
        }
        if ($minimumRecordingPlayerCount != 0) {
            cancel($checkAutoRecPlayerCountTimer);
            if ($recordingPlayerCountOkay == false) {
                deleteLastRecording("not enough players, need " @ $minimumRecordingPlayerCount);
            }
        }
    }
};
activatePackage(OnlineDemoCaptureShim);

function deleteLastRecording(%why) {
    addMessageHudLine("\c4Deleting last auto-capture file (" @ %why @ ")");
    echo("Delete last auto-capture: " @ %why);
    deleteFile($lastRecordingFilename);
}

function delayedStartOnlineCapture() {
    cancel($startOnlineCaptureTimer);
    $startOnlineCaptureTimer = schedule(2000, 0, "startOnlineCapture");
}

function checkAutoRecordPlayerCount() {
    if (!isObject(PlayerListGroup)) {
        // probably left the server
        return;
    }
    %playerCount = PlayerListGroup.getCount();
    %botCount = 0;
    %observerCount = 0;
    for (%i = 0; %i < %playerCount; %i++) {
        %player = PlayerListGroup.getObject(%i);
        if (%player.isBot) {
            %botCount += 1;
        }
        //if (%player.teamId == 0) {
        //   %observerCount += 1; // turned out to be unreliable for DM modes? i don't really remember
        //}
    }
    %playerCount = %playerCount - %botCount;
    if (%playerCount >= $minimumRecordingPlayerCount) {
        $recordingPlayerCountOkay = true;
    } else {
        cancel($checkAutoRecPlayerCountTimer);
        $checkAutoRecPlayerCountTimer = schedule(1000, 0, "checkAutoRecordPlayerCount");
    }
}

function startOnlineCapture() {
    cancel($startOnlineCaptureTimer);
    if (isPlayingDemo()) {
        return;
    }
    if ($onlineCaptureRunning) {
        echo("Can't capture, already in progress");
        return;
    }

    stopDemoRecord();

    %ts = formatTimeString("yy-mm-dd_HH-nn");
    %username = getField(WONGetAuthInfo(), 0);
    if (isObject(objectiveHud)) {
        %gameType = objectiveHud.gameType;
    } else {
        %gameType = "idk";
    }
    %servername = "";
    if ($filenameIncludesServerName) {
        %info = GMJ_Browser.getServerInfoString();
        %servername = "_"@strlwr(getRecord( %info, 0));
    }
    %file = "recordings/auto-capture/"@%ts@"_"@%username@%serverInfo@"_"@objectiveHud.gameType@"_"@$MissionName@".rec";
    if (!$silenceAutoRecordHudChat) {
        addMessageHudLine( "\c4Recording to file [\c2" @ %file @ "\cr] (auto-record).");
    }

    saveDemoSettings();
    startRecord(%file);
    $lastRecordingFilename = %file;
    $onlineCaptureRunning = true;
    $recordingStartTime = getRealTime();
    $recordingPlayerCountOkay = false;
    if ($minimumRecordingPlayerCount != 0) {
        schedule(1000, 0, "checkAutoRecordPlayerCount");
    }
}

function stopOnlineCapture() {
    $onlineCaptureRunning = false;
    stopRecord();
}

function delayedStopOnlineCapture() {
   cancel($stopOnlineCaptureTimer);
   $stopOnlineCaptureTimer = schedule(2000, 0, "stopOnlineCapture");
}

function handleOnlineDebrief() {
   delayedStopOnlineCapture();
}

function handleOnlineGameOver(%msgType, %msgString)
{
   delayedStopOnlineCapture();
}

addMessageCallback('MsgDebriefResult', handleOnlineDebrief);
addMessageCallback('MsgGameOver', handleOnlineGameOver);
