+++
date = "2023-09-10T10:24:42-07:00"
title = "Manifest Glogstiny"
category = ["Site News"]
+++

[Eight years ago]($SiteBaseURL$2015/10/24/glog-now-with-100-less-server-execution/), I turned the Glog into a static site, using Hugo to generate webpages from post text and metadata.  And while this was a big improvement on what came before, that Glog still ... wasn't ... <i>quite</i> everything I wanted it to be.

Though if I'm being honest with myself, finally pulling the trigger on <a href="https://knowyourmeme.com/memes/im-going-to-build-my-own-theme-park-with-blackjack-and-hookers">my own</a> static site generator had less to do with <a href="https://gohugo.io/templates/introduction/">Hugo's (and golang's)</a> awkward extensibility limitations, and more to do with my hunger for weekend software development.

So now the Glog has its own static site generator, <a href="https://github.com/tsuereth/glog">and it's on GitHub</a>.  (The code is public, not for general-purpose use or for community-building, just to "show my work.")

I've already been building live Glog content with `GlogGenerator` for a couple weeks -- once I got it over the hurdle of matching Hugo's output.  Now I'm at the fun part: gradually working through an endless TO-DO list of workflow fixes and output enhancements.

What does this mean for the Glog as you see it?  ... not a hell of a lot!  So far, there are only a few <i>intentional</i> changes, like ensuring that inter-Glog links (such as a post's links to Game or Tag pages) are always valid.

(There've also been some <i>unintentional</i> changes, like new URLs to some of those Game and Tag pages, as a result of fixing how punctuation is URL-ized.)

But, at least for the foreseeable future, `GlogGenerator` isn't meant to revolutionize how I complain about videogames.  It's meant to be a project I can hack away on as a hobby.  ... like the Glog itself.
