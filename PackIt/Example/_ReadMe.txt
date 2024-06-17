Welcome to the example scene. The purpose of which is to illustrate potential use cases for PackIt, and what the precision loss looks like in practice.

The main script can be found attached to the camera component called "PackItExample". It contains numbers to play around with help visualize what exactly
PackIt is doing. It is intended to be played with while in Play Mode.

While in Play Mode, within the scene view, change source cube. Changes to it will be reflected at various levels of precision to other cubes. Each cube
and it's corresponding box has a different precision/LOD.

Blue    = Source
Green   = Full precision, exactly the same as the source cube.
Yellow  = Half precision, some data is lost.
Red     = Lowest precision, 75% of the data is lost.
Magenta = Balanced precision, as low as one could go, with it still looking good. (45% the size of full precision)

The corresponding box around each cube is the extents that are given to the example. Floating point values are bound within it. Any lossy packing/unpacking
that wants a value to go outside the range will experience artifacting/undefined behaviours. The only exception is when full precision functionality is used.
(The green cube will never suffer artifacting.)

To exaggerate the loss in precision, increase to WorldExtents by several times.

To go over the editable options/fields:

WorldExtents and MaxUniformScale - When packing floats in a lossy manner, an extent is needed to quantize the float into a data buffer. This restricts 
	floats and their precision to some user defined range. In this case, WorldExtents is used to bound positions, and MaxUniformScale for scalar values.

UseInterpolators - Because lossy packing and unpacking of floats at low bit counts causes visual artifacting in the appearance of 'stepping', this boolean
	can be used to turn on and off interpolation towards value targets. This smooths out translations to the transform targets, but it does mean that they
	lag behind the source cube, slightly.

	Additionally is UseFixedUpdate is enabled, it will interpolate those delayed changes.

UseFixedUpdate - Tells the program to pack/unpack values on the fixed update interval rather than every frame. This more closely mimics senarios where
	you would be unpacking data from a source periodically and applying it to game objects. Set 'UseInterpolators' to true to smooth out the stepping
	cause by enabling this.

InterpolatorRotationRate - When no constant rotation rate is given, this is the rate at which the interpolators rotate a transform to match the source transform.

InterpolatorPositionRate - When no constant position rate is given, this is the rate at which the interpolators translate a transform to match the source transform.

InterpolatorScaleRate - When no constant scale rate is given, this is the rate at which the interpolators scale a transform to match the source transform.

ConstantRotationRate - Applies a constantly changing rotation to the source transform.

ConstantPositionRate - Applies a constantly changing position to the source transform.

ConstantPositionAmplitude - The amplitude of the position changes.

ConstantScaleRate - Applies a constantly changing scale to the source transform.

ConstantScaleAmplitude - The amplitude of the scalar changes.
