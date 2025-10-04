// #autoload
// #name = demo-playback
// #date = October 4th, 2025
// #author = loop
// #warrior = looop
// #email = loop@tribesforever.com
// #description = Various fixes for demo playback issues. 1) Command Circuit always being shown (recorder was using a minimap / engineer hud)

package BiteMyShinyMetalAss {
	function Canvas::setContent(%this,%canvas)
	{
		if(isPlayingDemo() && %canvas $= "CommanderMapGui") {
            return;
        }
		parent::setContent(%this,%canvas);
	}
};

activatePackage(BiteMyShinyMetalAss);
