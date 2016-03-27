The BulletPattern object consists of FireTags and BulletTags. When the BulletPattern starts the first index in the 
FireTag array will be started. That FireTag will then run through its array of FireTag Actions, creating bullets and 
even calling other FireTags. This process creates logic flow similar to programming patterns with code, but through the GUI.

Bullets that are created will travel forward along the direction that their "creator" (either a FireTag or Bullet) has set for them.
If they have any BulletActions, they will be executed one by one just like with FireTags.

Keep in mind FireTags are NOT physical entities, but just structs of data to control pattern flow. BulletTags aren't physical either,
but contain the data needed to create a physical Bullet object. A Bullet object is seperate from the BulletPattern or Bullet that created it.

The BulletManager object is very important. It should always be present in the scene with its BulletManager script attached. 
There should only be one.


FIRE TAGS
------------

FireTags are completely useless without at least one FireAction. The BulletPattern, once started, will call the first FireTag in the Inspector
and proceed to execute its actions. Below the different actions and what they do will be explained. Change the type of teh action with the 
enum pop up box in the GUI.

WAIT - This will start a "yield WaitForSeconds" command, in which the FireTag will pause and do nothing while the game continues running.
This is useful for breaks in large amounts bullet creation, or pausing between FireTag calls or Loops. Remember that without Wait commands 
the entire FireTag will run in frame. In some cases this will freeze the Unity editor, such as in cases with loops with large amounts of 
bullet creation.
--------

Variables
--------

WaitTime - The amount of time to wait, in frames. 20 frames is equal to about 0.333 of a second. Clicking the Randomize box will allow
you to enter two values so a random value will be chosen between the specified range.

AddRank - Clicking this will add a value to "WaitTime" based on the Global Rank value. Rank is "global" or "game wide" and is a value
between 0 and 1. The AddRank value will be a multiplier to Rank then added to WaitTime. For example, WaitTime could be 20 frames, but 
AddRank is 10 frames, meaning up to 10 frames will be added to WaitTime depending on Rank. Though keep in mind usually having a
longer pause in FireTag action would make the game easier, so it would make more sense to have AddRank as -10 frames so that
up to 10 LESS frames would be paused. You can adjust Rank using the slider at the bottom of any BulletPattern object.

FIRE - Actually creates and fires a bullet object.
-------

Variables
--------

DirectionType - Controls how we are actually going to point or aim the bullet. Four different types.
TargetPlayer - Aim directly at the player, plus the angle offset
Absolute - Fire at an absolute angle, clockwise. 0 degrees is straightup, 90 is right, 180 is down, 270 is left
Relative - Fire relative to the creator objects rotation. Really only useful if a Bullet is firing a bullet
		   The Bullet could be traveling at 45 degree angle and fire two bullets at 45 and -45 degree angles in
		   relation to itself
Sequence - Fire in relation to the last firing angle. For example you could fire at Absolute 0, then Sequence 5
			multiple times to make a circle
			
Angle - The offset to the directionType as explained above. TargetPlayer with value 10 would shoot 10 degrees off
of the players direction. This value can be randomized and affected by rank as WaitTime above.

UseParam - If checked, we will use this Tag's paramater value instead of Angle for the offset. More on Param later.

OverwriteBulletSpeed - If checked, we will use the values set here for the bullets speed, instead of the bulletTag
values. This is useful if several Tags call the same BulletTag but want different speeds.

NewSpeed - The speed of the bullet. If sequence is checked, then speed will increase sucessively. For example, 5 bullets are 
created in a loop with Sequence Speed 8. The bullets speed will be 8,16,24,32,40 repectively.

PassParam - Pass a random value to be used as a parameter. Params are useful for multiple instances of the same object 
to perform differently using their param as a value.

PassMyParam - Similar to above but this Tags own param that was passes to it will be passed. Passing the torch so to speak.

