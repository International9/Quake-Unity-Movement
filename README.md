# Quake I Movement In Unity
An implementation of quake I's movement in the unity engine.

which I tried to make as close as possible to the original source code (at least implementation-wise) but the physics also should be accurate, as such - tech from the original game is also possible.

The codebase is relatively organized and readable with summaries, comments, regions, sources etc.. so if you want to change something it shouldn't be hard to do so.
Feel free to use it however the MIT license allows you to.

VERY IMPORTANT - read the section 'Potential Issues' carefully before working with the controller!

Another important thing - this project was made using unity 6000.0.25f1 and it utilizes unity's new input system so take that into consideration when importing the controller into your project.

a bit more about accuracy:

it is important to state that despite it's accuracy to the original movement controller - I did actually take some creative liberties due to the fact that the code was written in a different language and engine, and for the user to have quality of life options. it does not reduce from its accuracy at all, only the way the end user can interact with the codebase provided and its compatibility with the unity game engine. (that also doesn't mean that I copied the code/implementation 1:1 everywhere)

GIF's Kinda Low Quality So Sorry In Advance!


![QuakeUnityMovementGIF_Combined](https://github.com/user-attachments/assets/68319b8b-c07a-4f04-9f04-dd07d9db6bdb)


# Sources Used:

https://github.com/id-Software/Quake/blob/master/QW/client/pmove.c (original source code)

https://github.com/myria666/qMovementDoc (Quake I movement paper)

https://github.com/Olezen/UnitySourceMovement (Inspo)

etc..

these helped me a lot in order to understand the movement system.


# How To Use:

- download the 'player' folder and put it in your project
- make sure there are no other active cameras in the scene (reccomended)
- drag the player prefab from the folder to the scene
- make sure that the layers 'layerColl' and 'layerGround' in the main movement script are setup (reccomended)

congrats, now use it however you want, go crazy!


# Potential Issues:

IMPORTANT: When building out your levels, because the movement and velocity clipping are dependent on accurate normals - use primitive collider for 
100% accurate normals. unity calculates the normal from boxcast (and other cast functions) on mesh colliders (convex and concave) using an approximation which may 
not be accurate all the time. meaning collisions may often be very buggy and inconsistent at walls or acute (90 degree) angles so take this into consideration



if you'd like more details about normal calculation in unity you can check it out here: https://docs.unity3d.com/ScriptReference/RaycastHit-normal.html

luckily - this only happens on walls in the function FlyMove, so if you're using a gameobject with a mesh collider it shouldn't give out any little errors at all.
as such - primitive colliders work very well and smooth and shouldn't have any problems.


if there are any issues please report them so i could invastigate, thanks!  

# Features:

The movement system, as of currently, support:

- standard WASD
- jumping
- stairs
- slopes
- triggers (handled using a rigidbody on the player)

and other features for Quake I's 'physics' (as with all of the weird tech in the og game)

however, the movement system does NOT support any water movement or a 'nudgeplayer' function, you could implement those yourself using the custom trace system in the project, helper functions and/or the sources which are linked above.
