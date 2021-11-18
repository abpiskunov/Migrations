msbuild /r %~dp0\targets\build.proj /t:Build;Pack /p:NuGetToolsPath=%NuGetToolsPath%