BulletTagIndex - The actual index of the BulletTag that has the data needed for the bullet we will create. Make sure you are calling 
the right bulletTag and that the array isnt empty. Index 1 = First BulletTag in the array


CALL FIRE TAG - Call a FireTag so it can start its process and run its own actions. After the called tag is finished, this tag(the caller)
will resume its process.
------

Variables 
---------

FireTagIdx - The index of the Firetag to call. Index 1 = First FireTag in index, BE VERY CAREFUL about a FireTag calling itself, as this 
will almost always result in infinite recursion.

PassParam - Pass parameter as described above

START REPEAT, END REPEAT - Extremely important in dictating logic flow. All actions in between these two actions will be repeated the 
specified number of times. Its just like starting a FOR loop in programming terms. Remember that without a WAIT action somewhere in the 
loop the entire thing will run in one frame, possibly resulting in freezes. Also possible to nest these for more complex patterns.




BULLET TAGS
---------------

The data for creating an individual Bullet. Unlike FireTags, they dont have to have any actions. The actions in this tag will
be performed by every bullet created with this tag.

Speed - set the speed of any bullet that uses this Tag

PrefabIndex - the "type" of this bullet. The BulletManager has an array of BulletPrefabs used in your game. For example, array index 0
can be pink bullets and index 1 can be blue bullets.

Actions
--------

WAIT - Exactly like FireTags wait function.

CHANGE DIRECTION - Change direction from current angle to new angle, over specified amount of time. The DirectionTypes work similarly 
as when creating a bullet - TargetPlayer 20 will move the bullet 20 degrees from the players direction. Relative will chnage the direction
in relation to the currrent one. Sequenmce is the exception, it will continually add to the current rotation every frame for the specified 
amount of time.

Variables
----------
Angle - set angle offset with random and rank options
Time - how long it will take to reach teh new rotation, set time to zero for instant change
WaitToFinish - Normally this is an ongoing process, meaning direction can change over time while other actions are being performed. But if this
is checked, all other actions will be halted until the process is finished (meaning the Time has expired). You could get the same effect by
setting a WAIT action directly after this for the same amount of time as this action, but this is easier.

CHANGE SPEED - Exactly the same as above but with speed instead of direction.

START REPEAT, END REPEAT - Repeat functions that work the same as FireTag

FIRE - Again same as FireTag, except much more interesting when a bullet is doing the firing. Be careful when firing a bullet using the same 
BulletTag as the creator bullet. This could lead to a large amount of bullets on screen.

VERTICAL SPEED CHANGE - Speed causes a bullet to go forward in its own forward direction. Using this causes "vertical speed" to go in effect,
which will cause the bullet to go straight up or down regardless of its forward dretion or rotation. Otherwise this performs similarly to 
ChangeSpeed action. The bullets regular speed will remain in effect.

DEACTIVATE - Immediately deactivate and diable this bullet. Useful if a bullet fires some bullets then no longer has any use, thus 
deactivates - creating the illusion it "exploded" into multiple bullets.


--------------------------------------

Under the tags are "WaitBeforeRepeating" And "Rank". The former is the amount in time in frames the Pattern itself waits before repating itself
after the Firetags have done their thing. Rank is a global static variable handled by BulletManager that affects Rank related variables as 
described above.

Thats about it. I hope I Didnt forget anything. If you've encontered any issues, bugs or have questions contact me. Either post in the 
Bulletpattern thread in the forums, PM "dhendrix" in the Unity Forums, or email me at "drayhendrix@gmail.com"

If you feel generous, you can send a small donation with Paypal to "drayhendrix@gmail.com". Thanks. 


SPECIAL THANKS

You - of course, no support = no script packages like this one
Unity - for the awesome engine
Kenta Cho - His BulletML system gave me the idea for this package. Plus his pattern breakdowns showed me how some of the more intricate 
Bullet Patterns in games work. Seriously, I wish more people had this info out there. His website is "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml/index_e.html"

THANKS