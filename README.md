# Project Guide: Weather Node Puzzle

Hey there! This guide walks through the entire project and explains the hows and whys of each of the major parts. All non-script points of interest are broken down here, while the scripts themselves contain comments that elaborate on the code inside. 

**BE WARNED: the code is terrible and I currently have no plans to change it.** If you have questions or want clarification on anything in the project, open up an issue and I'll see what I can do. You are also free to leave suggestions, but I probably won't see what I can do about those at all. I'll just read them and think, "Well, that's nice to know".

### Main Scripts: WeatherNodeGen.cs,DragAndDrop.cs
* When reading the comments in these scripts, start with WeatherNodeGen

### Other Scripts: ExperimentScript.cs, PilotScript.cs
* PilotScript.cs: a leftover from an old idea I had for “presenting” the project
  * Is basically empty
* ExperimentScript.cs: a script I made to understand how the tilemap stuff worked
  * Contains no comments

## Scene Hierarchy:
WeatherNodeMarker:
* The node that is dragged around in the maze
* Is inactive by default so the player can’t drag it around until a maze is generated
* Has DragAndDrop.cs attached to it
* Has a BoxCollider2D for OnMouseDrag() in DragAndDrop.cs

Canvas:
* The UI canvas for the maze-gen buttons
* Uses Screen Space - Overlay as its render mode. Adjusting anything in this canvas was difficult since I didn’t know how (or if) I could control the tilemaps’ positions; I ended up having to trial-and-error my way to a configuration that looked ‘good enough’ at most 16:9 resolutions. I was hoping to be able to control the tilemaps as if they were UI objects to ensure that they stayed in place on screen (and below the button panel)
* Thinking back on this, I wonder if I could just replace the transform component of the tilemaps with a RectTransform (which the UI elements use for their positioning)?
* Thinking further, I think this is a sign that I don’t really “get” how tilemaps work just yet
  * Or lots of stuff involving the camera really, that thing remains a mystery to me

ButtonPanel:
* Has a light panel image to visually group the two buttons nested in it
* Uses Horizontal Layout Group to automatically manage the buttons’ positioning

GenPathButton:
* OnClick: Triggers the GenMaze() method in WeatherNodeGen.cs and activates the WeatherNodeMarker game object

GenWallButton:
* A leftover from when I was first working on the MazeGen() and PlaceWalls() algorithm. Initially, the path part of the maze would be generated on Start() when the scene played and then I would use this button to test the wall placement

ResetMazeButton:
* OnClick: Triggers the ResetMaze() and GenMaze() methods in WeatherNodeGen.cs as well as SetNode() in DragAndDrop.cs
* This button once had a use, but as I write this I now realize that it has been made redundant since the GenPathButton can do these things as well
* Furthermore, this button’s call to ResetMaze() in OnClick() is also redundant because, when it calls GenMaze() later in this same step, ResetMaze() happens anyways as it’s the very first line of GenMaze()
* The only different thing here is the call to SetNode(), which isn’t really necessary since the node shouldn’t be out of place if this button is being pressed
* Basically, this button is worthless but still here
* So. That’s a thing. :|

BlockerPanel (and FinishText):
* Used in the coroutine called by the CompleteMaze() sequence in DragAndDrop.cs
* Has an Animator component for its one animation clip played in the coroutine
* In the Animator: Only point of interest is from the Neutral state to PanelFadeIn. Both transitions between them are triggered by the “CompleteNode” parameter. The PanelFadeIn state contains the PanelFadeIn animation clip, while Neutral has none
* The PanelFadeIn animation clip is 100 frames (1.4s). In the clip:
  * BlockerPanel’s raycast target is set to true from 0f, then false at 100f
  * BlockerPanel’s alpha quickly goes from 0 to 0.6, creating a ‘dimming’ effect, then quickly fades back to 0 near the end
  * The FinishText object, positioned above the camera by default, quickly moves down toward the center of the screen (in time with the panel’s alpha increase), then moves back up near the end

WeatherNodeGrid:
* A grid object that is the parent of WNMaze, the tilemap where the maze itself exists
* The wire objects are just decorative sprites indicating the start/end points of the maze

WNMaze:
* The maze itself
* The block tiles are already placed, and a Tilemap Collider 2D component handles the collision for all of them (and for every other tile that gets placed) automatically
* Has the WeatherNodeGen script attached to it
* Has a child object, a Canvas, with decorative images for the taupe background of the maze and the blue maze border
* Probably could’ve put the wires here too, but they came along a while after these

BlocksOverlayGrid:
* Another grid object, the parent of the WNWallBlock Tilemap

WNWallBlock:
* A tilemap with a configuration of block tiles placed exactly like WNMaze
* Purely for aesthetic purposes, as part of a quick final “polish” phase I did
* In the original Weather Node, the path highlighted as the node marker was dragged would appear to be underneath the block tiles, but my recreation didn’t match; after trying a few different things, I found this solution
* I’m sure there’s a way to control the tile rendering order without having to resort to this, but like many parts of this project I didn’t give enough fucks to go looking for it



	



