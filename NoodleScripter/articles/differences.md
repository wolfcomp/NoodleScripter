#Differences

###Color differences
Due to the difference in how this tool is coded defining colors on walls has changed and instead of being `255,255,255` or `White` for the color white on the wall you have to define wall colors this way
```yml
colorMode: SolidWhite
    p1: White,0
    type: Solid

4: Wall
    startHeight: 4
    startRow: -2
    colorMode: SolidWhite
```
or
```yml
colorMode: SolidWhite
    p1: #FFFFFF
    type: Solid

4: Wall
    startHeight: 4
    startRow: -2
    colorMode: SolidWhite
```
this is done due to the sheer dynamic ability of the colorMode accepting in theory infinite color definitions the only limit being your computer and how much ram you have when generating the walls

For instance you can set a color gradient to cycle between 8 different colors where BeatWalls can only accept 2 at the time of writing being set this way
```yml
colorMode: SolidWhite
    p1: White,0
    p2: Red,0.14285
    p3: Yellow,0.2857
    p4: Green,0.42855
    p5: Cyan,0.5714
    p6: Blue,0.71425
    p7: Coral,0.8571
    p8: Black,1
    type: Gradient
```

For a full list of named colors you can look at this [page](https://docs.microsoft.com/en-us/dotnet/api/system.drawing.color)

###Curve differences
Curves in this program has added functionality where they are not limited to 4 points and have the same theoretical no limit as color this is also due to curves are not limited to use only Bezier as their interpolation between the points but allow for all kinds of smoothing curves including a linear curve this linear curve is also the function to use for a Line wall in BeatWalls. Due to the curve being able to define infinite rotations there is only one curve wall type as this should handle all curve wall types.
To use a curved wall look to the example bellow, this will create a quarter spiral pattern around the player
```yml
spinCurvePath1: Curve
    p1: 0,-4,0
    p2: -1.6,-4,0.0625
    p3: -3.5,-3.5,0.125
    p4: -4,-1.6,0.1875
    p5: -4,0,0.25
    amount: 48
    color: SolidWhite
    rotationMode: QuarterRot1
    height: 0.1
    width: 0.3
    duration: -2.5
```

###Rotation differences
As you can see above there is something called `rotationMode` this is the replacement for both localRot and rotation of BeatWalls the reason for this is because rotation has gotten an overhaul