# Shader Stripping Tool
A tool which help to strip shader variants in Unity


I will suggest 2 strategies ti use this tool:
1) Manually control all shader variants by using ShaderVariantCollection asset
2) Build your game with "Log Shader Compilation" checkbox enabled without shaders stripping. Then play through your game, grab Player.Log file (on Android is possible only with logcat) and attach it to stripping settings. Then build your game again with stripping enabled. Try to clean up your Player.log file from other any other lines without any shader information (look to example file)
