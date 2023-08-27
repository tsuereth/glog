+++
date = "2021-02-06T10:04:41-08:00"
title = "Editor's note: No, don't do this."
category = ["Playing A Game"]
game = ["Copy Editor"]
platform = ["PC"]
rating = ["Awful"]
+++

<a href="https://en.wikipedia.org/wiki/Regular_expression">Regular expressions</a> get a <a href="https://thedailywtf.com/articles/and-now-you-have-two-problems">bad rap</a> -- a regex is a powerful tool, good for some jobs and terrible for others.

I would be pretty interested in a puzzle game with gradually-escalating regex complexity (especially using the search text for storytelling).  But {{% game "Copy Editor" %}}Copy Editor{{% /game %}} takes an early turn in ... another direction.

In this level, the search text has gendered pronouns that need to be gender-swapped.  So how do you change "His" to "Her" and "Her" to "His" without undoing yourself?  The real-life answer is <b>don't use regex</b>. It's the <a href="https://en.wikipedia.org/wiki/Law_of_the_instrument">wrong tool for this job</a>, and this problem shouldn't be in a regex game at all.

Copy Editor teaches you to look for specific instances, instead of general patterns, which is <b>the wrong way</b> to use regular expressions.

{{% absimg src="copy-editor_hint.png" width="640" height="360" %}}

Like, at this point, why not re-type the text?

I know I'm digressing into computer science philosophy here, and these guys are just trying to make a game.  Okay.  I only hope that I don't have to work with a junior programmer who learns to <a href="https://thedailywtf.com/articles/look-ahead-look-out">abuse regexes</a> from Copy Editor.

<i>Progress: I think this is in the demo's second level.</i>
