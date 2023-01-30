### CURRENT SITUATION AND ISSUES:
- Hands transformations neither changing in Unity interface nor in recorded data.
    - **hypothesis:** When hand appears, ArrayData recordings don't happen.
- HandData recordings are blank
- I oriented ultraleap xr detector and vive camera such that their centers coincide. But now that I'm documenting, I realise I coincided it with the stable camera, the wrong one. The other camera is mobile (not sure if the tranformation is also changing, check)
   - **recommendation:** could attach ultraleap detector to camera but it highly will create TONS of errors :(


### SOLUTIONS TO COINCIDED PROBLEMS:
- There was a problem with multiplication, it wasn't working as expected. It turned out that Time.realtimeSinceStartup returns double but I assign the value to a float variable.
