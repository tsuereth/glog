+++
date = "2018-07-23T22:11:54-07:00"
title = "Fall from greatest"
category = ["Playing A Game"]
game = ["Nonogram - The Greatest Painter"]
platform = ["PC"]
rating = ["Good"]
+++

I found <i>the problem</i> with {{% game "Nonogram - The Greatest Painter" %}}Nonogram{{% /game %}}.

The "Classic" puzzles are big, right?  I got up to a <b>70 x 40</b> puzzle.  <b>That's big.</b>  But as Nonogram's puzzles get bigger, its technical issues become harder to ignore.

When ex-ing out or filling in a bunch of squares in sequence - as one is likely to do in a large puzzle - the game often hitches for several seconds at a time.  One moment, you'll be dragging the cursor along a row; then suddenly, visual feedback stops; then three seconds later, it'll catch up again, and you'll be unsure how many boxes got filled.  And then, <i>because of how many boxes you filled</i>, it'll likely hitch again pretty soon.

This is annoying, but not breaking; the <i>breaking</i> issue I encountered was in trying to correct a logical flaw in that 70 x 40 puzzle.  I jammed on the Undo button to revert to a known-good state, and after a certain amount of undos (admittedly, quite a few) the game <b>crashed</b> displaying a "gc" error.

Clearly, something very memory-inefficient is going on each time a square is clicked.  These allocations pile up, and eventually fragment the game's heap to a critical point, upon which the game has no choice but to wait on some garbage collection.  (I'm guessing that's what the hitching is about.)  And when the Undo/Redo history is invoked too frequently, the garbage collector can no longer keep up.  Boom.

But the <i>real</i> problem, the icing on this crash-cake, is that <b>there's no autosave</b>.  That crash lost me 45 minutes of puzzle-solving.  This wasn't my first crash, either, and I just assume it would continue recurring in similar circumstances.

I might have been willing to put up with Nonogram's technical infidelity if it at least saved my progress.  But it doesn't, so I can't.

It's a shame, because otherwise, Nonogram is a great implementation of picross puzzling.  Its UX is the best I've ever seen, and its puzzle gallery might be, too.  <i>If only it didn't crash and lose progress</i>.

<b>Better than</b>: {{% game "InfiniPicross" %}}InfiniPicross{{% /game %}}  
<b>Not as good as</b>: {{% game "Paint it Back" %}}Paint it Back{{% /game %}}, {{% game "Pepper's Puzzles" %}}Pepper's Puzzles{{% /game %}}  
<b>Just because you're using Unity to make checkbox puzzles</b>: doesn't mean that you can ignore your memory usage.

<i>Progress: 126/126 Gallery, 13/50 Classic, 35/50 Speed</i>
