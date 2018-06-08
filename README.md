# BioVisDaydream

This project aims to allow for easy visualization of biological imaging data. The application currently takes in either a CSV file list of points or an OBJ file that describes the mesh. Once loaded, a user can move around adn view the mesh. Currently, this project is only compatible with Google Daydream VR.

# Usage

After installing the application on a compatible Android phone, open up the application. This should show either a floating warning that there is not an attached Daydream controller, or just a valley-like view. Turn around, where there is a floating set of options. Select the option that has the type of file you want to import. A dialog box will pop up to the left of you that looks like a file explorer. Navigate to the correct directory and choose the file you want to import, and then press "Open". The application will become somewhat unresponsive until the model is loaded. 

Once the model is loaded, you can load another model by repeating the above steps, or you can explore the currently loaded model. This involves using the Daydream controller to point where you want to go, a white arc will appear that shows where you will teleport. Click on the left hand side of the controller to move there. Repeat as much as you want. 

# Build Instructions

There are some prerequiesites before this can be built.

```
Unity Editor 2017.3.0f3
Android Build Tools and SDK with latest version of Google Daydream
Android Device that supports Daydream (Pixel devices, etc.) 
```

To build the application, clone this repository, and open the UnitySource folder as a project in Unity Editor. Then, you should be able to click on run or build and run to build the APK on an attached Android device, or locally. 

# Notes

This project was built with Unity 2017.3.0f3 and might or might not work with other versions of Unity's editor. In addition, for large number of points or verticies, there can be issues displaying the model on the phone. If the VR application becomes unresponsive or very laggy, either way to see if it needs to finish loading the model, or try visualizing the data in MeshLab for instance. Other option would be to create a lower resolution model and load that instead.
