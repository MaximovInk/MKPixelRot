# MKPixelRot
- No shaders
- Drag&Drop script
- Realtime/Cached
- Fine results

First attempt to make pixel rotation script. Perfomance not good, but i will optimize it soon

## Some notes:
- Better use cache, realtime perfomance is bad
- Script using rotSprite adoptation 
- If you found mistake, please let me know
- Pull requests are appreciated!
- Supports only SpriteRenderer

## Some feature plans:
- [ ] Threaded generation
- [ ] Using arrays insted of texture2d
- [ ] Cache all textures in one scriptableObject? 
      
## How it works
- Script calculates all angles
- Makes one big atlas texture for rotations
- For every rotation it generate texture
  - Scale2X 3 times => Scale texture up to 8 times
  - Rotate texture
  - Downscale to original size
- Then script add rotated texture to one main atlas
- And split atlas to sprites


# Check YT Demo

[![Everything Is AWESOME](https://i.imgur.com/emlDe7J.png)](https://www.youtube.com/watch?v=l8ct4Z0CiEw "Check demo video")
