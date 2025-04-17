# AnimeToonShader
This sample is recreate and mimic the shader of ' Star Rail ' and ' ZenLess Zone Zero '. 

https://github.com/user-attachments/assets/7e6b42d4-b998-41bd-8984-7a419df5921c

# How to use it?
1. Download this github code.  
2. Unzip the file and load this unity project with 2022.3.38f1 or newer. (This project is not support Unity6 yet.)  
3. First time open the project might takes 5~10 minutes to compress skybox textures.  
4. Open the 'AnimeToonSample.unity' scene to view the sample.  

# Does it allow for comercial use?
Yes, only shader can! The models are belong to Mihoyo company.  
This source code doesn't get any optimization, and make sure you have test target platform first when you want to use in your project.

# Function Explain
### MainTex
![image](https://github.com/user-attachments/assets/dced23eb-2cbb-492a-b157-edb77b7ed75d)  
BaseMap texture and color.

### Outline (Optional)
![image](https://github.com/user-attachments/assets/ac38e19b-9773-417a-a2fd-7e24fe135116)  
```
Outline Normal Source : 
  - Mesh Normal : Directly use the normal of the mesh to expand the outline.
  - CustomNormal(UV4) : Take the pre-smoothNormal data from the mesh of uv4 to expand the outline.

Outline Color (Lerp Mode) : Alpha is a weight to control the baseMap color and outline Color.  
Outline Width : Control the thickness of outline.  
Receieve Light Color : Enable to multiply with mainLight color.  
Impacted By Vertex Color : Enable to multiply with single channel color of vertex. 
```
> [!TIP]
> ### Why can't i see the outline?  
> Turn on the 'Enable Character Outline' from the render features.  
> ![image](https://github.com/user-attachments/assets/4e1ba8d8-dac4-4a5e-ad3a-44a6b59f923c)  
> ### Where to use the smooth normal function and put it in UV4?  
> There is a extension script called 'AnimeToonFBXImporter.cs' to extend the function in your FBX model inspector.  
> Turn on the 'Store Smooth Normal in UV4' and 'Tangents' to get a perfect smooth normal data. 
![image](https://github.com/user-attachments/assets/4f7fe742-231a-440c-b737-109b3baeb7a6)


### LightMap (Optional)
![image](https://github.com/user-attachments/assets/bb90c0bb-a173-4300-8e72-3dd6eec0a5a2)  
```
LightMap Mode :
  - Normal : Deal with the cloth, body, hair and so on. 
  - Face SDF : Deal with the face.

LightMap : 4 channel deal with four different ways.
  - R : Constant Shadow
  - G : None (It means no data be used here, you can custom this property for you like.)
  - B : Metal (Metcap Texture)
  - A : Gradient Color Index (The maximum is 8 colors to be safe on each different platform and texture compression.)

Gradient Color : Map the light and shadow color with gradient map.  
MatCap Tex : Deal with the reflection quality.  
Metal : Control the MatCap Tex Intensity.
```

### EmissionMap (Optional)
![image](https://github.com/user-attachments/assets/61fc6ff2-5362-4317-afb8-662d88d1a37c)  
```
Emission Map : Sample this texture color and add on.  
Emission Color : Multiply with the Emission Tex.
```

### NormalMap (Optional)
![image](https://github.com/user-attachments/assets/59d120a8-896a-473f-b851-f195b5ff0cf0)  
```
Normal Map and Intensity.
```

### Light
![image](https://github.com/user-attachments/assets/b1dbae18-3f7e-43ea-adf6-e77ae9f085d8)  
```
MainLight Shadow Range : Control the ratio of light and shadow.  
AdditionalLight Shadow Range : Control the ratio of light and shadow.  
AdditionalLight Clip Range : Control the smoothness of light and shadow.
```

### Depth-Rim (Optional)
![image](https://github.com/user-attachments/assets/24a9494d-4c76-4d63-ac20-1d1d903758dc)  
```
Depth Rim Mode : There are three mode can be choosed.
  - Additive  
  - Multiply  
  - Replace

Offset Depth Rim : Make sure the offset value be positive. (MainLight's direction would impact the rim's offset)  
Rim Color : Multiply the rim color. (Mainlight's color would also multiply with it.)
```

### Blend Setting (https://docs.unity3d.com/2020.1/Documentation/Manual/SL-CullAndDepth.html)
![image](https://github.com/user-attachments/assets/ed7881ff-8c40-437e-bcaa-9d54426ebf70)  
```
Render Mode : 
  - Opaque : This mode the transparent is not work.
  - Transparent : This mode the transparent is work. (Use mainColor's alpha to control)
  - Custom : Use SrcBlend and DestBlend to combine what you want.

SrcBlend : Source blend setting.  
DestBlend : Destnation blend setting.  
CullMask : Controls which sides of polygons should be culled.  
ZWrite : Controls whether pixels from this object are written to the depth buffer (default is On).  
ZTest : How should depth testing be performed. Default is LEqual
```

### Stencil Setting (https://docs.unity3d.com/2020.1/Documentation/Manual/SL-Stencil.html)  
![image](https://github.com/user-attachments/assets/19f7b400-f53a-4739-b3c5-a25c875e97c0)  

### Debug Mode
![image](https://github.com/user-attachments/assets/86230452-9420-4e8b-922e-4885206b6ce6)  
```
Show Vertex Color (Debug) : Show the vertex color on.
```

### Additional Setting (Optional)  
![image](https://github.com/user-attachments/assets/95258ec3-5aff-4909-9c0a-e37e408725e1)    
> [!WARNING]  
> Make sure the render features 'Anime Toon Features' is enable and turn on the 'Eye Through Hair' and 'Depth Hair Caster' first.  
> Hair and Face have different setting, please follow the info to set up.   
> Each skinMesh have to set the correct root bone with it. And root bone's direction is need to be set like this (-z, -x, y).    
> ![image](https://github.com/user-attachments/assets/8883d2fd-3d98-4d9c-9b63-462853913753)  
> If you're using blender, change the bone's setting like this when you export the fbx.  
> ![image](https://github.com/user-attachments/assets/1840c9df-34ea-442b-98ee-4b88210109d1)  
 
Finish!    
If you still have question or want to get a custom-plan, you can DM me with mail.   

# Contact Me  
Gamil : tn86lab@gmail.com  

# Project Resource  
Mihoyo models:  
  - Kafka  
  - Gallagher  
  - Lycaon  
  - Miyabi  

Skybox : 
  - https://assetstore.unity.com/packages/2d/textures-materials/sky/free-hdr-sky-61217  
  - https://assetstore.unity.com/packages/2d/textures-materials/sky/skybox-series-free-103633  

Sox Animation Bone :  
  - https://assetstore.unity.com/packages/tools/animation/sox-animation-toolkit-110431  

