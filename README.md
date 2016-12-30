# Vive Camera texture utility script

### Assets you care about:
- Assets\Scripts\ViveCameraTexture.cs
  - The only important thing here, this tries to open a video stream and populate a texture with it, exposed as a property called "Texture". It also tries to set the texture of the object it's attached to, if there is a material available.
- Assets\Materials\UpsideDown
   - This is simply an unlit material with a -1 vertical scale, because the frame coming from the camera is upside-down

The rest of the stuff in the repository is simply SteamVR gunk.

### Points you should care about:
- Don't forget to enable the camera first in the SteamVR Settings panel.
- You may need to drop the refresh rate from 60 down to e.g., 30 if you are experiencing tracking problems.
