+++
category = [ "Site News" ]
date = "2015-10-24T14:02:50-07:00"
title = "Glog: Now With 100% Less Server Execution"
draft = false
+++

This has been "in the works" (barely) for a while, but I've finally transitioned the Glog to a fully static site.  I'm now using the <a href="https://gohugo.io">Hugo</a> site generator, with a personal modification of the <a href="https://github.com/cxfksword/greyshade">Greyshade</a> theme, to render and serve Glog pages completely statically.

Why the move away from [WordPress]($SiteBaseURL$2013/05/11/onward-and-upward/)?  Well, aside from the ever-present risk of <a href="http://www.cvedetails.com/vulnerability-list/vendor_id-2337/product_id-4096/">shitty PHP exploits</a>, I just got sick of running software upgrades on the site, and its plugins, that required me to re-apply Glog customizations.  Now I can write a post in my text editor, hit a button to publish it online, and never have to worry about the site again until <i>I decide that I want to</i>.

Editing my Hugo theme is a lot easier than screwing around with a PHP theme in WordPress, too, although it still has the same basic problem of not making it very clear what templates/functions live in the core framework vs. what lives in the theme customizations.

(I had begun a plan to write my own static site generator, instead of using something pre-baked like Hugo, but when I realized that I'd forgotten to implement list/archive pages ... I gave up.  Maybe I'll try again in another two years.)

All the old content should be here, and all old posts should even be accessible at their original URLs.  Paths for some game-specific post lists have changed, though, due to differences between how Hugo and WordPress make URL-safe strings.

Of course, as a fully static site, there is no longer any such thing as a comment or user account here.  Even better!
