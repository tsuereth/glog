+++
date = "2026-10-25T10:22:31.7164029-07:00"
title = "Change the channel"
category = [ "Playing A Game" ]
game = [ "QuantumPulse 2A" ]
platform = [ "PC" ]
+++
<game:QuantumPulse 2A> is confounding on purpose: it takes basic computing principles, the simplest of *real* CPU instructions, and makes them incredibly difficult. *Considerably* more confounding than <game:TIS-100> was. A real <game:QWOP> of computer programming.

As an illustrative example, the seventh puzzle - which is where I ragequit - challenges you to implement a `for` loop. One input is the number of expected iterations, *so okay, just decrement that and jump/branch back if it's greater than zero*. But QuantumPulse doesn't have jumps conditional on the local accumulator register -- only on cross-processor "channel" registers. *And* a processor can't read from a channel that it's written, only from channels written by other processors.

So the `INC`/`DEC` of a loop, and the conditional branching of a loop, need to be done in separate processors linked by synchronized channel communications. And that's just the scaffolding of the loop! The stuff *inside* a loop will take even more processors!

I've seen this style of intentionally-frustrating programming puzzle before, in <game:Prime Mover>. I guess some people are into it? If [code golf](https://en.wikipedia.org/wiki/Code_golf) isn't exciting enough?

This just isn't for me. I can't get past my irritated feeling that, as one [Steam reviewer put it](https://steamcommunity.com/profiles/76561198000774400/recommended/3175190), *"it's giving you a hammer to put in a screw."*
