namespace BossMod.Dawntrail.Ultimate.DMU;

/*

 Requirements:
 - Need to know which sides are safe, e.g. left north or left south
    - So we can assign positions correctly as they change depending on it
- Need to know which side is supports / DPS go? - most likely optional but maybe consider it
    - This should work fine as we should have a data model with the coordinates inside it assigned to the role?
    - e.g. OT & M2 are the same spots just on different sides? - like
- A function to calculate the KB distance to correctly assign with the spot - you also have a small time to adjust which could be considered
    - can be used for the knockback players as a hint of where to stand

- A coordinate system could be I guess would be set distances, e.g. OT & M2
    - H2 at wall, H1 middle of safe spot, OT west of boss, MT N/S of boss
        - H2 solver would be 19.0f away from boss going west, then 1.0f down or up depending on safe zone
        - H1 solver would be 10.0f away from boss going west, then 10.0f down or up depedning on safe zone
        - Each strat would need different maths for the logic of where the point should be
            - Which would result in a large codebase of similar code for each different strat to apply the maths

- Another coordinate system could be set positions in a data model with the assignment of the player like OT
    - Might mean we have to store 2x spots for each player as it can be north or south which would be 16 in total
    - Would easily allow other strats to work since we could just create another data model with the other positions inside it
        - Only problem is that would grow pretty large if we have like 5 different strats as it would be 80 different positions
            - Could be reduce by having a common point for each role and then apply maths to it base on if its north / south, but
                there is no saying this would work for every strat as the maths that we apply might be different for each strat

We will go with the point coordinate system and we will use logic to apply transformation on points to keep 8 points per strat
E.g. if we have north positions then we should be able to apply a transformation for the mirror of it

Things to consider:
- Melee uptime for the KB? - Have a small time to adjust to the correct spot afterwards - maybe a future update
- Positions correct for spreads for melee - range are fine due to the amount of space

 GravenImage 1:
 - Must have a config option - set the default to none

 - 4 people will get tethers, 4 people won't
 - Couple of seconds before tethers resolve, blizzard will start + stack / spread

 Non-Tether players:
 - Simply just go to the stack/spread spot assigned to you

 Tether players:
 - Will need to go to a spot where it will knock them back into the correct spot instead
 - Think of a way to display this - maybe one green circle with a line to the yellow circle?
    - Just one green circle


 */
