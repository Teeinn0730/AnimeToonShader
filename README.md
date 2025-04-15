# AnimeToonShader
This sample is recreate and mimic the shader of ' Star Rail ' and ' ZenLess Zone Zero '. 

https://github.com/user-attachments/assets/7e6b42d4-b998-41bd-8984-7a419df5921c

# How to use it?
1. Download this github code.
2. Unzip the file and create new unity project with 2022.3.38f1 or newer. (This project is not support Unity6 yet.)
3. First time open the project might takes 5~10 minutes to compress skybox textures.
4. Open the 'AnimeToonSample.unity' scene to view the sample.

# Does it allow for comercial use?
Yes, only shader can! The models are belong to Mihoyo company.
This source code doesn't get any optimization, and make sure you have test target platform first when you want to use in your project.

# Function Explain
### MainTex
![image](https://github.com/user-attachments/assets/dced23eb-2cbb-492a-b157-edb77b7ed75d)  
BaseMap texture and color.

### Outline
![image](https://github.com/user-attachments/assets/ac38e19b-9773-417a-a2fd-7e24fe135116)  
Outline Normal Source : 
  - Mesh Normal : Directly use the normal of the mesh to expand the outline.
  - CustomNormal(UV4) : Take the pre-smoothNormal data from the mesh of uv4 to expand the outline.

Outline Color (Lerp Mode) : Alpha is a weight to control the baseMap color and outline Color.  
Outline Width : Control the thickness of outline.  
Receieve Light Color : Enable to multiply with mainLight color.  
Impacted By Vertex Color : Enable to multiply with single channel color of vertex. 
> [!TIP]
> ### Where to use the smooth normal function and put it in UV4? 
> There is a extension script called 'AnimeToonFBXImporter.cs' to extend the function in your FBX model inspector.  
> Turn on the 'Store Smooth Normal in UV4' and 'Tangents' to get a perfect smooth normal data. 
![image](https://github.com/user-attachments/assets/4f7fe742-231a-440c-b737-109b3baeb7a6) 


### LightMap
![image](https://github.com/user-attachments/assets/bb90c0bb-a173-4300-8e72-3dd6eec0a5a2)  
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
