// #autoload
// #name = auto-record
// #date = January 27th, 2025
// #author = loop
// #warrior = looop
// #email = loop@tribesforever.com
// #description = Allows you to automatically record your perspective for each match you play in. Recordings are stopped on exit or debrief.

// config
$silenceAutoRecordHudChat = false;

// internal vars
$onlineCaptureRunning = false;
$shouldCaptureNextPlayWake = false;

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
    }
};
activatePackage(OnlineDemoCaptureShim);

function delayedStartOnlineCapture() {
    cancel($startOnlineCaptureTimer);
    $startOnlineCaptureTimer = schedule(2000, 0, "startOnlineCapture");
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
    %file = "recordings/auto-capture/"@%ts@"_"@%username@"_"@objectiveHud.gameType@"_"@$MissionName@".rec";
    if (!$silenceAutoRecordHudChat) {
        addMessageHudLine( "\c4Recording to file [\c2" @ %file @ "\cr] (auto-capture).");
    }

    saveDemoSettings();
    startRecord(%file);
    $onlineCaptureRunning = true;
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
