# RwiUnity
The C#/Unity scripting side of the RWImport library. Includes a sample loader to get something viewable quickly. 

The actual "heart" of this code are the static RwiDll class, which implements the actual managed/native interface and the static RwiUnity class, 
which implements methods that turn the marshalled data into actually usable Unity objects.

The WorldManager script can be thought of as a quick template script to get the loader running with *something*. 
You *will* have to modify to get it running yourself as I do not supply any game data and some of the filepaths 
that it uses are not publicly exposed. At this point I do not see this as a problem though as it is very much an 
experimental proof of concept to show that the DLL and the managed/native interface are actually working as intended. 

These are some screenshots from within the Unity Editor that show a paused game loop after an object has been loaded:

![Image 1](images/DFF_object_mesh_marked.png)
![Image 2](images/DFF_object_collision_marked.png)
![Image 3](images/DFF_object_scene_graph_marked.png)
![Image 4](images/WorldManager_component_view.png)
![Image 5](images/WorldManager_texture_list_marked.png)
