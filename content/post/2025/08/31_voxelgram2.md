+++
date = "2025-08-31T13:57:22.3307061-07:00"
title = "Instructions unclear, block destroyed"
category = [ "Playing A Game" ]
game = [ "Voxelgram 2" ]
platform = [ "PC" ]
+++
... *oh, no*. I think I hate this.

<game:Voxelgram> remains one of my absolute favorite nonogram games because, on top of [spoiling me with puzzle content]($SiteBaseURL$2024/07/28/bigger-and-voxelier/), it went considerable lengths to make multi-dimensional puzzle solving *accessible*. By treating each slice of the puzzle as its own grid, even though an individual slice may not be fully solvable, checking another slice for open hints allowed me to make step-by-step progress -- like typical picross logic with an extra dimension.

<game:Voxelgram 2> has chosen to add a two-color mechanic, so grid spaces aren't just "filled," they're marked Blue or Green depending on similarly-colored hints for the row or column. And this affects the fundamental meaning of hints: if three spots are hinted with a Blue 2, that doesn't necessarily mean that one of those 3 spots is empty ... *it might be Green.*

And therein lies the accessibility conflict that, I hate to say, turned me off of Voxelgram 2. It's no longer possible to make iterative progress on a puzzle by following one row's or column's hints at a time, like a typical nonogram; those hints no longer contain enough information.

Now there are situations where the meaning of a hint is unclear without seeing all of its intersections, which, y'know, *might not be visible* in the slice you're looking at.

To illustrate, given an orange underline highlights that something can be done in a row or column...

![]($SiteBaseURL$voxelgram-2_highlight-tutorial.jpg){width=960 height=540}

... the middle columns here, with Green "2"s at the top and consecutive green blocks in the middle...

![]($SiteBaseURL$voxelgram-2_highlight-unclear-1.jpg){width=960 height=540}

... may look like their topmost and bottommost blocks can be removed. But in fact, only one of those logical deductions is correct...

![]($SiteBaseURL$voxelgram-2_highlight-unclear-2.jpg){width=960 height=540}

... because the bottommost blocks are relevant to the depth-wise Blue 5, which in turn is relevant to layers that're hidden in this slice view.

It's not as bad as [having to guess](game:CrossCells), since all the necessary information does *exist,* but it's frustrating - especially as its predecessor was so user-friendly - that Voxelgram 2's hints require so much additional scanning and inspection to accurately understand.
