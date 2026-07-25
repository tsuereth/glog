+++
date = "2026-07-25T11:09:33.2275449-07:00"
title = "The year of the ... SteamOS Desktop?"
category = [ "Video Game Theory" ]
+++
When [Steam Machine reviews](https://www.ign.com/articles/steam-machine-review) enlightened me about its hardware details, I wondered how my home theater PC - which was a gaming desktop, before my last upgrade - compared to those specifications. (I'm imagining that future Steam games might be benchmarked with them.) And when the specs turned out to be fairly similar, *then* I wondered: what's up with [this SteamOS thing](https://store.steampowered.com/steamos)?

Now, after a few weeks of using SteamOS for living room gaming - <game:Hades II> is going great, by the way - as well as casual video streaming, I'm overall happy with it.

- Installation wasn't *completely* painless, but very close; the first-launch setup wizard needed [some CLI help finding my network adapter](https://github.com/ValveSoftware/SteamOS/issues/2345#issuecomment-4793761076). After that, everything wizard-ed just fine.

- For my personal use-case, I wanted to default to "Desktop Mode" at startup, since web browsing and video streaming works better there than in Valve's "Game Mode." A single `steamosctl` command [made that happen](https://github.com/ValveSoftware/SteamOS/issues/2238#issuecomment-3649813478).

- I did struggle for some time with audio output. There seem to be some common issues in the current SteamOS stable release (3.8) with [surround sound](https://github.com/ValveSoftware/SteamOS/issues/2627), even [on Steam Decks](https://github.com/ValveSoftware/SteamOS/issues/2582) -- though I [found a working hackaround](https://github.com/ValveSoftware/SteamOS/issues/2415#issuecomment-5016877794), the built-in sound configuration definitely needs some attention.

That latter part is where some of SteamOS's cracks started becoming more visible to me. As it's a Linux build and ecosystem curated *specifically* for Valve's own devices, any use-case other than those devices - and the features Valve tests on them - is liable to rot, unsupported.

And while I'm personally willing (happy, even) to be my own tech support and [sysadmin this](https://www.youtube.com/watch?v=dFUlAQZB9Ng), the other thing about SteamOS is that it treats almost all system configuration as sacred: making large swaths of it [read-only by default, and overwriting customizations in updates](https://steamcommunity.com/app/1675200/discussions/1/4633734546101122629/#c4633734546101122989). Which, hey, is a nice safeguard for rolling updates. But an annoyance for [personal customizations](https://github.com/ValveSoftware/SteamOS/issues/2623#issuecomment-4983913511).

Hopefully, features like built-in surround-sound support will become more of a priority as the Steam Machine itself lands in more homes.

I'll say though, that despite its warts, and despite those administrative hurdles, SteamOS in my living room is already a more-than-satisfying replacement for Windows. I was genuinely impressed by [gamescope](https://github.com/ValveSoftware/gamescope)'s runtime performance; [Proton compatibility](https://www.protondb.com/) is practically a given these days, as is Linux support for USB and/or Bluetooth gamepads; and the desktop mode's [KDE Plasma](https://kde.org/plasma-desktop/) environment does everything I need in a home theater context.

Between Microsoft's continuous pressure to [force online accounts](https://arstechnica.com/gadgets/2025/10/microsoft-removes-even-more-microsoft-account-workarounds-from-windows-11-build/) and [cloud storage](https://www.tomshardware.com/software/windows/microsoft-now-forces-automatic-onedrive-backups-feature-enabled-during-clean-windows-installs-users-surprised-with-desktop-icons-and-files), Windows updates pushing in [unwanted AI features](https://arstechnica.com/gadgets/2025/11/new-windows-11-ai-agents-can-work-in-the-background-but-create-new-security-risks/) while simultaneously [breaking existing systems](https://tech.slashdot.org/story/26/01/18/1932246/microsoft-forced-to-issue-emergency-out-of-band-windows-update) - really making a [habit of it](https://it.slashdot.org/story/26/03/28/2013229/do-emergency-microsoft-oracle-patches-point-to-wider-issues) - and, fuckin' *[ads in the menus](https://www.howtogeek.com/windows-11-start-menu-ads-how-to-turn-them-off/)*, I don't miss the old Windows install at all.

As for my *main* desktop, a dual use-case of software development plus gaming, SteamOS doesn't fit; it's just not flexible enough. But now I'm wondering how easily I might squeeze the same juice from `gamescope` and `proton` in a more generic Linux distro...
