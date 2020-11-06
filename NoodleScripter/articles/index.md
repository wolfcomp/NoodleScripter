#Introduction

Each Beatmap will have its own .yml file tied to it when initialized where the default will be
```yml
# this is the script file for ${beatmapname} and the difficulty ${beatmapdifficulty} with characteristic ${beatmapcharacteristic}
# the file under are a set of default values that changes dependent on what wall type you use
default:
    seed: ${randomseed}
    width: 1
    height: 1
    duration: 0.005

4: Wall
    startHeight: 4
    startRow: -2
```
you can modify mostly everything except that the default line and seed needs to be there.