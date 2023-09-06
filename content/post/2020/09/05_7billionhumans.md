+++
date = "2020-09-05T15:36:16-07:00"
title = "PEBKAC"
category = ["Playing A Game"]
game = ["7 Billion Humans"]
platform = ["PC"]
rating = ["Good"]
+++

{{% game "7 Billion Humans" %}}7 Billion Humans{{% /game %}} isn't "just" a sequel to {{% game "Human Resource Machine" %}}Human Resource Machine{{% /game %}}; it also expands the fake-programming domain to <i>multi-threading</i>.  Your program runs on multiple agents/workers at once!

Unfortunately 7BH doesn't fix the biggest problems I had with its predecessor: dragging-and-dropping instructions is a chore, the instruction set is awkwardly limited, as is its concept of memory or variables... and this game's new complexities <i>exacerbate</i> a voodoo-polymorphism problem that the previous game merely hinted at.

Egotistical professional programmers (like myself) often look down on languages like PHP and JavaScript because their type systems, or lack thereof, inhibit strict definitions of a program's expected behavior -- encouraging <i>un</i>expected behavior that's difficult to debug, or even to detect.  7 Billion Humans may keep its instructions "simple" by dodging the question of type-safety, but this leads to haphazard consequences like `step` sometimes not working depending on the state of another worker in the destination, or `giveTo` throwing your worker into a shredder if it isn't holding anything.

Personally I find this kind of unpredictable mechanic tiring and unsatisfying - in programming <i>or</i> in video games generally - and this eventually dulled my interest in solving the game's ongoing puzzles.  I didn't even get far enough to unlock functional synchronization instructions; in most of the puzzles I completed, 7BH's multi-threading concept was more like a proxy for running the same program over multiple data sets.

7BH also doesn't address my most-superficial criticism of HRM, that it doesn't compare your solution's memory- or runtime-efficiency with other users.  I kinda feel like this is a requirement for modern programming games.

7 Billion Humans was mostly fun as far as I played it, and it does boast significantly more puzzles than Human Resource Machine did.  But I got bored of its programmer-unfriendly UI and "magic" behavior around the halfway point.

<b>Better than</b>: {{% game "Opus Magnum" %}}Opus Magnum{{% /game %}}, {{% game "Silicon Zeroes" %}}Silicon Zeroes{{% /game %}}  
<b>Not as good as</b>: {{% game "Exapunks" %}}Exapunks{{% /game %}}, {{% game "Human Resource Machine" %}}Human Resource Machine{{% /game %}}  
<b>Nerd alert</b>: if the game supported modular code (functions), I might even have written some type-safe helper modules.  Alas.

<i>Progress: 34 "years" (puzzles completed).</i>
