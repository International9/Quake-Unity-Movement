# Quake I Movement In Unity
A half - assed implementation of the quake I's movement system in the unity engine.

The codebase is relatively organized and readable with summaries, comments, regions etc.. so if you want to change something it shouldn't be hard to do so.
Feel free to use it wherever you want and change whatever you want, you don't have to credit me too.

Another important thing - this project was made using unity 6000.0.25f1 and it utilizes unity's new input system so take that into consideration when importing the controller into your project.
GIF's Kinda Low Quality So Sorry In Advance!


![QuakeUnityMovementGIF_Combined](https://github.com/user-attachments/assets/68319b8b-c07a-4f04-9f04-dd07d9db6bdb)


# Sources Used:

https://github.com/id-Software/Quake/blob/master/QW/client/pmove.c (original source code)

https://github.com/myria666/qMovementDoc (Quake I movement paper)

https://github.com/Olezen/UnitySourceMovement (Inspo)

etc..

these helped me a lot in order to understand the movement system.


# How To Use:

- download the 'player' folder and drag it into your project
- make sure there are no other active cameras in the scene (reccomended)
- drag the player prefab from the folder to the scene
- make sure that the layers 'layerColl' and 'layerGround' in the main movement script are setup (reccomended)

congrats, now use it however you want!


# Potential Issues:

The collision detection helper function 'Helpers.GetSafeEndPos' which is used to offset the end position 
of a trace so the player could be immediately spawned there can (under certain circumstances) sometimes fail (probably cause it wasn't written by me).

I've worked on the controller since and it seems like problems usually refrain from happening but still weird stuff could occur so keep an eye out for that.

if anybody finds issues, problems, errors, fails, etc... please let me know, so I would know to check it out.


# Features:

The movement system, as of currently, support:

- standard WASD
- jumping
- stairs
- slopes
- triggers (the triggers are handled using a rigidbody on the player)

and other features for Quake I's 'physics'.

however, the movement system does NOT support any water movement or a 'nudgeplayer' function, you could implement those yourself using the custom trace system in the project, helper functions (will be added soon) and/or the original source code which is linked above.
