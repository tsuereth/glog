#!/usr/bin/env perl
use FindBin;
use lib $FindBin::Bin;
use strict;
use warnings;

use GameCache;
use Glog;

binmode STDOUT, ":utf8";

if(scalar(@ARGV) < 1) {
	die "Usage: ./check-game-names.pl LOCAL_CACHE_FILE";
}

my $cache_file = $ARGV[0];

# Find all game names referenced in Glog content.
my @gamenames;
Glog::GetAllGameNames(\@gamenames);

# Create or load the local cache.
if(-f($cache_file)) {
	print "Opening local cache at \"$cache_file\" ...\n";
	GameCache::LoadFromFlatFile($cache_file);
}
else {
	print "Creating new cache to save at \"$cache_file\" ...\n";
	GameCache::Initialize();
}

# Check each game name against the cache.
foreach my $gamename(@gamenames) {
	my $igdb_id = Glog::GetIgdbId($gamename);
	if(defined($igdb_id)) {
		my $cachename = GameCache::GetGameName($igdb_id);
		if(defined($cachename)) {
			$gamename = Glog::NormalizeGameName($gamename);
			$cachename = Glog::NormalizeGameName($cachename);
			if($gamename ne $cachename) {
				print "IGDB ID $igdb_id is locally \"$gamename\" but IGDB calls it \"$cachename\"\n";
			}
		}
		else {
			print "No cache name for game \"$gamename\"\n";
		}
	}
	else {
		print "No IGDB ID for game \"$gamename\"\n";
	}
}
